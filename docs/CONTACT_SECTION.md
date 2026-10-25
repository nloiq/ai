# Editing the homepage contact section

Added 2026-10-25 at the owner's request. Open **Contact section / قسم التواصل** in the admin sidebar, or `/en/admin/content/contact` (`/ar/admin/content/contact`). This opens the existing Contact record with stable key `primary`; it does not create another independent contact record.

## Edit and publish

1. Complete both language tabs. **Title** is the main heading, **Summary** is the introduction and **Body** is extra formatted text. **Small section label** controls the eyebrow (such as “04 / Start a conversation”); **Highlighted heading line** is the optional gold second line.
2. Add contact rows for email, phone, WhatsApp, address and map. Value is the actual email/number/map URL; English/Arabic labels and optional localized display values control what visitors read. Use complete verified company details. Add business hours and supported social links using their row controls.
3. Click **Save draft**, then **Saved revision preview**. Check both languages. The preview uses the same contact section layout as the public view. Unsaved form text is not included in a saved preview.
4. As SuperAdmin, tick the publication verification checkbox and choose **Publish saved draft**. Editors can save/preview but cannot publish. Refresh the homepage and scroll to `#contact`.

The contact information record supplies all heading and detail text for this section. The generic Section record named `contact` still controls where a contact block is composed in a published CMS page; it is not the source of these contact words. On the existing Development design-preview homepage, publishing Contact `primary` updates the section without requiring publication/replacement of the whole homepage. On a published CMS page containing a contact section, the same contact component is used.

Your previous contact draft/content has not been overwritten or automatically published. Replace its placeholder text with company-approved information before publishing. When no eligible published non-demo contact record exists, the Development homepage retains its explicit awaiting-details design preview. Unpublishing removes the managed contact information from public readers. The navigation text “Let’s connect” remains a separate navigation/localization concern; this change edits the contact section itself.

## Implementation and verification

No schema migration is needed. Existing immutable contact translations retain the label/highlight using their optional text slots; existing ContactDetail/ContactTranslation, BusinessHours and SocialLink snapshots retain structured data. All values follow existing validation, publication, safe-link and formatted-text rules. The standalone contact reader only selects eligible published non-demo revisions in supported cultures, and private previews remain authenticated/noindex/no-store.

Release solution build passes with zero warnings/errors. 46 rollback database scenarios pass, including independent contact draft isolation, exact bilingual publication, actionable email/WhatsApp links, hours/social data, invalid culture and unpublish privacy. All five existing browser interaction tests pass. The quality runner reports 51 automated scans with no violations. `node tests/contact-review.cjs` adds four accessibility scans and checks real bilingual sidebar/editor fields, row controls, saved-preview isolation, anonymous denial and mobile overflow without saving or publishing over existing content. Screenshots are under ignored artifacts/contact-review. These are local Edge checks, not an accessibility certification or a live-production deployment.
