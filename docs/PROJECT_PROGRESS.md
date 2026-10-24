# NLO IQ project progress

Updated: 2026-10-24 (Asia/Baghdad).
Current phase: **10 — Publishing, preview and revision system**.
Status: **Delivered for review; awaiting explicit Phase 10 approval**.
Approved phases: **0 through 9**.
Last approved checkpoint: `38512d3` (Phase 9 localization).
Selected design: Concept A, engineering folio.

## Phase 10 delivered

- Draft/public-state filters and unpublished-change indicators.
- Protected saved-revision previews with private revision-scoped images, no-store/noindex and no public SEO metadata.
- Paged immutable revision history; exact snapshot restoration as a new draft with preserved publication.
- SuperAdmin scheduled publication in Baghdad time, pinned revisions, cancellation and obsolete-job invalidation.
- Persistent hosted worker with actor/role checks, row locking, once-only completion and transactional failure isolation.
- Bilingual history/settings/preview flows and append-only audit records.
- Applied backed-up migration `20261024054947_PublicationScheduleOwnership`.
- Clean build; 41 rollback-only database scenarios, actual worker success/failure, role/CSRF and media/localization/public regressions pass.

Checkpoint: 25 roots, 85 revisions, 170 translations, 6 media assets, 5 published roots, 1 local review account and **0 pending schedules**. Counts grow when stateful review tools run. See PHASE_10_INVENTORY.json.

## Review

Published: http://127.0.0.1:5080/en/pages/publishing-review
Private draft: http://127.0.0.1:5080/en/admin/content/334/preview
History/restore/schedule: http://127.0.0.1:5080/en/admin/content/334/history
Arabic uses /ar/; the published Arabic slug is مراجعة-النشر.
Existing login: ignored artifacts/PHASE_6_LOGIN.txt.
Guide: PHASE_10_PUBLISHING.md. Reviewed screenshots: docs/previews/phase10/.

The public page is version B; a different working draft is saved privately. History includes deliberately cancelled/failed test schedules. None remains pending. Older media/SEO/localization examples are preserved.

## Remaining phases

Phase 11: testing and optimization. Phase 12: deployment and handover. **Do not start Phase 11 without explicit approval.** Normal verification of Phase 10 is part of this delivery.

Final approved company copy/projects, logo/favicon, contact/social links and public HTTPS origin still require supplied values. CMS home/other demo roots remain drafts. Development remains noindex. Preserve ignored App_Data/media with its database. Schedules need the application running; unchanged overdue jobs execute after restart.

APPROVE PHASE advances one phase. REVISE modifies this phase. SHOW PREVIEW reports the running preview. EXPLAIN makes no changes. STATUS reports progress. ROLLBACK requires reviewing discarded changes and database implications first.
