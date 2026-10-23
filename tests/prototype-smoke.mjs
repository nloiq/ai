import './foundation-smoke.mjs';
import assert from 'node:assert/strict';
const base = process.env.NLO_PREVIEW_URL || 'http://127.0.0.1:5080';
const decode = html => html.replace(/&#x([0-9a-f]+);/gi, (_, value) => String.fromCodePoint(parseInt(value,16))).replace(/&#([0-9]+);/g, (_, value) => String.fromCodePoint(Number(value)));
for (const [culture, expected] of [['en',['Where precision','Connections, made visible.','Have a system','Not a client project']], ['ar',['حيث تلتقي الدقّة','روابط تتّضح صورتها.','هل تفكّر','ليست مشروع عميل']]]) {
    const html = decode(await (await fetch(`${base}/${culture}/`)).text());
    for (const title of expected) assert.ok(html.includes(title), `${culture} missing localized section: ${title}`);
    for (const id of ['about','projects','expertise','contact']) {
        assert.ok(html.includes(`id="${id}"`));
        assert.ok(html.includes(`href="#${id}"`));
    }
    assert.ok(html.includes('aria-controls="primary-navigation"'));
    assert.ok(html.includes('id="primary-navigation"'));
    assert.equal((html.match(/<h1\b/g) || []).length,1);
    assert.equal((html.match(/<article\b/g) || []).length,3);
    const images = [...html.matchAll(/<img\b[^>]*>/g)].map(match => match[0]);
    assert.equal(images.length,3);
    for (const image of images) {
        assert.ok(/alt="[^"\s][^"]*"/.test(image));
        assert.ok(/width="\d+"/.test(image) && /height="\d+"/.test(image));
        assert.ok(image.includes('loading="lazy"'));
    }
    assert.ok(!html.includes('<iframe'), 'No premature video embeds');
    assert.ok(!/href="(?:mailto:|tel:|https:\/\/wa\.me)/.test(html), 'Unverified contact details must not be fabricated');
    console.log(`PASS /${culture}/: all public sections, localized copy, menu references, three labeled portfolio studies and image accessibility attributes`);
}
for (const asset of ['network-study.svg','scada-study.svg','automation-study.svg']) {
    const response=await fetch(`${base}/images/${asset}`);
    assert.equal(response.status,200);
    assert.ok(response.headers.get('content-type').startsWith('image/svg+xml'));
    assert.ok((await response.text()).includes('<svg'));
    console.log(`PASS /images/${asset}: local SVG delivered`);
}
