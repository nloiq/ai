# Homepage Hero editing

2026-10-26: fixed the current Development design-preview homepage using fixed resource text instead of the published Hero section. It now reads the eligible published Section with key `hero` and subtype Hero in the requested language. No schema migration or replacement of the user's wording is involved.

Edit `/en/admin/content/2` or find Section / hero in Content workspace. Complete English/Arabic Title and Summary/Body as needed, keep **Visible** checked, save, preview and publish. The owner's exact published Arabic title now begins **هندسة**. The published Hero had Visible disabled; it was enabled and republished while retaining the owner's wording.

The live heading follows Title; introduction follows Summary and formatted extra text follows Body. Published media are rendered if selected, otherwise the original engineering illustration remains. Configured button labels/targets are used, with the original projects/about links as defaults. Invisible published Hero content is hidden. Draft/demo/unpublished records do not replace the design fallback, and private drafts do not leak to public readers.

A complete published CMS homepage continues to use its pinned section revisions: if that is enabled later, select/save/publish its desired Hero revision in the page composition. This fix connects the existing Development preview without silently publishing or replacing the entire home composition.

Verification: Release solution build zero warnings/errors; 47 rollback database scenarios pass, including published bilingual Hero selection, draft isolation, invalid culture and unpublication. The fixed six-section/four-service SQL fixture now isolates pre-existing published global lists inside its rollback transaction so user-added services do not change the workload; the unchanged 95-query budget passes at 80 commands. No user content is unpublished outside that rollback test. Public Arabic heading is checked against the owner's exact saved title; both languages pass four-width responsive checks and light/dark accessibility scans. Existing interaction/quality checks were rerun. Screenshots under ignored artifacts/hero-cms.

Current visual design: the architecture schematic is the main Hero visual; selected CMS images remain available under Supporting media. Default calls to action lead to Contact and Projects; explicit saved targets take precedence. See [public redesign](PUBLIC_REDESIGN.md).
