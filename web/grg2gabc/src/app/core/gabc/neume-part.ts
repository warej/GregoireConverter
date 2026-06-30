import { Grg2NeumeForGabc } from './grg2-neume-for-gabc';

export class NeumePart {
  startX: number;
  startY: number;
  before: string;
  height: string;
  after: string;
  sizeX: number;
  sizeY: number;

  constructor(startX: number, startY: number, before: string, height: string, after: string, sizeX: number, sizeY: number) {
    this.startX = startX;
    this.startY = startY;
    this.before = before;
    this.height = height;
    this.after = after;
    this.sizeX = sizeX;
    this.sizeY = sizeY;
  }

  findClosestPart(thisNeume: Grg2NeumeForGabc, mainNeume: Grg2NeumeForGabc): NeumePart {
    let candidateAbove: NeumePart | null = null;
    let candidateBelow: NeumePart | null = null;

    for (const part of mainNeume.format!.parts) {
      if (NeumePart.above(thisNeume, this, mainNeume, part)) {
        if (!candidateAbove) candidateAbove = part;
        else if (
          (mainNeume.positionY + part.startY) - (thisNeume.positionY + this.startY + this.sizeY) <
          (mainNeume.positionY + candidateAbove.startY) - (thisNeume.positionY + this.startY + this.sizeY)
        ) candidateAbove = part;
      }
      if (NeumePart.below(thisNeume, this, mainNeume, part)) {
        if (!candidateBelow) candidateBelow = part;
        else if (
          (thisNeume.positionY + this.startY + this.sizeY) - (mainNeume.positionY + part.startY) <
          (thisNeume.positionY + this.startY + this.sizeY) - (mainNeume.positionY + candidateBelow.startY)
        ) candidateBelow = part;
      }
    }

    if (candidateAbove && candidateBelow) {
      return (
        (mainNeume.positionY + candidateAbove.startY) - (thisNeume.positionY + this.startY + this.sizeY) >
        (thisNeume.positionY + this.startY + this.sizeY) - (mainNeume.positionY + candidateBelow.startY)
      ) ? candidateAbove : candidateBelow;
    }
    return candidateAbove ?? candidateBelow ?? mainNeume.format!.parts[mainNeume.format!.parts.length - 1];
  }

  static above(an: Grg2NeumeForGabc, ap: NeumePart, bn: Grg2NeumeForGabc, bp: NeumePart): boolean {
    const sx = NeumePart.rangeSubset(an.positionX + ap.startX, an.positionX + ap.startX + ap.sizeX,
                                     bn.positionX + bp.startX, bn.positionX + bp.startX + bp.sizeX);
    if (sx < NeumePart.min(2, Math.trunc(bp.sizeX / 2))) return false;
    return (an.positionY + ap.startY < bn.positionY + bp.startY) &&
      NeumePart.rangeSubset(an.positionY + ap.startY, an.positionY + ap.startY + ap.sizeY,
                            bn.positionY + bp.startY, bn.positionY + bp.startY + bp.sizeY) < 2;
  }

  static below(an: Grg2NeumeForGabc, ap: NeumePart, bn: Grg2NeumeForGabc, bp: NeumePart): boolean {
    const sx = NeumePart.rangeSubset(an.positionX + ap.startX, an.positionX + ap.startX + ap.sizeX,
                                     bn.positionX + bp.startX, bn.positionX + bp.startX + bp.sizeX);
    if (sx < NeumePart.min(2, Math.trunc(bp.sizeX / 2))) return false;
    return (an.positionY + ap.startY > bn.positionY + bp.startY) &&
      NeumePart.rangeSubset(an.positionY + ap.startY, an.positionY + ap.startY + ap.sizeY,
                            bn.positionY + bp.startY, bn.positionY + bp.startY + bp.sizeY) < 2;
  }

  static rangeSubset(al: number, ar: number, bl: number, br: number): number {
    const rightOne    = al <= bl ? ar : br;
    const secondLeft  = al <= bl ? bl : al;
    const secondRight = al <= bl ? br : ar;
    if (rightOne < secondLeft) return rightOne - secondLeft;
    if (rightOne > secondRight) return secondRight - secondLeft;
    return rightOne - secondLeft;
  }

  static min(a: number, b: number): number { return a < b ? a : b; }
  static max(a: number, b: number): number { return a > b ? a : b; }
}
