import { Grg2 } from '../grg2/grg2.model';
import { GRG2_SAMPLE_FIXTURE_NAMES, loadGrg2SampleFixture } from '../grg2/testing/grg2-sample-fixtures';
import { GabcExporter } from './gabc-exporter';

// Deliberately corrupted by the original author (magic bytes "GRG1" instead of "GRG2")
// to exercise the invalid-file-format error path; see the dedicated negative test below.
const INTENTIONALLY_INVALID_FIXTURE = 'Document3-AaacB_cleDo_Aaa-1_BrokenByGRG1.GRG';

describe('GRG2 -> GABC conversion of real sample files', () => {
  for (const name of GRG2_SAMPLE_FIXTURE_NAMES) {
    if (name === INTENTIONALLY_INVALID_FIXTURE) continue;

    it(`parses and converts "${name}" without throwing`, async () => {
      const buffer = await loadGrg2SampleFixture(name);

      const grg = Grg2.parse(buffer);
      expect(grg.documents.length).toBeGreaterThan(0);

      const result = new GabcExporter().convert(grg, name);
      expect(result.gabc.length).toBeGreaterThan(0);
    });
  }

  it(`rejects "${INTENTIONALLY_INVALID_FIXTURE}" for its corrupted magic bytes`, async () => {
    const buffer = await loadGrg2SampleFixture(INTENTIONALLY_INVALID_FIXTURE);
    expect(() => Grg2.parse(buffer)).toThrowError(/Invalid file format/);
  });
});