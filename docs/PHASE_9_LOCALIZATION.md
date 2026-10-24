# Phase 9 — Complete localization

Status: delivered for review. Phase 8 checkpoint `74e3a24` was approved. Phase 10 has not started.

## Review in the running local application

1. Open http://127.0.0.1:5080/en/pages/localization-review and choose العربية in the header. The Arabic URL is http://127.0.0.1:5080/ar/pages/مراجعة-اللغات. Check the Arabic title, summary, engineering fields and body. Choose English to return to the English slug.
2. After choosing Arabic, open http://127.0.0.1:5080/ in the same browser. It redirects to `/ar/`. Choosing English changes that preference. An explicit `/en/` or `/ar/` URL always wins. A first visit without a preference uses the published site default.
3. Sign in using the existing ignored `artifacts/PHASE_6_LOGIN.txt`. Open `/en/admin/translations` and `/ar/admin/translations`. Both SuperAdmin and Editor can review translations; Viewer cannot edit or open the translation manager.
4. Each card compares the latest English and Arabic draft text, localized slugs, paired button labels, and image alt/caption snapshots. Missing fields link to the corresponding editor tab. Coverage is a translation aid, not publication approval; structured navigation/contact rows and optional SEO overrides still need review inside the editor.
5. Switch languages on a filtered content listing or login page. Filters remain, and a local login return path switches to the matching admin language. Arabic administration opens the Arabic translation tab; an explicit editor hash can open either language.
6. Check Arabic contact-type/layout/background/icon choices, weekday names, native validation messages and server validation. Language-specific input directions keep English, Arabic, URLs and identifiers readable. Both themes and narrow layouts work.

The new `localization-review` page is an honest demonstration of the four supplied engineering specialties. It is not a claim about completed projects or clients. Original home content and other company demo roots remain drafts until reviewed. Existing `cms-review` media/SEO examples remain available.

## Behavior delivered

- Complete paired English/Arabic interface resources (367 keys), localized editor listing/choice titles, module choices, social labels, audit actions and validation.
- Paged, searchable translation overview (50 roots per page), with immutable-revision editing through the existing CMS. Empty optional fields in both languages do not create false missing-translation warnings; text present in one language identifies the corresponding missing field in the other.
- Independent Unicode/NFC slugs; current published content determines reciprocal language-switch destinations. Arabic drafts do not replace published Arabic text until explicit publication.
- HTML language/direction, `Content-Language`, translated SEO, reciprocal `hreflang`, default-language `x-default`, and Open Graph locale/alternate metadata. English sharing uses `en_US`; Arabic uses `ar_IQ`.
- One-year HttpOnly/SameSite=Lax language preference cookie; Secure outside Development. Only validated `en`/`ar` values affect the root redirect. Root redirects are not cached. URL prefixes remain authoritative; query-string culture values cannot override them.
- Arabic typography keeps joined letters and diacritics readable, with natural tracking and comfortable leading. Arabic formatting uses a Gregorian calendar. Navigation and animations retain their existing RTL and reduced-motion behavior.
- No new dependency, database migration, automatic translation service, or external font is required.

A public HTTPS origin must still be supplied before production so canonical/alternate URLs are fully qualified. Local previews deliberately use relative URLs instead of inventing a production origin. Development remains noindex. Company facts, logo, contact/social destinations and final copy still require supplied/approved values.

Metadata follows [Google's localized-page guidance](https://developers.google.com/search/docs/specialty/international/localized-versions) and the [Open Graph locale specification](https://ogp.me/).

## Verification

- Eight projects build with zero warnings/errors.
- 32 rollback-only database scenarios plus formatted-text safety checks pass, including independent Arabic slugs, localized listing titles, reciprocal alternates, wrong-language route rejection and private Arabic draft isolation.
- Real Edge checks verify language preference and invalid-cookie fallback, URL authority, preserved filters/return URLs, native/server validation, translated choices/weekdays, missing-field detection, bilingual publishing and metadata, both themes and widths 320/390/768/1440, and zero browser script errors.
- Existing Phase 8 media/SEO/settings/appearance browser flow, five public interaction/browser tests, prototype smoke checks, content-rule checks and four interaction unit checks pass.
- Authentication/role/CSRF checks include the translation manager; schema model check has no pending migration.
- Resource keys and format placeholders match; literal resource references resolve; files remain UTF-8. Screenshots were visually reviewed in `docs/previews/phase9/`.

Run stateful review scripts sequentially on the dedicated development database only:

```powershell
dotnet build NLOIQ.sln --no-restore
dotnet run --project tests/NLOIQ.DatabaseChecks --no-build
node tests/phase9-review.cjs
node tests/phase8-review.cjs
npm.cmd run test:browser
node tests/prototype-smoke.mjs
npm.cmd run test:interactions
dotnet run --project tests/NLOIQ.Checks --no-build
dotnet run --project tests/NLOIQ.AuthChecks --no-build
```

`phase9-review.cjs` creates/updates and publishes the retained localization demonstration, temporarily clears its Arabic title to test missing-translation reporting, then restores complete bilingual content. Do not run it against production or repurposed review content. The account test exhausts its login limiter; restart the preview or wait one minute before another login check. Retained database/media content is not recreated by a Git checkout alone.

**Stop for Phase 9 approval.** Phase 10 covers the remaining publishing/preview/revision workflow. Release QA and deployment remain Phases 11–12.
