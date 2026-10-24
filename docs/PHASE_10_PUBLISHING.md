# Phase 10 — Publishing, preview and revision system

Status: delivered for approval. Phase 9 checkpoint `38512d3` is approved. Phase 11 has not started.

## Review in the running app

Use the existing local login in ignored `artifacts/PHASE_6_LOGIN.txt`.

1. Open http://127.0.0.1:5080/en/pages/publishing-review (Arabic: `/ar/pages/مراجعة-النشر`). This is the published version B.
2. Open http://127.0.0.1:5080/en/admin/content/334/preview. The saved working draft has a different title/body and a private-preview banner. Switch to Arabic in the header. Opening this preview in a signed-out browser asks for login; it cannot be read anonymously.
3. Open http://127.0.0.1:5080/en/admin/content/334/history (Arabic uses `/ar/`). Compare current draft/published markers, timestamps, authors and notes. Open a saved revision preview. Confirm and restore an older revision: a new draft is created; the public version stays intact until explicitly published.
4. As SuperAdmin, schedule the saved draft at least one minute ahead using Baghdad time (UTC+03:00), with the review checkbox. Check the pending schedule and cancellation control. Saving another draft, restoring, or manually publishing/unpublishing/archiving cancels pending schedules. A new schedule replaces the previous pending schedule.
5. Wait for the scheduled time with the app running. The worker checks every 15 seconds and publishes only the pinned reviewed revision. Review the completed status and audit trail. Failed jobs preserve the previous public version and give a review message.
6. Use content-workspace publication-state and unpublished-change filters. Editor can edit, preview and restore ordinary content; SuperAdmin alone publishes, schedules, cancels publication schedules and accesses site/theme settings. Viewer cannot open the editing workflows.

The retained page is a clearly described local publishing demonstration, not a company project claim. Its history includes deliberate test failures from a conflicting route, successful worker publications and cancelled schedules. **No review schedule remains pending.** Other approved examples and the original home are preserved.

## Draft and revision guarantees

Saved revisions remain append-only. Restoration creates a fresh revision, keeps the old published pointer, and records the source revision and actor. It copies the entire mapped snapshot graph: both translations, typed payloads, exact pinned section/category versions, navigation hierarchy/targets, contact hours, social links, media ordering, bilingual alt/caption snapshots, covers, SEO/sharing references, branding and palette values. Internal child IDs are remapped; external revision pins stay exact. Restoring an archived item reopens it as a draft.

A restored historical page can reference an older section/category revision. Publication still applies the existing dependency checks. To adopt newer published dependencies, review and save the composition before publishing. Restoration does not silently switch dependencies to their latest versions.

History pages show 30 revisions per page and the latest 20 publication jobs. Draft preview links always show a saved revision; save unsaved form edits first. Pages, projects and sections reuse public rendering. Navigation/contact/service previews include their structured content; site/theme previews show readonly saved settings and palette examples. Linked sections use pinned snapshots; the rest of the site uses current published content.

Preview endpoints require ContentEdit, with additional settings restrictions. Private local images use authenticated revision-scoped URLs and no-store responses. Public media eligibility remains unchanged. Preview pages omit canonical/hreflang/sharing metadata and are always noindex, nofollow, noarchive. Neither preview flags nor revision IDs can enable draft rendering on public routes.

## Scheduled publication

Schedules store a specific revision, requester and UTC due time. Creating/cancelling a schedule uses optimistic versions and a content-row lock. Save/restore/manual publication cancels obsolete pending jobs. The worker locks the same content row, checks the original publisher is still active, unlocked and SuperAdmin, and rechecks publication rules before committing routes, public pointer, schedule result and audit records together. Completed jobs do not run twice. Route conflicts or other validation failures roll back before a separate failure record is saved.

The hosted worker runs with the application. It processes up to 20 due jobs per sweep; missed unchanged jobs are picked up after restart. It does not send email or call an external publishing service. A failed job needs review and rescheduling; it is not retried indefinitely as a new publication.

One migration, `20261024054947_PublicationScheduleOwnership`, adds requester/time metadata and allows repeat scheduling history for a revision. The composite foreign-key index stays available while uniqueness is changed. Legacy jobs without a requester fail closed. A downgrade must reconcile repeated jobs before restoring the old unique constraint; it never silently deletes schedule history. An ignored development backup was taken in `artifacts/phase10-before.sql` before applying the migration.

## Verification performed

- Eight projects build without warnings/errors; model matches applied migrations.
- 41 rollback-only database scenarios plus formatted-text safety checks pass. New coverage includes private draft/cross-owner previews, exact restore pins, project media/SEO remapping, navigation parent/target remapping, contact hours, site logo/favicon references, all module payloads, stale/foreign restores, schedule time/confirmation checks, cancellation/rescheduling, authorization revocation and once-only execution.
- Real Edge review proves private bilingual previews/images, anonymous denial, no-store/noindex, absent preview SEO, draft/public isolation, restoration, actual hosted-worker publication and actual rollback after a conflicting route. Both public routes remain intact after failure.
- English/Arabic history fits 320/390/768/1440px; saved settings previews are readonly; screenshots were visually reviewed; no browser script errors occurred.
- SuperAdmin/Editor/Viewer and CSRF tests cover history, preview, restore and schedule, including settings restrictions. Temporary test users are removed.
- Media/SEO/appearance and localization review flows, five public browser regressions, prototype smoke, content rules and interaction checks pass. English/Arabic resources remain paired (409 keys).

Stateful browser tools must run sequentially on the dedicated development schema, never on production or repurposed review data:

```powershell
dotnet build NLOIQ.sln --no-restore
dotnet run --project tests/NLOIQ.DatabaseChecks --no-build
$env:NLO_ALLOW_SCHEDULE_CLOCK_TEST='1'
node tests/phase10-review.cjs
Remove-Item Env:NLO_ALLOW_SCHEDULE_CLOCK_TEST
node tests/phase8-review.cjs
node tests/phase9-review.cjs
dotnet run --project tests/NLOIQ.AuthChecks --no-build
npm.cmd run test:browser
node tests/prototype-smoke.mjs
```

The Phase 10 script retains/updates its review page and one engineering diagram. For bounded actual-worker tests, it adjusts only its own pending job's due time through the local MySQL client, after checking the explicit environment flag and exact local schema/server. It exercises a deliberate route conflict and restores clean final public/draft copy. It leaves no pending test job. Authentication checks exhaust the login limiter; restart before another sign-in review or wait a minute.

Checkpoint: `PHASE_10_INVENTORY.json`. Reviewed images: `docs/previews/phase10/`. Runtime uploads, local configuration and backups remain ignored and must be preserved with the database. Final company data and production hosting remain separate work.

**Stop for Phase 10 approval. Phase 11 testing/optimization has not started.**
