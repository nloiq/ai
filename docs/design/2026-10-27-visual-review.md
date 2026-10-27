# NLO IQ: visual review and redesign proposal

Date: 27 October 2026. Scope: local English/Arabic homepage, Projects and Services directories, project details, service template, contact section, mobile layout, shared CSS and public layouts. This is a design assessment, not a new security/performance certification. The separate HTML concept is illustrative and does not change public pages or CMS records.

## Assessment

The current visual direction does not support the intended positioning as a high-end software and engineering firm. Functional CMS workflows, category filtering and a working gallery are useful foundations, but passing functional/accessibility checks does not establish design quality. Recent changes solved individual requests without establishing a coherent public design system. Another round of isolated border, card and color edits would repeat that problem.

### Findings from the rendered pages

| Area | Observed problem | Design consequence | Proposed response |
|---|---|---|---|
| Brand | Small red NLO header logo, large iMDES hero image, NLO IQ text footer; detail footer says NLO---IQ | Visitors cannot identify one company confidently | Confirm the official name and mark; apply one consistent lockup, clear space and light/dark assets everywhere. Concept uses NLO IQ as a working wordmark, not a replacement logo. |
| Positioning | English hero says Engineering the invisible, with Development sample underneath | Does not quickly explain what the firm delivers or for whom | Lead with software and connected engineering capabilities; one clear project/contact action and one work link. |
| Typography | Georgia/Times serif display type, oversized headings, repeated small tracked labels; Arabic has different density | Editorial character, weak scanning and uneven bilingual hierarchy | A consistent sans-serif hierarchy. Evaluate locally hosted Manrope for English; retain Cairo for Arabic. Use separate Arabic optical sizing and natural line height. |
| Color | Cream, olive, rust and gold dominate; hard section rules repeat | Reads as a restrained editorial brochure rather than advanced engineering | Graphite/navy and cool neutral surfaces with electric blue and a restrained cyan signal accent. Provide a designed light theme, not an inverted dark screenshot. |
| Hero | Most visual space is occupied by another logo, with no product or project evidence | The strongest screen fails to demonstrate expertise | Create a meaningful technical composition linking field systems, control, software and operations; replace with authorized real project imagery where available. |
| Structure | Large About block precedes small generic Projects/Services rows | Work is delayed; compact rows resemble utility links | Move concise capability navigation and a compact project invitation directly after the hero; move longer About content to Company. |
| Projects directory | One ordinary card beside large empty space; detached gallery repeats unrelated assets | Looks unfinished and loses the category/work relationship | Responsive project grid with intentional one-item layout, useful category controls, concise scope and consistent cover crops. Put every gallery inside its project. |
| Services | Categories work, but directory and detail are thin title/text/card layouts | Looks like a content inventory rather than a service offering | Category-led service presentation with deliverables, use cases, approach and related projects. Include scope only when accurate. |
| Project detail | Dominant leaf image and partner-style logo; body contains QA instructions | The displayed work does not substantiate engineering capability | Owner-curated screenshots, installation photos, diagrams; narrative of challenge, solution, architecture and outcome. Exclude test instructions from marketing copy. |
| Navigation | Homepage, directory and project detail use different header/footer implementations; detail currently has no primary menu | Inconsistent identity and wayfinding | One shared public shell across all public templates; mobile drawer, clear current page, compact locale/theme controls. |
| Contact | Form and seven-day hours dominate homepage; most days show Not provided | Incomplete data becomes a major part of the sales pitch | Compact homepage contact invitation; full form, verified contact channels and complete business-hours schedule on Contact. Unknown hours need honest optional display rules, never invented opening times. |
| Footer | Development/prototype text and a statement that inquiries are not accepted, despite a functioning form | Contradiction undermines trust | One CMS-managed footer with accurate contact, legal and navigation content. Retain preview notices in private review, remove public demo messages only as part of an approved content launch. |
| Mobile | Header wraps and About precedes work; captured homepage is about 5,435px tall at 390px width | Long journey to relevant information | Shorten the first journey, prioritize work/services/contact, compact navigation and omit decorative artwork that adds scrolling without information. |

The screenshots document the current local CMS content at review time; that content can change independently. Leaf photos, mismatched logos and test text are content issues as well as presentation issues. A CSS redesign alone cannot make those assets credible evidence.

## Recommended direction: precision engineering, presented like a software company

Use a disciplined grid, confident sans-serif typography and imagery that explains what the company builds. Technical detail should be legible and purposeful: system layers, interface captures, signal connections and architecture diagrams. Avoid decorative code rain, fake telemetry, stock robots, constant glow, exaggerated animations and unsupported claims about AI or scale.

Proposed tokens: dark background #080F1D, raised surface #111D30, primary text #F4F7FC, secondary text #A7B5CB, electric blue #2864E8 and cyan #59E0D1. Light theme uses #F5F7FC/#FFFFFF surfaces and #101A2C text, with darker accent variants where needed for contrast. These are proposal values, not yet published brand settings. Use a 1,280px content grid, 24px gutters, 8px spacing steps, 12-16px component corners and subtle neutral borders. Reserve strong contrast for the primary action and key information.

