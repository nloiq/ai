# NLO IQ project progress

Updated: 2026-10-23 (Asia/Baghdad).
Current phase: **7 - Core CMS Content Management**.
Status: **Delivered for functional review; awaiting explicit Phase 7 approval**.
Approved phases: **0, 1, 2, 3, 4, 5 and 6**.
Last approved checkpoint: a0f88cf (Phase 6 authentication and admin shell).
Selected design: revised Concept A, engineering folio.

## Completed

- Hero/About section editors, Pages/Sections, Projects/categories, Services, Contact, Navigation and Footer modules.
- Bilingual translation tabs, safe formatting toolbar, existing-image selection and per-revision bilingual alt/captions.
- Ordering controls with keyboard-friendly move buttons and drag handles, visibility and project featured/video settings.
- Structured page composition, navigation hierarchy, contact details, business hours and social links.
- Create/read/update and reversible archive; immutable revisions, audit entries, stale-form conflict protection.
- Explicit basic publish/unpublish/archive by SuperAdmin; Editor saves drafts; Viewer retains dashboard-only access.
- Atomic route/pointer/publication updates, dependency validation and demo-media publication safeguards.
- Published-only CMS public renderer, localized page/project routes and language switching.
- Original home prototype retained in Development until the real home page is explicitly published. An already verified home does not fall back to the prototype when unpublished.
- A factual bilingual CMS review page created and updated through the real browser form; original 22 seed roots remain draft/demo.
- Clock conversion migration applied: BusinessHours time columns widened to time(6) with TimeOnly/TimeSpan conversion; backup saved locally before this correction.
- Build clean; no pending model changes; 26 rollback-only database/CMS scenarios plus formatted-text checks, actual CMS browser flow, Identity/role checks and five public browser regressions passed.

## Review

CMS: http://127.0.0.1:5080/en/admin/content (Arabic: /ar/admin/content).
Published demonstration: http://127.0.0.1:5080/en/pages/cms-review (Arabic: /ar/pages/cms-review).
Use the same local login from ignored artifacts/PHASE_6_LOGIN.txt.
Review instructions: PHASE_7_CMS.md.
Inventory checkpoint: PHASE_7_INVENTORY.json.
Restart: dotnet run --project src/NLOIQ.Web --no-build.

## Limits and next phases

- Existing sample illustrations can be selected in drafts; demo media remains blocked from publication. Approved upload/media-library management is Phase 8.
- Public home remains the approved prototype until its actual content/dependencies are reviewed and published. Publishing a section requires resaving/republishing pages that should use its new revision.
- Formatting supports bold, italic, links and lists; raw HTML is encoded rather than accepted as trusted markup.
- Basic publishing is implemented for functional CMS proof. Scheduling, preview links, revision browsing/restoration and complete editorial workflow remain Phase 10.
- SEO/appearance editors, full localization QA, media optimization and deployment remain their planned phases.
- Production TLS/proxy/shared Data Protection, account recovery/MFA and release QA remain outstanding.

8 Media/SEO/appearance; 9 Complete localization; 10 Publishing/preview/revisions; 11 QA; 12 Deployment/handover. Each requires explicit approval. **Do not start Phase 8 yet.**

## Commands

APPROVE PHASE advances one phase. REVISE modifies the current phase. SHOW PREVIEW reports actual preview. EXPLAIN makes no changes. STATUS reports progress. ROLLBACK requires confirming discarded changes and database implications before restoring a checkpoint.
