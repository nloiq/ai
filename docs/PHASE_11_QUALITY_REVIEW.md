# Phase 11 — Testing and optimization

Status: complete for final quality review. Phase 10 checkpoint `baa369a` is approved. Phase 12 has not started. The application is running locally in Release configuration.

## Findings fixed

| Finding | Change and evidence |
|---|---|
| Public CMS projection probed unrelated snapshot tables for every node | Query only the matching content kind and reuse immutable asset metadata within one read. A six-section/four-service rollback fixture went from **211 to 80 SQL reader commands (62% fewer)**. A 95-command regression budget covers that fixture. Draft/public, media and revision isolation checks pass. |
| Light-theme input border was difficult to distinguish | Measured **1.73:1** contrast before the fix. Use a contrasting existing palette token, with safe fallbacks for customized moss. Tested controls now measure **3.42–3.99:1** across the checked themes/pages. Both fallback CSS and published branding CSS are covered. |
| Skip targets and dynamic editor rows needed predictable keyboard focus | Main regions are explicitly focusable by skip links. Adding a row focuses its first control; moving keeps focus on the action; removing focuses a remaining row or Add. Real browser assertions verify the behavior. |
| Empty public CMS navigation still exposed a mobile menu button | Hide the empty menu and navigation. Existing populated navigation behavior still passes. |
| Empty error responses could look like failed browser navigation | Accessible English/Arabic status pages preserve actual HTTP error codes, use no-store/noindex and never echo the requested URL or exception details. API/image requests retain their existing non-HTML behavior. |
| Original public prototype still displayed a Phase 4 development label | Replace the stale internal phase label with a clear bilingual design-preview label. Demo content remains explicitly identified. |

The query measurement covers the reader, not every SQL command in an entire HTTP request. Observed timings on this local machine were about 101 ms before and 42–45 ms after; those timings are illustrative, not a load-capacity or production latency guarantee. Asset metadata reuse is cleared for each public/preview read; published content is not cached across requests.

## Verification matrix

| Area | Evidence/result |
|---|---|
| Builds and packages | Locked .NET restore; all eight projects build in Debug and Release with zero warnings/errors. Release publish succeeds. npm and configured NuGet audits report no known vulnerabilities. New axe dependency is development-only and pinned. |
| Unit/content rules | Video ID, safe-link, slug/culture, theme contrast, formatted-text and four interaction unit checks pass. |
| Database/integration | **43 rollback-only scenarios** pass, including CMS writes, immutable history, stale/foreign restores, media/SEO references, exact dependency pins, scheduling/cancellation/revocation, SQL budget and custom-palette border fallback. Test transactions preserve existing content/accounts. |
| Authentication/authorization | Actual Identity login/lockout, disabled-account session revocation, rate limiting, safe redirects, logout and CSRF checks pass. SuperAdmin/Editor/Viewer checks cover editing, settings, preview/history, restore and schedules. Temporary accounts are removed. |
| CMS workflows | Real create/save/update, bilingual editing, stale browser conflict, publish/unpublish/archive/restore, composition ordering and encoded content checks pass. |
| Media | Static PNG/JPEG/WebP processing, EXIF/transparency, bounded sizes, responsive variants/icons, MIME spoofing/active-content rejection, path containment, private storage, stale hints, unused deletion and audit checks pass. |
| Publishing | Actual hosted worker publishes a pinned revision; cancellation and draft invalidation work. A deliberate route conflict rolls back without disturbing either public route. No test job remains pending. |
| Localization/RTL/themes/responsiveness | Existing language persistence, independent Arabic slugs, both themes, all ten module previews, RTL layouts and widths 320/390/768/1440 are verified. Resource keys/placeholders match: **413 paired keys**. |
| Accessibility | axe-core Playwright **4.14.0**, WCAG 2 A/AA, 2.1 AA and 2.2 AA tags: **51 scans, zero reported automated violations**. Includes public pages, login, open image lightbox, CMS modules, media, SEO, settings, audit, history, private preview and localized 404. Manual control-border measurements, skip-link and dynamic-row keyboard checks pass. |
| Motion/lightbox | Five existing real-browser tests pass: bounded parallax, reduced-motion reset, theme persistence, lazy constrained iframe, image/video navigation, focus trap/return, Escape/backdrop cleanup and responsive layouts. Scheduler recorded **zero idle animation requests and one request for 50 same-turn scroll events**. |
| Security/SEO | Private configuration/source/upload/Git paths return 404. CSP, nosniff, referrer policy and development noindex pass. Bilingual metadata, canonical/hreflang and private-preview exclusions pass. No unsafe-inline exception was added. |
| Release-mode smoke | Published artifact starts on isolated **loopback HTTPS** in Production mode with a one-day untrusted test certificate. Secure/HttpOnly/Strict antiforgery cookies, published-page access, anonymous preview denial, localized 404, generic error details, private-config/upload exclusion and disabled development prototype pass. The probe shuts down afterward. |

