import { Grg2Neume } from '../grg2/grg2-neume.model';
import { NeumeFormat } from './neume-format';
import { NeumePart } from './neume-part';

export interface UnmappedNeume {
  id: number;
  positionX: number;
  positionY: number;
  caption: string;
}

export class Grg2NeumeForGabc extends Grg2Neume {
  format: NeumeFormat | null;
  private gabcRhythmics: Grg2NeumeForGabc[];

  get sizeX(): number { return this.format?.sizeX ?? 0; }

  constructor(orig: Grg2Neume) {
    super();
    this.segmentLength = orig.segmentLength;
    this.id = orig.id;
    this.positionX = orig.positionX;
    this.positionY = orig.positionY;
    this.unknownValue4 = orig.unknownValue4;
    this.unknownValue5 = orig.unknownValue5;

    // Reformat trailing whitespace/hyphens in caption
    const match = orig.caption.match(/([\-\ ]+)$/);
    if (match) {
      const stripped = orig.caption.slice(0, orig.caption.length - match[1].length);
      this.caption = match[1].includes(' ') ? stripped + ' ' : stripped;
    } else {
      this.caption = orig.caption;
    }

    const template = Grg2NeumeForGabc.NEUME_FORMATS.get(orig.id);
    this.format = template ? template.clone() : null;

    this.gabcRhythmics = orig.rhythmics.map(r => new Grg2NeumeForGabc(r));
  }

  translateNeume(): string {
    if (!this.format) return '';

    for (const part of this.format.parts) this.resolvePartFormat(part);
    for (const rhythmic of this.gabcRhythmics) this.applyRhythmic(rhythmic);

    return this.format.toString();
  }

  private applyRhythmic(rhythmic: Grg2NeumeForGabc): void {
    if (!rhythmic.format) return;
    try {
      const rPart = rhythmic.format.parts[0];
      if (rhythmic.id === 23) {
        const applyTo = this.format!.parts[this.format!.parts.length - 1];
        applyTo.after += rPart.before;
      } else if (rhythmic.id === 80) { // Ictus
        const applyTo = rPart.findClosestPart(rhythmic, this);
        if (NeumePart.above(rhythmic, rPart, this, applyTo))
          applyTo.after += rPart.before + '1';
        else if (NeumePart.below(rhythmic, rPart, this, applyTo))
          applyTo.after += rPart.before + '0';
        else
          applyTo.after += rPart.before;
      } else if (rhythmic.id === 81 || rhythmic.id === 82) { // Episemas
        for (const part of this.format!.parts) {
          const sx = NeumePart.rangeSubset(
            rhythmic.positionX + rPart.startX, rhythmic.positionX + rPart.startX + rPart.sizeX,
            this.positionX + part.startX, this.positionX + part.startX + part.sizeX);
          if (sx < NeumePart.min(2, Math.trunc(part.sizeX / 2))) continue;

          let toAdd: string;
          if (NeumePart.above(rhythmic, rPart, this, part)) toAdd = rPart.before + '1';
          else if (NeumePart.below(rhythmic, rPart, this, part)) toAdd = rPart.before + '0';
          else toAdd = rPart.before;

          if (!part.after.includes(toAdd)) part.after += toAdd;
        }
        const last = this.format!.parts[this.format!.parts.length - 1];
        if (
          (last.after.endsWith(rPart.before + '0') ||
           last.after.endsWith(rPart.before + '1') ||
           last.after.endsWith(rPart.before)) &&
          (this.positionX + this.format!.sizeX) - (rhythmic.positionX + rPart.startX + rPart.sizeX) >= -1
        ) {
          last.after += '2';
        }
      }
    } catch {
      // No format for this rhythmic neume — skip silently
    }
  }

