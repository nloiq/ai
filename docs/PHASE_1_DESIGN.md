# Phase 1 — Creative direction and design system

Date: 2026-10-23 (Asia/Baghdad).
Status: delivered for design review; Phase 1 approval pending.
Phase 0 was explicitly approved by the user. No Phase 2 application work has started.

## Review the two concepts

Open http://127.0.0.1:4173 while the preview server is running, or open docs/design/index.html directly in a browser. All preview assets are local; no CDN or font service is required. The committed preview.css is already compiled.

To restart from the repository root:

```powershell
node docs/design/serve.mjs
```

To rebuild after design changes:

```powershell
npm.cmd ci --prefix docs/design/tooling
node docs/design/tooling/node_modules/@tailwindcss/cli/dist/index.mjs -i docs/design/input.css -o docs/design/preview.css --minify
```

Use the two concept buttons, Appearance selector, and Arabic/English switch. Resize to 1440, 1024, 768, 390 and 320 pixels. Check headings, leaf/landscape composition, navigation wrapping, and the service disclosure. Use Tab to inspect focus and Enter/Space to operate native controls.

This is a local design review, not the public website or CMS. No public deployment was performed. Exact browser renderings and screenshots have not been verified because the computer-use browser runtime failed to start. HTTP readiness does not establish visual correctness.

## Concept A — Botanical folio

A warm editorial composition combines large, left-aligned English typography and an offset botanical plate. A leaf silhouette turns into orthogonal technical pathways; grid marks and copper detailing connect organic structure with engineered order. Arabic reverses reading flow using document direction and logical spacing while preserving the leaf's physical shape.

Below the hero, a ruled introduction leads into a portfolio study beside a numbered expandable service row. Space, image proportions and alignment establish the story instead of repeated rounded cards. This is the recommended starting point for the distinctive botanical identity in the brief.

## Concept B — Autumn field

An architectural composition uses a deep evergreen canvas, a narrow brand rail, an expansive two-column headline/deck, and a panoramic autumn illustration below. The reading sequence is horizontal and landscape-led, rather than the folio's vertical botanical plate. The rail becomes a horizontal brand strip on smaller screens.

The illustrated landscape is an art-direction study, not a photograph of an NLO IQ location. Gold pathways connect the terrain to the engineering language. The lower portfolio uses a wide image area and adjacent caption. The evergreen hero is an intentional branded composition in both themes; surrounding surfaces and controls follow the selected theme.

## Shared visual foundation

Brand swatches: Deep Evergreen #14271F, Forest Green #203D32, Moss #687C50, Burnt Orange #C56A3D, Copper #A65E40, Autumn Gold #D8AD64, Warm Ivory #F3EFE5 and Charcoal #222722.

Semantic runtime tokens define paper/background, ink/text, muted text, line/border, panel/surface, accent and on-accent. Tailwind v4 @theme inline binds them to utilities; @custom-variant supports the dark selector. Both eventual public and admin shells will reuse these roles.

Light: ivory background, evergreen text, pale botanical panel, muted green secondary copy. Dark: evergreen background, ivory text, forest panel, pale moss secondary copy. Copper and gold are restrained technical accents.

Burnt Orange remains the brand swatch, but small-text CTAs use a darker orange #884125 in light mode to preserve contrast with ivory. Dark CTAs use gold with evergreen text. The palette is not permission to put any brand color on any background.

Measured WCAG relative luminance ratios:

| Pair | Contrast |
| --- | --- |
| Evergreen text / ivory | 13.65:1 |
| Light muted text / ivory | 5.54:1 |
| Ivory / accessible orange CTA | 6.46:1 |
| Dark muted text / evergreen | 9.00:1 |
| Evergreen / gold CTA | 7.52:1 |
| Light focus / ivory | 6.46:1 |
| Dark focus / evergreen | 7.52:1 |

These measurements cover the listed token pairs only, not a complete accessibility audit or imagery/overlay proof.

### Typography and spacing

English display: Georgia with Times New Roman/serif fallbacks. English interface and body: system sans serif. Arabic: Tahoma with Arial/sans serif fallback, looser leading, no artificial letter spacing. This intentionally requires no remote font; an approved licensed Arabic/Latin family can replace it before application implementation. Rendering still needs review on the target devices.

