# Phase 4 — Motion and project interactions

Date: 2026-10-23 (Asia/Baghdad). Status: ready for motion/interaction review; approval pending.

## Scope and approvals

Phase 3 was explicitly approved. Its layout, bilingual content, database-backed services and honest placeholder policy are preserved. This phase adds motion and interaction behavior only; it does not start the Phase 5 CMS content schema.

The user supplied https://www.youtube.com/watch?v=NNiszX0-A7M for the lightbox. The first study uses this video for interaction review and explicitly avoids claiming that its conceptual illustration represents a completed client project. The other two studies show their illustrations with a localized video-not-supplied message.

## Implemented

- Layered, bounded scroll offsets for hero typography (6px), systems artwork (24px), About linework (16px), project imagery (bounded by crop allowance), service linework (8px), and footer wordmark (8px).
- One event-scheduled requestAnimationFrame renderer; cached document geometry, IntersectionObserver activation and grouped read/write work. Layout changes, image loads, viewport changes and service disclosure invalidate measurements.
- Progressive scroll reveals for supporting content; meaningful content is visible before JavaScript initialization. Mobile below 768px disables parallax and pending reveals. Reduced-motion changes clear transforms/reveals at runtime and disable CSS animations/transitions/smooth scrolling.
- Restrained image scale on hover, service row hover/open indicators, short mobile menu transition, sticky-header shadow, and modal entrance transition.
- Native modal dialog with localized name/description, inert background, explicit focus-boundary wrapping, autofocus Close, Escape and backdrop dismissal, scroll lock, opener focus restoration, and previous/next navigation in document order. Arabic controls and direction are supported.
- Stable dialog header/footer with a scrollable media/description area so title, close and navigation remain available on short screens.
- Responsive 16:9 media area. YouTube iframe is created on demand and destroyed on close or project navigation. No autoplay and no initial YouTube iframe/network dependency.
- Shared server/client video-ID validation, startup validation for configured video IDs and a fixed youtube-nocookie embed origin. CSP grants frame access only to that origin. No arbitrary URLs or markup are forwarded to an iframe.

Video IDs are currently Development preview settings, not CMS records. The complete project/video content model remains a later approved phase.

## Preview and review

- [English](http://127.0.0.1:5080/en/)
- [Arabic](http://127.0.0.1:5080/ar/)

Scroll slowly on desktop to inspect artwork and type depth. On mobile, confirm static readable compositions. Click a portfolio image or Open study. The first study loads the supplied video; Next/Previous move among studies. Check Close, Escape, backdrop, Tab/Shift+Tab, opener focus, and background scroll lock. Try light/dark/system and the operating system's reduced-motion preference.

Actual screenshots captured in installed Microsoft Edge, with reduced motion for stable layout review:

- [English desktop](previews/phase4/english-desktop.png)
- [Arabic mobile dark](previews/phase4/arabic-mobile-dark.png)
- [Project dialog](previews/phase4/project-dialog.png)

The dialog screenshot shows an illustration-only study; it does not fake live YouTube playback. Full-page screenshot preparation scrolls images into view and waits for their real lazy-loaded assets before capture.

If the server stops, run dotnet run --project src/NLOIQ.Web --no-build from the repository root. Build/run instructions remain in README.md.

## Actual verification

| Check | Result |
| --- | --- |
| .NET solution build | Passed, zero warnings/errors |
| Tailwind v4 build and JS syntax | Passed |
| JavaScript helper tests | Four groups passed: constrained embed URLs, invalid input, bounded motion, navigation wrapping |
| Server video validation | 12 cases passed |
| Invalid configured video | Rejected at startup |
| Bilingual foundation/prototype HTTP checks | Passed |
| Edge browser interactions | Five tests passed |
| Desktop dialog | Focus boundaries, lazy iframe, previous/next, Escape/backdrop, iframe removal, scroll unlock and opener focus passed |
| Arabic mobile | Menu focus/Escape, theme persistence/system changes, RTL dialog and reduced-motion reset passed |
| Responsive layout | No horizontal overflow at 320, 390, 768 and 1440px in both languages |
| Dynamic motion | Desktop offsets bounded; runtime reduced-motion change clears transforms and reveal states |
| Scheduler | Zero frame requests during 200ms idle observation; one frame request for 50 same-turn scroll events |
| npm audit | Zero reported vulnerabilities |
| Live external embed | HTTP 200 and supplied video title resolved |
| Live video playback | Not confirmed: playable control did not become available within the headless check timeout |

The iframe lifecycle browser test uses an explicitly controlled response for deterministic assertions. It proves creation/removal/focus behavior, not external video playback. A separate unmocked check loaded the supplied video's document/title but did not prove playback. User review of live playback remains required. The scheduler measurement is not a device-wide FPS or full performance audit.

The interactive browser runtime previously failed to initialize; installed Edge headless testing now provides real browser coverage and screenshots. It found a focus-wrap issue, which was corrected and retested. Visual inspection also led to the fixed dialog controls described above. These checks do not replace final multi-browser, screen-reader or production QA.

## Relevant files

- src/NLOIQ.Web/wwwroot/js/motion.mjs: single scheduler, geometry caching and progressive reveals.
- src/NLOIQ.Web/wwwroot/js/project-dialog.mjs: modal lifecycle, focus, iframe creation/removal and navigation.
- src/NLOIQ.Web/wwwroot/js/interaction-utils.mjs: pure validation, URL construction, motion and ordering helpers.
- src/NLOIQ.Application/Validation/YouTubeVideoId.cs: shared server ID validator.
- src/NLOIQ.Web/Configuration/PreviewOptions.cs: allowed study keys and fail-closed video configuration.
- src/NLOIQ.Web/Views/Shared/_ProjectDialog.cshtml and section partials: accessible controls and motion hooks.
- src/NLOIQ.Web/Program.cs: options registration and constrained frame-src CSP.
- src/NLOIQ.Web/appsettings.Development.json: user-supplied non-secret preview video ID.
- src/NLOIQ.Web/Assets/styles/app.css: minimal reveal/menu/dialog rules and reduced-motion safeguards; Tailwind output remains compiled.
- tests/interactions.test.mjs, tests/NLOIQ.Checks/, tests/browser/interactions.spec.cjs and playwright.config.cjs: repeatable helper/server/browser checks.

## Re-run checks

With the development app running:

```powershell
npm.cmd run test:interactions
dotnet run --project tests/NLOIQ.Checks
node tests/prototype-smoke.mjs
npm.cmd run test:browser
```

Browser tests require an installed Microsoft Edge; they use a fresh isolated headless profile and do not operate the user's signed-in browser. Playwright is a development-only dependency. No public deployment or existing-user browser data changes were made.

Optional additional preview videos use Preview:VideoIds:scada and Preview:VideoIds:automation, with 11-character IDs only. Bad values fail validation. Do not add arbitrary embed HTML. No schema or seed data was changed in Phase 4.

## Review gate

Revise parallax intensity, hover behavior, reveal timing, dialog composition, project order or video assignments if desired. Await explicit Phase 4 approval before beginning Phase 5 database/content architecture.

Primary implementation references: [native dialog behavior](https://developer.mozilla.org/en-US/docs/Web/HTML/Reference/Elements/dialog), [YouTube player parameters](https://developers.google.com/youtube/player_parameters), [Playwright branded-browser testing](https://playwright.dev/docs/browsers#google-chrome--microsoft-edge).
