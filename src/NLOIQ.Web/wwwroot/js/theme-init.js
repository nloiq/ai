'use strict';
(() => { let preference = 'system'; try { preference = localStorage.getItem('nlo-theme') || 'system'; } catch {} document.documentElement.dataset.theme = preference === 'light' || preference === 'dark' ? preference : (matchMedia('(prefers-color-scheme:dark)').matches ? 'dark' : 'light'); })();
