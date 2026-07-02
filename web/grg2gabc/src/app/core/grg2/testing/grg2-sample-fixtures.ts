/**
 * Real-world .GRG files exported from the original Gregoire application, used as
 * regression fixtures. Binaries live under ./fixtures/grg2-samples and are served
 * to the Karma test runner via the `grg2-fixtures` asset glob in angular.json.
 *
 * Source: original author's sample library (.local/gregoire_samples), copied in
 * with permission for use as test fixtures.
 */
export const GRG2_SAMPLE_FIXTURE_NAMES: readonly string[] = [
  'AllInOne.GRG',
  'AllInOneV2.GRG',
  'BlankDocument.GRG',
  'BlankDocument2.GRG',
  'Colors-RedBlackBlue-1xEmptyPortee_15mm-space.GRG',
  'Colors-RedBlackBlue-1xEmptyPortee_171mm-space.GRG',
  'Colors-RedBlackBlue-1xEmptyPortee_291mm-space.GRG',
  'Colors-RedBlackBlue-1xEmptyPortee_9mm-space.GRG',
  'Colors-RedBlackBlue-1xEmptyPortee_intro2Test.GRG',
  'Document3-AaacB.GRG',
  'Document3-AaacB-copy.GRG',
  'Document3-AaacB_cleDo.GRG',
  'Document3-AaacB_cleDo_Aaa-1.GRG',
  'Document3-AaacB_cleDo_Aaa-1_BrokenByGRG1.GRG',
  'LittleExample.GRG',
  'LittleExampleWithColors.GRG',
  'LittleExampleWithColorsMessed45.GRG',
  'LittleExampleWithColorsMessed45dpi300.GRG',
  'Portee_CleFa_12px.GRG',
  'Portee_CleFa_18_bemol_24.GRG',
  'Portee_CleFa_18_bemol_24_saved.GRG',
  'Portee_CleFa_18px.GRG',
  'Portee_CleFa_18px_bemol.GRG',
  'Portee_CleFa_24px.GRG',
  'Portee_CleFa_30px.GRG',
  'Portee_CleFa_36px.GRG',
  'Portee_CleFa_42px.GRG',
  'Portee_CleFa_48px.GRG',
  'Portee_CleFa_48px_15px.GRG',
  'Portee_CleFa_48px_2px.GRG',
  'Portee_CleFa_48px_2px_50.GRG',
  'Portee_with_ant_modus_initial.GRG',
  'Portee_with_ant_modus_initial(MS Reference Sans Serif).GRG',
  'emptyPortee_x1.GRG',
  'emptyPortee_x1_258mm_wide.GRG',
  'emptyPortee_x2.GRG',
  'grg_document_header_14TimesNewRomanRedPortee.GRG',
  'Colors/EmptyDoc_Black1_Bordo2_DarkGreen3.GRG',
  'Colors/EmptyDoc_Col10RedLines_Col11GreenNeumes_Col12YellowText.GRG',
  'Colors/EmptyDoc_Col13_Col14_Col15.GRG',
  'Colors/EmptyDoc_Col13_Col16_Col15.GRG',
  'Colors/EmptyDoc_Col4_Col5_Col6.GRG',
  'Colors/EmptyDoc_Col7_Col8_Col9.GRG',
];

const FIXTURE_BASE_URL = '/grg2-fixtures/grg2-samples/';

export async function loadGrg2SampleFixture(name: string): Promise<ArrayBuffer> {
  const url = FIXTURE_BASE_URL + name.split('/').map(encodeURIComponent).join('/');
  const response = await fetch(url);
  if (!response.ok) throw new Error(`Failed to load fixture "${name}": HTTP ${response.status}`);
  return response.arrayBuffer();
}
