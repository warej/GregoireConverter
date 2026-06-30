# Project Plan: Gregorian to GABC Converter

## Purpose

Static web app converting Gregorian chant files from Gregoire binary format (.grg) into GABC text format, allowing musicians and musicologists to port their Gregoire documents into the modern, open GABC format for further editing and typesetting with Gregorio.

Continuation of a master's thesis by Artur Warejko, Poznań University of Technology.

## Repository

`github.com/warej/GregoireConverter`

## License

MIT

## Phase 1 Scope

- Upload a `.grg` file (max 10 MB).
- Validate file signature (`GRG2` magic bytes at offset 0, minimum size 40 bytes); show error if invalid.
- Convert client-side to GABC (TypeScript port of the C# logic).
- Display output GABC text with inline `(?)` placeholder markers where neume IDs have no mapping.
- Show a sidebar warnings panel listing every unmapped neume ID and its position in the score.
- Offer download of the `.gabc` output.
- Link to external GABC viewer (URL TBD).
- After 1-in-5 conversions: show a prompt asking if the output was correct, with a mailto link pre-filling subject and a suggested email template. If no mailto application is configured, show a fallback copy-paste panel with the email template.

## Phase 2 Scope (Future)

- In-browser GABC editor.

## Tech Stack

- Angular + TypeScript (`grg2gabc` project, located in `web/grg2gabc/`).
- GitHub Pages hosting (`/GregoireConverter/` base href).
- GitHub Actions: build Angular static output and deploy to GitHub Pages on merge to main.

## Project Layout

```
GregoireConverter/
├── original_app/        # Original C# Mono solution — reference only, not modified
├── web/
│   └── grg2gabc/        # Angular application
└── Development-plan.md
```

## Angular Application Structure

Use lazy-loaded feature modules from Phase 1 to keep the structure future-proof:

- `ConvertModule` — file upload, conversion, output display, warnings panel, feedback prompt (`/#/convert`)
- `EditorModule` — placeholder route for Phase 2 GABC editor (`/#/editor`)

Routing: `HashLocationStrategy` (required for GitHub Pages static hosting).

## Conversion Logic

Port the C# GRG2 parser and GABC exporter to TypeScript as Angular services:

- Binary parsing uses `DataView` / `Uint8Array` (equivalent to C# `StreamHelper` + `BinaryReader`).
- Segment types: `DOCUMENT` (0xf1ff), `STAFF` (0xf2ff), `INITIAL` (0xf3ff), `NEUME` (0xf4ff).
- Neume-to-GABC mapping from `GRG2NeumeForGabc.cs` — approximately half is a lookup dictionary, half conditional logic. Some neume IDs are currently unmapped (commented out in the C# source); these produce inline `(?)` markers and sidebar warnings in the web UI.
- Reference implementation: `original_app/`.

## UX & Localization

- Simple, clear UI.
- Errors explained in plain language.
- **ngx-translate** for runtime i18n; Polish is the default locale. English also supported. Open to additional locales.
- Locale preference persisted in `localStorage`.

## Validation & Errors

- Check `GRG2` magic bytes and minimum file size (40 bytes); show error if invalid.
- Unmapped neume IDs: insert inline `(?)` placeholder in GABC output and list in sidebar warnings panel. Conversion continues.
- Contact email for error reports configurable via Angular environment file (`environment.ts`); initially `artur.warejko@a4w.pl`.

## Feedback Mechanism

- After 1-in-5 conversions (random), prompt the user to verify the output.
- Prompt contains a mailto link pre-filled with subject and suggested body template.
- If no mailto application is available, a fallback panel displays the template for copy-paste.
- Shown/dismissed state is not persisted — each page load is independent.

## Deployment

- GitHub Actions workflow: build `web/grg2gabc/` with `--base-href /GregoireConverter/` and deploy to GitHub Pages.
- Deploy on merge to `main`.
- One-time repo setup (GitHub Pages source, secrets) done via GitHub UI.

## CI/CD Extras

- ESLint for code quality (Angular default config).
- Jest unit tests in CI pipeline:
  - **Priority**: conversion logic (GRG2 parser + GABC exporter) tested with real `.grg` fixture files (fixtures TBD — add when sample files are available).
  - **Selective**: UI component tests for non-trivial logic only (e.g., feedback prompt probability).
- Dependabot for npm and GitHub Actions dependencies.

## Open Items

- [ ] Identify and link to the external GABC viewer.
- [ ] Obtain sample `.grg` files to use as unit test fixtures.
- [ ] Update `LICENSE` file content to MIT.
