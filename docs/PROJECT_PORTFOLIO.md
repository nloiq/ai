# Adding Projects and showing them on the homepage

2026-10-26: the owner's `project-fnished` item was created as a Page, so publication required its selected page sections to be verified and published. Projects have a different content type and do not require page sections.

The owner's bilingual text, slugs, SEO and image selection were copied from Page 976 into Project 977. At the initial copy, Page 976 was retained as a Draft at revision 2. Project 977 is published; its public routes are `/en/projects/projectfinished` and `/ar/projects/projectfinished`. No source page was deleted or converted in place. Self-referential page canonical paths, if present, were adapted to the corresponding project path.

## Administration

Use the **Projects** sidebar link or `/en/admin/content?kind=Project`, then **+ Projects**. Complete English/Arabic titles and slugs, descriptions, approved images with both alt texts, optional video ID/category/order, then save, preview, verify and publish. Use Pages for compositions assembled from published sections. Editors now display short type-specific guidance to make this distinction clear.

The current Development preview homepage reads eligible published Project records and displays a Project portfolio above the section's manually selected image gallery. Each project card links to its own details page, which shows its content/images and optional video. The three original illustration/video studies remain separately identified as Concept studies; these are not represented as company projects. Published records are ordered by DisplayOrder then ID. Draft/demo/unpublished Projects and unsupported cultures are excluded.

A complete published CMS homepage continues using its existing composition/project reader. The new standalone portfolio reader connects the current preview without silently publishing the entire home page. The Projects section's Visible control continues to govern visibility of that area.

## Verification

Release solution build passes with zero warnings/errors. 49 rollback database scenarios pass, including portfolio root-type filtering, bilingual title/slug selection, publication/unpublication and working-draft privacy. Actual Project 977 is checked on both homepages and both details routes, its cover image loads, and the source Page draft remains intact. Four light/dark accessibility scans and mobile overflow checks pass. Existing five interaction tests pass. No schema migration. Screenshots under ignored artifacts/project-portfolio.

## Updating the correct item

After creation, further updates were entered into the retained Page draft (976), so its page-section dependency error correctly remained. The latest English body edit from that draft was copied into Project 977 and published while preserving other project fields. Continue editing Project 977. Page editors now detect an active Project with the same key and show a prominent translated notice with an Open Project editor link; the notice does not rewrite, delete or silently redirect legitimate Page content. Release Web build and two focused bilingual accessibility/navigation checks pass.
