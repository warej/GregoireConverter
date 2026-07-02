import { Grg2, MAGIC, MAX_FILE_SIZE, MIN_FILE_SIZE } from './grg2.model';

class BufferBuilder {
  private bytes: number[] = [];

  string(s: string, padTo: number): this {
    const bytes = [...s].map(c => c.charCodeAt(0));
    while (bytes.length < padTo) bytes.push(0);
    this.bytes.push(...bytes);
    return this;
  }

  word(value: number): this {
    this.bytes.push(value & 0xff, (value >> 8) & 0xff);
    return this;
  }

  dword(value: number): this {
    this.bytes.push(value & 0xff, (value >> 8) & 0xff, (value >> 16) & 0xff, (value >> 24) & 0xff);
    return this;
  }

  byte(value: number): this {
    this.bytes.push(value & 0xff);
    return this;
  }

  rgba(r: number, g: number, b: number, a: number): this {
    return this.byte(r).byte(g).byte(b).byte(a);
  }

  build(): ArrayBuffer {
    return new Uint8Array(this.bytes).buffer;
  }
}

const SEGMENT = { DOCUMENT: 0xf1ff, STAFF: 0xf2ff, NEUME: 0xf4ff };

function validMinimalGrgBuffer(): ArrayBuffer {
  return new BufferBuilder()
    .string(MAGIC, 4)
    .word(SEGMENT.DOCUMENT)
    .word(0x0020) // segmentLength
    .word(12) // fontSize
    .string('Arial', 0x10) // fontFamily
    .rgba(0, 0, 0, 255) // staffColor
    .rgba(0, 0, 0, 255) // neumesColor
    .rgba(0, 0, 0, 255) // fontColor
    .word(5) // spaceUnderStaff
    .word(SEGMENT.STAFF)
    .word(0x0003) // segmentLength
    .word(100) // width
    .byte(1) // justify
    .word(SEGMENT.NEUME)
    .word(0x001a) // segmentLength
    .word(24) // id (Punctum)
    .word(30) // positionY
    .word(0) // positionX
    .word(0) // unknownValue4
    .word(0) // unknownValue5
    .string('Ky', 0x10) // caption
    .build();
}

describe('Grg2.parse', () => {
  it('rejects buffers larger than the 10 MB limit', () => {
    const buffer = new ArrayBuffer(MAX_FILE_SIZE + 1);
    expect(() => Grg2.parse(buffer)).toThrowError(RangeError);
  });

  it('rejects buffers shorter than the minimum valid size', () => {
    const buffer = new ArrayBuffer(MIN_FILE_SIZE - 1);
    expect(() => Grg2.parse(buffer)).toThrowError();
  });

  it('rejects a buffer whose magic bytes are not "GRG2"', () => {
    const buffer = new BufferBuilder().string('XXXX', MIN_FILE_SIZE).build();
    expect(() => Grg2.parse(buffer)).toThrowError(/Invalid file format/);
  });

  it('rejects an unrecognized segment type', () => {
    const buffer = new BufferBuilder().string(MAGIC, 4).word(0x9999).string('', MIN_FILE_SIZE - 6).build();
    expect(() => Grg2.parse(buffer)).toThrowError(/Unknown segment type/);
  });

  it('parses a minimal document/staff/neume buffer end to end', () => {
    const grg = Grg2.parse(validMinimalGrgBuffer());

    expect(grg.documents.length).toBe(1);
    const doc = grg.documents[0];
    expect(doc.fontFamily).toBe('Arial');
    expect(doc.staffs.length).toBe(1);

    const staff = doc.staffs[0];
    expect(staff.width).toBe(100);
    expect(staff.neumes.length).toBe(1);
    expect(staff.neumes[0].id).toBe(24);
    expect(staff.neumes[0].caption).toBe('Ky');
  });

  it('sorts neumes within a staff by positionX', () => {
    const buffer = new BufferBuilder()
      .string(MAGIC, 4)
      .word(SEGMENT.DOCUMENT)
      .word(0x0020).word(12).string('Arial', 0x10)
      .rgba(0, 0, 0, 255).rgba(0, 0, 0, 255).rgba(0, 0, 0, 255)
      .word(5)
      .word(SEGMENT.STAFF)
      .word(0x0003).word(100).byte(1)
      .word(SEGMENT.NEUME)
      .word(0x001a).word(24).word(0).word(10).word(0).word(0).string('B', 0x10)
      .word(SEGMENT.NEUME)
      .word(0x001a).word(24).word(0).word(0).word(0).word(0).string('A', 0x10)
      .build();

    const grg = Grg2.parse(buffer);
    const captions = grg.documents[0].staffs[0].neumes.map(n => n.caption);
    expect(captions).toEqual(['A', 'B']);
  });
});
