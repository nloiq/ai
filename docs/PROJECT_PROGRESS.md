# NLO IQ project progress

Updated: 2026-10-23 (Asia/Baghdad).

Current phase: **1 — Creative Direction and Design System**.
Status: **Delivered for review; awaiting explicit design approval**.
Approved phases: **Phase 0**, approved by user with APPROVE PHASE.
Last approved code checkpoint: 0f40d6b (Phase 0 and repository setup).

## Completed

- Phase 0 discovery, architecture, schema plan and roadmap.
- Saved supplied database configuration in ignored appsettings.Local.json; no password in Git.
- Initialized main and synchronized with https://github.com/nloiq/ai.git.
- Phase 1: two distinct local hero/layout concepts, palette, theme tokens, type/spacing, controls, image and motion directions, responsive and RTL design rules.
- Added English/Arabic and light/dark/system review controls.
- Compiled pinned Tailwind v4.3.3 styles; verified syntax, HTTP responses, secret URL rejection, anchors and selected contrast pairs.

## Preview and review

- See docs/PHASE_1_DESIGN.md for the full review report and exact run instructions.
- Run node docs/design/serve.mjs, then visit http://127.0.0.1:4173.
- Or open docs/design/index.html directly; compiled CSS is committed.
- Choose Botanical folio (A), Autumn field (B), or specify a combination.
- Do not start Phase 2 without explicit approval.

## Known limitations

- Browser automation runtime failed; screenshots, real browser interactions and responsive rendering remain unverified.
- Current previews are design studies; no ASP.NET application, CMS or database migration exists.
- Motion is specified but not implemented; full parallax is Phase 4.
- EF Core 10/MySQL stable provider compatibility is unresolved.
- MySQL server patch, schema access and authenticated connection remain unverified; credentials have been supplied locally.
- Docker daemon unavailable; native MySQL service and TCP port were reachable in Phase 0.
- Real company assets, approved bilingual content and hosting details remain missing.
- Existing GitHub repository visibility was preserved.

## Remaining phases

2 Foundation; 3 Public prototype; 4 Motion; 5 Content schema; 6 Identity/admin shell; 7 Core CMS; 8 Media/SEO/appearance; 9 Complete localization; 10 Publishing/revisions; 11 QA; 12 Deployment/handover. Each requires a separate approval.

## Commands

APPROVE PHASE advances one phase. REVISE modifies the current phase. SHOW PREVIEW reports the latest actual preview. EXPLAIN makes no changes. STATUS reports progress. ROLLBACK requires confirming exactly what to discard before restoring the last approved checkpoint.

## GitHub sync limitation

Phase 1 is committed locally. Two pushes failed with curl 52 (empty reply from server), including an HTTP/1.1 retry. Verified remote main still points to 0f40d6b. Phase 1 changes have not been synchronized; retry git push origin main after connectivity recovers.

