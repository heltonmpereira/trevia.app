const CACHE_VERSION = 'treviaapp-cache-v2';
const ASSETS_CACHE = `${CACHE_VERSION}-assets`;
const RUNTIME_CACHE = `${CACHE_VERSION}-runtime`;

const STATIC_ASSETS = [
    '/',
    '/index.html',
    '/offline.html',
    '/manifest.json',
    '/favicon.png',
    '/icon-192.png',
    '/icon-512.png',
    '/apple-touch-icon.png',
    '/css/app.css',
    '/lib/bootstrap/dist/css/bootstrap.min.css',
    '/lib/bootstrap/dist/js/bootstrap.bundle.min.js'
];

const OFFLINE_URL = '/offline.html';

self.addEventListener('install', (event) => {
    event.waitUntil((async () => {
        const cache = await caches.open(ASSETS_CACHE);
        await cache.addAll(STATIC_ASSETS);
    })());
    self.skipWaiting();
});

self.addEventListener('activate', (event) => {
    event.waitUntil((async () => {
        const cacheNames = await caches.keys();
        await Promise.all(
            cacheNames
                .filter(name => name !== ASSETS_CACHE && name !== RUNTIME_CACHE)
                .map(name => caches.delete(name))
        );
    })());
    self.clients.claim();
});

self.addEventListener('fetch', (event) => {
    const request = event.request;

    if (request.method !== 'GET') {
        return;
    }

    const url = new URL(request.url);

    if (url.pathname.startsWith('/api/') || url.pathname.startsWith('/health')) {
        event.respondWith(NetworkFirst(request));
        return;
    }

    if (request.mode === 'navigate') {
        event.respondWith(NetworkFirstWithOfflineFallback(request));
        return;
    }

    if (url.pathname.startsWith('/_framework/') ||
        url.pathname.startsWith('/css/') ||
        url.pathname.startsWith('/lib/') ||
        url.pathname.startsWith('/js/') ||
        url.origin === self.location.origin && (
            request.destination === 'image' ||
            request.destination === 'style' ||
            request.destination === 'script'
        )) {
        event.respondWith(CacheFirst(request));
        return;
    }

    event.respondWith(CacheFirst(request));
});

async function CacheFirst(request) {
    const cachedResponse = await caches.match(request);
    if (cachedResponse) {
        return cachedResponse;
    }
    try {
        const networkResponse = await fetch(request);
        if (networkResponse.ok && networkResponse.type !== 'opaque') {
            const cache = await caches.open(RUNTIME_CACHE);
            cache.put(request, networkResponse.clone());
        }
        return networkResponse;
    } catch (err) {
        if (request.destination === 'image') {
            return new Response('', { status: 408, statusText: 'Offline Image Placeholder' });
        }
        throw err;
    }
}

async function NetworkFirst(request) {
    try {
        const networkResponse = await fetch(request);
        return networkResponse;
    } catch (err) {
        const cachedResponse = await caches.match(request);
        if (cachedResponse) {
            return cachedResponse;
        }
        throw err;
    }
}

async function NetworkFirstWithOfflineFallback(request) {
    try {
        const networkResponse = await fetch(request);
        if (networkResponse.ok) {
            const cache = await caches.open(RUNTIME_CACHE);
            cache.put(request, networkResponse.clone());
        }
        return networkResponse;
    } catch (err) {
        const cachedResponse = await caches.match(request);
        if (cachedResponse) {
            return cachedResponse;
        }
        const offlineCache = await caches.open(ASSETS_CACHE);
        return offlineCache.match(OFFLINE_URL);
    }
}

self.addEventListener('message', (event) => {
    if (event.data === 'SKIP_WAITING') {
        self.skipWaiting();
    }
});
