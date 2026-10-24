# NLO IQ

ASP.NET Core MVC modular monolith — Phase 9 localization review.

## Quick start (PowerShell)

```powershell
dotnet tool restore
npm.cmd ci
npm.cmd run css:build
dotnet restore NLOIQ.sln --locked-mode
dotnet build NLOIQ.sln
# Create src/NLOIQ.Web/appsettings.Local.json from appsettings.example.json,
# then supply the password locally. Never commit the local file.
dotnet run --project src/NLOIQ.Web -- --initialize-database
dotnet run --project src/NLOIQ.Web --no-build
```

The launch profile selects Development and http://127.0.0.1:5080.

- English: http://127.0.0.1:5080/en/
- Arabic: http://127.0.0.1:5080/ar/
- With the application running: `node tests/prototype-smoke.mjs`

Initialization applies all committed migrations and creates the initial preview services, draft content fixtures and role definitions. Re-running it preserves existing records. It creates no user/password. Demo initialization and the fallback prototype are Development-only; normal startup does not migrate or seed. On a new checkout, run `dotnet run --project src/NLOIQ.Web --no-build -- --create-admin` to create your own administrator interactively.

See [Phase 7 functional review](docs/PHASE_7_CMS.md), [local setup](docs/LOCAL_SETUP.md), and [progress](docs/PROJECT_PROGRESS.md). Media/SEO/appearance editors are now available; Arabic/English localization is available; the complete publishing workflow remains Phase 10.

Interaction checks: npm.cmd run test:interactions and dotnet run --project tests/NLOIQ.Checks. With the app running and Microsoft Edge installed, npm.cmd run test:browser runs the real browser checks. Screenshots are in docs/previews/phase4/. Actual external YouTube playback still needs user review.

Database review: dotnet run --project src/NLOIQ.Web -- --inspect-content. Integrity checks: dotnet run --project tests/NLOIQ.DatabaseChecks (dedicated development database, rollback-only scenarios). Full schema map: docs/DATABASE_ARCHITECTURE.md.

## Phase 6 admin preview

Historical Phase 6 checkpoint (now approved). Open http://127.0.0.1:5080/en/account/login or /ar/account/login. Local review credentials are in ignored artifacts/PHASE_6_LOGIN.txt. See [Phase 6 review](docs/PHASE_6_ADMIN.md) for role permissions, administrator setup and verification. Phase 7 is now delivered for functional review.

## Phase 7 core CMS review

Open http://127.0.0.1:5080/en/admin/content (Arabic: /ar/admin/content). Use the existing ignored local login file. Published edit demonstration: http://127.0.0.1:5080/en/pages/cms-review and /ar/pages/cms-review. See [functional review instructions](docs/PHASE_7_CMS.md). Editor saves drafts; SuperAdmin publishes. Phase 8 is now delivered for review.

## Phase 8 media, SEO and appearance

Use the existing login for [media library](http://127.0.0.1:5080/en/admin/media), [SEO](http://127.0.0.1:5080/en/admin/seo), [site settings](http://127.0.0.1:5080/en/admin/settings/site), [appearance](http://127.0.0.1:5080/en/admin/settings/appearance), and [audit history](http://127.0.0.1:5080/en/admin/audit). Arabic mirrors /ar/. See [Phase 8 review](docs/PHASE_8_MEDIA_SEO_APPEARANCE.md). Preserve ignored App_Data/media with the database; user uploads are runtime data, not Git/publish artifacts. Phase 8 is approved. Phase 9 is delivered for localization review.

## Phase 9 localization

Open the [translation overview](http://127.0.0.1:5080/en/admin/translations) or [Arabic overview](http://127.0.0.1:5080/ar/admin/translations). Review the published [English language demonstration](http://127.0.0.1:5080/en/pages/localization-review) and [Arabic demonstration](http://127.0.0.1:5080/ar/pages/مراجعة-اللغات), then use the header language link. Visiting `/` remembers your last language. See [Phase 9 instructions](docs/PHASE_9_LOCALIZATION.md). Phase 10 requires explicit approval.
