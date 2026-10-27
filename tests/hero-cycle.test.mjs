import test from 'node:test';
import assert from 'node:assert/strict';
import {heroCycle} from '../src/NLOIQ.Web/wwwroot/js/hero-cycle.mjs';
test('signal completes its lap before the circle transformation',()=>{
 assert.equal(heroCycle(0).phase,'infinity');assert.equal(heroCycle(0).travel,0);
 assert.equal(heroCycle(5.9).morph,0);assert.ok(heroCycle(5.9).travel<1);
 assert.equal(heroCycle(6).travel,1);assert.equal(heroCycle(6).morph,0);
 assert.equal(heroCycle(7).morph,.5);assert.equal(heroCycle(8).morph,1);
 assert.equal(heroCycle(9).phase,'circle');assert.equal(heroCycle(9).morph,1);
 assert.equal(heroCycle(11).morph,.5);assert.equal(heroCycle(12).phase,'infinity');assert.equal(heroCycle(12).morph,0);
});
test('shape transitions are continuous, bounded and repeat without a geometry jump',()=>{
 for(const t of [6,8,10,12])assert.ok(Math.abs(heroCycle(t-.0001).morph-heroCycle(t+.0001).morph)<.0001);
 for(let t=0;t<36;t+=.07){const c=heroCycle(t);for(const key of ['morph','travel','completion'])assert.ok(c[key]>=0&&c[key]<=1);}
 for(const t of [-1,NaN,Infinity])assert.equal(heroCycle(t).morph,0);
 assert.deepEqual(heroCycle(25),heroCycle(1));
});

test('signal keeps moving around the circle and across both transitions',()=>{
 for(const [a,b] of [[6.1,6.6],[8.1,8.6],[10.1,10.6]])assert.notEqual(heroCycle(a).travel,heroCycle(b).travel);
 assert.equal(heroCycle(8.5).morph,1);assert.equal(heroCycle(9.5).morph,1);
 assert.ok(Math.abs(heroCycle(9.5).travel-heroCycle(8.5).travel)>.49,'The signal should travel half a circular lap in one second');
 for(const t of [6,8,10,12]){const delta=Math.abs(heroCycle(t-.0001).travel-heroCycle(t+.0001).travel);assert.ok(Math.min(delta,Math.abs(1-delta))<.001);}
});
