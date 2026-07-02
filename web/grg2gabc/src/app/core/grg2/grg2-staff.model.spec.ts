import { Grg2Staff } from './grg2-staff.model';
import { Grg2Neume } from './grg2-neume.model';

const NON_RHYTHMIC_ID = 24; // Punctum
const RHYTHMIC_ID = 80; // Ictus

function makeNeume(id: number, positionX: number): Grg2Neume {
  const n = new Grg2Neume();
  n.id = id;
  n.positionX = positionX;
  n.positionY = 0;
  n.unknownValue4 = 0;
  n.unknownValue5 = 0;
  n.caption = '';
  return n;
}

function staffWith(...neumes: Grg2Neume[]): Grg2Staff {
  const staff = new Grg2Staff();
  staff.neumes = neumes;
  return staff;
}

describe('Grg2Staff.assignRhythmics', () => {
  it('attaches a rhythmic neume to the closest following neume within range', () => {
    const a = makeNeume(NON_RHYTHMIC_ID, 0);
    const r = makeNeume(RHYTHMIC_ID, 5);
    const b = makeNeume(NON_RHYTHMIC_ID, 6);
    const staff = staffWith(a, r, b);

    staff.assignRhythmics();

    expect(staff.neumes).toEqual([a, b]);
    expect(b.rhythmics).toEqual([r]);
    expect(a.rhythmics).toEqual([]);
  });

  it('prefers the closer neume when both a preceding and following candidate are in range', () => {
    const pad = makeNeume(NON_RHYTHMIC_ID, -100);
    const a = makeNeume(NON_RHYTHMIC_ID, 0);
    const r = makeNeume(RHYTHMIC_ID, 5);
    const b = makeNeume(NON_RHYTHMIC_ID, 6);
    const staff = staffWith(pad, a, r, b);

    staff.assignRhythmics();

    expect(staff.neumes).toEqual([pad, a, b]);
    expect(b.rhythmics).toEqual([r]);
    expect(a.rhythmics).toEqual([]);
  });

  it('falls back to the preceding neume when the following one is too far away', () => {
    const pad = makeNeume(NON_RHYTHMIC_ID, -100);
    const a = makeNeume(NON_RHYTHMIC_ID, 0);
    const r = makeNeume(RHYTHMIC_ID, 5);
    const b = makeNeume(NON_RHYTHMIC_ID, 10);
    const staff = staffWith(pad, a, r, b);

    staff.assignRhythmics();

    expect(staff.neumes).toEqual([pad, a, b]);
    expect(a.rhythmics).toEqual([r]);
    expect(b.rhythmics).toEqual([]);
  });

  it('leaves a trailing rhythmic neume unassigned when its only preceding neume is at index 0', () => {
    // Known quirk: the backward scan's loop condition (`i > 0`) never inspects
    // index 0, so a rhythmic neume with nothing after it and only a single
    // leading neume before it finds no candidate on either side.
    const a = makeNeume(NON_RHYTHMIC_ID, 0);
    const r = makeNeume(RHYTHMIC_ID, 5);
    const staff = staffWith(a, r);

    staff.assignRhythmics();

    expect(staff.neumes).toEqual([a, r]);
    expect(r.rhythmics).toEqual([]);
  });

  it('resets rhythmics on every call', () => {
    const a = makeNeume(NON_RHYTHMIC_ID, 0);
    a.rhythmics = [makeNeume(RHYTHMIC_ID, 1)];
    const staff = staffWith(a);

    staff.assignRhythmics();

    expect(a.rhythmics).toEqual([]);
  });
});
