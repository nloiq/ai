import assert from 'node:assert/strict';
const base = process.env.NLO_PREVIEW_URL || 'http://127.0.0.1:5080';
const decode = html => html.replace(/&#x([0-9a-f]+);/gi, (_, value) => String.fromCodePoint(parseInt(value, 16))).replace(/&#([0-9]+);/g, (_, value) => String.fromCodePoint(Number(value)));
for (const [culture,direction] of [['en','ltr'],['ar','rtl']]) {
  const response=await fetch(`${base}/${culture}/`);assert.equal(response.status,200);const html=decode(await response.text());
  assert.ok(html.includes(`lang="${culture}"`)&&html.includes(`dir="${direction}"`));
  assert.ok(!html.includes('\uFFFD'),'Rendered page must retain UTF-8 text');
  for(const type of ['projects','services']){assert.ok(html.includes(`data-home-directory="${type}"`));assert.ok(html.includes(`href="/${culture}/${type}"`));}
  assert.ok(!html.includes('data-project="'),'Large prototype cards must not remain on the homepage');
  assert.ok(response.headers.get('content-security-policy').includes("frame-ancestors 'none'"));assert.equal(response.headers.get('x-robots-tag'),'noindex, nofollow');
  const ids=[...html.matchAll(/\bid="([^"]+)"/g)].map(match=>match[1]);assert.equal(new Set(ids).size,ids.length);
  for(const [,anchor]of html.matchAll(/href="#([^"]+)"/g))assert.ok(ids.includes(anchor));
  console.log(`PASS /${culture}/: localization, compact directory links, security and anchors`);
}
for (const file of ['/appsettings.Local.json', '/appsettings.json', '/fr/']) {
  assert.equal((await fetch(base + file)).status, 404);
  console.log(`PASS ${file}: 404`);
}
for (const file of ['/css/app.css', '/js/app.js', '/js/theme-init.js']) {
  assert.equal((await fetch(base + file)).status, 200);
  console.log(`PASS ${file}: 200`);
}
const root = await fetch(base + '/', { redirect: 'manual' });
assert.equal(root.status, 302);
assert.equal(root.headers.get('location'), '/en/');
console.log('PASS /: redirect to /en/');
