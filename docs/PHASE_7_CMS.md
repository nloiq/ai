# Phase 7: Core CMS functional review

Phase 6 was explicitly approved on 2026-10-23. Phase 7 is delivered for review; Phase 8 has not started.

## Review the working CMS

1. Open http://127.0.0.1:5080/en/admin/content or http://127.0.0.1:5080/ar/admin/content.
2. Sign in using the same local review account in ignored `artifacts/PHASE_6_LOGIN.txt`.
3. Open the `cms-review` page. Its published versions are http://127.0.0.1:5080/en/pages/cms-review and http://127.0.0.1:5080/ar/pages/cms-review.
4. Change a title or summary in a translation tab and select **Save draft**. The public page keeps the previous published text.
5. Check the review confirmation and select **Publish saved draft**. Refresh the public page; it now shows the saved change.
6. Unpublish or archive to remove the page from public readers. Saving an archived item restores a draft; publish explicitly to make it public again.

The browser verification performed this complete flow, including a stale form rejected with HTTP 409. The review page contains factual CMS demonstration text and no fictional client achievements, projects or contact details. The original 22 sample roots remain drafts. The review page and its immutable edit/audit history are intentionally retained for functional review.

## Implemented modules

| Module | Controls |
| --- | --- |
| Hero / About | Section type, bilingual copy/buttons, safe targets, images, layout/background, visibility and bounded motion intensity |
| Pages / Sections | Bilingual text and page slugs, ordered section selection and immutable composition snapshots |
| Projects / Categories | Translations/slugs, category selection, featured/order, existing cover/gallery images with alt/captions and safe YouTube ID |
| Services | Copy, approved technical icon, order, visibility and optional cover |
| Contact | Copy, typed email/phone/WhatsApp/address/map data, localized labels/values, hours in Asia/Baghdad and social links |
| Navigation | Main/footer locations, bilingual links, visibility, order and parent row references |
| Footer | Bilingual copy, copyright visibility and social links |

New stable keys use lowercase letters/numbers/hyphens. Existing keys and types are immutable. Navigation locations use `main`/`footer`; Contact uses `primary`; Footer uses `main`. A unique-key conflict produces a validation response; edit the existing singleton rather than trying to create a second copy with its key.

Body editing supports bold, italic, HTTPS/local links and unordered lists through a small formatting toolbar. All other input, including raw HTML, is encoded as text. Formatting uses a bounded non-backtracking parser. This is deliberately restricted formatting, rather than arbitrary HTML or script injection.

Translation tabs have keyboard navigation and work without JavaScript by displaying both panels. Collection rows have move up/down/remove controls and drag handles. Navigation parent references use zero-based row indexes and must precede children; moving a child above its parent is rejected rather than silently changing the hierarchy. Image role `cover` selects the project/service cover; other role names identify gallery images. Role names must be unique within a revision.

## Publication rules

Editors and SuperAdmin can manage drafts. Only SuperAdmin can publish, unpublish or archive. Viewer has dashboard access only. CMS writes require anti-forgery tokens and every route enforces its policy on the server.

Each save creates a complete new revision and updates the draft pointer atomically. Existing snapshots are never edited. Existing SEO metadata is preserved until its dedicated editor phase; removed media references are cleared safely. Audit records identify actor, action and revision. A submitted version must match the current root version; stale forms cannot overwrite another editor's work.

Publication requires explicit confirmation, both language titles, page/project slugs, valid media alt text and published dependencies. Visible navigation/contact rows need both labels. Route conflicts, failed validation and concurrency errors roll back the whole operation.

Demo assets are still blocked from publication. The three bundled illustration assets are available for draft selection; uploading/verifying real assets belongs to Phase 8. Do not mark fictional client work as approved simply to publish it.

Page compositions pin section revisions. After publishing an updated section, save and publish the page to adopt the new section revision. Projects similarly pin their selected category revisions. Archiving a referenced section/category removes it from public readers without deleting history.

## Public integration

The home reader uses published CMS key `home`. Until it is explicitly reviewed/published, the existing Development-only prototype stays available at `/en/` and `/ar/`. After a verified home is unpublished, the route returns 404 and does not resurrect the prototype.

Published page/project routes are `/{culture}/pages/{slug}` and `/{culture}/projects/{slug}`. Draft/archived/demo roots and stale routes are excluded. Published section types select the Projects, Services and Contact components; those components read only published module content. The header/footer use published main/footer navigation, primary contact and main footer. Links may use `{culture}` in local paths for bilingual destinations.

The public design reuses semantic Tailwind tokens, engineering linework, responsive folio composition, themes, motion and the accessible project dialog. Language switching on CMS pages uses the matching published localized slug. Mobile navigation handles both anchors and full links safely. CMS-owned content is not cached, so publication is visible on the next request.

## Database correction

Migration `20261023134044_BusinessHoursClockConversion` converts the provider-facing clock representation to TimeSpan and widens nullable `OpensAt`/`ClosesAt` from `time` to `time(6)`. This fixes the actual MySQL TimeSpan-to-TimeOnly read failure caught by integration tests. It preserves rows, adds no tables and is applied locally. Backup: ignored `artifacts/before-phase7-clock.sql`. Four migrations are now applied and the model has no pending changes.

## Verification

```powershell
dotnet restore NLOIQ.sln --locked-mode
dotnet build NLOIQ.sln --no-restore
dotnet run --project tests/NLOIQ.DatabaseChecks --no-build
dotnet run --project tests/NLOIQ.AuthChecks --no-build
node tests/cms-review.cjs
npm.cmd run test:browser
node tests/prototype-smoke.mjs
dotnet run --project tests/NLOIQ.Checks --no-build
npm.cmd run test:interactions
```

Run these sequentially against the dedicated development database. Database checks run 26 rollback-only scenarios plus formatted-text safety assertions, including CMS CRUD, published/draft isolation, pinned composition, gallery/SEO preservation, stale versions, structured fields, visibility, selected approved media and audit records. Authentication checks create/remove temporary accounts and verify the CMS policy/CSRF matrix; they exhaust the login limiter, so restart the local app or wait one minute before further sign-in testing.

`tests/cms-review.cjs` uses the local review login and intentionally updates the retained `cms-review` page. It checks actual HTML form saves, bilingual publication, isolation, stale-form conflicts, unpublish/archive/restore, encoded injection attempts, eight module forms, row ordering/removal, theme and RTL layouts at 320/390/768/1440px. It removes the displayed injection-test text from the final published review page. Do not run it against production or against a `cms-review` page you have repurposed.

Review screenshots are in `docs/previews/phase7/`. The current checkpoint inventory is `PHASE_7_INVENTORY.json`; revision/audit counts naturally increase when the functional review is repeated. Earlier Phase 5/6 reports remain historical checkpoints.

Pending Phase 8: upload/media library/optimization, SEO manager, site settings and appearance editor. Pending Phase 10: scheduler, draft preview links, revision browsing/restoration and full editorial workflow. No production-readiness claim is made.
