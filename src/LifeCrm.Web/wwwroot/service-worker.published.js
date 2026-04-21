// Lazy loading + caching for production PWA
self.importScripts('./service-worker-assets.js');
self.addEventListener('install', event => event.waitUntil(onInstall(event)));
self.addEventListener('activate', event => event.waitUntil(onActivate(event)));
self.addEventListener('fetch', event => event.respondWith(onFetch(event)));

const cacheNamePrefix = 'offline-cache-';
const cacheName = `${cacheNamePrefix}${self.assetsManifest.version}`;

async function onInstall(event) {
    self.skipWaiting();
    const assetsRequests = self.assetsManifest.assets
        .filter(asset => offlineAssetsInclude.some(pattern => pattern.test(asset.url)))
        .filter(asset => !offlineAssetsExclude.some(pattern => pattern.test(asset.url)))
        .map(asset => new Request(asset.url, { integrity: asset.hash, cache: 'no-cache' }));
    await caches.open(cacheName).then(cache => cache.addAll(assetsRequests));
}

const offlineAssetsInclude = [ /\.dll$/, /\.pdb$/, /\.wasm/, /\.html/, /\.js$/, /\.json$/, /\.css$/, /\.woff$/, /\.woff2$/ ];
const offlineAssetsExclude = [ /^service-worker\.js$/ ];

async function onActivate(event) {
    const cacheKeys = await caches.keys();
    await Promise.all(cacheKeys.filter(k => k.startsWith(cacheNamePrefix) && k !== cacheName).map(k => caches.delete(k)));
}

async function onFetch(event) {
    if (event.request.method !== 'GET') return;
    const cachedResponse = await caches.match(event.request);
    return cachedResponse || fetch(event.request);
}
