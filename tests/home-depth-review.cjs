const { chromium } = require('@playwright/test');
const assert = require('node:assert/strict');

(async () => {
  const browser = await chromium.launch({ channel: 'msedge', headless: true });
  try {
    const context = await browser.newContext({ viewport: { width: 1440, height: 900 }, reducedMotion: 'no-preference' });
    const page = await context.newPage();
    await page.goto('http://127.0.0.1:5080/en/', { waitUntil: 'networkidle' });
    const panels = page.locator('main .public-directory-invitation,main .public-approach,main .public-contact-invitation');
    assert((await panels.count()) >= 3, 'The home page should contain its modernized section panels.');
    const panel = panels.first();
    const box = await panel.boundingBox();
    assert(box);
    await page.mouse.move(box.x + box.width * .82, box.y + box.height * .23);
    await page.waitForTimeout(80);
    const depth = await panel.evaluate(element => ({
      x: element.style.getPropertyValue('--tilt-x'),
      y: element.style.getPropertyValue('--tilt-y'),
      transform: getComputedStyle(element).transform
    }));
    assert.notEqual(depth.x, '');
    assert.notEqual(depth.y, '');
    assert.notEqual(depth.transform, 'none');
    assert.equal(await page.evaluate(() => document.documentElement.scrollWidth <= innerWidth), true, 'Desktop page must not overflow horizontally.');
    await page.screenshot({ path: 'artifacts/home-depth-desktop.png', fullPage: true });
    await context.close();

    const reducedContext = await browser.newContext({ viewport: { width: 390, height: 844 }, reducedMotion: 'reduce' });
    const reducedPage = await reducedContext.newPage();
    await reducedPage.goto('http://127.0.0.1:5080/en/', { waitUntil: 'networkidle' });
    const reducedPanel = reducedPage.locator('main .public-directory-invitation').first();
    assert.equal(await reducedPanel.evaluate(element => getComputedStyle(element).transform), 'none');
    assert.equal(await reducedPage.evaluate(() => document.documentElement.scrollWidth <= innerWidth), true, 'Mobile page must not overflow horizontally.');
    await reducedContext.close();
    console.log('Home depth review passed.');
  } finally {
    await browser.close();
  }
})().catch(error => { console.error(error); process.exitCode = 1; });
