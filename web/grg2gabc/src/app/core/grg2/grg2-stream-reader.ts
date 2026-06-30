export class Grg2StreamReader {
  private view: DataView;
  private offset = 0;

  constructor(buffer: ArrayBuffer) {
    this.view = new DataView(buffer);
  }

  get bytesRead(): number {
    return this.offset;
  }

  get bytesRemaining(): number {
    return this.view.byteLength - this.offset;
  }

  readString(length: number): string {
    if (length <= 0) throw new RangeError('length must be positive');
    if (this.offset + length > this.view.byteLength) throw new Error('Unexpected end of stream');
    const bytes = new Uint8Array(this.view.buffer, this.offset, length);
    this.offset += length;
    let end = bytes.indexOf(0);
    if (end < 0) end = length;
    return new TextDecoder('utf-8').decode(bytes.slice(0, end));
  }

  readByte(): number {
    if (this.offset >= this.view.byteLength) throw new Error('Unexpected end of stream');
    return this.view.getUint8(this.offset++);
  }

  readWord(): number {
    if (this.offset + 2 > this.view.byteLength) throw new Error('Unexpected end of stream');
    const value = this.view.getUint16(this.offset, true);
    this.offset += 2;
    return value;
  }

  readDword(): number {
    if (this.offset + 4 > this.view.byteLength) throw new Error('Unexpected end of stream');
    const value = this.view.getInt32(this.offset, true);
    this.offset += 4;
    return value;
  }

  readRgba(): { r: number; g: number; b: number; a: number } {
    const r = this.readByte();
    const g = this.readByte();
    const b = this.readByte();
    const a = this.readByte();
    return { r, g, b, a };
  }

  isAtEnd(): boolean {
    return this.offset >= this.view.byteLength;
  }
}
