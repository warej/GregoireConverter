import { GabcExporter } from './gabc-exporter';
import { Grg2 } from '../grg2/grg2.model';
import { Grg2Document } from '../grg2/grg2-document.model';
import { Grg2Staff } from '../grg2/grg2-staff.model';
import { Grg2Neume } from '../grg2/grg2-neume.model';

const PUNCTUM = 24;
const HALF_BARRE = 1; // divisio

function makeNeume(id: number, positionX: number, positionY: number, caption = ''): Grg2Neume {
  const n = new Grg2Neume();
  n.id = id;
  n.positionX = positionX;
  n.positionY = positionY;
  n.unknownValue4 = 0;
  n.unknownValue5 = 0;
  n.caption = caption;
  return n;
}

function grgWithStaff(staff: Grg2Staff): Grg2 {
  const doc = new Grg2Document();
  doc.staffs = [staff];
  const grg = new Grg2();
  grg.documents = [doc];
  return grg;
}

function staffOf(justify: number, ...neumes: Grg2Neume[]): Grg2Staff {
  const staff = new Grg2Staff();
  staff.justify = justify;
  staff.neumes = neumes;
  return staff;
}

describe('GabcExporter', () => {
  it('throws when the GRG2 file contains no documents', () => {
    const grg = new Grg2();
    grg.documents = [];
    expect(() => new GabcExporter().convert(grg, 'x')).toThrowError(/no documents/);
  });

  it('converts a single mapped neume and terminates the staff with (Z) when not justified', () => {
    const staff = staffOf(0, makeNeume(PUNCTUM, 0, 72, 'Ky'));
    const result = new GabcExporter().convert(grgWithStaff(staff), 'x');

    expect(result.gabc).toContain('Ky(a)(Z)\n\n');
    expect(result.warnings).toEqual([]);
  });

  it('terminates the staff with (z) when justified', () => {
    const staff = staffOf(1, makeNeume(PUNCTUM, 0, 72, 'Ky'));
    const result = new GabcExporter().convert(grgWithStaff(staff), 'x');

    expect(result.gabc).toContain('(z)\n\n');
  });

  it('emits a placeholder and a warning for an unmapped neume id', () => {
    const staff = staffOf(0, makeNeume(9999, 0, 72));
    const result = new GabcExporter().convert(grgWithStaff(staff), 'x');

    expect(result.gabc).toContain('_???(A)');
    expect(result.warnings).toEqual([{ id: 9999, positionX: 0, positionY: 72, caption: '' }]);
  });

  it('appends the unmapped marker after existing syllable text', () => {
    const staff = staffOf(0, makeNeume(9999, 0, 72, 'Ky'));
    const result = new GabcExporter().convert(grgWithStaff(staff), 'x');

    expect(result.gabc).toContain('Ky_???(A)');
  });

  it('separates two close neume glyphs within the same syllable', () => {
    // distance = 7 - (0 + sizeX=6) = 1 → falls in the "/" bucket
    const staff = staffOf(0, makeNeume(PUNCTUM, 0, 72, 'Ky'), makeNeume(PUNCTUM, 7, 72));
    const result = new GabcExporter().convert(grgWithStaff(staff), 'x');

    expect(result.gabc).toContain('Ky(a/a)(Z)\n\n');
  });

  it('starts a new syllable group when neumes are far enough apart', () => {
    // distance = 20 - (0 + sizeX=6) = 14 → beyond every bucket → forces a flush
    const staff = staffOf(0, makeNeume(PUNCTUM, 0, 72, 'Ky'), makeNeume(PUNCTUM, 20, 72, 'ri'));
    const result = new GabcExporter().convert(grgWithStaff(staff), 'x');

    expect(result.gabc).toContain('Ky(a)ri(a)(Z)\n\n');
  });

  it('moves a trailing "*" from a caption onto the following divisio', () => {
    const staff = staffOf(
      0,
      makeNeume(PUNCTUM, 0, 72, 'Ky *'),
      makeNeume(HALF_BARRE, 10, 30),
    );
    const result = new GabcExporter().convert(grgWithStaff(staff), 'x');

    expect(result.gabc).toContain('Ky(a) *(;)(Z)\n\n');
  });
});
