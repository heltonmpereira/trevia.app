const DB_NAME = 'TreviaAppDB';
const DB_VERSION = 2;

const STORES = {
    WORKOUTS: 'workouts_in_progress',
    SYNC_QUEUE: 'sync_queue'
};

function getDb() {
    return new Promise((resolve, reject) => {
        if (!window.indexedDB) {
            reject(new Error('IndexedDB not supported'));
            return;
        }
        const request = indexedDB.open(DB_NAME, DB_VERSION);
        request.onerror = () => reject(request.error);
        request.onsuccess = () => resolve(request.result);
        request.onupgradeneeded = (event) => {
            const db = event.target.result;

            if (!db.objectStoreNames.contains(STORES.WORKOUTS)) {
                const ws = db.createObjectStore(STORES.WORKOUTS, { keyPath: 'userId' });
                ws.createIndex('savedAt', 'savedAt', { unique: false });
            }

            if (!db.objectStoreNames.contains(STORES.SYNC_QUEUE)) {
                const sq = db.createObjectStore(STORES.SYNC_QUEUE, { keyPath: 'id' });
                sq.createIndex('userId', 'userId', { unique: false });
                sq.createIndex('status', 'status', { unique: false });
                sq.createIndex('createdAt', 'createdAt', { unique: false });
            }
        };
    });
}

export async function saveWorkout(userId, data) {
    const db = await getDb();
    return new Promise((resolve, reject) => {
        const tx = db.transaction(STORES.WORKOUTS, 'readwrite');
        const store = tx.objectStore(STORES.WORKOUTS);
        const record = {
            userId,
            ...data,
            savedAt: new Date().toISOString(),
            version: DB_VERSION
        };
        store.put(record);
        tx.oncomplete = () => resolve(true);
        tx.onerror = () => reject(tx.error);
    });
}

export async function loadWorkout(userId) {
    const db = await getDb();
    return new Promise((resolve, reject) => {
        const tx = db.transaction(STORES.WORKOUTS, 'readonly');
        const store = tx.objectStore(STORES.WORKOUTS);
        const request = store.get(userId);
        request.onsuccess = () => {
            const record = request.result;
            if (record && record.version !== DB_VERSION) {
                console.warn('Workout schema mismatch, clearing old data');
                resolve(null);
            } else {
                resolve(record || null);
            }
        };
        request.onerror = () => reject(request.error);
    });
}

export async function clearWorkout(userId) {
    const db = await getDb();
    return new Promise((resolve, reject) => {
        const tx = db.transaction(STORES.WORKOUTS, 'readwrite');
        const store = tx.objectStore(STORES.WORKOUTS);
        store.delete(userId);
        tx.oncomplete = () => resolve(true);
        tx.onerror = () => reject(tx.error);
    });
}

export async function hasWorkout(userId) {
    const record = await loadWorkout(userId);
    return !!record;
}

export async function enqueueSync(item) {
    const db = await getDb();
    return new Promise((resolve, reject) => {
        const tx = db.transaction(STORES.SYNC_QUEUE, 'readwrite');
        const store = tx.objectStore(STORES.SYNC_QUEUE);
        store.put({
            ...item,
            status: 'pending',
            retryCount: 0,
            lastError: null,
            createdAt: new Date().toISOString()
        });
        tx.oncomplete = () => resolve(true);
        tx.onerror = () => reject(tx.error);
    });
}

export async function getPendingSync(userId) {
    const db = await getDb();
    return new Promise((resolve, reject) => {
        const tx = db.transaction(STORES.SYNC_QUEUE, 'readonly');
        const store = tx.objectStore(STORES.SYNC_QUEUE);
        const index = store.index('userId');
        const items = [];
        const request = index.openCursor(IDBKeyRange.only(userId));
        request.onsuccess = (event) => {
            const cursor = event.target.result;
            if (cursor) {
                items.push(cursor.value);
                cursor.continue();
            } else {
                resolve(items.filter(i => i.status === 'pending' || i.status === 'failed'));
            }
        };
        request.onerror = () => reject(request.error);
    });
}

export async function getAllSyncStatus(userId) {
    const db = await getDb();
    return new Promise((resolve, reject) => {
        const tx = db.transaction(STORES.SYNC_QUEUE, 'readonly');
        const store = tx.objectStore(STORES.SYNC_QUEUE);
        const index = store.index('userId');
        const items = [];
        const request = index.openCursor(IDBKeyRange.only(userId));
        request.onsuccess = (event) => {
            const cursor = event.target.result;
            if (cursor) {
                items.push(cursor.value);
                cursor.continue();
            } else {
                resolve({
                    pending: items.filter(i => i.status === 'pending').length,
                    processing: items.filter(i => i.status === 'processing').length,
                    failed: items.filter(i => i.status === 'failed').length,
                    completed: items.filter(i => i.status === 'completed').length,
                    failedItems: items.filter(i => i.status === 'failed').map(i => ({
                        id: i.id,
                        operationType: i.operationType,
                        lastError: i.lastError
                    }))
                });
            }
        };
        request.onerror = () => reject(request.error);
    });
}

export async function updateSyncItem(id, changes) {
    const db = await getDb();
    return new Promise((resolve, reject) => {
        const tx = db.transaction(STORES.SYNC_QUEUE, 'readwrite');
        const store = tx.objectStore(STORES.SYNC_QUEUE);
        const req = store.get(id);
        req.onsuccess = () => {
            const item = req.result;
            if (!item) { reject(new Error('Item not found')); return; }
            Object.assign(item, changes);
            store.put(item);
        };
        tx.oncomplete = () => resolve(true);
        tx.onerror = () => reject(tx.error);
    });
}

export async function clearCompletedSync(userId) {
    const db = await getDb();
    return new Promise((resolve, reject) => {
        const tx = db.transaction(STORES.SYNC_QUEUE, 'readwrite');
        const store = tx.objectStore(STORES.SYNC_QUEUE);
        const index = store.index('userId');
        const request = index.openCursor(IDBKeyRange.only(userId));
        request.onsuccess = (event) => {
            const cursor = event.target.result;
            if (cursor) {
                if (cursor.value.status === 'completed') {
                    cursor.delete();
                }
                cursor.continue();
            } else {
                resolve(true);
            }
        };
        request.onerror = () => reject(request.error);
    });
}

export function isOnline() {
    return navigator.onLine;
}

export function onOnlineChange(callback) {
    const handler = () => callback(isOnline());
    window.addEventListener('online', handler);
    window.addEventListener('offline', handler);
    return () => {
        window.removeEventListener('online', handler);
        window.removeEventListener('offline', handler);
    };
}
