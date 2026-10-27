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
