import test from 'node:test';
import assert from 'node:assert/strict';
import { isYouTubeVideoId, youtubeEmbedUrl, parallaxOffset, wrappedIndex } from '../src/NLOIQ.Web/wwwroot/js/interaction-utils.mjs';
test('approved YouTube ID produces a constrained embed without autoplay', () => {
    const id='NNiszX0-A7M';
    assert.ok(isYouTubeVideoId(id));
    const url=new URL(youtubeEmbedUrl(id));
    assert.equal(url.origin,'https://www.youtube-nocookie.com');
    assert.equal(url.pathname,'/embed/'+id);
    assert.equal(url.searchParams.get('autoplay'),'0');
});
test('video IDs reject URLs, markup, controls, unicode and invalid length', () => {
    for (const value of [undefined,null,'','short','NNiszX0-A7M\n','NNiszX0-A7M\r','NNiszX0-A7M?autoplay=1','https://youtu.be/NNiszX0-A7M','<iframe />','NNiszX0/A7M','ＮＮiszX0-A7M']) {
        assert.equal(isYouTubeVideoId(value),false);
        assert.equal(youtubeEmbedUrl(value),null);
    }
    assert.ok(isYouTubeVideoId('a_B-cD12345'));
});
test('parallax is centered, bounded and clamped for extreme input', () => {
    assert.equal(parallaxOffset(0,1000,250,500,24),0);
    assert.equal(parallaxOffset(-100000,1000,250,500,24),-24);
    assert.equal(parallaxOffset(100000,1000,250,500,24),24);
    assert.equal(parallaxOffset(100000,1000,250,500,100000),32);
    for (let y=-1000;y<=10000;y+=25) assert.ok(Math.abs(parallaxOffset(y,800,700,300,16))<=16);
});
test('invalid geometry is inert and previous/next wrap in document order', () => {
    for (const height of [0,-1,NaN,Infinity]) assert.equal(parallaxOffset(100,800,0,height,16),0);
    assert.equal(wrappedIndex(-1,3),2);
    assert.equal(wrappedIndex(3,3),0);
    assert.equal(wrappedIndex(1,0),0);
});
