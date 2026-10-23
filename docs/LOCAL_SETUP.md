# Local configuration and database setup

## Configuration

The supplied connection is stored locally in src/NLOIQ.Web/appsettings.Local.json, excluded from Git and publish output. A retained ignored root copy was used during discovery; the Web application reads the Web-project copy only.

The committed appsettings.example.json contains the intended shape and a placeholder password. For a new checkout, copy that file into src/NLOIQ.Web/appsettings.Local.json and enter the password locally. The application loads this file only in Development, then reapplies environment variables and command-line settings so deployment overrides retain precedence.

Production must configure ConnectionStrings__DefaultConnection through the hosting secret/environment system. Do not include passwords in shell command arguments or tracked files. HTTPS/domain/proxy configuration remains part of deployment preparation.

## Database and seed initialization

Dedicated target: 127.0.0.1:3306 / WebSiteNLOIQDb, user nloiq. Verified server: MySQL 8.0.43; the Windows server normalizes the database name to lowercase. The migration and data use utf8mb4.

From the repository root:

```powershell
dotnet tool restore
npm.cmd ci
npm.cmd run css:build
dotnet restore NLOIQ.sln --locked-mode
dotnet build NLOIQ.sln
dotnet run --project src/NLOIQ.Web -- --initialize-database
dotnet run --project src/NLOIQ.Web --no-build
```

The initial development command creates the dedicated schema when absent and the configured account has sufficient privileges. A deployment database and account should be provisioned separately; no other schema is touched. Migration identity: InitialPreviewFoundation.

Expected seed result: four ServicePreviews and eight ServicePreviewTranslations (four en, four ar). The four categories are company-confirmed; summaries are explicitly demo content. There are no fictional client/project/contact records, users, or administrator passwords.

Seeding runs in a transaction, checks stable keys, and never overwrites existing records. Re-run inserts zero when all keys exist. If a record is edited or deleted manually, do not assume seeding is an editor or a restoration tool. Production rejects the initialization command before migrations or seed writes.

The seed tables are a minimal read-only foundation preview, not revision-backed CMS entities. Phase 5 adds the full content model; preserve the confirmed categories and migrate approved content explicitly at that point.

## Checks

```powershell
node tests/foundation-smoke.mjs
$env:ASPNETCORE_ENVIRONMENT='Development'
dotnet tool run dotnet-ef migrations has-pending-model-changes --project src/NLOIQ.Infrastructure --startup-project src/NLOIQ.Web --no-build
npm.cmd audit
dotnet list NLOIQ.sln package --vulnerable --include-transitive
```

Smoke checks need the initialized database and running development application. They exercise real HTTP responses and database-backed English/Arabic content; they are not browser interaction or screenshot tests.

The full Phase 1 design comparison remains available via node docs/design/serve.mjs on port 4173. The actual ASP.NET foundation runs on port 5080. Do not confuse the design-only preview with database-backed application behavior.

## Phase 5 content fixtures

The initialization command now also creates 22 draft-only content identities/revisions, 44 translations, metadata for the three actual bundled illustrations and SuperAdmin/Editor/Viewer role definitions. It creates no users or passwords. Existing preview tables remain intact.

Run dotnet run --project src/NLOIQ.Web -- --inspect-content to read the new inventory. Database architecture and migration details are in PHASE_5_DATABASE.md and DATABASE_ARCHITECTURE.md. Upload workflows and administrator authentication are later phases.

## Phase 7 CMS review

Core CMS is at /en/admin/content and /ar/admin/content. Use the Phase 6 local review login. The retained bilingual published example is /en/pages/cms-review and /ar/pages/cms-review. Apply all four checked-in migrations, including BusinessHoursClockConversion, when preparing a fresh database. Existing-file selection is available; uploads/SEO/appearance remain Phase 8. See PHASE_7_CMS.md for safe functional checks.
