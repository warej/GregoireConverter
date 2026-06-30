import { Grg2Staff } from './grg2-staff.model';
import { Grg2StreamReader } from './grg2-stream-reader';

export class Grg2Document {
  segmentLength!: number;
  fontSize!: number;
  fontFamily!: string;
  staffColor!: { r: number; g: number; b: number; a: number };
  neumesColor!: { r: number; g: number; b: number; a: number };
  fontColor!: { r: number; g: number; b: number; a: number };
  spaceUnderStaff!: number;
  staffs: Grg2Staff[] = [];

  static fromStream(reader: Grg2StreamReader): Grg2Document {
    const d = new Grg2Document();
    d.segmentLength = reader.readWord();    // expected 0x0020
    d.fontSize = reader.readWord();
    d.fontFamily = reader.readString(0x10);
    d.staffColor = reader.readRgba();
    d.neumesColor = reader.readRgba();
    d.fontColor = reader.readRgba();
    d.spaceUnderStaff = reader.readWord();
    return d;
  }
}
