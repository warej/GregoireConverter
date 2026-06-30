import { Grg2StreamReader } from './grg2-stream-reader';

export class Grg2Neume {
  segmentLength!: number;
  id!: number;
  positionY!: number;
  positionX!: number;
  unknownValue4!: number;
  unknownValue5!: number;
  caption!: string;
  rhythmics: Grg2Neume[] = [];

  get isClef(): boolean { return this.id === 6 || this.id === 7; }
  get isBemol(): boolean { return this.id === 5; }
  get isCustos(): boolean { return this.id === 47; }
  get isDivisio(): boolean { return [1, 2, 3, 4, 211, 212, 213, 214, 215].includes(this.id); }
  get isRhythmic(): boolean { return [23, 80, 81, 82].includes(this.id); }

  getClefIndicator(withBemol: boolean): string {
    let result: string;
    if (this.id === 6) result = 'c';
    else if (this.id === 7) result = 'f';
    else throw new RangeError(`Id ${this.id} does not point to a clef`);

    if (withBemol) result += 'b';

    const height = Math.trunc(-this.positionY / 12) + 5;
    if (height < 1 || height > 4)
      throw new RangeError(`${height} is not a valid clef height`);

    return `${result}${height}`;
  }

  static fromStream(reader: Grg2StreamReader): Grg2Neume {
    const n = new Grg2Neume();
    n.segmentLength = reader.readWord();  // expected 0x001A
    n.id = reader.readWord();
    n.positionY = reader.readWord();
    n.positionX = reader.readWord();
    n.unknownValue4 = reader.readWord();
    n.unknownValue5 = reader.readWord();
    n.caption = reader.readString(0x10);
    return n;
  }

  toString(): string {
    return `[Neume:(${this.id})'${this.caption}'@${this.positionX},${this.positionY}]`;
  }
}
