# NLO IQ

ASP.NET Core MVC modular monolith — Phase 5 database/content architecture review.

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

Initialization applies the committed migration and inserts missing demo service keys only. Re-running it preserves existing records. Demo initialization and demo pages are blocked outside Development. Normal application startup does not run migrations or insert seed data.

See [Phase 5 review](docs/PHASE_5_DATABASE.md), [local setup](docs/LOCAL_SETUP.md), and [progress](docs/PROJECT_PROGRESS.md). This is not the complete public website or CMS.

Interaction checks: npm.cmd run test:interactions and dotnet run --project tests/NLOIQ.Checks. With the app running and Microsoft Edge installed, npm.cmd run test:browser runs the real browser checks. Screenshots are in docs/previews/phase4/. Actual external YouTube playback still needs user review.

Database review: dotnet run --project src/NLOIQ.Web -- --inspect-content. Integrity checks: dotnet run --project tests/NLOIQ.DatabaseChecks (dedicated development database, rollback-only scenarios). Full schema map: docs/DATABASE_ARCHITECTURE.md.
