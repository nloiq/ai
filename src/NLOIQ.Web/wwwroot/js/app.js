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
})();
