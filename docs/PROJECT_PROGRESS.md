# NLO IQ project progress

Updated: 2026-10-23 (Asia/Baghdad).
Current phase: **3 — Public Website Prototype**.
Status: **Delivered for visual review; awaiting explicit Phase 3 approval**.
Approved phases: **0, 1 and 2**. Phase 2 approval received before this work.
Selected design: revised Concept A, engineering folio.
Last approved checkpoint: 30a0a15 (Phase 2 foundation and seed preview).

## Completed

- .NET 10 layered MVC foundation, stable EF/MySQL provider and verified MySQL database.
- Four confirmed specialty categories with demo summaries and eight translations; database content preserved.
- Shared theme tokens and localized English/Arabic routes.
- Full public prototype: responsive navigation, hero, About, illustrated portfolio, database-backed services, Contact and footer.
- Three original local engineering illustrations, clearly labeled as non-client concept studies.
- Responsive menu and focus handling; sticky navigation; semantic headings and local anchor targets.
- Reusable Razor section partials and 68 matching resource keys.
- Build, JavaScript syntax and foundation/prototype HTTP checks passed.

## Preview

English http://127.0.0.1:5080/en/; Arabic http://127.0.0.1:5080/ar/.
Restart: dotnet run --project src/NLOIQ.Web --no-build.
Verification: node tests/prototype-smoke.mjs with the app running.
See PHASE_3_PROTOTYPE.md for review instructions, implementation files and limitations.

## Known limitations

- Browser runtime unavailable; screenshots, responsive visual inspection and actual menu/theme/keyboard interactions remain unverified.
- About/service copy remains proposed/demo. Portfolio is illustration-only; no actual project claims.
- Logo, real project media/video IDs, approved bilingual company text and verified contact details are missing.
- Contact placeholders are explicit; no inquiry form or invented email/phone links.
- Only services are database-backed now; full CMS content and publishing are future phases.
- Parallax and video lightbox are Phase 4; no fake playback or motion claims.
- Preview is Development-only. Production configuration/deployment and provider license inventory remain later work.

## Remaining phases

4 Motion/interactions; 5 Full content schema; 6 Identity/admin shell; 7 Core CMS; 8 Media/SEO/appearance; 9 Complete localization; 10 Publishing/revisions; 11 QA; 12 Deployment/handover. Each requires explicit approval. Do not start Phase 4 yet.

## Review commands

APPROVE PHASE advances one phase. REVISE modifies the current phase. SHOW PREVIEW reports the actual preview. EXPLAIN makes no changes. STATUS reports progress. ROLLBACK requires confirming discarded changes before restoring an approved checkpoint.