Display type scales with clamp, starting around 48–50px on small screens and rising to roughly 108–112px. Body is 16px with approximately 28px leading. Labels are 10–12px and reserved for secondary annotations. Arabic display uses a 1.5 line height and removes tight tracking.

Spacing scale: 4, 8, 12, 16, 24, 32, 48, 64 and 96px. Content maximum: 1500px. Mobile horizontal padding: 20px; larger layouts: 40px. Use square edges, fine horizontal rules and a restrained hierarchy. No dashboard mockup or generic service-card grid.

### Controls and navigation

Primary actions use solid accessible accent; secondary actions use an underline or border. All key controls have minimum 44–48px height, visible 3px focus outlines, native keyboard behavior and explicit labels. The sample primary action produces local feedback; the disabled example is clearly unavailable. Concept selectors expose aria-pressed. Theme defaults to system, follows system changes, and persists explicit choices. A small head initializer resolves theme before styles paint.

The design preview navigation wraps for mobile. Phase 3 will implement the production mobile menu, sticky/scrolling header, language-aware routes and full footer. The displayed NLO IQ wordmark is explicitly a placeholder, not an invented company logo.

### Image direction

Prefer rights-cleared macro leaf veins, autumn canopy light, architectural detail and genuine engineering project photography. Use natural textures and restrained overlays; do not use stock project images as purported company achievements. Current SVG leaf and landscape studies are source-native concept illustrations. They can remain illustrative brand assets or be replaced with approved photography during the prototype phase.

Project placeholders contain no invented project names, metrics or client identities. Suggested low-current service scope is marked unconfirmed. Contact fields remain pending verified company details.

### Responsive and bilingual composition

Below 1024px, hero columns stack and the architectural rail becomes horizontal. Smaller screens wrap review controls and navigation instead of truncating them. Palette swatches use two columns on phones and four at larger widths. Each concept has a single visible h1.

Arabic changes html lang/dir, navigation flow, column order, logical margins, button arrows, line height and heading rhythm. Landscape/leaf shapes retain physical direction rather than mechanically mirroring every asset. Brand names, hex codes and technical annotations remain as brand/reference material. Arabic text in this review is proposed copy requiring company language review; the CMS's complete localization is a later phase.

### Motion specification — planned, not implemented

Hero leaf: slow vertical offset up to 24px; technical pathways move up to 10px. Field landscape: separate distant/near layers with up to 12px/24px offsets. About illustration: up to 16px; portfolio imagery: a subtle 1.02 scale within its crop. Reveal: 12px translation and short opacity transition; service rows expand without decorative bounce.

Use one requestAnimationFrame loop and IntersectionObserver activation. Keep text static, cache measurements, avoid scroll-time layout reads and stop off-screen work. At mobile widths simplify to one shallow layer or disable. Under prefers-reduced-motion, remove decorative movement and smooth scrolling. Phase 4 implements and measures motion; current preview does not claim live parallax.

## Files and checks

- docs/design/index.html: two compositions and shared component/palette samples.
- docs/design/input.css: minimal Tailwind v4 entry, semantic tokens, Arabic/reduced-motion rules.
- docs/design/preview.css: compiled review styles.
- docs/design/preview.js: local concept, theme, language and sample-control interactions.
- docs/design/serve.mjs: loopback-only server with an explicit three-file allowlist.
- docs/design/tooling/package.json and package-lock.json: pinned Tailwind 4.3.3 review tooling. These are isolated design dependencies; no ASP.NET solution was scaffolded.

Executed successfully: Tailwind compilation; node --check for both JavaScript files; HTTP 200 for HTML/CSS/JS; local credential URL returned 404; unique HTML IDs; local anchor targets; mathematical contrast checks above. No application build, database operation, browser screenshot, browser interaction test or full responsive visual inspection was performed. Browser runtime failure is the remaining verification limitation.

## Review choices and remaining work

Choose A, B, or a combination; revise typography, autumn warmth, artwork direction, text scale, project proportions, navigation, Arabic spacing or planned motion intensity. Approval should identify the preferred composition so Phase 2 can carry a concrete design decision forward. Unselected concepts remain in version control as review history.

Phase 1 design deliverables are prepared. Final logo, licensed imagery, approved bilingual company content, verified service/project/contact data and browser visual review remain outstanding. Phase 2 starts only after explicit Phase 1 approval and establishes the ASP.NET/MySQL foundation; the full public prototype belongs to Phase 3.
