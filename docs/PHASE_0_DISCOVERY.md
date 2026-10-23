# NLO IQ — Phase 0: discovery and plan

Date: 2026-10-23 (Asia/Baghdad). Status: ready for review; approval pending.

## Scope and evidence

The workspace was empty at discovery. No existing application, dependencies, Git repository, or applicable AGENTS.md was found in the workspace or checked parent directories. Only planning documentation is delivered in this phase. No application code, package installation, database modification, or public deployment has been performed.

| Check | Observed result |
| --- | --- |
| OS | Windows x64 |
| .NET | SDK 10.0.402 installed; ASP.NET Core runtime 10.0.13 available |
| Other SDKs | 9.0.311 and 9.0.319; older runtimes also present |
| Node / npm | v24.13.0 / 11.6.2 |
| Git | 2.55.0.windows.5; workspace is not a repository |
| MySQL | MySQL80 Windows service running; MySQL Server 8.0 directory present; TCP connection to 127.0.0.1:3306 succeeded |
| MySQL client | mysql command not on PATH; exact server patch, credentials, privileges, schema, and authenticated queries unverified |
| Docker | CLI 28.4.0 installed; daemon unavailable; user config access denied in this session |
| Build / tests | Not applicable: no application exists yet |

TCP reachability is not proof of successful database authentication. No existing database was read or changed. Docker is optional because native MySQL is available.

## Architecture and technology decisions

Use one ASP.NET Core MVC modular monolith targeting .NET 10 LTS, with C#, Razor, Identity, EF Core, MySQL, Tailwind CSS v4 and vanilla JavaScript. Public pages render on the server; narrowly scoped APIs support necessary CMS interactions. No SPA framework, Bootstrap, jQuery, admin template, or microservices.

.NET 10 is the proposed baseline because it is installed and supported through November 2028. Before Phase 2 implementation, verify and pin a stable compatible EF Core/MySQL provider combination against vendor documentation and actual restore/migration/query tests. Do not assume EF provider majors are interchangeable, use preview providers in production, silently downgrade the platform, or replace MySQL. The reviewed MySQL compatibility table still lists EF Core 10 preview support; provider selection remains an open dependency.

Dependencies point inward:

- Domain: entities, publication invariants, value objects; no EF or Identity dependencies.
- Application: use cases, DTOs, validation, permission requirements and storage/content interfaces; references Domain.
- Infrastructure: EF mappings, migrations, Identity, storage, revision serialization, caching and background publishing; references Application and Domain.
- Web: MVC controllers, Areas/Admin, Razor partials/view components, localized resources and assets; references Application. The composition root wires Infrastructure through DI.

Modules: identity/access, pages/sections, portfolio, services, media, navigation/contact, appearance/SEO, publishing/revisions, audit. Prefer focused application services and EF projections over a generic repository framework. Start with one database and explicit transaction boundaries.

```text
NLOIQ.sln
src/
  NLOIQ.Domain/Entities/
  NLOIQ.Application/{Interfaces,Services,DTOs,Validation}/
  NLOIQ.Infrastructure/{Data,Identity,Migrations,Storage,Publishing}/
  NLOIQ.Web/
    Areas/Admin/{Controllers,Views,ViewModels}/
    Controllers/
    Views/{Shared,Home,Projects}/
    ViewModels/
    Resources/
    Assets/{styles,scripts}/
    wwwroot/
tests/
  NLOIQ.UnitTests/
  NLOIQ.IntegrationTests/
  NLOIQ.BrowserTests/
docs/
package.json
```

This is a proposed structure, not generated application scaffolding. Git initialization and a documentation checkpoint are planned for Phase 2; no commit exists yet.

## Initial database plan

Use MySQL InnoDB, utf8mb4, explicit collations, UTC timestamps and application-managed optimistic concurrency tokens. Bound indexed text lengths and verify Arabic storage, slug comparisons and key limits against the selected server/provider.

| Group | Entities and relationships |
| --- | --- |
| Access | Identity users, roles, user-role links, claims and tokens; role keys SuperAdmin, Editor, Viewer |
| Composition | Pages → PageTranslations; Pages → Sections → SectionTranslations; ordered sections with approved type, layout, visibility and bounded motion settings |
| Portfolio | Projects → ProjectTranslations; Projects → ProjectMedia → MediaAssets; ProjectCategories → ProjectCategoryTranslations; ProjectCategoryAssignments joins projects/categories |
| Services | Services → ServiceTranslations; optional approved icon and media reference |
| Media | MediaAssets → MediaAssetTranslations for localized alt text; explicit usage references from content/revisions |
| Navigation | NavigationItems → NavigationTranslations; self-referencing parent for hierarchy; FooterSettings and FooterTranslations |
| Contact | ContactDetails and ContactTranslations, BusinessHours and SocialLinks; typed validated targets |
| Configuration | SiteSettings and ThemeSettings; approved values only; localized site text via SiteSettingTranslations |
| Discovery | SeoMetadata linked to a localized page/project revision; canonical, title, description and Open Graph media |
| Publication | ContentRevisions, RevisionTranslations, PublicationSchedules and AuditLogs; draft and published pointers on stable content roots |

