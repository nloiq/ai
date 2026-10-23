import assert from 'node:assert/strict';
const base = process.env.NLO_PREVIEW_URL || 'http://127.0.0.1:5080';
const decode = html => html.replace(/&#x([0-9a-f]+);/gi, (_, value) => String.fromCodePoint(parseInt(value, 16))).replace(/&#([0-9]+);/g, (_, value) => String.fromCodePoint(Number(value)));
const samples = [
  ['en', 'ltr', ['Low-current engineering', 'Software solutions', 'SCADA system integration', 'System automation'], 'Engineering'],
  ['ar', 'rtl', ['هندسة التيار الخفيف', 'الحلول البرمجية', 'تكامل أنظمة SCADA', 'أتمتة الأنظمة'], 'نهندس ما']
];
for (const [culture, direction, titles, heading] of samples) {
  const response = await fetch(`${base}/${culture}/`);
  assert.equal(response.status, 200);
  const html = decode(await response.text());
  assert.ok(html.includes(`lang="${culture}"`));
  assert.ok(html.includes(`dir="${direction}"`));
  assert.ok(html.includes(heading), 'Interface localization must resolve');
  assert.ok(!/\?{2,}/.test(html), 'Rendered page must not contain encoding replacement runs');
  for (const title of titles) assert.ok(html.includes(title), `Database translation missing: ${title}`);
  assert.equal((html.match(/<details\b/g) || []).length, 4);
  assert.ok(response.headers.get('content-security-policy').includes("frame-ancestors 'none'"));
  assert.equal(response.headers.get('x-robots-tag'), 'noindex, nofollow');
  const ids = [...html.matchAll(/\bid="([^"]+)"/g)].map(match => match[1]);
  assert.equal(new Set(ids).size, ids.length);
  for (const [, anchor] of html.matchAll(/href="#([^"]+)"/g)) assert.ok(ids.includes(anchor));
  console.log(`PASS /${culture}/: localized heading, direction, four database records, security headers and anchors`);
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
