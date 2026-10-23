# NLO IQ project progress

Updated: 2026-10-23 (Asia/Baghdad).
Current phase: **8 - Media, SEO and Appearance**.
Status: **Delivered for review; awaiting explicit Phase 8 approval**.
Approved phases: **0, 1, 2, 3, 4, 5, 6 and 7**.
Last approved checkpoint: 7235c4d (Phase 7 core CMS).
Selected design: revised Concept A, engineering folio.

## Completed

- Authenticated media library, upload, private previews, search/paging and bilingual alt suggestions.
- PNG/JPEG/WebP validation, 8 MiB/16 MP bounds, static-image decoding, orientation correction and metadata-free re-encoding.
- Optimized WebP master/thumbnail/640/1280 variants and generated 64 × 64 PNG icons.
- Private runtime storage outside wwwroot; public delivery checks published revision references, including pinned sections.
- Immutable media replacement; referenced/history assets cannot be deleted; unused uploads can be removed by SuperAdmin.
- Bilingual SEO editor and manager: search/sharing titles/descriptions, canonical paths, sharing images and noindex.
- Published site defaults, logo/favicon selection and social links; existing visitor theme preferences retain precedence.
- Approved palette-token editor with local light/dark previews, contrast validation and published CSS shared by public/admin pages.
- Append-only audit history with content filtering/paging; uploads, hint edits, deletions, content saves and publishing are recorded.
- Real browser proof of persisted media, SEO, branding and palette changes; default name/colors restored after tests.
- One technical review image retained on the bilingual cms-review page. No new company achievements, real logo or social/contact data invented.
- Current checkpoint: 23 roots, 50 revisions/100 translations, 4 media assets, 3 published roots, 1 local review account. Detailed checkpoint: PHASE_8_INVENTORY.json.
- Eight projects build clean; locked restore and dependency audit pass; no new migration or pending model changes.
- 31 rollback-only database/CMS/settings scenarios, formatted-text checks, media security/optimization checks, role/CSRF checks, Phase 8 browser flow and five public browser regressions passed.

## Review links

Media: http://127.0.0.1:5080/en/admin/media
SEO: http://127.0.0.1:5080/en/admin/seo
Site: http://127.0.0.1:5080/en/admin/settings/site
Appearance: http://127.0.0.1:5080/en/admin/settings/appearance
Audit: http://127.0.0.1:5080/en/admin/audit
Arabic uses /ar/ in the same paths. Use the existing local login in ignored artifacts/PHASE_6_LOGIN.txt.
Published demonstration: /en/pages/cms-review and /ar/pages/cms-review.
Instructions: PHASE_8_MEDIA_SEO_APPEARANCE.md. Screenshots: docs/previews/phase8/.

## Limits and remaining phases

- Uploads support static PNG/JPEG/WebP, not SVG/GIF/ICO or arbitrary files. Bundled SVG illustrations remain trusted demo assets.
- Published revision media/alt metadata remain immutable; replacement uploads use new asset IDs. Old referenced files are preserved with history.
- Uploaded bytes live in ignored src/NLOIQ.Web/App_Data/media and are not distributed by Git or application publish. Deployment must preserve/back up this runtime directory with its database.
- Windows image/runtime behavior is tested; Linux native assets are included but target-host verification remains release QA/deployment work.
- Real logo/favicon, approved company projects/copy, contact/social destinations and public HTTPS origin still need user-provided values.
- Development stays noindex even when a content SEO flag permits indexing.
- Original home prototype remains until its CMS home/dependencies are reviewed/published; this phase verifies a separate CMS review page.
- Full localization, scheduler/preview/revision restoration, release QA and deployment remain future phases.

9 Complete localization; 10 Publishing/preview/revisions; 11 QA; 12 Deployment/handover. Each requires explicit approval. **Do not start Phase 9 yet.**

## Commands

APPROVE PHASE advances one phase. REVISE modifies the current phase. SHOW PREVIEW reports actual preview. EXPLAIN makes no changes. STATUS reports progress. ROLLBACK requires confirming discarded changes and database implications before restoring a checkpoint.
