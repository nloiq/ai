import http from 'node:http';
import { readFile } from 'node:fs/promises';
const allowed = new Map([['/', ['index.html','text/html; charset=utf-8']], ['/index.html',['index.html','text/html; charset=utf-8']], ['/preview.css',['preview.css','text/css']], ['/preview.js',['preview.js','text/javascript; charset=utf-8']]]);
const server = http.createServer(async(req,res) => { const item = allowed.get(new URL(req.url,'http://localhost').pathname); if (!item) { res.writeHead(404); res.end('Not found'); return; } try { const data = await readFile(new URL(item[0],import.meta.url)); res.writeHead(200,{'Content-Type':item[1], 'Cache-Control':'no-store'}); res.end(data); } catch { res.writeHead(500); res.end('Preview unavailable'); } });
server.listen(4173,'127.0.0.1',()=>console.log('NLO IQ Phase 1: http://127.0.0.1:4173'));