Foreign keys protect all relationships. Use unique translation keys (owner, culture), unique localized slugs (culture, slug) within route scope, unique setting keys and category assignments. Add indexes for publication state/time, ordering, category filters, media usage and audit time/actor. Restrict deletion of referenced assets and published dependencies; cascade only owned translations or joins where safe. Pagination is mandatory for growing admin tables.

### Publishing consistency from the beginning

A stable content root identifies each item. Immutable revisions contain its editable content, translations, localized SEO and versioned relationships (media, categories, navigation and layout configuration where relevant). Separate DraftRevisionId and PublishedRevisionId point to those revisions. Do not overwrite a published revision when editing.

Translation tables in the initial entity plan will be revision-scoped for publishable content; avoid a second mutable copy that public reads might accidentally expose. Explicit relationship/revision mapping is finalized in Phase 5, including relational foreign keys and deletion rules. Structured section payloads, where needed, use versioned validated schemas; never arbitrary executable markup.

Publishing validates the complete revision, then atomically swaps the published pointer with a concurrency check, writes the audit record and invalidates relevant caches. Public queries read only published pointers and exclude archived items. Preview uses authorized, expiring tokens and no-store responses; drafts are excluded from sitemap and public caches. Restore creates a new draft from an old revision. Scheduling uses an idempotent background worker and UTC timestamps if implemented.

Phase 5 establishes revision-safe persistence; Phase 7 exposes basic save/publish; Phase 10 completes preview, restoration, scheduling and their interfaces. This prevents the later workflow phase from requiring unsafe retrofit of mutable published data.

## Localization and RTL

- ASP.NET Core localization and resource files handle public/admin interface strings. Database translation tables handle editorial content.
- Public routes: /en/, /ar/, /{culture}/projects/{localizedSlug}. Admin routes include culture too. Route culture takes precedence over saved preference; the root redirects using the configured default or explicit saved language.
- Set html lang and dir together. Use logical spacing, direction-aware arrows and focus order, suitable Arabic font metrics, and selectively mirror motion direction. Videos, numbers and code retain appropriate direction.
- Require English and Arabic translations for publishing at launch. Missing required translations block publication with visible field-level indicators. Optional translated fields may be omitted; do not silently display English as Arabic. Public missing routes return a localized 404 and a link to the available language.
- Language switching resolves the equivalent content identity and localized slug. Emit hreflang only for published equivalents. Metadata, validation and CMS notifications are localized.

## Design, themes and motion

Creative premise: “Nature has a system. We engineer yours.” Use botanical structure, copper linework, editorial typography and architectural spacing. Company achievements, clients and service claims require supplied evidence.

| Token role | Light direction | Dark direction |
| --- | --- | --- |
| Background | Warm Ivory #F3EFE5 | Deep Evergreen #14271F |
| Surface / elevated | Ivory-derived neutrals | Forest Green #203D32 / Charcoal #222722 |
| Text | Deep Evergreen | Warm Ivory |
| Muted text / borders | Tested green/neutral variants | Tested ivory/green variants |
| Primary / secondary accent | Burnt Orange #C56A3D / Moss #687C50 | Autumn Gold #D8AD64 / muted Copper #A65E40 |

These are candidate roles, not a verified contrast matrix. Phase 1 tests text, buttons, focus and interaction pairs; accents are not automatically valid small-text colors.

One minimal Tailwind v4 entry imports Tailwind, defines semantic @theme tokens linked to runtime CSS variables, and uses a supported @custom-variant for dark mode. Public and CMS reuse it. Only unavoidable rules belong outside utilities. CMS color settings accept validated hex values from approved roles, check relevant contrast pairs and produce typed CSS variables rather than arbitrary CSS.

Appearance precedence: explicit visitor choice (light/dark/system), otherwise site default; system follows matchMedia. A small CSP-compatible initialization script resolves appearance before paint. Store preference, listen for system changes in system mode, and provide accessible controls. Validate image overlays and dialogs in both themes.

Phase 1 explores two compositions: an asymmetrical botanical engineering folio with a large leaf/linework panel, and an architectural landscape composition with a vertical editorial rail. Neither is a finished design until reviewed.

Motion uses one requestAnimationFrame scheduler, IntersectionObserver visibility tracking, cached geometry and transform-based updates. Disable decorative motion for reduced-motion preferences; simplify mobile layers. No essential content depends on JavaScript or animation.

## Security, media and operational plan

Identity handles passwords and lockout; no public registration or seeded password. Bootstrap the first SuperAdmin using a one-time configured workflow. SuperAdmin controls users/settings/publishing, Editor edits drafts, Viewer reads authorized admin content; Editor publication permission can be revised before implementation. Enforce policies on the server, including every mutation and preview.

