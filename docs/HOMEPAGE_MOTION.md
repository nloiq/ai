# Immersive hero and public motion

Implemented 27 October 2026 following the supplied AICM reference by Alex Bender / FANCY: https://dribbble.com/shots/26126156-AICM-landing-page-web-design-3D-animation . Its sculptural hero composition informed the direction; none of the reference images, models, video or branding is shipped in this project.

## What changed

- Original parametric S-curve sculpture rendered with native WebGL. A custom shader provides metallic lighting, a blue integration band and a moving cyan signal. Slow pose changes and desktop pointer response create visible depth. There is no external renderer, video download or model dependency.
- Timed hero entrances: content starts at 100ms with 110ms staggering and 850ms easing; sculpture entrance lasts 1,400ms. Arabic text stays whole rather than splitting joined letters into spans.
- Public content reveals on viewport entry, including mobile: 720ms vertical/opacity transitions with 90ms staggering. No content is permanently hidden while waiting for JavaScript.
- Home section navigation links to Services, Projects, About and Contact. Links scroll smoothly, update the hash, move keyboard focus and briefly accent the destination. The scroll offset follows the actual responsive header height. Directory links still open their dedicated pages.
- Scroll progress, current-section indicators, floating hero labels and restrained hover feedback.
- The published/pinned CMS homepage uses the same Hero component and section navigation. Public directory pages load the motion module for reveal and preference controls as well.

## Controls and fallbacks

Pause motion in the header stops the renderer and decorative animation and persists locally. Resume motion restores it. Operating-system reduced motion takes priority and displays Motion reduced; the sculpture is drawn once without an animation loop, and reveals/smooth scrolling/ambient animation stop. The text, calls to action and CMS publishing behavior remain available in every state.

The renderer suspends offscreen or when the browser tab is hidden. Device pixel ratio is capped at 1.75 desktop / 1.25 narrow screens. Scroll work is event-driven and coalesced; only the visible, unpaused hero has a continuous rendering loop. No-WebGL/context-loss and no-JavaScript cases retain the existing architecture illustration. The sculpture is decorative and does not claim to represent live operational data.

The new motion is presentation code. Hero titles, summaries, buttons and Supporting media remain revision-owned CMS fields. No content records, categories or palette revisions were changed for this update.

## Verification

Release solution build passed with zero warnings/errors. `node tests/motion-review.cjs` verifies changing rendered pixels, a frozen paused image, preference persistence, section focus/offset, offscreen idle behavior, English/Arabic themes and narrow layouts, four WCAG scans, live reduced-motion switching and no-WebGL/no-JavaScript fallbacks. Five browser interaction regressions pass; paused idle scheduling uses zero animation-frame requests and coalesces 50 scroll events into one frame. The 20-scan public redesign review passes. The full 51-scan quality suite and homepage smoke checks are also run before sync. Local visual evidence is under ignored `artifacts/motion`.
