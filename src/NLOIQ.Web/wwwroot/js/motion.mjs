import { parallaxOffset } from './interaction-utils.mjs';
const reduced = matchMedia('(prefers-reduced-motion:reduce)');
const compact = matchMedia('(max-width:767px)');
const header = document.getElementById('site-header');
const layers = [...document.querySelectorAll('[data-parallax]')].map(element => ({
    element, target: element.querySelector('[data-parallax-layer]') || element,
    amount: Math.min(32, Math.max(0, Number(element.dataset.parallax) || 0)),
    top: 0, height: 0, offset: 0, visible: true
}));
let frameId = 0;
let needsMeasurement = true;
function active() { return !reduced.matches && !compact.matches; }
function schedule(measure = false) {
    needsMeasurement ||= measure;
    if (!frameId) frameId = requestAnimationFrame(render);
}
function render() {
    frameId = 0;
    const scroll = window.scrollY;
    const viewport = window.innerHeight;
    // Geometry reads occur together, only after layout-affecting events.
    if (needsMeasurement) {
        for (const layer of layers) {
            const rect = layer.element.getBoundingClientRect();
            layer.top = rect.top + scroll - (layer.target === layer.element ? layer.offset : 0);
            layer.height = rect.height;
        }
        needsMeasurement = false;
    }
    // Writes follow all geometry reads. No perpetual animation loop.
    header?.classList.toggle('shadow-md', scroll > 24);
    for (const layer of layers) {
        if (!layer.visible && active()) continue;
        const amount = layer.target.tagName === 'IMG' ? Math.min(layer.amount, layer.height * 0.018) : layer.amount;
        layer.offset = active() ? parallaxOffset(scroll, viewport, layer.top, layer.height, amount) : 0;
        layer.target.style.transform = layer.offset ? 'translate3d(0,' + layer.offset.toFixed(2) + 'px,0)' : '';
    }
}
if ('IntersectionObserver' in window) {
    const visibility = new IntersectionObserver(entries => {
        for (const entry of entries) {
            const layer = layers.find(item => item.element === entry.target);
            if (layer) layer.visible = entry.isIntersecting;
        }
        schedule();
    }, { rootMargin: '100px' });
    layers.forEach(layer => visibility.observe(layer.element));
    const reveals = [...document.querySelectorAll('[data-reveal]')];
    const revealObserver = new IntersectionObserver(entries => {
        for (const entry of entries) if (entry.isIntersecting) {
            entry.target.classList.remove('reveal-pending');
            revealObserver.unobserve(entry.target);
        }
    }, { threshold: 0.08 });
    for (const element of reveals) {
        if (active() && element.getBoundingClientRect().top > innerHeight) element.classList.add('reveal-pending');
        revealObserver.observe(element);
    }
    const clearReveals = () => { if (!active()) reveals.forEach(element => element.classList.remove('reveal-pending')); };
    reduced.addEventListener('change', clearReveals);
    compact.addEventListener('change', clearReveals);
}
window.addEventListener('scroll', () => schedule(), { passive: true });
window.addEventListener('resize', () => schedule(true), { passive: true });
window.addEventListener('load', () => schedule(true));
document.addEventListener('load', () => schedule(true), true);
document.addEventListener('toggle', () => schedule(true), true);
reduced.addEventListener('change', () => schedule(true));
compact.addEventListener('change', () => schedule(true));
if ('ResizeObserver' in window) new ResizeObserver(() => schedule(true)).observe(document.body);
document.fonts?.ready.then(() => schedule(true));
schedule(true);