Use secure HttpOnly cookies, antiforgery protection, server validation, login rate limiting, HTTPS, CSP, output encoding and an allowlist HTML sanitizer. Select and review a minimal rich text editor dependency only if necessary; no external UI framework. Validate YouTube IDs and construct youtube-nocookie embed URLs; create the iframe on demand and remove it on close. The accessible modal traps focus, restores it, handles Escape/backdrop and supports previous/next.

Media storage sits behind an interface; local development storage is outside executable paths. Validate signatures, size/dimensions and allowed types, generate random names, strip metadata and create optimized variants. Reject active formats by default. Preserve assets referenced by drafts, published revisions and restorable history. Handle file/database consistency with staging and cleanup rather than assuming filesystem writes are transactional.

Secrets use development user-secrets and deployment environment variables/secret storage. Persist and protect Data Protection keys in production. Define backup/restore, structured logging without credentials, health checks and cache invalidation. A contact form is deferred unless a real delivery provider and spam workflow are configured; editable verified contact links remain required.

## Roadmap and approval gates

| Phase | Delivery and acceptance evidence |
| --- | --- |
| 0 | Discovery, architecture, data plan, risks and roadmap; this documentation |
| 1 | Two distinct hero compositions, bilingual responsive design rules, tokens, typography and controls; visual review |
| 2 | Solution, Tailwind build, DI, configuration, provider pinning and minimal migration; running page and authenticated MySQL query |
| 3 | Browser-reviewable public prototype, all sections, theme and basic bilingual layouts; visibly marked temporary content |
| 4 | Parallax, reveals, navigation, service interactions and accessible YouTube modal; motion/performance review |
| 5 | Full domain schema, relational translations, immutable revision foundation, media/audit/settings and migrations; database integration checks |
| 6 | Identity, permissions and bilingual themed responsive admin shell; login/authorization checks |
| 7 | Core content CRUD, ordering, translations and basic draft/publish; demonstrated CMS-to-public update and draft isolation |
| 8 | Media, optimization, SEO, appearance, logo/favicon, settings and audit UI; persistence and validation checks |
| 9 | Complete public/admin Arabic and English, RTL, localized URLs and SEO; visual and functional review |
| 10 | Preview, history, restore, archive and feasible scheduling; prove private changes cannot leak |
| 11 | Unit, real-MySQL integration and browser testing; responsive, accessibility, performance, security and SEO review |
| 12 | Deployment configuration, migrations, bootstrap, backup/restore, CMS guide and handover; no public deployment without explicit authorization |

Every row ends with explicit approval. Silence is not approval. Revisions remain in the current phase. ROLLBACK requires confirmation of the changes to discard. Maintain progress, decisions and changelog at each checkpoint.

Phase 2 contains only a minimal connectivity migration; full content migrations belong to Phase 5. Early localization/theme infrastructure is extended in Phase 9, not deferred entirely until then.

## Missing assets and information

Needed for design: actual logo files (prefer SVG and light/dark variants), favicon source, existing brand rules, image rights, preferred Arabic/English type direction and approved company name in Arabic. Use a clearly labeled logo placeholder until supplied.

Needed for publication: approved bilingual company description; confirmed service list and descriptions; authentic project names, descriptions, categories, covers, galleries, permissions and YouTube videos; verified contact email, telephone, WhatsApp, office address, map, business hours and social links. Do not invent these facts or treat suggested services as confirmed offerings.

Needed before foundation/deployment: dedicated development database name and least-privilege access supplied through secret configuration, target MySQL version, hosting OS/provider and .NET support, domain, TLS/reverse proxy, storage choice, backup destination and retention, administrator identity and recovery method. Contact form delivery details and privacy/consent text are required only if that feature is selected. Do not paste secrets into project documents.

## Risks and next actions

1. EF Core 10/MySQL provider compatibility is not established. Resolve before migrations; assess release stability, licensing and supported server matrix.
2. Native MySQL exists but authentication and exact version remain unverified. Use a dedicated development schema; do not modify unknown existing databases.
3. Missing company content prevents final publication, but approved placeholders permit design review.
4. Arabic, theme contrast and large motion compositions require early mobile/keyboard/reduced-motion checks, not just final QA.
5. Draft isolation, versioned relationships and media retention must be designed before CMS writes begin.
6. Hosting capabilities and asset rights remain unknown; these affect storage, font delivery and deployment planning.

Review can change module boundaries, fields, translation policy, roles, visual direction or phase acceptance criteria. After Phase 0 approval, proceed only to Phase 1. No runnable preview exists in Phase 0; review these Markdown files directly in the editor.

## Primary references reviewed

- [.NET support policy](https://dotnet.microsoft.com/en-us/platform/support/policy): .NET 10 LTS baseline.
- [EF Core providers](https://learn.microsoft.com/en-us/ef/core/providers/): match provider and EF major versions.
- [MySQL Connector/NET compatibility](https://dev.mysql.com/doc/connector-net/en/connector-net-entityframework-core.html): compatibility requires further release verification.
- [Tailwind theme variables](https://tailwindcss.com/docs/theme) and [dark mode](https://tailwindcss.com/docs/dark-mode): semantic tokens and supported custom variant strategy.
