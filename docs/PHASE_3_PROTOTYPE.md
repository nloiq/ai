# Phase 3 — Public website prototype

Date: 2026-10-23 (Asia/Baghdad). Status: delivered for visual review; approval pending.

## Approval and scope

The user explicitly approved Phase 2. This phase extends selected Concept A into the public website prototype without implementing the CMS, full content schema, parallax or video lightbox. The prior foundation, MySQL service seed data, language routing and theme controls are preserved.

## Implemented

- Sticky branded navigation with About, Projects, Expertise and Contact anchors, language switching, and light/dark/system selector.
- Responsive menu with native button, aria-controls/aria-expanded, breakpoint state reset, Escape handling and focus transfer to selected sections. Navigation remains visible if JavaScript is unavailable.
- Editorial engineering-folio hero, conceptual systems architecture illustration and work/about CTAs.
- Asymmetrical About section with subtle branch-to-pathway artwork and bilingual introductory copy.
- Staggered portfolio composition with three local engineering illustrations in different proportions, caption/description treatments, lazy loading and explicit image dimensions.
- Four expandable database-backed services with consistent line icons and an open-state indicator.
- Contact composition with clearly marked pending email, telephone/WhatsApp, office/map and hours slots.
- Full footer with company introduction, section links, contact status, logo placeholder and dynamic copyright year.
- Shared semantic Tailwind v4 themes, logical spacing and English/Arabic resources throughout the new sections.

The deep evergreen Contact/footer remains a deliberate brand composition in both appearance modes; other surfaces and controls follow semantic theme tokens. Sections use scroll margins for sticky navigation. Reduced-motion rules from the approved foundation remain active.

## Content boundaries

The four specialty categories are company-confirmed. Service descriptions remain labeled demo copy. About text is marked proposed company copy. Portfolio artwork and captions explicitly identify concept studies, not completed client projects. There are no invented client names, project achievements, contact details, certifications or statistics.

The portfolio intentionally has no fake YouTube playback, made-up video IDs, or nonfunctional project buttons. Real project media and videos remain needed. Project lightbox behavior is Phase 4. Contact has no inquiry form or fabricated mailto/tel links; verified company details must be supplied before those links can become functional.

Public preview content still uses the development-only route guard established in Phase 2. Only the service list is read from MySQL at this stage. Other prototype copy is in resources/Razor, to be moved into revision-backed CMS content in later approved phases. The site is not represented as dynamically managed yet.

## Preview and run

- English: http://127.0.0.1:5080/en/
- Arabic: http://127.0.0.1:5080/ar/

The application was rebuilt and restarted for review. If it stops, from the repository root run:

```powershell
npm.cmd run css:build
dotnet build NLOIQ.sln
dotnet run --project src/NLOIQ.Web --no-build
```

Keep the last command running while reviewing. Database setup remains in LOCAL_SETUP.md; existing seed data was preserved and no migration was added in Phase 3.

Review at 1440, 1024, 768, 390 and 320px. Test both themes and system preference, switch languages, open/close the mobile menu, follow section links, expand each service, and inspect the complete footer. Use keyboard navigation, including Escape on an open mobile menu. These are review instructions, not claims of completed browser testing.

## Files

- src/NLOIQ.Web/Views/Home/Index.cshtml: document, navigation, section composition and footer.
- src/NLOIQ.Web/Views/Shared/Sections/: reusable Hero, About, Projects, Services and Contact Razor partials.
- src/NLOIQ.Web/Resources/SharedResource.en.resx and .ar.resx: 68 matching localized interface/content keys.
- src/NLOIQ.Web/wwwroot/images/: network-study.svg, scada-study.svg and automation-study.svg, source-native illustrations.
- src/NLOIQ.Web/wwwroot/js/app.js: preserved theme behavior and responsive menu/focus handling.
- src/NLOIQ.Web/wwwroot/css/app.css: recompiled Tailwind output.
- tests/prototype-smoke.mjs: full-prototype HTTP checks extending foundation checks.

## Actual checks and limitations

Passed: .NET build with zero warnings/errors; Tailwind build; JavaScript syntax; real HTTP smoke checks for both cultures; four database-backed specialties in each language; one h1 and three portfolio articles per page; all section anchors; menu reference attributes; lazy image dimensions/alt attributes; local SVG delivery; configuration-file and invalid-culture 404 responses; root redirect; matching resource keys with no duplicates; no fabricated contact URLs or early video iframes.

To repeat with the app running:

```powershell
node tests/prototype-smoke.mjs
```

Browser runtime initialization failed again. No screenshot, real browser layout inspection, interactive menu/theme verification or measured responsive/accessibility audit was produced. HTTP and source checks cannot prove those behaviors. This remains an explicit verification limit for user review.

## Next review gate

Approve or request changes to composition, typography, palette, spacing, image proportions, Arabic flow, menu layout, or temporary copy. Missing logo, real project photography/videos and verified contacts can be supplied for later replacement. Phase 4 begins only after explicit approval; it implements parallax and interaction work. Full CMS/publishing remains later phases.
