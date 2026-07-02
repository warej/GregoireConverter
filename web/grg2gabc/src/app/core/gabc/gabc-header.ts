import { Grg2Document } from '../grg2/grg2-document.model';

type AttributeKey =
  | 'name' | 'gabc-copyright' | 'score-copyright' | 'office-part' | 'occasion'
  | 'meter' | 'commentary' | 'arranger' | 'author' | 'date' | 'manuscript'
  | 'manuscript-reference' | 'manuscript-storage-place' | 'book' | 'language'
  | 'transcriber' | 'transcription-date' | 'mode' | 'initial-style'
  | 'user-notes' | 'annotation' | 'generated-by';

export class GabcHeader {
  private attributes: [AttributeKey, string][] = [];
  name: string;

  constructor(name: string) {
    this.name = name;
    this.add('name', name)
        .add('generated-by', 'grg2gabc')
        .add('transcription-date', new Date().toISOString().split('T')[0]);
  }

  add(key: AttributeKey, value: string): this {
    this.attributes.push([key, value]);
    return this;
  }

  addInitial(doc: Grg2Document): this {
    const initial = doc.staffs[0]?.initial ?? null;
    if (initial) {
      this.add('initial-style', '1');
      if (initial.antiphonCaption.trim())
        this.add('annotation', initial.antiphonCaption);
      if (initial.modusCaption.trim()) {
        if (!this.attributes.some(([k]) => k === 'annotation'))
          this.add('annotation', '');
        this.add('annotation', initial.modusCaption);
      }
    } else {
      this.add('initial-style', '0');
    }
    return this;
  }

  toString(): string {
    let header = `% !TEX TS-program = LuaLaTeX+se\n% !TEX root = ${this.name}.tex\n\n`;
    for (const [key, value] of this.attributes) {
      header += `${key}:${value};\n`;
    }
    return header + '\n%%\n\n';
  }
}
