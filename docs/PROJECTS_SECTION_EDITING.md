# Projects section images

2026-10-26: connected the current Development preview homepage's Projects section to eligible published Section key `projects`, subtype Projects (currently content ID 4). Previously publication succeeded but the homepage still used fixed prototype headings/images.

The published section now supplies its heading, summary, formatted body, visibility and all selected images with localized alt text/captions and responsive variants. The uploaded section gallery appears above the existing three original project studies. The study cards, YouTube/lightbox behavior and their illustration labels are retained. Selecting an image in this section does not change the header logo; use Site settings for that.

Workflow: edit `/en/admin/content/4` or the section found in Content workspace; choose images with unique roles (e.g. cover, cover2), complete both alt-text languages, keep Visible checked, save, preview, verify and publish. Blank captions are optional. The section's saved draft does not replace public images until publication. No user text or image selections were overwritten in this fix.

A complete published CMS homepage continues using its pinned section revisions, as documented in the CMS guide. This connection makes section edits visible on the current Development preview without silently publishing/replacing the whole home composition.

Verification: Release solution build zero warnings/errors. 48 rollback database scenarios pass, including section image publication, bilingual text/alt snapshots, draft isolation, unsupported culture and unpublication. Actual public asset URLs for IDs 76 and 28 return image responses with status 200 in both language views. Four light/dark accessibility scans pass, mobile does not overflow, and the five existing interaction tests pass. Screenshots under ignored artifacts/projects-cms. No schema migration.
