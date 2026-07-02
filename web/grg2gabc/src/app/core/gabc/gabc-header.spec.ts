import { GabcHeader } from './gabc-header';
import { Grg2Document } from '../grg2/grg2-document.model';
import { Grg2Staff } from '../grg2/grg2-staff.model';
import { Grg2Initial } from '../grg2/grg2-initial.model';

function documentWithInitial(initial: Grg2Initial | null): Grg2Document {
  const doc = new Grg2Document();
  const staff = new Grg2Staff();
  staff.initial = initial;
  doc.staffs = [staff];
  return doc;
}

function makeInitial(overrides: Partial<Grg2Initial> = {}): Grg2Initial {
  const init = new Grg2Initial();
  init.antiphonCaption = '';
  init.modusCaption = '';
  Object.assign(init, overrides);
  return init;
}

describe('GabcHeader', () => {
  it('includes the LaTeX preamble, name and generated-by attributes', () => {
    const header = new GabcHeader('introit-01').toString();
    expect(header).toContain('% !TEX TS-program = LuaLaTeX+se');
    expect(header).toContain('% !TEX root = introit-01.tex');
    expect(header).toContain('name:introit-01;');
    expect(header).toContain('generated-by:grg2gabc;');
    expect(header).toMatch(/transcription-date:\d{4}-\d{2}-\d{2};/);
  });

  it('ends with the GABC header/body separator', () => {
    const header = new GabcHeader('x').toString();
    expect(header.endsWith('\n%%\n\n')).toBe(true);
  });

  it('supports chaining additional attributes', () => {
    const header = new GabcHeader('x').add('author', 'Anon').toString();
    expect(header).toContain('author:Anon;');
  });

  it('marks initial-style 0 when the first staff has no initial', () => {
    const header = new GabcHeader('x').addInitial(documentWithInitial(null)).toString();
    expect(header).toContain('initial-style:0;');
    expect(header).not.toContain('annotation:');
  });

  it('marks initial-style 1 and skips blank captions when the first staff has an initial', () => {
    const initial = makeInitial();
    const header = new GabcHeader('x').addInitial(documentWithInitial(initial)).toString();
    expect(header).toContain('initial-style:1;');
    expect(header).not.toContain('annotation:');
  });

  it('adds the antiphon caption as an annotation', () => {
    const initial = makeInitial({ antiphonCaption: 'Ad te levavi' });
    const header = new GabcHeader('x').addInitial(documentWithInitial(initial)).toString();
    expect(header).toContain('annotation:Ad te levavi;');
  });

  it('adds an empty annotation placeholder before the modus caption when there is no antiphon caption', () => {
    const initial = makeInitial({ modusCaption: 'I' });
    const header = new GabcHeader('x').addInitial(documentWithInitial(initial)).toString();
    expect(header).toContain('annotation:;\nannotation:I;');
  });
});
