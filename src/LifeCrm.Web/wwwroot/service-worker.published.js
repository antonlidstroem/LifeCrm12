self.importScripts('./service-worker-assets.js');
self.addEventListener('install', event => { self.skipWaiting(); });
self.addEventListener('activate', event => { self.clients.claim(); });
