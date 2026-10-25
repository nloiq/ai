# Header appearance controls

Added 2026-10-25 at the user's request. Open `/en/admin/settings/appearance` or `/ar/admin/settings/appearance` as SuperAdmin, then scroll to **Header appearance / مظهر رأس الموقع**.

- Show logo border: enable/disable the 1 px frame.
- Logo border color: six-digit color, chosen using the color picker.
- Logo corner radius: 0–32 px (0 gives square corners).
- Logo height: 24–96 px, bounded by a responsive maximum width to preserve layout.
- Theme selector style: Dropdown or Segmented buttons (System / Light / Dark).
- Theme control corner radius: 0–28 px.

Light and dark previews respond as you edit and show the currently published logo. Select the actual logo image through **Site settings**, not Appearance. To expose your changes: **Save draft → Saved revision preview → review → tick publication verification → Publish saved draft**. Unsaved changes do not change visitors' settings. Appearance applies to public headers and the admin theme selector; the admin title remains the CMS wordmark. Segmented controls preserve the visitor's saved preference and System follows the device preference.

The public logo no longer includes the design prototype's “LOGO PLACEHOLDER” caption. Safe defaults: no logo border, 48 px logo, 8 px corners, dropdown. Existing saved appearance choices are retained during review; tests do not publish over the user's drafts.

## Persistence and deployment

Six fields are stored on immutable ThemeSettings revisions, using existing save/publication/restore guards. Colors are hex-validated; numeric values and selector style are bounded on input and domain validation. Public CSS is generated only for the current eligible published appearance revision. No inline CSS or CSP exception was added.

Additive migration: `20261025070549_HeaderAppearance`. It initializes historical rows with valid defaults (48 px height, 8 px corners, #687C50 border color, no frame, dropdown). A private local SQL backup was taken before applying it to the development database. For another host, use the reviewed migration process in DEPLOYMENT.md and back up first. Down migration removes these settings; avoid downgrading without preserving newer appearance revisions. Earlier Phase 12 artifact/script evidence predates this change; rebuild the release and regenerate migration SQL before deployment.

## Verification

Release solution build: zero warnings/errors. EF model matches committed migration. 45 rollback database scenarios pass, including header draft CSS isolation, publication/round-trip, restoration and invalid-height rejection. All five existing interaction/browser tests pass, including Arabic mobile/theme persistence and idle/coalesced motion. The quality runner passes 51 automated accessibility scans plus security/SEO checks. These remain local Edge checks, not a full accessibility certification.

`node tests/header-appearance-review.cjs` checks both languages' live preview controls, logo rendering, segmented preference/pressed states, responsive overflow and four additional accessibility scans without saving or publishing. It uses the ignored local review account; run only on the dedicated development preview. Screenshots are ignored under artifacts/header-appearance. The script temporarily reveals the real segmented control in its browser DOM when needed to exercise behavior without changing stored settings.
