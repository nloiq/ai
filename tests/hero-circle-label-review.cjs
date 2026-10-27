const { chromium } = require('@playwright/test');
const assert = require('node:assert/strict');

async function reviewLocale(browser, culture, expectedLabel) {
  const context = await browser.newContext({
    viewport: { width: 1440, height: 900 },
    reducedMotion: 'no-preference'
  });
  const page = await context.newPage();
  await page.goto(`http://127.0.0.1:5080/${culture}/`, { waitUntil: 'networkidle' });

  const canvas = page.locator('[data-hero-sculpture]');
  const scene = page.locator('.hero-scene');
  const circleLabel = page.locator('[data-circle-label]');
  const outsideLabels = page.locator('.hero-scene-tag');

  await page.waitForFunction(() => document.querySelector('[data-hero-sculpture]')?.dataset.phase === 'circle', null, { timeout: 15000 });
  await expectVisibleText(circleLabel, expectedLabel);
  await page.waitForTimeout(350);
  assert.equal(await outsideLabels.count(), 3);
  for (let index = 0; index < 3; index += 1) {
    assert.equal(await outsideLabels.nth(index).evaluate(element => getComputedStyle(element).visibility), 'hidden');
  }

  const [sceneBox, labelBox] = await Promise.all([scene.boundingBox(), circleLabel.boundingBox()]);
  assert(sceneBox && labelBox);
  assert(Math.abs((labelBox.x + labelBox.width / 2) - (sceneBox.x + sceneBox.width / 2)) < 2);
  assert(Math.abs((labelBox.y + labelBox.height / 2) - (sceneBox.y + sceneBox.height / 2)) < 2);

  const firstFrame = await canvas.screenshot();
  await page.waitForTimeout(500);
  const secondFrame = await canvas.screenshot();
  assert.notDeepEqual(firstFrame, secondFrame, 'The internal glow must continue moving during the circle phase.');

  await page.waitForFunction(() => document.querySelector('[data-hero-sculpture]')?.dataset.phase === 'infinity', null, { timeout: 15000 });
  assert.equal(await circleLabel.evaluate(element => getComputedStyle(element).visibility), 'hidden');
  for (let index = 0; index < 3; index += 1) {
    assert.equal(await outsideLabels.nth(index).evaluate(element => getComputedStyle(element).visibility), 'visible');
  }

  await context.close();
}

async function expectVisibleText(locator, expected) {
  assert.equal((await locator.textContent()).trim(), expected);
  assert.equal(await locator.evaluate(element => getComputedStyle(element).visibility), 'visible');
}

(async () => {
  const browser = await chromium.launch({ channel: 'msedge', headless: true });
  try {
    await reviewLocale(browser, 'en', 'Close Loop System');
    await reviewLocale(browser, 'ar', 'نظام الحلقة المغلقة');
    console.log('Hero circle label review passed for English and Arabic.');
  } finally {
    await browser.close();
  }
})().catch(error => {
  console.error(error);
  process.exitCode = 1;
});
