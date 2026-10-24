const {chromium, expect} = require('@playwright/test');
const fs = require('node:fs/promises');
const accounts=JSON.parse(process.env.NLO_AUTH_FIXTURES);
const base='http://127.0.0.1:5080';
(async()=>{
 const browser=await chromium.launch({channel:'msedge',headless:true});
 try {
 if(process.env.NLO_AUTH_MODE==='revoked') {
   const context=await browser.newContext({storageState:'artifacts/auth-revoked-state.json'});
   const page=await context.newPage(); await page.goto(base+'/en/admin');
   await expect(page).toHaveURL(/account\/login/);
   console.log('PASS: disabled account loses existing authenticated session');
   await context.close(); return;
 }
 const anonymous=await browser.newContext(); const page=await anonymous.newPage();
 await page.goto(base+'/ar/admin'); await expect(page).toHaveURL(/ar\/account\/login/);
 await expect(page.locator('html')).toHaveAttribute('dir','rtl');
 await expect(page.locator('h1')).toHaveText('تسجيل الدخول');
 const missingToken=await anonymous.request.post(base+'/en/account/login',{form:{Email:'bad@example.invalid',Password:'bad'}});
 expect(missingToken.status()).toBe(400);
 const getLogout=await anonymous.request.get(base+'/en/account/logout'); expect(getLogout.status()).toBe(405);
 console.log('PASS: anonymous challenge, Arabic login, login CSRF and POST-only logout');
 async function login(page,name,query='') {
   await page.goto(base+'/en/account/login'+query);
   await page.locator('#Email').fill(accounts[name].email); await page.locator('#Password').fill(accounts[name].password);
   await page.locator('button[type=submit]').click();
 }
 await login(page,'Disabled'); await expect(page).toHaveURL(/account\/login/); await expect(page.getByRole('alert')).toContainText('Unable to sign in');
 await page.locator('#Password').fill('bad-password');await page.locator('#Email').fill('unknown@example.invalid');await page.locator('button[type=submit]').click();
 await expect(page.getByRole('alert')).toContainText('Unable to sign in');
 for(let i=0;i<5;i++) {
   await page.goto(base+'/en/account/login'); await page.locator('#Email').fill(accounts.Lockout.email);await page.locator('#Password').fill('bad-password');await page.locator('button[type=submit]').click();
   await expect(page.getByRole('alert')).toContainText('Unable to sign in');
 }
 await login(page,'Lockout');await expect(page).toHaveURL(/account\/login/);await expect(page.getByRole('alert')).toContainText('Unable to sign in');
 console.log('PASS: disabled/unknown accounts and locked account cannot sign in');
 await anonymous.close();
 for(const role of ['SuperAdmin','Editor','Viewer']) {
   const context=await browser.newContext();const p=await context.newPage();
   const errors=[];p.on('pageerror',e=>errors.push(e.message));
   await login(p,role,'?returnUrl=https%3A%2F%2Fexample.com');
   await expect(p).toHaveURL(base+'/en/admin');
   await expect(p.locator('h1')).toContainText('engineering content');
   const cookie=(await context.cookies()).find(x=>x.name==='NLOIQ.Admin');expect(cookie.httpOnly).toBe(true);expect(cookie.sameSite).toBe('Strict');expect(cookie.expires).toBe(-1);
   expect((await context.request.get(base+'/en/admin')).headers()['cache-control']).toContain('no-store');
   const editor=await context.request.get(base+'/en/admin/editor');expect(editor.status()).toBe(role==='Viewer'?403:200);
   const settings=await context.request.get(base+'/en/admin/settings');expect(settings.status()).toBe(role==='SuperAdmin'?200:403);
   const workspace=await context.request.get(base+'/en/admin/content');expect(workspace.status()).toBe(role==='Viewer'?403:200);
   const cmsCsrf=await context.request.post(base+'/en/admin/content/new',{form:{}});expect(cmsCsrf.status()).toBe(role==='Viewer'?403:400);
   const publishing=await context.request.post(base+'/en/admin/content/1/status',{form:{action:'publish'}});expect(publishing.status()).toBe(role==='SuperAdmin'?400:403);
   for(const path of ['media','seo','translations','settings/site','settings/appearance','audit']){const r=await context.request.get(base+'/en/admin/'+path);expect(r.status()).toBe(['media','seo','translations'].includes(path)?(role==='Viewer'?403:200):(role==='SuperAdmin'?200:403));}
   for(const path of ['/history','/preview']){const r=await context.request.get(base+'/en/admin/content/'+process.env.NLO_CONTENT_ID+path);expect(r.status()).toBe(role==='Viewer'?403:200);}
   for(const path of ['/history','/preview']){const r=await context.request.get(base+'/en/admin/content/'+process.env.NLO_SITE_ID+path);expect(r.status()).toBe(role==='SuperAdmin'?200:403);}
   for(const action of ['restore','schedule']){const r=await context.request.post(base+'/en/admin/content/'+process.env.NLO_CONTENT_ID+'/'+action,{form:{}});expect(r.status()).toBe(action==='restore'?(role==='Viewer'?403:400):(role==='SuperAdmin'?400:403));}
   const mediaCsrf=await context.request.post(base+'/en/admin/media/upload',{form:{}});expect(mediaCsrf.status()).toBe(role==='Viewer'?403:400);
   if(role==='Editor'){await p.goto(base+'/en/admin/content/new?kind=Page');const token=await p.locator('input[name=__RequestVerificationToken]').first().inputValue();const protectedPost=await context.request.post(base+'/en/admin/content/'+process.env.NLO_SITE_ID,{form:{__RequestVerificationToken:token,Id:process.env.NLO_SITE_ID,Kind:'Service',Key:'site'}});expect(protectedPost.status()).toBe(403);}
   const csrf=await context.request.post(base+'/en/account/logout');expect(csrf.status()).toBe(400);
   if(role==='SuperAdmin') {
     await fs.mkdir('artifacts/phase6',{recursive:true});
     await p.setViewportSize({width:1440,height:1000});await p.screenshot({path:'artifacts/phase6/admin-en-desktop.png',fullPage:true});
     for(const width of [320,390,768,1440]) {
       await p.setViewportSize({width,height:1000});await p.goto(base+'/ar/admin');
       await expect(p.locator('html')).toHaveAttribute('dir','rtl');
       expect(await p.evaluate(()=>document.documentElement.scrollWidth<=innerWidth)).toBe(true);
     }
     await p.setViewportSize({width:390,height:844});await p.selectOption('#theme','dark');await expect(p.locator('html')).toHaveAttribute('data-theme','dark');
     await p.screenshot({path:'artifacts/phase6/admin-ar-mobile-dark.png',fullPage:true});
     await p.reload();await expect(p.locator('#theme')).toHaveValue('dark');
   }
   await p.locator('form[action$="/logout"] button').click();await expect(p).toHaveURL(/account\/login/);
   await p.goto(base+'/en/admin');await expect(p).toHaveURL(/account\/login/);
   expect(errors).toEqual([]);await context.close();console.log('PASS: '+role+' permissions, safe redirect, protected logout and cookie/cache settings');
 }
 const revokedContext=await browser.newContext();const revoked=await revokedContext.newPage();
 await login(revoked,'Revoked');await expect(revoked).toHaveURL(base+'/en/admin');await revokedContext.storageState({path:'artifacts/auth-revoked-state.json'});await revokedContext.close();
 const rateContext=await browser.newContext();await rateContext.request.get(base+'/en/account/login');
 let limited=false;for(let i=0;i<22;i++){const r=await rateContext.request.post(base+'/en/account/login',{form:{}});if(r.status()===429){limited=true;break;}}
 expect(limited).toBe(true);await rateContext.close();console.log('PASS: login endpoint rate limit enforced');
 } finally {await browser.close();}
})().catch(error=>{console.error(error);process.exitCode=1;});
