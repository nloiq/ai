'use strict';
(() => {
const selector = document.getElementById('theme');
const system = matchMedia('(prefers-color-scheme:dark)');
let preference = 'system';
try { preference = localStorage.getItem('nlo-theme') || 'system'; } catch {}
if (!['light','dark','system'].includes(preference)) preference = 'system';
selector.value = preference;
function apply() { document.documentElement.dataset.theme = preference === 'system' ? (system.matches ? 'dark' : 'light') : preference; }
selector.addEventListener('change', () => { preference = selector.value; try { localStorage.setItem('nlo-theme', preference); } catch {} apply(); });
system.addEventListener('change', apply);
apply();
const menuButton = document.getElementById('menu-toggle');
const navigation = document.getElementById('primary-navigation');
const desktop = matchMedia('(min-width:1024px)');
let menuOpen = false;
function syncMenu() {
    navigation.hidden = !desktop.matches && !menuOpen;
    menuButton.setAttribute('aria-expanded', String(menuOpen));
    navigation.dataset.menuOpen = String(menuOpen && !desktop.matches);
}
menuButton.hidden = false;
menuButton.addEventListener('click', () => { menuOpen = !menuOpen; syncMenu(); });
desktop.addEventListener('change', () => { menuOpen = false; syncMenu(); });
navigation.addEventListener('click', event => {
    const link = event.target.closest('a');
    if (link && !desktop.matches) {
        menuOpen = false;
        syncMenu();
        const url = new URL(link.href, location.href);
        let target = null;
        if (url.origin === location.origin && url.pathname === location.pathname && url.hash) {
            try { target = document.getElementById(decodeURIComponent(url.hash.slice(1))); } catch {}
        }
        if (target) { target.tabIndex = -1; target.focus({ preventScroll: true }); }
    }
});
document.addEventListener('keydown', event => {
    if (event.key === 'Escape' && menuOpen && !desktop.matches) { menuOpen = false; syncMenu(); menuButton.focus(); }
});
syncMenu();
})();
