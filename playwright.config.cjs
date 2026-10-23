const { defineConfig } = require('@playwright/test');
module.exports = defineConfig({
    testDir: './tests/browser', timeout: 30000, workers: 1,
    outputDir: 'artifacts/browser-results',
    reporter: [['list']],
    use: { baseURL: 'http://127.0.0.1:5080', channel: 'msedge', headless: true, screenshot: 'only-on-failure' }
});
