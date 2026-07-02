import { Grg2Neume } from './grg2-neume.model';

function makeNeume(id: number, positionY = 0): Grg2Neume {
  const n = new Grg2Neume();
  n.id = id;
  n.positionX = 0;
  n.positionY = positionY;
  n.unknownValue4 = 0;
  n.unknownValue5 = 0;
  n.caption = '';
  return n;
}

describe('Grg2Neume', () => {
  it('classifies clef ids', () => {
    expect(makeNeume(6).isClef).toBe(true);
    expect(makeNeume(7).isClef).toBe(true);
    expect(makeNeume(24).isClef).toBe(false);
  });

  it('classifies the bemol id', () => {
    expect(makeNeume(5).isBemol).toBe(true);
    expect(makeNeume(6).isBemol).toBe(false);
  });

  it('classifies the custos id', () => {
    expect(makeNeume(47).isCustos).toBe(true);
    expect(makeNeume(46).isCustos).toBe(false);
  });

  it('classifies divisio ids', () => {
    for (const id of [1, 2, 3, 4, 211, 212, 213, 214, 215]) {
      expect(makeNeume(id).isDivisio).withContext(`id ${id}`).toBe(true);
    }
    expect(makeNeume(24).isDivisio).toBe(false);
  });

  it('classifies rhythmic ids', () => {
    for (const id of [23, 80, 81, 82]) {
      expect(makeNeume(id).isRhythmic).withContext(`id ${id}`).toBe(true);
    }
    expect(makeNeume(24).isRhythmic).toBe(false);
  });

  describe('getClefIndicator', () => {
    it('returns "c" for a do clef and "f" for a fa clef', () => {
      expect(makeNeume(6, 12).getClefIndicator(false)).toBe('c4');
      expect(makeNeume(7, 12).getClefIndicator(false)).toBe('f4');
    });

    it('appends "b" before the height when withBemol is true', () => {
      expect(makeNeume(6, 12).getClefIndicator(true)).toBe('cb4');
    });

    it('maps positionY to a height between 1 and 4', () => {
      expect(makeNeume(6, 24).getClefIndicator(false)).toBe('c3');
      expect(makeNeume(6, 48).getClefIndicator(false)).toBe('c1');
    });

    it('throws for a non-clef id', () => {
      expect(() => makeNeume(24).getClefIndicator(false)).toThrowError(RangeError);
    });

    it('throws when the computed height falls outside 1-4', () => {
      expect(() => makeNeume(6, -100).getClefIndicator(false)).toThrowError(RangeError);
    });
  });

  it('formats toString with id, caption and position', () => {
    const n = makeNeume(24, 5);
    n.caption = 'Ky';
    expect(n.toString()).toBe("[Neume:(24)'Ky'@0,5]");
  });
});
