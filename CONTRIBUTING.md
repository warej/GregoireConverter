# Contributing

## Prerequisites

Node.js 24+ (LTS), `make`

## Workflow

1. Fork and clone the repository.
2. Run `make install` inside the repo root (`--ignore-scripts` is applied automatically).
3. Make changes under `web/grg2gabc/src/`.
4. Run `make lint` and `make test` — both must pass.
5. Open a pull request against `main`.

## Project structure

- `original_app/` — original C# reference implementation (read-only)
- `web/grg2gabc/` — Angular application
  - `src/app/core/grg2/` — GRG2 binary parser
  - `src/app/core/gabc/` — GABC exporter and neume mapping
  - `src/app/features/convert/` — converter UI
- `.github/workflows/` — CI/CD (build + GitHub Pages deploy)

## Adding neume mappings

Unknown neumes appear as `(?)` in the output and are listed in the
warnings panel. To add a mapping, edit
`web/grg2gabc/src/app/core/gabc/grg2-neume-for-gabc.ts` — find the
commented-out entry for the neume ID and add a `NeumeFormat` entry
following the existing pattern.

## Commit style

Conventional Commits (`feat:`, `fix:`, `chore:`, `docs:`).
