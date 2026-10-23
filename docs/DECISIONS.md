# NLO IQ decisions

Date: 2026-10-23. All architecture choices below are proposed pending Phase 0 approval.

| ID | Decision | Reason / status |
| --- | --- | --- |
| D001 | Strict approval gate after each phase | Explicit user requirement; Phase 0 only authorized now |
| D002 | .NET 10 LTS MVC modular monolith | Installed supported baseline; domain/application/infrastructure/web boundaries |
| D003 | MySQL and EF Core | Required stack; stable provider version must be verified before Phase 2 setup |
| D004 | Razor, Tailwind v4, vanilla JS | Required stack; no SPA or prebuilt admin framework |
| D005 | Route cultures en/ar and revision-scoped translation tables | Real bilingual content, RTL and localized SEO |
| D006 | Complete required Arabic/English translations before publication | Explicit indicators and blocking validation avoid silent fallback |
| D007 | Immutable revisions and separate published pointers | Draft isolation and reliable restoration; foundational schema in Phase 5 |
| D008 | Shared semantic theme tokens | Public/admin consistency with constrained runtime customization |
| D009 | Native MySQL first for development | Running service; Docker unavailable; schema access still unverified |
| D010 | No fabricated company facts | Only approved content can become real public claims |
| D011 | Editor drafts; SuperAdmin publishes | Proposed least-privilege role policy; may be revised |
| D012 | Documentation only in Phase 0 | No application scaffolding until its approved phase; Git setup planned in Phase 2 |

User revision: perform local configuration and Git setup now, before Phase 1. Store credentials only in ignored local configuration. Target private GitHub repository: nloiq/NLOIQ; remote is not created yet.


User revision: use the existing https://github.com/nloiq/ai.git repository as origin instead of creating nloiq/NLOIQ. Keep main as the tracked branch and preserve existing repository visibility.


## Phase 1 — proposed design decisions

- Phase 0 approved explicitly; Phase 1 approval pending.
- Preserve two composition choices: Botanical folio and Autumn field.
- Use source-native concept artwork and local system fonts until company assets and typography are approved.
- Use accessible orange #884125 for light CTAs; preserve #C56A3D as the brand swatch.
- Compile Tailwind 4.3.3 in isolated design tooling; no application foundation yet.
- Keep previews local and do not deploy publicly. Browser rendering remains unverified due to runtime failure.


## Confirmed company expertise and selected visual direction

Concept A selected. Business areas confirmed by user: low-current engineering, software solutions, SCADA system integration and system automation. Use labeled technical line icons, topology/monitoring/logic motifs, evergreen/charcoal base, copper connections and restrained theme-aware teal accents. Botanical elements support the engineering story. Phase 1 revision awaits approval.


## Phase 2 foundation

- Phase 1 approved explicitly; user requested visible seed data.
- Use MySql.EntityFrameworkCore 10.0.12 with EF Core 10.0.13 on .NET 10.
- Seed minimal development-only ServicePreview records and translation rows; full revision-backed CMS model stays in Phase 5.
- Never seed invented company achievements, projects, contacts, users or passwords.
- Initialize/migrate only via explicit development command; preserve existing seed keys on rerun.
- Web-project local configuration is the active development source, ignored and excluded from publish; retain root ignored copy for reference.
- Pin transitive @parcel/watcher 2.6.0 to remove the affected braces dependency in both tooling sets.
- Browser visual verification remains pending; HTTP/SQL checks are real and recorded.


## Phase 3 public prototype

- Phase 2 explicitly approved; build the full visual public prototype using selected Concept A.
- Preserve four database-backed service categories and demo descriptions without schema changes.
- Use local engineering illustrations for portfolio layout studies; never present them as client projects.
- Keep pending contact fields honest; no invented links or disconnected form.
- Split public sections into Razor partials; retain shared semantic tokens and bilingual route architecture.
- Basic responsive navigation belongs to this phase; parallax, video lightbox and advanced motion remain Phase 4.
- Browser runtime remains unavailable; report HTTP/source verification separately from visual review.

