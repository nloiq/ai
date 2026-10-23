# NLO IQ project progress

Updated: 2026-10-23 (Asia/Baghdad).
Current phase: **2 — Project Foundation**.
Status: **Delivered for review; awaiting explicit Phase 2 approval**.
Approved phases: **0 and 1**, explicitly approved by the user. Selected design: revised Concept A, engineering folio.
Last approved checkpoint: 4215e1e (revised Phase 1 and recovered GitHub sync).

## Completed

- Phase 0 discovery/architecture and Phase 1 visual system.
- Four confirmed business areas: low-current engineering, software solutions, SCADA integration and system automation.
- .NET 10 MVC solution with four architectural projects and DI.
- Stable MySQL EF Core 10 provider, migration and verified MySQL connection.
- Created dedicated development schema; charset utf8mb4; server 8.0.43.
- Added four demo services/eight translations; repeated seeding inserts zero and does not overwrite content.
- Development-only database initialization and bilingual database-backed foundation page.
- Shared Tailwind v4 theme tokens, light/dark/system controls and Arabic interface resources/RTL.
- Builds, migration consistency, HTTP smoke checks, production seed rejection and secret publish exclusion verified.
- npm audits clear after patched watcher pin; .NET vulnerable-package report clear.

## Preview

Run dotnet run --project src/NLOIQ.Web --no-build.
English http://127.0.0.1:5080/en/; Arabic http://127.0.0.1:5080/ar/.
See PHASE_2_FOUNDATION.md, README.md and LOCAL_SETUP.md for exact instructions and limitations.

## Known limitations

- Browser runtime failed to start; screenshots and responsive/theme/keyboard visual review remain unverified.
- Foundation preview only: no full public site, CMS, authentication or publishing/revisions yet.
- Seed summaries are demo copy; no fictional projects/clients/contact data seeded.
- Final logo, actual project media, approved bilingual copy, contact information and hosting details are missing.
- Docker unavailable; native MySQL used successfully.
- Local credentials are ignored and excluded from publish. Production requires environment/secret configuration.
- Provider license inventory and production deployment configuration remain handover work.

## Remaining phases

3 Public prototype; 4 Motion; 5 Full content schema; 6 Identity/admin shell; 7 Core CMS; 8 Media/SEO/appearance; 9 Complete localization; 10 Publishing/revisions; 11 QA; 12 Deployment/handover. Each requires explicit approval. Do not start Phase 3 yet.

## Review commands

APPROVE PHASE advances one phase. REVISE modifies the current phase. SHOW PREVIEW reports the actual preview. EXPLAIN makes no changes. STATUS reports progress. ROLLBACK requires confirming what will be discarded before restoring the last approved checkpoint.
