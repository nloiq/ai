# NLO IQ project progress

Updated: 2026-10-23 (Asia/Baghdad).
Current phase: **5 — Database and Content Architecture**.
Status: **Delivered for database review; awaiting explicit Phase 5 approval**.
Approved phases: **0, 1, 2, 3 and 4**.
Selected design: revised Concept A, engineering folio.
Last approved checkpoint: 11d7fda (Phase 4 plus video-action clarification).

## Completed

- Approved bilingual public prototype, shared themes, restrained motion and accessible video dialog preserved.
- Full normalized content/revision, translation, media, navigation/contact, SEO/theme/settings and audit/schedule schema.
- Identity user/role persistence added; three roles seeded, zero users/passwords.
- 40 total database tables; new content fixtures are 22 draft identities/revisions and 44 translations.
- Three actual bundled media metadata records with immutable revision alt/caption usages.
- Same-owner pointers, restricted deletion, uniqueness/culture/order constraints and root concurrency tokens.
- Append-only saved revisions/children/audit and published-only read projections.
- Publication prerequisites prevent missing translations, draft section/category dependencies, demo content/media and missing bilingual media alt text.
- Additive migrations applied; row-size and explicit identity generation verified on MySQL; no pending model changes.
- Seed rerun inserts zero; existing preview services/translations retained.
- 16 rollback-only MySQL scenarios, server validations, bilingual HTTP checks and five Edge regressions passed.
- Read-only development inspection command and actual seed inventory added.

## Preview and review

English http://127.0.0.1:5080/en/; Arabic http://127.0.0.1:5080/ar/.
Restart: dotnet run --project src/NLOIQ.Web --no-build.
Inspect backend fixtures: dotnet run --project src/NLOIQ.Web -- --inspect-content.
Review PHASE_5_DATABASE.md, DATABASE_ARCHITECTURE.md and PHASE_5_SEED_INVENTORY.json.

## Limitations

- Current preview keeps its existing preview data reader; CMS/public content wiring is Phase 7.
- Identity schema exists; login, policies and admin UI are Phase 6.
- Revision/schedule/audit models and guards exist; full editing/publishing/preview/restoration and scheduler services remain later phases.
- Upload/storage management, rich-text sanitization and final SEO/appearance interfaces remain future CMS work.
- All new fixtures are draft/demo; real logo, projects, approved bilingual copy and contacts remain missing.
- Live external YouTube playback still needs user review; dialog/iframe lifecycle is browser-tested.
- Multi-browser/screen-reader/production QA and deployment remain later phases.

## Remaining phases

6 Authentication/admin shell; 7 Core CMS; 8 Media/SEO/appearance; 9 Complete localization; 10 Publishing/preview/revisions; 11 QA; 12 Deployment/handover. Each requires explicit approval. Do not start Phase 6 yet.

## Commands

APPROVE PHASE advances one phase. REVISE modifies the current phase. SHOW PREVIEW reports actual preview. EXPLAIN makes no changes. STATUS reports progress. ROLLBACK requires confirming discarded changes and database implications before restoring a checkpoint.
