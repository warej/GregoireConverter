import { Grg2 } from '../grg2/grg2.model';
import { Grg2Document } from '../grg2/grg2-document.model';
import { Grg2Staff } from '../grg2/grg2-staff.model';
import { GabcHeader } from './gabc-header';
import { Grg2NeumeForGabc, UnmappedNeume } from './grg2-neume-for-gabc';

export interface ConversionResult {
  gabc: string;
  warnings: UnmappedNeume[];
}

const UNMAPPED_PLACEHOLDER = '(?)';

export class GabcExporter {
  private warnings: UnmappedNeume[] = [];

  convert(grg: Grg2, fileName: string): ConversionResult {
    this.warnings = [];
    const doc = grg.documents[0];
    if (!doc) throw new Error('GRG2 file contains no documents');

    const header = new GabcHeader(fileName).addInitial(doc);
    const body = this.generateGabcPart(doc);

    return { gabc: header.toString() + body, warnings: this.warnings };
  }

  private generateGabcPart(document: Grg2Document): string {
    let result = '';
    let lastClef: Grg2NeumeForGabc | null = null;

    for (let i = 0; i < document.staffs.length; i++) {
      result += this.convertStaff(document.staffs[i], i, lastClef, (clef) => { lastClef = clef; });
    }
    return result;
  }

  private convertStaff(
    staff: Grg2Staff,
    _staffIdx: number,
    lastClef: Grg2NeumeForGabc | null,
    setLastClef: (c: Grg2NeumeForGabc) => void,
  ): string {
    let result = '';
    let initialLetter = staff.initial?.initialCaption ?? '';
    let textBuf = '';
    let neumeBuf = '';

    const neumes = [...staff.neumes]
      .sort((a, b) => a.positionY - b.positionY)
      .sort((a, b) => a.positionX - b.positionX)
      .map(n => new Grg2NeumeForGabc(n));

    const flush = (): void => {
      if (textBuf.trim() || neumeBuf.trim()) {
        result += `${textBuf.trimEnd()}(${neumeBuf})`;
        textBuf = textBuf.endsWith(' ') ? ' ' : '';
        neumeBuf = '';
      }
    };

    for (let it = 0; it < neumes.length; it++) {
      const cur = neumes[it];
      const separator = it > 0 ? this.getSeparator(neumes[it - 1], cur) : null;

      if ((textBuf.trim() && cur.caption.trim()) || separator === null) {
        flush();
      }

      if (cur.isClef) {
        if (lastClef && lastClef.getClefIndicator(false) === cur.getClefIndicator(false)) continue;
        flush();
        const nextNeume = neumes[it + 1];
        if (nextNeume?.isBemol && nextNeume.positionY === cur.positionY + 6) {
          it++;
          neumeBuf += cur.getClefIndicator(true);
        } else {
          neumeBuf += cur.getClefIndicator(false);
        }
        setLastClef(cur);
        flush();
        continue;
      }

      if (cur.isCustos && it === neumes.length - 1) continue;

      if (cur.isDivisio) {
        flush();
        textBuf += cur.caption.trim() ? cur.caption : ' ';
        const translated = this.translateNeume(cur);
        neumeBuf = translated;
        if (neumeBuf.trim()) flush();
        textBuf += ' ';
        continue;
      }

      if (cur.caption.trim()) {
        if (initialLetter) {
          textBuf += initialLetter;
          initialLetter = '';
        }

        // Move trailing star to the following divisio
        const starMatch = cur.caption.match(/([* ]+)$/);
        if (starMatch && neumes[it + 1]?.isDivisio) {
          cur.caption = cur.caption.slice(0, cur.caption.length - starMatch[1].length);
          neumes[it + 1].caption = ' *' + neumes[it + 1].caption;
        }

        textBuf += cur.caption;
      }

      const translated = this.translateNeume(cur);
      if (translated) neumeBuf += (separator ?? '') + translated;
    }

    flush();
    result += (staff.justify > 0) ? '(z)\n\n' : '(Z)\n\n';
    return result;
  }

  private translateNeume(neume: Grg2NeumeForGabc): string {
    if (!neume.format) {
      this.warnings.push({ id: neume.id, positionX: neume.positionX, positionY: neume.positionY, caption: neume.caption });
      return UNMAPPED_PLACEHOLDER;
    }
    try {
      return neume.translateNeume();
    } catch {
      this.warnings.push({ id: neume.id, positionX: neume.positionX, positionY: neume.positionY, caption: neume.caption });
      return UNMAPPED_PLACEHOLDER;
    }
  }

  private getSeparator(prev: Grg2NeumeForGabc, cur: Grg2NeumeForGabc): string | null {
    const distance = cur.positionX - (prev.positionX + prev.sizeX);
    if (distance < 1)  return '!';
    if (distance < 3)  return '/';
    if (distance < 5)  return '//';
    if (distance < 7)  return ' ';
    return null;
  }
}
