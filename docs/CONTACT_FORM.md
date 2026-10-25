# Contact form and inbox

Implemented 2026-10-25. The homepage contact section now contains a professional inquiry form alongside a seven-day business-hours card and existing contact/social links. English and Arabic/RTL layouts use the site's ivory, evergreen and gold palette. The remaining seeded heading was changed to Contact us / تواصل معنا; existing email, social links and Sunday hours were preserved.

## Administration

- **Contact section**: `/en/admin/content/contact` (Arabic `/ar/admin/content/contact`). Edit the published heading/summary/body and bilingual contact details/social links. Business hours are rows near the bottom: add one per day, choose Open/Closed, then opening/closing time. Save, preview, verify and publish.
- The hours card shows all seven days. Only configured days show opening times or Closed; missing days say Not provided. All times are Baghdad (UTC+3), with numeric ranges isolated left-to-right in Arabic. No additional company opening hours were invented.
- **Contact inbox**: `/en/admin/inquiries` (Arabic mirror). SuperAdmin and Editor can list/open inquiries and mark them read. Viewer and anonymous visitors cannot access the inbox. Rows paginate in groups of 25, unread first and then newest first. Read status changes require POST and antiforgery protection.
- Form submissions are stored in the private MySQL inbox. **No email notification, email delivery, attachment upload or auto-reply is configured.** Staff must check the inbox. The submitted email is available to copy into an approved email client; no external message is sent by the site.

## Visitor behavior

The form collects name, email, optional company/phone, an interest area (general, low-current, software, SCADA, automation), a 10–4,000 character message and permission to use the details to respond. A successful save redirects to a localized confirmation. Invalid requests return the same editable form with errors and status 400; the server keeps entered values. A separate `/en/contact` or `/ar/contact` page handles validation and standalone access. Neither route accepts submissions when Contact primary is unpublished/demo/ineligible.

Saved revision previews display the form disabled. They do not submit inquiries. The form is enabled only in the public published contact view. Its use of antiforgery tokens requires cookies and excludes pages containing the form from shared HTML caching; never add a CDN rule that caches another visitor's token-bearing page.

## Persistence and protections

Additive migration `20261025082554_ContactInquiries` creates ContactInquiries with a unique SubmissionId and an unread/time index. Run the normal reviewed migration procedure before deploying this code elsewhere. A local private SQL backup was taken before applying it. Down migration drops the table and its inquiries: preserve data before any downgrade.

Requests use antiforgery checks, a 32 KiB limit, bounded server-side validation/allowlisted interest areas, a hidden honeypot and a limit of five submissions per IP per ten minutes. Replaying the same submission UUID does not duplicate the inquiry; its unique database constraint also covers racing requests. Messages are rendered as encoded text in the inbox; they are never interpreted as HTML. No IP address is stored with messages. Existing proxy and single-instance limiter limitations in DEPLOYMENT.md still apply.

The inbox contains personal contact information. Include it in encrypted database backups; restrict database/backup access, assign an operator to review it and choose an appropriate retention/deletion process. No automatic retention job or inquiry-delete UI is implemented. No claims about legal-policy approval or real-host readiness are implied.

## Verification

Release solution builds with zero warnings/errors, and the model matches the migration. 46 existing rollback database scenarios, all five browser interaction checks and 51 quality accessibility scans pass. The dedicated `node tests/inquiry-review.cjs` checks bilingual layout/mobile overflow, seven displayed business days, five additional automated accessibility scans (including invalid-form and confirmation states), real Arabic submission/confirmation, validation, CSRF, honeypot, duplicate prevention, throttling, anonymous inbox denial, encoded message display and marking read. It deletes only its uniquely identified fixture inquiry in finally using the dedicated local database guard; existing inquiries are preserved. The tool exhausts the inquiry limiter: restart the local preview afterward before manual submission. It uses the local review account and installed Windows MySQL client, not production. Screenshots are under ignored artifacts/inquiry-review.

The authentication suite passes inbox access checks for all three roles. The published application also passes the isolated Production-mode loopback HTTPS smoke test. Local Edge/loopback tests do not replace testing the real production host, TLS, backup restoration and operations. Rebuild the release and regenerate migration SQL before deployment; Phase 12's original artifact predates this feature.
