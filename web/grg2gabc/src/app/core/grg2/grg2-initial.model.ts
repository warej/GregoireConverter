import { Grg2StreamReader } from './grg2-stream-reader';

export class Grg2Initial {
  segmentLength!: number;
  width!: number;

  antiphonFontSize!: number;
  antiphonFontFamily!: string;
  antiphonCaption!: string;

  modusFontSize!: number;
  modusFontFamily!: string;
  modusCaption!: string;

  initialFontSize!: number;
  initialFontFamily!: string;
  initialCaption!: string;

  static fromStream(reader: Grg2StreamReader): Grg2Initial {
    const init = new Grg2Initial();
    init.segmentLength = reader.readWord();   // expected 0x006A (106)
    init.width = reader.readDword();

    init.antiphonFontSize = reader.readWord();
    init.antiphonFontFamily = reader.readString(0x10);
    init.antiphonCaption = reader.readString(0x10);

    init.modusFontSize = reader.readWord();
    init.modusFontFamily = reader.readString(0x10);
    init.modusCaption = reader.readString(0x10);

    init.initialFontSize = reader.readWord();
    init.initialFontFamily = reader.readString(0x10);
    init.initialCaption = reader.readString(0x10);

    return init;
  }
}
