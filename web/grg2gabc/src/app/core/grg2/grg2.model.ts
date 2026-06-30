import { Grg2Document } from './grg2-document.model';
import { Grg2Initial } from './grg2-initial.model';
import { Grg2Neume } from './grg2-neume.model';
import { Grg2Staff } from './grg2-staff.model';
import { Grg2StreamReader } from './grg2-stream-reader';

export const MAGIC = 'GRG2';
export const MIN_FILE_SIZE = 0x28;
export const MAX_FILE_SIZE = 10 * 1024 * 1024;

const enum SegmentType {
  DOCUMENT = 0xf1ff,
  STAFF    = 0xf2ff,
  INITIAL  = 0xf3ff,
  NEUME    = 0xf4ff,
}

export class Grg2 {
  documents: Grg2Document[] = [];

  static parse(buffer: ArrayBuffer): Grg2 {
    if (buffer.byteLength > MAX_FILE_SIZE)
      throw new RangeError(`File exceeds ${MAX_FILE_SIZE / 1024 / 1024} MB limit`);
    if (buffer.byteLength < MIN_FILE_SIZE)
      throw new Error('File is shorter than the minimum valid GRG2 file size');

    const reader = new Grg2StreamReader(buffer);
    const magic = reader.readString(4);
    if (magic !== MAGIC)
      throw new Error(`Invalid file format: expected GRG2 header, got "${magic}"`);

    const grg = new Grg2();
    while (!reader.isAtEnd()) {
      if (reader.bytesRemaining < 2) break;
      const segType = reader.readWord();

      switch (segType as SegmentType) {
        case SegmentType.DOCUMENT:
          grg.documents.push(Grg2Document.fromStream(reader));
          break;
        case SegmentType.STAFF: {
          const doc = grg.documents[grg.documents.length - 1];
          const staff = Grg2Staff.fromStream(reader);
          doc.staffs.push(staff);
          break;
        }
        case SegmentType.INITIAL: {
          const doc = grg.documents[grg.documents.length - 1];
          const staff = doc.staffs[doc.staffs.length - 1];
          staff.initial = Grg2Initial.fromStream(reader);
          break;
        }
        case SegmentType.NEUME: {
          const doc = grg.documents[grg.documents.length - 1];
          const staff = doc.staffs[doc.staffs.length - 1];
          staff.neumes.push(Grg2Neume.fromStream(reader));
          break;
        }
        default:
          throw new Error(`Unknown segment type: 0x${segType.toString(16)}`);
      }
    }

    for (const doc of grg.documents) {
      for (const staff of doc.staffs) {
        staff.neumes.sort((a, b) => a.positionX - b.positionX);
        staff.assignRhythmics();
      }
    }

    return grg;
  }
}
