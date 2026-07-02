# CLAUDE.md — grg2gabc project context

## What this project is

Angular 19 static web app converting Gregoire binary chant files
(.grg / GRG2 format) to GABC text notation. Hosted on GitHub Pages.
Continuation of a master's thesis by Artur Warejko, Poznań University
of Technology. Repo: github.com/warej/GregoireConverter.

## Hard constraints — read before doing anything

- Respect local environment instructions from `./LOCAL.md`
- **Phase 2 (GABC editor) is not started** — `EditorModule` is a placeholder.
  Do not implement anything in it until explicitly asked.

## Layout

```
original_app/                   C# Mono reference — read only, do not modify
web/grg2gabc/                   Angular application
  src/app/core/grg2/            GRG2 binary parser (TypeScript port)
  src/app/core/gabc/            GABC exporter + neume map
  src/app/features/convert/     Phase 1 UI
  src/environments/             contactEmail + gabcViewerUrl config
  src/assets/i18n/              pl.json (default), en.json
.github/workflows/deploy.yml    build + GitHub Pages deploy
Makefile                        install / serve / build / lint / test / clean
Development-plan.md             full design decisions record
```

## Key decisions

| Topic | Decision |
|---|---|
| Routing | `HashLocationStrategy` (GitHub Pages) |
| i18n | ngx-translate, Polish default, `localStorage` for preference |
| File limit | 10 MB; magic bytes `GRG2` at offset 0, min 40 bytes |
| Unmapped neumes | inline `(?)` placeholder + sidebar warnings panel |
| Feedback prompt | 1-in-5 chance; mailto pre-fill; copy-paste fallback if no mail client |
| Config | `environment.ts` — `contactEmail` + `gabcViewerUrl` |
| Base href | `/GregoireConverter/` |
| License | MIT |

## How to run locally

```
make install && make serve
```

## Open items

- [ ] Sample `.grg` fixture files for unit tests (ask original author for permission)
- [ ] Enable GitHub Pages in repo settings (Actions source) — one-time manual step via GitHub UI
