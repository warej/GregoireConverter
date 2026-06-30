import { NeumePart } from './neume-part';

export class NeumeFormat {
  sizeX: number;
  sizeY: number;
  parts: NeumePart[];

  constructor(sizeX: number, sizeY: number, ...parts: NeumePart[]) {
    this.sizeX = sizeX;
    this.sizeY = sizeY;
    this.parts = parts;
  }

  toString(): string {
    return this.parts.map(p => p.before + p.height + p.after).join('');
  }

  clone(): NeumeFormat {
    return new NeumeFormat(
      this.sizeX,
      this.sizeY,
      ...this.parts.map(p => new NeumePart(p.startX, p.startY, p.before, p.height, p.after, p.sizeX, p.sizeY))
    );
  }
}
