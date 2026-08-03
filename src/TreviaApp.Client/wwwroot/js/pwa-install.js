let deferredPrompt = null;
let dotNetHelper = null;

function checkStandaloneInternal() {
    return (window.matchMedia('(display-mode: standalone)').matches) ||
           (window.navigator.standalone) ||
           document.referrer.includes('android-app://');
}

export async function registerInstallListener(dotNetRef) {
    dotNetHelper = dotNetRef;

    window.addEventListener('beforeinstallprompt', (e) => {
        e.preventDefault();
        deferredPrompt = e;
        if (dotNetHelper) {
            dotNetHelper.invokeMethodAsync('SetCanInstall', true);
        }
    });

    window.addEventListener('appinstalled', () => {
        deferredPrompt = null;
        if (dotNetHelper) {
            dotNetHelper.invokeMethodAsync('SetCanInstall', false);
        }
    });

    return !!deferredPrompt;
}

export function isStandalone() {
    return checkStandaloneInternal();
}

export async function showInstallPrompt() {
    if (!deferredPrompt) return false;
    try {
        await deferredPrompt.prompt();
        const choiceResult = await deferredPrompt.userChoice;
        deferredPrompt = null;
        return choiceResult.outcome === 'accepted';
    } catch (err) {
        console.error('Error showing install prompt:', err);
        return false;
    }
}
