'use strict';
(() => { let preference = document.documentElement.dataset.defaultTheme || 'system'; try { preference = localStorage.getItem('nlo-theme') || preference; } catch {} document.documentElement.dataset.theme = preference === 'light' || preference === 'dark' ? preference : (matchMedia('(prefers-color-scheme:dark)').matches ? 'dark' : 'light'); })();