  resolvePartFormat(part: NeumePart): void {
    let fmt = part.height;

    const baseHeight = Math.trunc((72 - this.positionY) / 6);
    if (baseHeight < 0 || baseHeight > 12)
      throw new RangeError(`Invalid neume base height: ${baseHeight}`);

    // x → lowercase letter [a-m]
    fmt = fmt.replace(/x(?:([+-]\d))?/g, (_, mod) => {
      const val = this.solve(`${baseHeight}${mod ?? ''}`);
      if (val < 0 || val > 12) throw new RangeError(`Height modifier out of range: ${val}`);
      return String.fromCharCode('a'.charCodeAt(0) + val);
    });

    // X → uppercase letter [A-M]
    fmt = fmt.replace(/X(?:([+-]\d))?/g, (_, mod) => {
      const val = this.solve(`${baseHeight}${mod ?? ''}`);
      return String.fromCharCode('A'.charCodeAt(0) + val);
    });

    // Q → clef indicator
    fmt = fmt.replace(/Q(?:[+-]\d)?/g, () => this.getClefIndicator(false));

    // N → movable line position [1-6]
    fmt = fmt.replace(/N(?:([+-]\d))?/g, (_, mod) => {
      let bHeight: number;
      if (this.positionY <= 22) bHeight = 5;
      else if (this.positionY <= 28) bHeight = 6;
      else if (this.positionY <= 34) bHeight = 3;
      else if (this.positionY <= 40) bHeight = 4;
      else if (this.positionY <= 46) bHeight = 1;
      else bHeight = 2;
      return String(this.solve(`${bHeight}${mod ?? ''}`));
    });

    part.height = fmt;
  }

  solve(expression: string): number {
    const match = expression.match(/(\d+)([+-])(\d+)/);
    if (match) {
      const a = parseInt(match[1], 10);
      const b = parseInt(match[3], 10);
      return this.solve(String(match[2] === '+' ? a + b : a - b));
    }
    return parseInt(expression, 10);
  }

  // ─── Neume format lookup table ────────────────────────────────────────────
  // Format: NeumeFormat(sizeX, sizeY, ...NeumePart(startX, startY, before, height, after, sizeX, sizeY))
  // Height placeholders: x → [a-m], X → [A-M], Q → clef indicator, N → movable line
  // Entries commented out have no equivalent GABC symbol and are left unmapped intentionally.

