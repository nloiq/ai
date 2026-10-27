# Projects and Services directories

The homepage now shows compact Projects and Services indicators with a short CMS introduction and one link each. Full collections are on `/en/projects` and `/en/services`, with Arabic equivalents under `/ar/`. Header links also open these directories.

## Manage Projects

Open Administration > Projects. Create or edit a Project, complete both translations and valid slugs, select approved images and published project categories, then save and publish. Publish a category before publishing projects that depend on it. Category labels come from their published English/Arabic translations. Projects without public categories appear under General. A project assigned to multiple categories appears in each relevant group; the All count counts unique projects.

## Manage Services

Open Administration > Services. Each service has a Category dropdown: Low current engineering, Software solutions, SCADA system integration, System automation, or General. Save and publish to change its public grouping. Original service keys matching these four areas were assigned automatically; other services default to General. Service details use `/en/services/{content-key}` and the equivalent Arabic route. Changing the content key changes this URL.

Only categories containing public items appear in filters. Filters work without JavaScript and preserve their stable category key when switching language. Drafts, demo content and hidden services do not appear.

## Homepage introductions and images

Edit the published Sections with keys `projects` and `services` to change the homepage introductions. Keep Visible enabled, save and publish. For a published CMS homepage that pins sections, publish its updated section selection too. The homepage indicators contain no galleries or project study cards. Existing selected images on the Projects section remain editable and appear in the Projects directory's All categories gallery. Individual project covers and galleries still belong to each Project record.

## Installation and verification

Apply migration `20261027060754_ServiceCategories` before running this version. It adds the service category column and backfills only the four known original service keys. Back up the database before migration or downgrade; downgrade removes service category assignments. The local database was backed up and migrated. This change does not publish owner content or deploy a public website.

Verification: Release solution build with zero warnings/errors; 51 database rollback scenarios; five browser interaction tests; homepage smoke checks; 11 directory accessibility scans, 51 quality scans and 10 project-detail scans with zero violations; category filtering without JavaScript; English/Arabic, both themes and 320/390/768px layouts; EF reports no pending model changes.
