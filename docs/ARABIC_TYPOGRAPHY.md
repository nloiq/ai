# Arabic typography

2026-10-25: Cairo selected explicitly by the user, replacing IBM Plex Sans Arabic across the public site and admin panel. Arabic headings use weight 600, with 400/500/600/700 available. English editorial typography remains unchanged. Natural Arabic tracking and comfortable existing line heights are preserved.

Original Google Fonts v31 variable WOFF2 Arabic/Latin subsets are hosted locally. Each subset file is reused across weights; font-display: swap keeps text visible while loading. No external request or CSP change is required. License and provenance accompany the fonts under wwwroot/fonts/cairo.

Source: https://fonts.google.com/specimen/Cairo
Stylesheet: https://fonts.googleapis.com/css2?family=Cairo:wght@400;500;600;700&display=swap

Verification: rebuilt minified CSS; checked actual Cairo loading in Edge on Arabic home, login and signed-in CMS at desktop/mobile widths, font responses and horizontal overflow. Screenshots under ignored artifacts/cairo-font. No database/content changes.
