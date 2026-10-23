# Phase 2 — Project foundation and seed preview

Date: 2026-10-23 (Asia/Baghdad). Status: ready for review; awaiting Phase 2 approval.

## Delivered

Phase 1 (revised Concept A) was explicitly approved. Created a .NET 10 solution with Domain, Application, Infrastructure and Web projects. Domain has no EF dependency; application contracts describe read models; infrastructure implements EF/MySQL persistence and seeding; Web composes services and renders MVC/Razor views. No Bootstrap, jQuery, SPA framework or admin template is included.

Stable package selection: MySql.EntityFrameworkCore 10.0.12, EF Core/design tooling 10.0.13, SDK 10.0.402, Tailwind/CLI 4.3.3. Package and tool versions are pinned; NuGet and npm lock files are committed. Root npm scripts compile the shared semantic theme entry, and vanilla JavaScript manages light/dark/system preferences. Early theme initialization is an external same-origin script compatible with the current CSP. Application interface strings use ASP.NET localization resources, while service content uses translation rows in MySQL.

Provider compatibility is no longer an open blocker. Sources: [Oracle-maintained NuGet package](https://www.nuget.org/packages/MySql.EntityFrameworkCore/10.0.12), [MySQL EF documentation](https://dev.mysql.com/doc/connector-net/en/connector-net-entityframework-core.html). The provider metadata declares GPL-2.0-only with Universal-FOSS-exception-1.0; retain a dependency/license inventory for the eventual production delivery.

The newly created dedicated MySQL database is WebSiteNLOIQDb (normalized by this Windows server to websitenloiqdb). Verified authenticated server version: 8.0.43. InitialPreviewFoundation creates ServicePreviews and ServicePreviewTranslations, with a foreign key, unique service keys, unique owner/culture pairs, ordering index and utf8mb4 charset. EF migration history tracks the applied migration.

## Requested seed data

Four company-confirmed specialty categories, each with English and Arabic titles and demo summaries:

1. Low-current engineering.
2. Software solutions.
3. SCADA system integration.
4. System automation.

Verified counts: four parent records and eight translations, four per language. Summaries are visibly marked demo copy. No fictional project/client/contact records, awards, statistics, credentials or users are seeded.

DevelopmentSeeder uses stable keys and a transaction. Repeated initialization inserted zero additional rows and preserved existing records. Initialization is explicit through --initialize-database; ordinary startup does not migrate or seed. Production rejects initialization before writes, and development preview pages return 404 outside Development.

These minimal tables exist to verify the foundation and show requested data. They do not implement mutable published CMS content. The complete revision-backed content architecture remains Phase 5; no public editing or publish workflow exists yet.

## Preview and run instructions

- English: http://127.0.0.1:5080/en/
- Arabic: http://127.0.0.1:5080/ar/
- Root redirects to /en/.

From the repository root, run dotnet run --project src/NLOIQ.Web --no-build. First-time setup, database initialization and build commands are in README.md and LOCAL_SETUP.md. The page shows the selected engineering folio composition, system illustration and four expandable database-backed specialties. This is a foundation page, not the full Phase 3 public site.

Keep local connection credentials in src/NLOIQ.Web/appsettings.Local.json. The file is ignored, loaded only in Development and explicitly excluded from output/publish. The ignored root copy is retained but is not read by the application. Environment configuration overrides local settings. No secret was placed in migrations or seed code.

## Relevant code

- NLOIQ.sln, global.json, Directory.Build.props and dotnet-tools.json: solution/tooling baseline.
- src/NLOIQ.Domain/Entities/ServicePreview.cs: minimal preview model and translation ownership.
- src/NLOIQ.Application/Interfaces/IPreviewContentReader.cs: application read contract/DTO.
- src/NLOIQ.Infrastructure/Data/: mappings, projection reader and idempotent development seed.
- src/NLOIQ.Infrastructure/Migrations/: initial schema and snapshot.
- src/NLOIQ.Infrastructure/DependencyInjection.cs: database/service registration.
- src/NLOIQ.Web/Program.cs: configuration, development initialization, localization, headers and routing.
- src/NLOIQ.Web/Controllers/HomeController.cs and Views/Home/Index.cshtml: database-backed foundation page.
- src/NLOIQ.Web/Resources/: English/Arabic interface resources.
- src/NLOIQ.Web/Assets/styles/app.css and wwwroot/: shared tokens, compiled CSS and theme JavaScript.
- tests/foundation-smoke.mjs: real HTTP smoke checks against the running app and seeded database.

## Executed checks

| Check | Result |
| --- | --- |
| .NET Debug build | Passed, zero warnings/errors |
| Tailwind v4 build | Passed |
| JavaScript syntax | Passed |
| Initial migration | Applied successfully to the dedicated database |
| Migration/model consistency | No pending model changes |
| Seed rerun | Zero inserted; four services/eight translations retained |
| Authenticated MySQL query | Server 8.0.43, intended database, utf8mb4 |
| /en/ and /ar/ | HTTP 200, localized headings/direction and four database-backed titles |
| Static CSS/JS | HTTP 200 |
| Root redirect | 302 to /en/ |
| Unsupported culture and config-file URLs | 404 |
| Production initialization | Explicit rejection before writes |
| Publish output | Release publish succeeded; local credential file absent |
| npm audit (app and retained design tooling) | Zero reported vulnerabilities |
| .NET vulnerable-package report | No vulnerable packages reported by configured sources |
| Browser screenshot / interactive visual checks | Not run successfully: browser runtime cannot start |

An HTTP smoke test caught Windows pipeline encoding damage in Arabic output. Arabic files were rewritten with explicit UTF-8, only affected demo translation rows were repaired, and both language checks passed afterward. The reusable smoke check verifies real Arabic heading text and rejects repeated question marks. An affected transitive watcher was pinned to 2.6.0 in both tooling packages; compilation and audits passed afterward.

To repeat HTTP checks while the application is running: node tests/foundation-smoke.mjs. These are not a complete browser accessibility, theme-interaction, responsive or security audit.

## Incomplete and next

Full public sections/portfolio/contact/footer, final company assets and approved copy remain pending Phase 3. Parallax/lightbox is Phase 4; full content schema Phase 5; Identity/admin/CMS arrive in later approved phases. No production deployment or production-readiness claim is made.

You can revise the visible sample summaries, category labels, seed scope, spacing or foundation configuration before approval. Phase 3 begins only after explicit approval of this phase.
