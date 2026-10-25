# Arabic typography update

2026-10-25: user requested a Google Font for Arabic. Replaced Tahoma with IBM Plex Sans Arabic across public pages and CMS, retaining system fallbacks. Arabic headings use weight 600; regular/medium/semibold/bold webfonts are available. Natural joining, tracking and existing comfortable line heights are preserved. English editorial typography is unchanged.

Original Google Fonts v15 WOFF2 Arabic/Latin subsets (400/500/600/700) are served locally, totaling 257,368 bytes across eight files; browsers request the subsets/weights they use. font-display: swap keeps text visible during loading. No external stylesheet, font request, CSP relaxation or runtime dependency is needed. License and provenance are distributed beside the font assets.

Source: https://fonts.google.com/specimen/IBM+Plex+Sans+Arabic
Stylesheet: https://fonts.googleapis.com/css2?family=IBM+Plex+Sans+Arabic:wght@400;500;600;700&display=swap
License: src/NLOIQ.Web/wwwroot/fonts/ibm-plex-sans-arabic/OFL.txt

Verification: Tailwind minified build passes. Real Edge checks verify document.fonts readiness and family selection on Arabic home (1440/390), login (390), signed-in CMS content (1440/390), all font responses 200, no horizontal overflow, no page errors, and unchanged English heading family. Desktop/mobile home and CMS screenshots reviewed under ignored artifacts/arabic-font. No database or content change.