Accessibility runner reference: [official axe Playwright documentation](https://github.com/dequelabs/axe-core-npm/tree/develop/packages/playwright).

Machine-readable evidence: `PHASE_11_QUALITY_RESULTS.json`, `PHASE_11_RELEASE_SMOKE.json`, `PHASE_11_INVENTORY.json`. Reviewed screenshots: `docs/previews/phase11/`.

## Review links

- English preview: http://127.0.0.1:5080/en/
- Arabic preview: http://127.0.0.1:5080/ar/
- Login / stronger control borders: http://127.0.0.1:5080/en/account/login
- Arabic status page: http://127.0.0.1:5080/ar/missing-quality-page (correctly returns HTTP 404)
- Published workflow example: http://127.0.0.1:5080/en/pages/publishing-review
- Private draft/history: `/en/admin/content/334/preview` and `/en/admin/content/334/history`

Use the existing ignored `artifacts/PHASE_6_LOGIN.txt` for local administration. No credentials, uploaded bytes, backups, temporary certificates or publish artifacts are committed.

## Repeating checks

Start the local preview and use the dedicated development database. Stateful scripts intentionally update retained examples; do not run them on production or repurposed review content. Run stateful checks sequentially.

```powershell
npm.cmd ci
npm.cmd run css:build
dotnet restore NLOIQ.sln --locked-mode
dotnet build NLOIQ.sln --no-restore -c Release
dotnet run --project tests/NLOIQ.DatabaseChecks --no-build -c Release
dotnet run --project tests/NLOIQ.MediaChecks --no-build -c Release
dotnet run --project tests/NLOIQ.Checks --no-build -c Release
npm.cmd run test:interactions
npm.cmd run test:browser
npm.cmd run test:quality
node tests/cms-review.cjs
node tests/phase8-review.cjs
node tests/phase9-review.cjs
$env:NLO_ALLOW_SCHEDULE_CLOCK_TEST='1'
node tests/phase10-review.cjs
Remove-Item Env:NLO_ALLOW_SCHEDULE_CLOCK_TEST
dotnet publish src/NLOIQ.Web --no-build -c Release -o artifacts/phase11-publish
node tests/release-smoke.cjs
dotnet run --project tests/NLOIQ.AuthChecks --no-build -c Release
```

Authentication checks exhaust the login limiter; restart the preview or wait one minute before another sign-in check. The HTTPS smoke tool generates a temporary certificate only under ignored quality artifacts, does not trust/install it system-wide, uses the dedicated loopback database and stops its own test process.

## Quality limits and release gate

Zero automated violations is not a WCAG certification. The report retains incomplete color/background checks and closed-dialog ARIA checks. Popup controls reference the unique native dialog and opening/focus behavior is tested; image/complex-background interpretation and a full assistive-technology audit remain manual review. Browser execution here uses installed Microsoft Edge; other browser/device engines and the target hosting environment are not verified by these results. External YouTube playback remains dependent on that service/network; lifecycle tests use a controlled frame.

This checkpoint verifies local application workflows, **not production readiness or deployment**. Production host/runtime (including Linux native image support if chosen), real TLS/proxy configuration, public HTTPS origin, backups/media restoration, shared Data Protection keys, operations/monitoring and final company copy/logo/contact/project facts remain Phase 12 inputs/work. The Production-mode probe uses a temporary untrusted test certificate; it does not provision a production certificate or recreate runtime uploads.

**Stop for final Phase 11 quality approval. Do not start Phase 12 until approved.**