English heading target: 64-76px desktop and 38-44px mobile; body 17-18px. Arabic heading target: approximately 52-64px desktop and 32-40px mobile, with independent wrapping/leading checks. Labels at least 12px, comfortable reading measure, real buttons/links and keyboard focus. Final sizing follows actual bilingual content, not fixed forced line counts.

## Page plan

1. Home: concise header; capability-led hero plus technical visual; compact four-area Services indicator and directory link; compact Projects invitation with a single representative crop and directory link; short delivery approach; compact Contact invitation; unified footer. No long homepage galleries, full service listings or full inquiry form. A small approved client/partner strip is optional only when authorized logos exist.
2. Projects: strong concise header, category filters and counts; consistent visual project cards. Each combines a cover, title, category and one sentence about the delivered work. Use a wider feature composition when there is one project rather than an almost-empty three-column grid. Keep category filters accessible; add search only when content volume merits it. Remove the detached section gallery from this page after assigning its legitimate images to the appropriate project records.
3. Project details: breadcrumb and title; concise project facts; high-quality cover; challenge, solution and system architecture; organized gallery with captions/video; verified outcomes; related projects; relevant contact action. Optional fields disappear cleanly if absent. Preserve full-image viewing and the accessible dialog.
4. Services: four engineering areas with clear category navigation, concise benefits and deliverables; dedicated service detail pages. Detail template: problem addressed, scope/deliverables, process, relevant technologies when confirmed, related work and inquiry action.
5. Company: positioning, actual team/expertise, delivery approach and verified qualifications. Replace generic philosophy text with factual content.
6. Contact: brief invitation, working form, verified address/channels, complete business hours and optional map link. No fabricated response-time promise.

## Engineering and CMS implementation

Keep ASP.NET Core MVC, current published-revision rules, category URLs, bilingual routing and media security. There is no design benefit to replacing the application stack merely to appear modern. Consolidate public header/footer/theme components; introduce scoped public design tokens and reusable navigation, buttons, cards, media frames and forms. Admin can retain its functional workspace styling while receiving a separate polish later.

CMS additions: project short description, optional client/industry/year/location, challenge/solution/outcomes, approved technology tags, image purpose and focal point; service deliverables/use cases/process and related projects; optional featured project selection; shared public footer/contact data. Fields that imply business facts remain optional and owner supplied. Use structured inputs and preview, rather than arbitrary HTML or uncontrolled layout settings. Preserve existing content with additive migrations and sensible fallbacks; save drafts before publication. Do not rewrite or auto-publish owner content.

Use responsive image variants, appropriate cover crops and full-resolution gallery links; reserve dimensions, lazy-load below the fold, and prioritize one hero visual. Implement the architecture illustration in SVG/CSS with a static fallback. Restrained hover/focus feedback and a short one-time signal trace are enough; honor reduced motion. WebGL/3D is optional only if it communicates something useful and fits measured performance budgets.

## Delivery order and review gates

1. Establish official branding, one visual system and final homepage concept in English/Arabic, desktop/mobile and both themes. The companion concept makes this proposal concrete; it is not a finished public template.
2. Build the shared public shell and homepage, preserving compact Projects/Services navigation. Review against the concept before propagating styles.
3. Implement Projects, Services and detail templates; add structured CMS fields with backward compatibility.
4. Curate actual work imagery and rewrite sample copy using owner-approved facts. Resolve incorrect brand assets and incomplete contact data.
5. Verify keyboard/focus, WCAG AA contrast, RTL/bidi, mobile navigation, image behavior, draft/publish and category workflows; measure performance on a representative mobile profile; review screenshots at 390/768/1440px in both languages/themes.

Proposed performance acceptance targets, not measured results: LCP <=2.5s, CLS <=0.1 and INP <=200ms at the 75th percentile where sufficient field data exists. Before launch, use repeatable lab checks and treat them as a proxy, not proof of field performance. No accessibility or speed badge should be claimed from the design mockup.

## Reference principles

- Linear: show real product/work in context and make the product story specific. https://linear.app/
- Vercel: clear technical hierarchy and capability-to-use-case organization. https://vercel.com/
- AVEVA: connect software expertise to industrial use cases and customer stories. https://www.aveva.com/

These official sites were consulted on the review date for positioning/content patterns. This proposal does not copy their assets, layouts or branding, and no visual screenshot comparison of those external sites is claimed.

## Deliverable

Open `nloiq-direction-concept.html` beside this document. It is a standalone homepage direction with a light/dark toggle and a compact mobile layout. Its wordmark, schematic and suggested copy are illustrative; it contains no invented clients, completed projects, outcomes or live telemetry. Text and links are design-review content, not CMS publication. The existing website has not been restyled or republished by this review.
