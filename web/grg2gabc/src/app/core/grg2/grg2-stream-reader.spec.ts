import { Grg2StreamReader } from './grg2-stream-reader';

function bufferFrom(bytes: number[]): ArrayBuffer {
  return new Uint8Array(bytes).buffer;
}

describe('Grg2StreamReader', () => {
  it('reports bytesRead and bytesRemaining as it consumes the buffer', () => {
    const reader = new Grg2StreamReader(bufferFrom([1, 2, 3, 4]));
    expect(reader.bytesRead).toBe(0);
    expect(reader.bytesRemaining).toBe(4);

    reader.readByte();
    expect(reader.bytesRead).toBe(1);
    expect(reader.bytesRemaining).toBe(3);
  });

  it('reads an unsigned byte', () => {
    const reader = new Grg2StreamReader(bufferFrom([0x00, 0xff]));
    expect(reader.readByte()).toBe(0);
    expect(reader.readByte()).toBe(255);
  });

  it('reads a little-endian word', () => {
    const reader = new Grg2StreamReader(bufferFrom([0x34, 0x12]));
    expect(reader.readWord()).toBe(0x1234);
  });

  it('reads a little-endian signed dword', () => {
    const reader = new Grg2StreamReader(bufferFrom([0xff, 0xff, 0xff, 0xff]));
    expect(reader.readDword()).toBe(-1);
  });

  it('reads an RGBA quadruplet in order', () => {
    const reader = new Grg2StreamReader(bufferFrom([10, 20, 30, 40]));
    expect(reader.readRgba()).toEqual({ r: 10, g: 20, b: 30, a: 40 });
  });

  it('reads a fixed-length string and stops at the first NUL byte', () => {
    const bytes = [...'ab'].map(c => c.charCodeAt(0));
    const reader = new Grg2StreamReader(bufferFrom([...bytes, 0, 0, 0]));
    expect(reader.readString(5)).toBe('ab');
  });

  it('reads the full fixed length when there is no NUL byte', () => {
    const bytes = [...'abcde'].map(c => c.charCodeAt(0));
    const reader = new Grg2StreamReader(bufferFrom(bytes));
    expect(reader.readString(5)).toBe('abcde');
  });

  it('throws for a non-positive string length', () => {
    const reader = new Grg2StreamReader(bufferFrom([1, 2, 3]));
    expect(() => reader.readString(0)).toThrowError(RangeError);
  });

  it('throws when reading past the end of the buffer', () => {
    expect(() => new Grg2StreamReader(bufferFrom([])).readByte()).toThrow();
    expect(() => new Grg2StreamReader(bufferFrom([1])).readWord()).toThrow();
    expect(() => new Grg2StreamReader(bufferFrom([1, 2, 3])).readDword()).toThrow();
    expect(() => new Grg2StreamReader(bufferFrom([1, 2])).readString(3)).toThrow();
  });

  it('reports isAtEnd once every byte has been consumed', () => {
    const reader = new Grg2StreamReader(bufferFrom([1, 2]));
    expect(reader.isAtEnd()).toBe(false);
    reader.readWord();
    expect(reader.isAtEnd()).toBe(true);
  });
});
