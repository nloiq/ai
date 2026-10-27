# Public website redesign

Implemented 27 October 2026. Review `/en/`, `/ar/`, `/en/projects`, `/en/services` and `/en/contact`, including the project/service detail links. Arabic equivalents are available for all public routes. No public-host deployment was performed.

## Presentation

The public site uses one shared header/footer, a 1,280px grid, Manrope for English and existing Cairo for Arabic, a graphite/blue/cyan palette, responsive category cards and a technical SVG architecture visual. One-item directories use a horizontal feature layout on desktop, stacking on mobile. Category filtering remains server rendered and works without JavaScript. A local font and static SVG avoid third-party font requests or a 3D runtime. Reduced motion is honored; no fake live telemetry or client metrics were added.

Homepage order is Hero, compact Services, compact Projects, short About/approach and Contact invitation. The full inquiry form and business hours are on Contact. The homepage does not list all projects or services. Published CMS text remains authoritative, so existing sample copy and unrelated owner-selected images still require editorial replacement before launch.

The Contact page retains antiforgery, invalid-post value preservation, rate limiting, consent, inbox persistence and existing business-hours data. Unknown hours are not invented. Project full-size galleries, captions, private previews, keyboard behavior and media permissions are retained.

## Editing

- **Appearance** (`/en/admin/settings/appearance`): colors, logo border/radius/height and theme selector style. The approved palette was saved and published through this workflow, retaining the old revision for restoration. Public colors inherit these tokens rather than bypassing the CMS. The header fits uploaded logos into responsive bounds; use a transparent, high-resolution official logo in Site settings.
- **Site settings**: company name, logo, favicon, default theme/language and approved social links. The public shell displays the same identity across page types.
- **Section / hero**: Title, Summary, Body and both CTA labels/targets. The architecture composition is the primary design visual; existing selected images remain under Supporting media. Save, preview and publish.
- **Section / about**: the published bilingual Title, Summary and Body replace the default approach text on the current development homepage. Visible controls display. A complete published CMS homepage retains its pinned About revision.
- **Sections / services and projects**: titles and summaries drive the compact homepage invitations and directory introductions. The four service-area labels are localized interface copy; individual service assignments remain editable.
- **Projects**: categories, localized slugs, cover/gallery order, alt text/captions, Summary, Body and video remain editable. Use Body for the actual challenge, solution, architecture and verified outcomes. Begin each paragraph group with a standalone bold label, such as `**Challenge**`; the public story typography emphasizes these labels. No new factual fields or invented outcomes were populated automatically.
- **Services**: category, Title, Summary, Body and images remain editable. Body should explain actual deliverables, applications and technical approach. Generic software/control schematic artwork is used when no cover is supplied; it is decorative, not a claimed project image.
- **Contact / primary**: edits both the short homepage invitation and the full Contact heading/body, contact details and seven-day business hours.
- **Footer / main and navigation**: complete published CMS pages use their published footer/navigation snapshots through the shared components. The development fallback homepage and standalone directories use common brand-based navigation/footer defaults. Publish the complete home Page composition to use its full footer/navigation selection. Existing footer summary/body/social/copyright values remain supported.

Existing revision/publish guards are unchanged. No schema migration was required. Drafts/demo records remain excluded. Section-selected Projects gallery images remain in a collapsed collection on the Projects directory so existing uploads were not lost or silently reassigned to unrelated project records.

## Palette deployment

On another database, publish these values through Appearance to match the reviewed local installation: Evergreen `#080F1D`, Forest `#4D5D75`, Moss `#708098`, BurntOrange/Copper/LightAction `#2259D2`, Gold `#477FFF`, Ivory `#F5F7FC`, Charcoal `#111D30`, SignalLight `#08756A`, SignalDark `#59E0D1`. Historical CMS field names are retained for compatibility. The source fallback palette matches this direction. Publishing palette settings also updates the existing admin previews.

Manrope is hosted under `wwwroot/fonts/manrope` with its original SIL Open Font License. Source: https://github.com/google/fonts/tree/main/ofl/manrope . Existing Cairo hosting is unchanged.

## Verification

Release solution build: zero warnings/errors. Five interaction tests passed, including mobile navigation, theme persistence, static architecture with reduced motion and idle/coalesced motion scheduling. Ten project-detail accessibility scans passed, including authenticated previews and anonymous media denial. The existing 51-scan quality suite passed with zero violations. The new `node tests/public-redesign-review.cjs` covers 20 English/Arabic light/dark scans across Home, Projects, Services, Project detail and Contact, responsive widths 320/390/768px, shared shell, visible image loading, inquiry validation/antiforgery and local font loading. Directory checks passed with category-language persistence and no-JS filters. Database checks include published About projection, draft isolation, visibility and unpublication in rollback transactions; all verification transactions rolled back. Homepage smoke checks passed.

Screenshots are local ignored artifacts under `artifacts/redesign`. These checks validate the delivered interface; they do not establish field Core Web Vitals or certify a complete content launch.
