# NLO IQ project progress

Updated: 2026-10-23 (Asia/Baghdad).
Current phase: **4 — Parallax and Interactions**.
Status: **Delivered for motion/interaction review; awaiting explicit Phase 4 approval**.
Approved phases: **0, 1, 2 and 3**.
Selected design: revised Concept A, engineering folio.
Last approved checkpoint: 2a2c175 (Phase 3 public prototype).

## Completed

- Layered .NET 10 MVC foundation, verified MySQL database and four bilingual seeded service specialties.
- Complete public prototype with English/Arabic routes and shared light/dark/system themes.
- Bounded hero/About/project/service/footer parallax and progressive scroll reveals.
- Reduced-motion safeguards and simplified mobile behavior.
- Refined hover/menu/header/service transitions.
- Accessible native project dialog, focus wrapping/restoration, scroll locking, navigation and Escape/backdrop close.
- Lazy safe YouTube embedding for user-supplied video NNiszX0-A7M in the first study; honest illustration fallback in the others.
- Build, HTTP, helper/server validation and five real Edge browser tests passed.
- Browser-verified layouts at 320/390/768/1440 in English and Arabic; screenshots saved in docs/previews/phase4/.
- Scheduler measurement: zero frame requests during idle; one for 50 simultaneous scroll events.

## Preview and checks

English http://127.0.0.1:5080/en/; Arabic http://127.0.0.1:5080/ar/.
Restart: dotnet run --project src/NLOIQ.Web --no-build.
Review report: PHASE_4_INTERACTIONS.md.
Checks: npm.cmd run test:interactions; dotnet run --project tests/NLOIQ.Checks; node tests/prototype-smoke.mjs; npm.cmd run test:browser.

## Known limitations

- Live YouTube document/title loaded, but actual playback was not confirmed in the headless check. User playback review is pending.
- Deterministic iframe lifecycle test uses a controlled response; it does not prove external streaming.
- Browser testing currently covers installed Edge; full multi-browser/screen-reader/performance QA remains Phase 11.
- Portfolio artwork and service/about copy remain clearly labeled sample/proposed content.
- Final logo, real project media, approved company text and verified contacts remain missing.
- Preview-only video configuration will move into the future CMS; no content schema or seed changes in Phase 4.
- CMS, Identity/admin shell, publishing/revisions and production deployment are future phases.

## Remaining phases

5 Full database/content schema; 6 Identity/admin shell; 7 Core CMS; 8 Media/SEO/appearance; 9 Complete localization; 10 Publishing/revisions; 11 QA; 12 Deployment/handover. Each requires explicit approval. Do not start Phase 5 yet.

## Review commands

APPROVE PHASE advances one phase. REVISE modifies the current phase. SHOW PREVIEW reports the actual preview. EXPLAIN makes no changes. STATUS reports progress. ROLLBACK requires confirming discarded changes before restoring an approved checkpoint.
