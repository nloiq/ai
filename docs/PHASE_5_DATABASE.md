# Phase 5 — Database and content architecture review

Date: 2026-10-23 (Asia/Baghdad). Status: ready for database review; Phase 5 approval pending.

Phase 4, including its video discoverability revision, was approved before this work. The last approved checkpoint is 11d7fda. No Phase 6 login/admin implementation has begun.

## Delivered

Implemented normalized content identities and typed immutable snapshots for pages, sections, projects/categories, services, navigation, contact, footer, site defaults and themes. Added revision-scoped translations, SEO, media usage/caption snapshots, audit models, schedule records and Identity persistence. Detailed relationships and the ER diagram are in DATABASE_ARCHITECTURE.md.

The existing preview and its four services/eight translations were preserved. New content fixtures are deliberately draft-only: 22 identities/revisions, 44 translations, three real bundled media metadata rows, three role definitions, zero users and zero published items. No administrator password or invented company contact/project achievement was added.

The phase also provides append-only revision/audit guards, optimistic concurrency, publication prerequisites, same-owner revision pointers and a published-only query contract. These are foundation services and database safeguards; CMS editors and publish/preview/restore interfaces are still later phases.

## Migrations and database verification

Applied migrations:

1. InitialPreviewFoundation (previously approved).
2. ContentArchitecture (new tables, constraints, relationships and indexes).
3. ContentIdentityGeneration (explicit MySQL identity-generation correction with dependent foreign-key rebuilds).

The current schema contains 40 tables. During development, the migration check caught MySQL's row-size limit and identity-generation convention for a key participating in a composite foreign key. Long body storage now uses longtext, and identity generation is explicitly annotated. Empty tables from the interrupted attempt were verified empty and recovered without touching the existing preview data. The final migrations are applied and their model snapshot has no pending changes.

A pre-change development backup exists locally at artifacts/before-phase5.sql. Generated final SQL exists at artifacts/content-architecture.sql. These local artifacts are ignored by Git. MySQL DDL is not treated as transactionally reversible; do not run destructive downgrade commands against a populated database without an agreed rollback/backup plan.

## Review and run

The public preview remains available:

- http://127.0.0.1:5080/en/
- http://127.0.0.1:5080/ar/

To inspect new backend fixtures from the repository root:

```powershell
dotnet run --project src/NLOIQ.Web -- --inspect-content
```

That development-only command performs read-only queries and prints counts, kinds, keys and draft/published flags. A captured output is committed as PHASE_5_SEED_INVENTORY.json. No schema/debug endpoint was added to the public site.

For a fresh development checkout, follow LOCAL_SETUP.md, then:

```powershell
dotnet tool restore
dotnet restore NLOIQ.sln --locked-mode
dotnet build NLOIQ.sln
dotnet run --project src/NLOIQ.Web -- --initialize-database
dotnet run --project src/NLOIQ.Web --no-build
```

The explicit initialization command applies all migrations and inserts missing initial fixtures; ordinary startup does not migrate or seed. Existing seed data is preserved on repeat initialization.

## Actual tests

Passed:

- Solution build and locked restore; migration application and model consistency.
- Expanded server checks for cultures, field limits, unsafe targets, icon/layout/motion/video options and theme contrast.
- 16 real-MySQL rollback-only scenarios: draft/archived/demo isolation; immutable translations/children/audit; required translations; same-owner pointers; unique translation/routes; culture constraints; referenced media deletion; stale-writer concurrency; pinned page sections; long Arabic text; same-revision cover media; bilingual media alt prerequisites; snapshot kind consistency.
- Final counts after all test rollbacks: 22 draft identities, 22 revisions, 44 translations, preserved four preview services, three roles and zero users.
- Seed rerun inserted zero; model has no pending changes.
- Existing bilingual HTTP checks and all five Edge browser regression tests passed.
- Vulnerable-package report found no vulnerable .NET packages in configured sources.

To repeat, from the repository root:

```powershell
dotnet run --project tests/NLOIQ.Checks
dotnet run --project tests/NLOIQ.DatabaseChecks
node tests/prototype-smoke.mjs
npm.cmd run test:browser
```

DatabaseChecks is explicitly limited to the dedicated WebSiteNLOIQDb development schema, uses the ignored local configuration/environment setting, and rolls back each scenario. It expects the initial fixture inventory and is intended for this review checkpoint; later content changes require updating the fixture expectations. Browser/HTTP checks require the app running, and browser checks use installed Edge headlessly.

## Relevant source

- src/NLOIQ.Domain/Content/: identities, revisions, typed snapshots, relationships, metadata and lifecycle entities.
- src/NLOIQ.Application/Validation/ContentRules.cs: bounded/allowlisted settings and publication translation checks.
- src/NLOIQ.Application/Interfaces/IPublishedContentReader.cs: public query contract.
- src/NLOIQ.Infrastructure/Data/ContentModelConfiguration.cs: relational schema and constraints.
- src/NLOIQ.Infrastructure/Data/ContentWriteGuard.cs: immutability, concurrency and publication prerequisites.
- src/NLOIQ.Infrastructure/Data/PublishedContentReader.cs: translated SQL projections through published pointers.
- src/NLOIQ.Infrastructure/Data/ContentArchitectureSeeder.cs: transactional draft/role/media fixtures.
- src/NLOIQ.Infrastructure/Identity/ApplicationUser.cs and Migrations/: Identity persistence and migration chain.
- src/NLOIQ.Web/Program.cs: explicit development initialization/inspection commands.
- tests/NLOIQ.DatabaseChecks/: real database integrity scenarios.

## Remaining work and review choices

Review entity fields, approved settings, relationships, translation policy, seed scope and publication safeguards. The shared ContentTranslations table is the translation source for all typed content revisions. This is a deliberate revision-scoped normalized model; editing interfaces will present content-specific bilingual fields.

Pending: authentication/authorization and admin shell (Phase 6); content CRUD/public CMS wiring (Phase 7); uploads/media/SEO/appearance tools (Phase 8); localization completion (Phase 9); publish/preview/restore/scheduler UI and workflow services (Phase 10); final QA and deployment (Phases 11–12). No claim of completed CMS or production readiness is made.

Await explicit Phase 5 approval before Phase 6.
