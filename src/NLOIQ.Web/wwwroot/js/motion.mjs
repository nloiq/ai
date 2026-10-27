import { parallaxOffset } from './interaction-utils.mjs';
import { createSculpture } from './hero-sculpture.mjs';
const reduced=matchMedia('(prefers-reduced-motion:reduce)');
const compact=matchMedia('(max-width:767px)');
const header=document.getElementById('site-header');
const control=document.getElementById('motion-toggle');
let paused=false;try{paused=localStorage.getItem('nlo-motion-paused')==='true';}catch{}
const isPaused=()=>paused||reduced.matches;
const animations=new Set();const sculptures=[];
function animate(element,keyframes,options){if(isPaused()||!element?.animate)return;const animation=element.animate(keyframes,options);animations.add(animation);animation.finished.then(()=>{animations.delete(animation);animation.cancel();}).catch(()=>animations.delete(animation));}
function syncPreference(){document.documentElement.dataset.motion=isPaused()?'paused':'running';if(control){control.hidden=false;control.disabled=reduced.matches;control.textContent=reduced.matches?control.dataset.reducedLabel:paused?control.dataset.resumeLabel:control.dataset.pauseLabel;control.setAttribute('aria-pressed',String(isPaused()));}if(isPaused())for(const animation of animations)animation.cancel();for(const sculpture of sculptures)sculpture.refresh();schedule(true);}
control?.addEventListener('click',()=>{paused=!paused;try{localStorage.setItem('nlo-motion-paused',String(paused));}catch{}syncPreference();});
const layers=[...document.querySelectorAll('[data-parallax]')].map(element=>({element,target:element.querySelector('[data-parallax-layer]')||element,amount:Math.min(32,Math.max(0,Number(element.dataset.parallax)||0)),top:0,height:0,offset:0,visible:true}));
let frameId=0,needsMeasurement=true;
function schedule(measure=false){needsMeasurement||=measure;if(!frameId)frameId=requestAnimationFrame(render);}
function render(){frameId=0;const scroll=window.scrollY,viewport=innerHeight;let scrollRange=0;
    if(needsMeasurement){for(const layer of layers){const rect=layer.element.getBoundingClientRect();layer.top=rect.top+scroll-(layer.target===layer.element?layer.offset:0);layer.height=rect.height;}needsMeasurement=false;}
    scrollRange=Math.max(1,document.documentElement.scrollHeight-viewport);
    const headerHeight=header?.offsetHeight||88;document.documentElement.style.setProperty("--public-header-offset",(headerHeight+24)+"px");
    header?.classList.toggle('header-scrolled',scroll>24);header?.style.setProperty('--page-progress',Math.min(1,Math.max(0,scroll/scrollRange)).toFixed(4));
    for(const layer of layers){if(!layer.visible&&!isPaused())continue;const amount=layer.target.tagName==='IMG'?Math.min(layer.amount,layer.height*.018):layer.amount;layer.offset=!isPaused()&&!compact.matches?parallaxOffset(scroll,viewport,layer.top,layer.height,amount):0;layer.target.style.transform=layer.offset?'translate3d(0,'+layer.offset.toFixed(2)+'px,0)':'';}
}
// The content stays visible without JavaScript; animations only begin when observed.
const targets=[...document.querySelectorAll('main .public-directory-invitation,main .public-approach,main .public-contact-invitation,main .public-directory-heading,main .public-work-card,main .public-project-heading,main .public-contact-grid>*,main [data-reveal]')];
const sectionLinks=[...document.querySelectorAll('[data-section-navigation] a')];
function reveal(element){if(isPaused())return;const children=element.matches('.public-directory-invitation,.public-approach')?[...element.children]:[element];children.forEach((child,index)=>animate(child,[{opacity:0,transform:'translate3d(0,28px,0)'},{opacity:1,transform:'translate3d(0,0,0)'}],{duration:720,delay:index*90,easing:'cubic-bezier(.2,.65,.25,1)'}));}
if('IntersectionObserver'in window){
    const observer=new IntersectionObserver(entries=>{for(const entry of entries)if(entry.isIntersecting){reveal(entry.target);observer.unobserve(entry.target);}},{threshold:.1});targets.forEach(element=>observer.observe(element));
    const visibility=new IntersectionObserver(entries=>{for(const entry of entries){const layer=layers.find(x=>x.element===entry.target);if(layer)layer.visible=entry.isIntersecting;if(entry.target.matches('.hero-scene'))entry.target.dataset.visible=String(entry.isIntersecting);}schedule();},{rootMargin:'50px'});layers.forEach(layer=>visibility.observe(layer.element));document.querySelectorAll('.hero-scene').forEach(scene=>visibility.observe(scene));
    const sections=sectionLinks.map(link=>document.getElementById(link.hash.slice(1))).filter(Boolean);
    const activeSections=new Map();const navigation=new IntersectionObserver(entries=>{for(const entry of entries)activeSections.set(entry.target.id,entry.isIntersecting?entry.intersectionRatio:0);let current=null,best=0;for(const[id,ratio]of activeSections)if(ratio>best){current=id;best=ratio;}for(const link of sectionLinks){if(link.hash==='#'+current)link.setAttribute('aria-current','location');else link.removeAttribute('aria-current');}},{rootMargin:'-90px 0px -25% 0px',threshold:[0,.15,.4,.7,1]});sections.forEach(section=>navigation.observe(section));
}
for(const link of sectionLinks)if(!document.getElementById(link.hash.slice(1)))link.hidden=true;
document.addEventListener('click',event=>{const link=event.target.closest('a[href]');if(!link||event.defaultPrevented||event.button!==0||event.metaKey||event.ctrlKey||event.shiftKey||event.altKey||link.target==='_blank'||link.hasAttribute('download'))return;const url=new URL(link.href,location.href);if(url.origin!==location.origin||url.pathname!==location.pathname||url.search!==location.search||!url.hash||url.hash==='#main')return;let target;try{target=document.getElementById(decodeURIComponent(url.hash.slice(1)));}catch{return;}if(!target)return;
    event.preventDefault();target.scrollIntoView({behavior:isPaused()?'instant':'smooth',block:'start'});if(location.hash!==url.hash)history.pushState(null,'',url.hash);target.tabIndex=-1;target.focus({preventScroll:true});if(!isPaused()){target.classList.remove('section-arriving');void target.offsetWidth;target.classList.add('section-arriving');target.addEventListener('animationend',()=>target.classList.remove('section-arriving'),{once:true});}
});
window.addEventListener('scroll',()=>schedule(),{passive:true});window.addEventListener('resize',()=>schedule(true),{passive:true});window.addEventListener('load',()=>schedule(true));document.addEventListener('load',()=>schedule(true),true);document.addEventListener('toggle',()=>schedule(true),true);
reduced.addEventListener('change',syncPreference);compact.addEventListener('change',()=>schedule(true));
document.addEventListener('visibilitychange',()=>{document.documentElement.classList.toggle('motion-page-hidden',document.hidden);});
if('ResizeObserver'in window)new ResizeObserver(()=>schedule(true)).observe(document.body);
syncPreference();
for(const canvas of document.querySelectorAll('[data-hero-sculpture]')){const sculpture=createSculpture(canvas,isPaused);if(sculpture)sculptures.push(sculpture);}
const hero=document.querySelector('.public-hero');if(hero&&!isPaused()){const content=hero.firstElementChild;[...content.children].forEach((child,index)=>animate(child,[{opacity:0,transform:'translate3d(0,30px,0)'},{opacity:1,transform:'translate3d(0,0,0)'}],{duration:850,delay:100+index*110,easing:'cubic-bezier(.16,1,.3,1)'}));animate(hero.querySelector('.hero-scene'),[{opacity:0,transform:'translate3d(0,24px,0) scale(.93)'},{opacity:1,transform:'translate3d(0,0,0) scale(1)'}],{duration:1400,delay:140,easing:'cubic-bezier(.16,1,.3,1)'});}
document.fonts?.ready.then(()=>schedule(true));