  static readonly NEUME_FORMATS = new Map<number, NeumeFormat>([
    [  0, new NeumeFormat( 6, 10, new NeumePart( 0,  0, '',   'X',   '>',   6, 10))], // Apostropha
    [  1, new NeumeFormat( 1, 27, new NeumePart( 0,  5, ';',  '',    '',    1, 22))], // 1/2 Barre
    [  2, new NeumeFormat( 1, 14, new NeumePart( 0,  6, ',',  '',    '',    1,  8))], // 1/4 Barre
    [  3, new NeumeFormat( 6, 46, new NeumePart( 0,  9, '::', '',    '',    6, 35))], // Double Barre
    [  4, new NeumeFormat( 1, 46, new NeumePart( 0,  9, ':',  '',    '',    1, 35))], // Barre
    [  5, new NeumeFormat( 6, 13, new NeumePart( 0,  1, '',   'x-1', 'x',   6, 12))], // Bemol
    [  6, new NeumeFormat( 7, 23, new NeumePart( 0,  3, '',   'Q',   '',    7, 20))], // Cle do
    [  7, new NeumeFormat(12, 19, new NeumePart( 0,  3, '',   'Q',   '',   12, 16))], // Cle fa

    [  8, new NeumeFormat(12, 19,
          new NeumePart( 0,  0, '',  'x',   'v',  6,  7),
          new NeumePart( 7,  5, '!', 'X-1', '',   5, 10))], // Climacus2
    [  9, new NeumeFormat(17, 20,
          new NeumePart( 0,  0, '',  'x',   'v',  6,  7),
          new NeumePart( 6,  5, '!', 'X-1', '',   5, 10),
          new NeumePart(10, 11, '',  'X-2', '',   5, 10))], // Climacus3
    [ 10, new NeumeFormat(21, 26,
          new NeumePart( 0,  0, '',  'x',   'v',  6,  7),
          new NeumePart( 6,  5, '!', 'X-1', '',   5, 10),
          new NeumePart(10, 11, '',  'X-2', '',   5, 10),
          new NeumePart(14, 17, '',  'X-3', '',   5, 10))], // Climacus4
    [ 11, new NeumeFormat(24, 33,
          new NeumePart( 0,  0, '',  'x',   'v',  6,  7),
          new NeumePart( 6,  5, '!', 'X-1', '',   5, 10),
          new NeumePart(10, 11, '',  'X-2', '',   5, 10),
          new NeumePart(14, 17, '',  'X-3', '',   5, 10),
          new NeumePart(18, 23, '',  'X-4', '',   5, 10))], // Climacus5

    [ 12, new NeumeFormat(11, 17,
          new NeumePart( 0,  2, '',  'x',   '',   6,  7),
          new NeumePart( 5,  6, '',  'x-1', '',   6,  7))], // Clivis2
    [ 13, new NeumeFormat(11, 19,
          new NeumePart( 0,  0, '',  'x',   '',   6,  7),
          new NeumePart( 5, 12, '',  'x-2', '',   6,  7))], // Clivis3
    [ 14, new NeumeFormat(11, 25,
          new NeumePart( 0,  0, '',  'x',   '',   6,  7),
          new NeumePart( 5, 18, '',  'x-3', '',   6,  7))], // Clivis4
    [ 15, new NeumeFormat(11, 32,
          new NeumePart( 0,  0, '',  'x',   '',   6,  7),
          new NeumePart( 5, 24, '',  'x-4', '',   6,  7))], // Clivis5

    [ 16, new NeumeFormat( 7, 10, new NeumePart( 0,  0, '',  'x',   'o',   7, 10))], // Oriscus
    [ 17, new NeumeFormat(11, 16,
          new NeumePart( 0,  6, '',  'x-1', 'o',  7, 10),
          new NeumePart( 5,  0, '',  'x',   '',   6,  7))], // Pes Quassus 2
    [ 18, new NeumeFormat(11, 22,
          new NeumePart( 0, 12, '',  'x-2', 'o',  7, 10),
          new NeumePart( 5,  0, '',  'x',   '',   6,  7))], // Pes Quassus 3

    [ 19, new NeumeFormat( 6, 14,
          new NeumePart( 0,  0, '',  'x-1', '',   6,  7),
          new NeumePart( 0,  8, '',  'x',   '',   6,  7))], // Podatus2
    [ 20, new NeumeFormat( 6, 19,
          new NeumePart( 0,  0, '',  'x-2', '',   6,  7),
          new NeumePart( 0, 13, '',  'x',   '',   6,  7))], // Podatus3
    [ 21, new NeumeFormat( 6, 25,
          new NeumePart( 0,  0, '',  'x-3', '',   6,  7),
          new NeumePart( 0, 19, '',  'x',   '',   6,  7))], // Podatus4
    [ 22, new NeumeFormat( 6, 31,
          new NeumePart( 0,  0, '',  'x-4', '',   6,  7),
          new NeumePart( 0, 25, '',  'x',   '',   6,  7))], // Podatus5

    [ 23, new NeumeFormat( 3,  6, new NeumePart( 0,  3, '.',  '',    '',    3,  3))], // Point
    [ 24, new NeumeFormat( 6,  7, new NeumePart( 0,  0, '',   'x',   '',    6,  7))], // Punctum
    [ 25, new NeumeFormat( 6,  7, new NeumePart( 0,  7, '',   'x',   'w',   6,  7))], // Quilisma

    [ 26, new NeumeFormat( 6, 14,
          new NeumePart( 0,  0, '',  'x-1', 'w',  6,  7),
          new NeumePart( 0,  7, '',  'x',   '',   6,  7))], // Quilisma-pes

    [ 27, new NeumeFormat(16, 19,
          new NeumePart( 0, 12, '',  'x-2', '',   6,  7),
          new NeumePart( 5,  6, '!', 'x-1', 'o',  7, 10),
          new NeumePart(10,  0, '',  'x',   '',   6,  7))], // Salicus2
    [ 28, new NeumeFormat(16, 25,
          new NeumePart( 0, 18, '',  'x-3', '',   6,  7),
          new NeumePart( 5, 12, '!', 'x-2', 'o',  7, 10),
          new NeumePart(10,  0, '',  'x',   '',   6,  7))], // Salicus3

    [ 29, new NeumeFormat(16, 13,
          new NeumePart( 0,  6, '',  'x-1', '',   6,  7),
          new NeumePart( 5,  0, '',  'x',   '',   6,  7),
          new NeumePart(10,  6, '',  'x-1', '',   6,  7))], // torculus22
    [ 30, new NeumeFormat(16, 19,
          new NeumePart( 0,  6, '',  'x-1', '',   6,  7),
          new NeumePart( 5,  0, '',  'x',   '',   6,  7),
          new NeumePart(10, 12, '',  'x-2', '',   6,  7))], // torculus23
    [ 31, new NeumeFormat(16, 25,
          new NeumePart( 0,  6, '',  'x-1', '',   6,  7),
          new NeumePart( 5,  0, '',  'x',   '',   6,  7),
          new NeumePart(10, 18, '',  'x-3', '',   6,  7))], // torculus24
    [ 32, new NeumeFormat(16, 31,
          new NeumePart( 0,  6, '',  'x-1', '',   6,  7),
          new NeumePart( 5,  0, '',  'x',   '',   6,  7),
          new NeumePart(10, 24, '',  'x-4', '',   6,  7))], // torculus25
    [ 33, new NeumeFormat(16, 19,
          new NeumePart( 0, 12, '',  'x-2', '',   6,  7),
          new NeumePart( 5,  0, '',  'x',   '',   6,  7),
          new NeumePart(10,  6, '',  'x-1', '',   6,  7))], // torculus32
    [ 34, new NeumeFormat(16, 19,
          new NeumePart( 0, 12, '',  'x-2', '',   6,  7),
          new NeumePart( 5,  0, '',  'x',   '',   6,  7),
          new NeumePart(10, 12, '',  'x-2', '',   6,  7))], // torculus33
    [ 35, new NeumeFormat(16, 25,
          new NeumePart( 0, 12, '',  'x-2', '',   6,  7),
          new NeumePart( 5,  0, '',  'x',   '',   6,  7),
          new NeumePart(10, 18, '',  'x-3', '',   6,  7))], // torculus34
    [ 36, new NeumeFormat(16, 31,
          new NeumePart( 0, 12, '',  'x-2', '',   6,  7),
          new NeumePart( 5,  0, '',  'x',   '',   6,  7),
          new NeumePart(10, 24, '',  'x-4', '',   6,  7))], // torculus35
    [ 37, new NeumeFormat(16, 25,
          new NeumePart( 0, 18, '',  'x-3', '',   6,  7),
          new NeumePart( 5,  0, '',  'x',   '',   6,  7),
          new NeumePart(10,  6, '',  'x-1', '',   6,  7))], // torculus42
    [ 38, new NeumeFormat(16, 25,
          new NeumePart( 0, 18, '',  'x-3', '',   6,  7),
          new NeumePart( 5,  0, '',  'x',   '',   6,  7),
          new NeumePart(10, 12, '',  'x-2', '',   6,  7))], // torculus43
    [ 39, new NeumeFormat(16, 25,
          new NeumePart( 0, 18, '',  'x-3', '',   6,  7),
          new NeumePart( 5,  0, '',  'x',   '',   6,  7),
          new NeumePart(10, 18, '',  'x-3', '',   6,  7))], // torculus44
    [ 40, new NeumeFormat(16, 31,
          new NeumePart( 0, 18, '',  'x-3', '',   6,  7),
          new NeumePart( 5,  0, '',  'x',   '',   6,  7),
          new NeumePart(10, 24, '',  'x-4', '',   6,  7))], // torculus45
    [ 41, new NeumeFormat(16, 31,
          new NeumePart( 0, 24, '',  'x-4', '',   6,  7),
          new NeumePart( 5,  0, '',  'x',   '',   6,  7),
          new NeumePart(10,  6, '',  'x-1', '',   6,  7))], // torculus52
    [ 42, new NeumeFormat(16, 31,
          new NeumePart( 0, 24, '',  'x-4', '',   6,  7),
          new NeumePart( 5,  0, '',  'x',   '',   6,  7),
          new NeumePart(10, 12, '',  'x-2', '',   6,  7))], // torculus53
    [ 43, new NeumeFormat(16, 31,
          new NeumePart( 0, 24, '',  'x-4', '',   6,  7),
          new NeumePart( 5,  0, '',  'x',   '',   6,  7),
          new NeumePart(10, 18, '',  'x-3', '',   6,  7))], // torculus54
    [ 44, new NeumeFormat(16, 31,
          new NeumePart( 0, 24, '',  'x-4', '',   6,  7),
          new NeumePart( 5,  0, '',  'x',   '',   6,  7),
          new NeumePart(10, 24, '',  'x-4', '',   6,  7))], // torculus55

    [ 45, new NeumeFormat( 6, 17, new NeumePart( 0,  0, '',  'x',   'v',   6,  6))], // Virga
    // 46: Guidon1 — no GABC symbol
    [ 47, new NeumeFormat( 3, 13, new NeumePart( 0,  6, '',  'x-1', '+',   3,  6))], // Custos (Gidon2)
    [ 48, new NeumeFormat( 6, 10, new NeumePart( 0,  0, '',  'x',   'v',   6,  6))], // Virga2
    [ 50, new NeumeFormat( 6, 10, new NeumePart( 0,  0, '',  'X',   '',    5, 10))], // Stropha
    [ 51, new NeumeFormat( 6, 10, new NeumePart( 0,  0, '',  'X',   '',    5, 10))], // Stropha1

    [ 52, new NeumeFormat(10, 15,
          new NeumePart( 0,  0, '',  'X',   '',   5, 10),
          new NeumePart( 5,  5, '',  'X-1', '',   5, 10))], // Stropha2
    [ 53, new NeumeFormat(14, 21,
          new NeumePart( 0,  0, '',  'X',   '',   5, 10),
          new NeumePart( 4,  5, '',  'X-1', '',   5, 10),
          new NeumePart( 8, 11, '',  'X-2', '',   5, 10))], // Stropha3

    [ 54, new NeumeFormat( 6, 10, new NeumePart( 0,  0, '',  'X',   '~',   5,  8))], // StrophaLiq
    [ 55, new NeumeFormat( 6, 10, new NeumePart( 0,  0, '',  'X',   '~',   5,  8))], // StrophaLiq1
    [ 56, new NeumeFormat(10, 15,
          new NeumePart( 0,  0, '',  'X',   '~',  5,  8),
          new NeumePart( 5,  5, '',  'X-1', '~',  5,  8))], // StrophaLiq2
    [ 57, new NeumeFormat(14, 21,
          new NeumePart( 0,  0, '',  'X',   '~',  5,  8),
          new NeumePart( 4,  5, '',  'X-1', '~',  5,  8),
          new NeumePart( 8, 11, '',  'X-2', '~',  5,  8))], // StrophaLiq3

    [ 60, new NeumeFormat(16, 13,
          new NeumePart( 0,  0, '',  'x',   '',   6,  7),
          new NeumePart(10,  8, '',  'x-1', '',   6,  7),
          new NeumePart(10,  0, '',  'x',   '',   6,  7))], // Porrectus2
    // 61, 62, 63: no GABC symbol
    [ 64, new NeumeFormat(16, 20,
          new NeumePart( 0,  0, '',  'x',   '',   6,  7),
          new NeumePart(10, 14, '',  'x-2', '',   6,  7),
          new NeumePart(10,  6, '',  'x-1', '',   6,  7))], // Porrectus32
    [ 65, new NeumeFormat(16, 20,
          new NeumePart( 0,  0, '',  'x',   '',   6,  7),
          new NeumePart(10, 14, '',  'x-2', '',   6,  7),
          new NeumePart(10,  0, '',  'x',   '',   6,  7))], // Porrectus33
    [ 67, new NeumeFormat(16, 20,
          new NeumePart( 0,  0, '',  'x',   '',   6,  7),
          new NeumePart(10, 14, '',  'x-2', '',   6,  7),
          new NeumePart(10,  6, '',  'x-1', '',   6,  7))], // Porrectus032
    // 68: no GABC symbol
    [ 69, new NeumeFormat(16, 13,
          new NeumePart( 0,  0, '',  'x',   '',   6,  7),
          new NeumePart(10,  8, '',  'x-1', '',   6,  7),
          new NeumePart(10,  0, '',  'x',   '',   6,  7))], // Porrectus2 variant

    [ 70, new NeumeFormat( 6,  7, new NeumePart( 0,  0, '',  'x',   '>',   6,  7))], // puncliq1
    [ 71, new NeumeFormat( 6,  7, new NeumePart( 0,  0, '',  'x',   'r',   6,  7))], // Punctum blanc
    [ 80, new NeumeFormat( 1,  9, new NeumePart( 0,  3, "'", '',    '',    1,  6))], // Ictus
    [ 81, new NeumeFormat( 6,  4, new NeumePart( 0,  3, '_', '',    '',    6,  1))], // Episeme
    [ 82, new NeumeFormat(12,  4, new NeumePart( 0,  3, '_', '',    '',   12,  1))], // Ligne

    [100, new NeumeFormat( 6, 13,
          new NeumePart( 0,  6, '',  'x-1', '',   6,  7),
          new NeumePart( 0,  0, '',  'x',   '~',  6,  3))], // podLiq2
    [101, new NeumeFormat( 6, 18,
          new NeumePart( 0, 12, '',  'x-2', '',   6,  7),
          new NeumePart( 0,  0, '',  'x',   '~',  6,  3))], // podLiq3
    [103, new NeumeFormat( 6, 24,
          new NeumePart( 0, 18, '',  'x-3', '',   6,  7),
          new NeumePart( 0,  0, '',  'x',   '~',  6,  3))], // podLiq4
    [104, new NeumeFormat(11, 12,
          new NeumePart( 0,  5, '-', 'x-1', '',   6,  6),
          new NeumePart( 5,  0, '',  'x',   '',   6,  7))], // PodDeb2
    [105, new NeumeFormat(11, 17,
          new NeumePart( 0, 10, '-', 'x-2', '',   6,  6),
          new NeumePart( 5,  0, '',  'x',   '',   6,  7))], // PodDeb3
    [106, new NeumeFormat( 6, 31,
          new NeumePart( 0, 24, '',  'x-4', '',   6,  7),
          new NeumePart( 0,  0, '',  'x',   '~',  6,  3))], // podLiq5

    [110, new NeumeFormat( 3,  5, new NeumePart( 0,  0, '`', '',    '',    3,  5))], // Virgule

    [112, new NeumeFormat( 6, 16,
          new NeumePart( 0,  0, '',  'x',   '',   6,  7),
          new NeumePart( 0,  5, '',  'x-1', '~',  6,  7))], // ClivisLiq2
    [113, new NeumeFormat( 6, 19,
          new NeumePart( 0,  0, '',  'x',   '',   6,  7),
          new NeumePart( 0, 12, '',  'x-2', '~',  6,  7))], // ClivisLiq3
    [114, new NeumeFormat( 6, 25,
          new NeumePart( 0,  0, '',  'x',   '',   6,  7),
          new NeumePart( 0, 17, '',  'x-3', '~',  6,  7))], // ClivisLiq4

    [120, new NeumeFormat(12, 19,
          new NeumePart( 0,  0, '',  'x',   'v',  6,  7),
          new NeumePart( 7,  5, '!', 'X-1', '~',  5,  8))], // Climacus2 Liq
    [121, new NeumeFormat(21, 26,
          new NeumePart( 0,  0, '',  'x',   'v',  6,  7),
          new NeumePart( 6,  5, '!', 'X-1', '~',  5,  8),
          new NeumePart(10, 11, '',  'X-2', '~',  5,  8),
          new NeumePart(14, 17, '',  'X-3', '~',  5,  8))], // Climacus4 Liq
    [122, new NeumeFormat(17, 20,
          new NeumePart( 0,  0, '',  'x',   'v',  6,  7),
          new NeumePart( 6,  5, '!', 'X-1', '~',  5,  8),
          new NeumePart(10, 11, '',  'X-2', '~',  5,  8))], // Climacus3 Liq
    [123, new NeumeFormat(25, 32,
          new NeumePart( 0,  0, '',  'x',   'v',  6,  7),
          new NeumePart( 6,  5, '!', 'X-1', '~',  5,  8),
          new NeumePart(10, 11, '',  'X-2', '~',  5,  8),
          new NeumePart(14, 17, '',  'X-3', '~',  5,  8),
          new NeumePart(18, 23, '',  'X-4', '~',  5,  8))], // Climacus5 Liq

    [130, new NeumeFormat( 5,  8, new NeumePart( 0,  0, '',  'x',   'y',   5,  8))], // becar

    // 131: Diese — no GABC symbol
    // 160, 161: internal building blocks — not used directly in GRG2 files
    // 201: Debilis2 — not present in standard files
    [202, new NeumeFormat( 6,  7, new NeumePart( 0,  0, '',  'x',   '<',   6,  7))], // Liquescent1
    // 203, 205: Debilis — no GABC symbol
    [211, new NeumeFormat( 1,  8, new NeumePart( 0,  0, ',', '',    '',    1,  8))], // Lien1
    [212, new NeumeFormat( 1, 15, new NeumePart( 0,  0, ';', 'N',   '',    1, 15))], // Lien2
    [213, new NeumeFormat( 1, 22, new NeumePart( 0,  0, ';', 'N',   '',    1, 22))], // Lien3
    [214, new NeumeFormat( 1, 29, new NeumePart( 0,  0, ';', 'N',   '',    1, 29))], // Lien4
    [215, new NeumeFormat( 1, 36, new NeumePart( 0,  0, ':', '',    '',    1, 36))], // Lien5
  ]);
}
