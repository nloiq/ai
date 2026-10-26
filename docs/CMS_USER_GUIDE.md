# CMS user guide

Open `/en/account/login` or `/ar/account/login` on your configured HTTPS site. Use your own administrator credentials. The local development login file is not a production account. Five failed attempts lock an account for 15 minutes; login is also rate-limited. Sign out using the provided button. There is no public account signup or self-service password reset.

## Permissions and navigation

SuperAdmin controls publication, scheduling, site/appearance settings and audit history. Editor can create/edit ordinary content, upload media, preview history and restore a revision into a draft. Viewer currently sees the dashboard only. Account/role administration is an operator responsibility; no user-management screen is provided.

The language switch changes the editing interface. English and Arabic content fields are separate: changing the interface does not translate text. `/admin/content` lists content with search/type/status/changes filters. `/admin/translations` highlights missing language fields. `/admin/seo` lists page/project SEO editors; `/admin/media` opens the media library. Prefix these routes with `/en` or `/ar`.

## Create and publish content

1. Create a content item with the correct type. Give it a stable unique key. For the actual homepage, use Page key **`home`**; other page/project slugs define their public routes. Do not rename a stable key casually. Demo items must be deliberately reviewed and marked non-demo before they can be public.
2. Fill both language tabs, including each language's title, text, slug and SEO. Use the supported formatted-text controls; HTML/script is not accepted as arbitrary content. Use approved company facts. Save creates an immutable working revision; it does not replace what visitors see.
3. Assemble services, projects and sections, then add section references to the page. Use Add/Move/Remove controls for rows. Sections can reference eligible content revisions; public dependencies must be published and non-demo. Publish children first, then save/publish the parent against the desired versions. Publishing a child alone does not update a parent's pinned snapshot.
4. Open the saved draft preview or its history preview while signed in. These links are private and excluded from SEO. Review both languages, mobile layout, image alt text, links, theme contrast and project media.
5. SuperAdmin publishes the saved revision. A conflicting route, stale browser version, missing dependency or invalid field blocks publication. Read the displayed error; do not repeatedly overwrite another editor's work. Reload the latest revision and reapply your intended changes if a stale-version conflict appears.
6. Open the public link in a signed-out session. The homepage is `/en/` and `/ar/`; ordinary pages use `/en/pages/SLUG`, projects `/en/projects/SLUG`, with Arabic equivalents. In Production there is no design-preview fallback for a missing homepage.

Unpublish removes public eligibility; archive retires the root. Review parents before retiring referenced content. Restoration from history creates a **new draft** copied from that revision, preserving history and leaving the public version unchanged until publication. This is distinct from database disaster recovery.

## Schedule publication

SuperAdmin chooses a future publication time from the saved draft's history/workflow view. The form uses **Asia/Baghdad** time; schedules are stored in UTC. A schedule pins that revision. Saving/restoring another draft or manual publication invalidates/cancels pending work. Cancel/reschedule explicitly when plans change.

The worker checks about every 15 seconds while the app is running; this is not a precise-time external scheduler. The scheduling account must still be active, unlocked and a SuperAdmin at execution. Conflicting routes or changed dependencies can fail a job. Inspect its status/error and audit history; fix the cause and create a new schedule. Do not leave old test schedules pending.

## Media, video and SEO

Upload a supported static image through Media. Processing validates size/type, strips metadata and creates bounded variants. Add meaningful English and Arabic alt text for each use. Public images are authorized through published content references; private storage is not a public folder. Historical usages can prevent deletion, intentionally preserving revisions.

A project's YouTube field accepts the **11-character video ID**, e.g. `NNiszX0-A7M`, rather than an iframe or full URL. The lightbox can contain a video and image entries; Next moves through those entries. Only use videos/images you have approved for the site.

Set language-specific SEO title/description, optional sharing text/image and noindex where appropriate. Review canonical and alternate-language URLs after publication. Site settings contain the canonical public HTTPS origin, default language/theme, logo and favicon; appearance contains the validated palette. Save and publish settings to expose changes. Review contrast after changing colors.

Before launch, replace all demonstration text/assets with approved company material for low-current engineering, software solutions, SCADA integration and system automation. Check navigation/contact links and both home compositions. Keep your backup operator informed before bulk edits.

## Logo frame and theme selector

Open Appearance and scroll to Header appearance to adjust logo border/color/corners/height and choose a dropdown or segmented theme selector with rounded corners. Review the live light/dark samples, save the draft and publish after verification. See [Header appearance controls](HEADER_APPEARANCE.md). The logo image itself is selected in Site settings.

## Homepage contact section

Use **Contact section** in the sidebar to edit the bilingual heading, optional gold heading line, small label, introduction and extra text, followed by email/phone/WhatsApp/address/map, hours and social rows. Save, open Saved revision preview, then verify and publish. This updates the contact section on the current homepage. See [Contact section guide](CONTACT_SECTION.md).

## Contact form messages and business hours

The public contact form saves messages to **Contact inbox** in the sidebar. SuperAdmin/Editor can open a message and mark it read. Email notifications are not configured. Edit the seven-day schedule in **Contact section → Business hours**, then save and publish; unconfigured days display Not provided. See [Contact form guide](CONTACT_FORM.md).

## Compact content table

The Content workspace now uses a table showing ID, localized title (or content key if untranslated), key, type/section subtype, publication state, draft revision, pending changes, demo/standard status and Edit/Preview/History actions. Existing search/type/status/change filters remain available. On narrow screens scroll the table horizontally; its region is keyboard-focusable. No content or publication state changes when switching this layout.

## Editing the current homepage Hero

Edit Section key **hero**, complete both language titles and keep **Visible** checked, then save and publish. Its published text now drives the current preview homepage heading. See [Hero editing](HERO_EDITING.md) for fields, fallback behavior and pinned CMS-page revisions.

## Projects section gallery on the current homepage

Section key **projects** now supplies the current preview homepage’s Projects heading/text and selected images. Use unique image roles, both alt-text languages and Visible, then save and publish. The uploaded gallery appears above the existing project studies. See [Projects section editing](PROJECTS_SECTION_EDITING.md).
