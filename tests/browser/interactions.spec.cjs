const { test, expect } = require('@playwright/test');
const fs = require('node:fs/promises');
async function chooseTheme(page,value) { if(await page.locator('#theme').isVisible()) await page.locator('#theme').selectOption(value); else await page.locator('[data-theme-choice="'+value+'"]').click(); }
test('project dialog: focus, lazy iframe, navigation, Escape, backdrop and cleanup', async ({ page }) => {
    const errors=[]; page.on('pageerror', error => errors.push(error.message));
    // Deterministic embed lifecycle check; actual external playback is reviewed separately.
    await page.route('https://www.youtube-nocookie.com/**', route => route.fulfill({contentType:'text/html',body:'<!doctype html><html><body><button>Video frame fixture</button></body></html>'}));
    await page.goto('/en/');
    await expect(page.locator('iframe')).toHaveCount(0);
    const trigger=page.locator('[data-project="network"] button[data-project-open]');
    await trigger.click();
    const dialog=page.locator('#project-dialog');
    await expect(dialog).toBeVisible();
    await expect(page.locator('#dialog-close')).toBeFocused();
    await expect(dialog.locator('iframe')).toHaveAttribute('src','https://www.youtube-nocookie.com/embed/NNiszX0-A7M?autoplay=0&playsinline=1');
    await expect(page.locator('html')).toHaveClass(/overflow-hidden/);
    await page.keyboard.press('Shift+Tab');
    await expect(page.locator('#dialog-next')).toBeFocused();
    await page.locator('#dialog-next').click();
    await expect(dialog.locator('iframe')).toHaveCount(0);
    await expect(page.locator('#dialog-video-note')).toContainText('No video supplied');
    await expect(dialog.locator('img')).toHaveCount(1);
    await fs.mkdir('artifacts/phase4',{recursive:true});
    await dialog.screenshot({path:'artifacts/phase4/project-dialog.png'});
    await page.locator('#dialog-previous').click();
    await expect(dialog.locator('iframe')).toHaveCount(1);
    await page.keyboard.press('Escape');
    await expect(dialog).not.toBeVisible();
    await expect(dialog.locator('iframe')).toHaveCount(0);
    await expect(trigger).toBeFocused();
    await expect(page.locator('html')).not.toHaveClass(/overflow-hidden/);
    await trigger.click();
    await page.mouse.click(2,2);
    await expect(dialog).not.toBeVisible();
    await expect(dialog.locator('iframe')).toHaveCount(0);
    expect(errors).toEqual([]);
});
test('Arabic mobile menu, system theme, persistence and reduced motion', async ({ page }) => {
    await page.setViewportSize({width:390,height:844});
    await page.emulateMedia({reducedMotion:'reduce',colorScheme:'dark'});
    await page.goto('/ar/');
    const menu=page.locator('#menu-toggle'), nav=page.locator('#primary-navigation');
    await expect(menu).toBeVisible();
    await expect(nav).not.toBeVisible();
    await menu.click(); await expect(nav).toBeVisible();
    await page.keyboard.press('Escape');
    await expect(nav).not.toBeVisible(); await expect(menu).toBeFocused();
    await menu.click(); await nav.locator('a[href="#about"]').click();
    await expect(nav).not.toBeVisible();
    await expect(page.locator('#about')).toBeFocused();
    await expect(page.locator('html')).toHaveAttribute('data-theme','dark');
    await chooseTheme(page,'light');
    await page.reload(); await expect(page.locator('html')).toHaveAttribute('data-theme','light');
    await chooseTheme(page,'system');
    await expect(page.locator('html')).toHaveAttribute('data-theme','dark');
    await page.emulateMedia({colorScheme:'light'});
    await expect(page.locator('html')).toHaveAttribute('data-theme','light');
    const transforms=await page.locator('[data-parallax-layer]').evaluateAll(nodes=>nodes.map(node=>getComputedStyle(node).transform));
    expect(transforms.every(value=>value==='none')).toBeTruthy();
    await page.locator('[data-project="scada"] button[data-project-open]').click();
    await expect(page.locator('#project-dialog')).toBeVisible();
    expect(await page.locator('#project-dialog').evaluate(node=>getComputedStyle(node).direction)).toBe('rtl');
    await page.locator('#dialog-close').click();
});
test('responsive layouts avoid horizontal overflow and produce review screenshots', async ({ page }) => {
    await page.emulateMedia({reducedMotion:'reduce'});
    await fs.mkdir('artifacts/phase4',{recursive:true});
    for (const culture of ['en','ar']) for (const width of [320,390,768,1440]) {
        await page.setViewportSize({width,height:900});
        await page.goto('/'+culture+'/');
        expect(await page.evaluate(()=>document.documentElement.scrollWidth<=innerWidth)).toBeTruthy();
        await expect(page.locator('h1')).toBeVisible();
        for (const image of await page.locator('main img').all()) {
            await image.scrollIntoViewIfNeeded();
            await expect.poll(()=>image.evaluate(node=>node.complete && node.naturalWidth>0)).toBeTruthy();
        }
        await page.evaluate(()=>scrollTo(0,0));
        if (width===1440 && culture==='en') await page.screenshot({path:'artifacts/phase4/english-desktop.png',fullPage:true});
        if (width===390 && culture==='ar') { await chooseTheme(page,'dark'); await page.screenshot({path:'artifacts/phase4/arabic-mobile-dark.png',fullPage:true}); }
    }
});
test('desktop parallax is bounded and resets when reduced motion changes', async ({ page }) => {
    await page.setViewportSize({width:1440,height:900});
    await page.emulateMedia({reducedMotion:'no-preference'});
    await page.goto('/en/');
    await page.evaluate(()=>document.fonts.ready);
    // The CMS Hero may replace its original illustration; use the retained project image.
    const layer=page.locator('[data-project="network"] [data-parallax-layer]');
    await layer.evaluate(node=>{const rect=node.closest('[data-parallax]').getBoundingClientRect();scrollTo(0,Math.max(0,scrollY+rect.top+rect.height/2-innerHeight/2+120));});
    await expect.poll(async()=>layer.evaluate(node=>node.style.transform)).not.toBe('');
    const offset=await layer.evaluate(node=>new DOMMatrix(getComputedStyle(node).transform).m42);
    expect(Math.abs(offset)).toBeLessThanOrEqual(14);
    await page.emulateMedia({reducedMotion:'reduce'});
    await expect.poll(async()=>layer.evaluate(node=>getComputedStyle(node).transform)).toBe('none');
    await expect(page.locator('.reveal-pending')).toHaveCount(0);
});
test('scroll work is coalesced and the scheduler rests when idle', async ({ page }) => {
    await page.addInitScript(() => {
        const original=window.requestAnimationFrame.bind(window);
        window.__previewRafCount=0;
        window.requestAnimationFrame=callback=>{window.__previewRafCount++;return original(callback)};
    });
    await page.goto('/en/');
    await page.waitForTimeout(300);
    const before=await page.evaluate(()=>window.__previewRafCount);
    await page.waitForTimeout(200);
    const idle=await page.evaluate(()=>window.__previewRafCount)-before;
    expect(idle).toBeLessThanOrEqual(1);
    await page.evaluate(()=>{window.__previewRafCount=0;for(let index=0;index<50;index++)window.dispatchEvent(new Event('scroll'))});
    await page.waitForTimeout(60);
    const burst=await page.evaluate(()=>window.__previewRafCount);
    expect(burst).toBeLessThanOrEqual(2);
    console.log(`Scheduler: ${idle} frame requests during idle; ${burst} for 50 same-turn scroll events`);
});
