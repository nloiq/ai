# NLO IQ project progress

Updated: 2026-10-24 (Asia/Baghdad).
Current phase: **9 — Complete localization**.
Status: **Delivered for review; awaiting explicit Phase 9 approval**.
Approved phases: **0, 1, 2, 3, 4, 5, 6, 7 and 8**.
Last approved checkpoint: `74e3a24` (Phase 8 media, SEO and appearance).
Selected design: Concept A, engineering folio.

## Phase 9 delivered

- Arabic/English public and admin content, independent language URLs and persisted language choice.
- Paged translation overview with missing-field checks and direct links to each editor language.
- Localized listing titles, dropdowns, weekday names, audit actions and native/server validation.
- RTL/LTR layouts, Arabic typography, correctly directed text fields and Gregorian Arabic dates.
- Reciprocal hreflang, x-default, Open Graph locales, translated SEO and Content-Language.
- A published bilingual language demonstration with an independent Arabic Unicode slug.
- 367 paired interface resources; reviewed screenshots and passing build/database/browser/authentication checks.
- No new schema, dependency, company claim or real contact/social destination.

Checkpoint: 24 roots, 57 revisions, 114 translations, 5 media assets, 4 published roots and 1 local review account. See PHASE_9_INVENTORY.json. Stateful checks increase revision/audit counts.

## Review links

English: http://127.0.0.1:5080/en/pages/localization-review
Arabic: http://127.0.0.1:5080/ar/pages/مراجعة-اللغات
Translations: http://127.0.0.1:5080/en/admin/translations or /ar/admin/translations
Existing media/SEO example: /en/pages/cms-review and /ar/pages/cms-review
Credentials: ignored artifacts/PHASE_6_LOGIN.txt
Instructions: PHASE_9_LOCALIZATION.md. Screenshots: docs/previews/phase9/.

Choose a language, then open `/` in the same browser to check persistence. Explicit localized URLs take precedence. Editor and SuperAdmin can manage translations; publishing remains SuperAdmin-only.

## Remaining work and approval boundary

Final company copy, logo/favicon, approved project facts, contact/social links and public HTTPS origin still require supplied values. Other demo content and the CMS home remain drafts until reviewed. Development remains noindex. Preserve ignored App_Data/media together with its database; Git does not recreate runtime uploads/content.

Phase 10 completes publishing/preview/revisions. Phases 11–12 cover release QA and deployment/handover. **Do not start Phase 10 without explicit approval.**

APPROVE PHASE advances one phase. REVISE modifies this phase. SHOW PREVIEW reports the running preview. EXPLAIN makes no changes. STATUS reports progress. ROLLBACK requires reviewing discarded changes and database implications first.
