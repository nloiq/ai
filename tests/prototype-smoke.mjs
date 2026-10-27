import './foundation-smoke.mjs';
import assert from 'node:assert/strict';
const base = process.env.NLO_PREVIEW_URL || 'http://127.0.0.1:5080';
const decode = html => html.replace(/&#x([0-9a-f]+);/gi, (_, value) => String.fromCodePoint(parseInt(value,16))).replace(/&#([0-9]+);/g, (_, value) => String.fromCodePoint(Number(value)));
for(const culture of ['en','ar']){
    const response=await fetch(`${base}/${culture}/`);assert.equal(response.status,200);const html=decode(await response.text());
    for(const id of ['about','projects','expertise','contact'])assert.ok(html.includes(`id="${id}"`));
    assert.ok(html.includes('id="primary-navigation"'));assert.equal((html.match(/<h1\b/g)||[]).length,1);
    assert.equal((html.match(/<article\b/g)||[]).length,0,'Homepage must link to collections rather than list projects');
    for(const type of ['projects','services'])assert.equal((await fetch(`${base}/${culture}/${type}`)).status,200);
    for(const image of html.match(/<img\b[^>]*>/g)||[]){assert.ok(/alt="[^"\s][^"]*"/.test(image));assert.ok(/width="\d+"/.test(image)&&/height="\d+"/.test(image));}
    assert.ok(!html.includes('<iframe'),'Videos belong on dedicated project pages');
    console.log(`PASS /${culture}/: compact sections, directory routes and image attributes`);
}
for (const asset of ['network-study.svg','scada-study.svg','automation-study.svg']) {
    const response=await fetch(`${base}/images/${asset}`);
    assert.equal(response.status,200);
    assert.ok(response.headers.get('content-type').startsWith('image/svg+xml'));
    assert.ok((await response.text()).includes('<svg'));
    console.log(`PASS /images/${asset}: local SVG delivered`);
}
