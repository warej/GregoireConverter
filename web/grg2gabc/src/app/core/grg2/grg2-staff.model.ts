import { Grg2Initial } from './grg2-initial.model';
import { Grg2Neume } from './grg2-neume.model';
import { Grg2StreamReader } from './grg2-stream-reader';

export class Grg2Staff {
  segmentLength!: number;
  width!: number;
  justify!: number;
  initial: Grg2Initial | null = null;
  neumes: Grg2Neume[] = [];

  static fromStream(reader: Grg2StreamReader): Grg2Staff {
    const s = new Grg2Staff();
    s.segmentLength = reader.readWord();  // expected 0x0003
    s.width = reader.readWord();
    s.justify = reader.readByte();
    return s;
  }

  assignRhythmics(): void {
    for (const n of this.neumes) n.rhythmics = [];

    for (let it = 0; it < this.neumes.length; it++) {
      if (!this.neumes[it].isRhythmic) continue;

      let closest: Grg2Neume | null = null;

      // look backwards
      for (let i = it - 1; i > 0; i--) {
        if (!this.neumes[i].isRhythmic) { closest = this.neumes[i]; break; }
      }

      // look forward
      for (let i = it + 1; i < this.neumes.length; i++) {
        if (!this.neumes[i].isRhythmic) {
          const closestDist = closest ? this.neumes[it].positionX - closest.positionX : Infinity;
          const iDist = this.neumes[i].positionX - this.neumes[it].positionX;
          if (iDist < 3 && iDist < closestDist) closest = this.neumes[i];
          break;
        }
      }

      if (closest !== null) {
        closest.rhythmics.push(this.neumes[it]);
        this.neumes.splice(it--, 1);
      }
    }
  }
}
