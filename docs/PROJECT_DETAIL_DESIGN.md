# Professional project details and image gallery

2026-10-27: added a dedicated Project presentation shared by public details and authenticated revision previews. Existing project titles, summaries, formatted bodies, categories, video IDs and image selections remain CMS-controlled; no project content or publication state was changed for this redesign.

## Presentation

- Clear Back to projects link, portfolio heading, large localized title, summary and category chips.
- Framed responsive cover viewer, uncropped thumbnail navigation, selected-image indicator, localized captions/counter and a full-size native dialog.
- A project overview uses the existing formatted body. A compact sidebar shows actual categories/image count and a contact action; no client, completion date or other company facts are invented.
- A supplied video appears in its own titled section. The gallery navigates images, independently of video playback.
- All surfaces, text, borders and actions use the published light/dark palette, and Arabic keeps the Cairo font/RTL layout.

The image with role `cover` is ordered first by the existing public reader. Add other images with unique lowercase roles (gallery-1, gallery-2), both alt-text languages and optional captions. Save, preview and publish in the existing Project editor. Public pages continue to use the published revision only; working drafts stay private.

## Interaction and security

Click a thumbnail or use arrow/Home/End keys while a thumbnail has focus. Click View full-size image to open the dialog; use Previous/Next or arrow keys, Escape, Close or the outside backdrop. The dialog confines keyboard focus, restores focus to its opener and releases the background scroll lock. Full-image source data is cleared when closing. Without JavaScript, the cover and thumbnail links still open their images directly.

The new external project-gallery.js uses DOM text/attribute updates and needs no inline script/style exception or new dependency. Preview image URLs retain existing authentication/authorization checks. Other page types and the homepage study lightbox keep their existing presentation. SEO/canonical/hreflang and private-preview headers remain in the shared layout. No migration.

## Verification

Release solution/Web builds pass with zero warnings/errors. All five existing interaction tests and the 51-scan quality runner pass. `node tests/project-detail-review.cjs` performs ten additional accessibility scans covering public views, open dialogs and private previews in both languages; it checks thumbnail selection, wraparound, focus trapping/return, Escape/backdrop cleanup, responsive widths 320/390/768, anonymous preview-image denial and no-JavaScript fallback. It performs no content writes. Images/captions/text are verified against current Project 977. Screenshots are ignored under artifacts/project-detail. Local Edge checks are not a full accessibility certification or production-host validation.
