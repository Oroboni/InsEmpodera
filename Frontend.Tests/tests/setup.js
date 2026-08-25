import { afterEach, beforeEach, vi } from 'vitest';

const nativeDocumentAddEventListener = document.addEventListener;
const nativeDocumentRemoveEventListener = document.removeEventListener;
const nativeWindowAddEventListener = window.addEventListener;
const nativeWindowRemoveEventListener = window.removeEventListener;
const nativeAlert = window.alert;
const nativeConfirm = window.confirm;
const nativeMutationObserve = MutationObserver.prototype.observe;
let globalEventListeners = [];
let activeObservers = [];

beforeEach(() => {
  document.head.innerHTML = '';
  document.body.innerHTML = '';
  window.history.replaceState(null, '', '/');
  window.localStorage.clear();
  window.sessionStorage.clear();

  delete window.currentStep;
  delete window.pageInit;
  delete window.__empoderaMaps;
  delete window.atoresDisponiveis;
  delete window.countEquipe;
  delete window.countInst;
  delete window.initMapSelector;
  delete window.setPageState;
  delete window.fecharModal;
  delete window.removerItemGrid;
  delete window.translateText;

  window.alert = nativeAlert;
  window.confirm = nativeConfirm;

  globalEventListeners = [];
  activeObservers = [];
  vi.spyOn(document, 'addEventListener').mockImplementation((type, listener, options) => {
    globalEventListeners.push({ target: document, type, listener, options });
    return nativeDocumentAddEventListener.call(document, type, listener, options);
  });
  vi.spyOn(window, 'addEventListener').mockImplementation((type, listener, options) => {
    globalEventListeners.push({ target: window, type, listener, options });
    return nativeWindowAddEventListener.call(window, type, listener, options);
  });
  vi.spyOn(MutationObserver.prototype, 'observe').mockImplementation(function (...args) {
    activeObservers.push(this);
    return nativeMutationObserve.apply(this, args);
  });

  window.alert = vi.fn();
  window.confirm = vi.fn(() => true);
});

afterEach(() => {
  for (const { target, type, listener, options } of globalEventListeners) {
    if (target === document) {
      nativeDocumentRemoveEventListener.call(document, type, listener, options);
    } else {
      nativeWindowRemoveEventListener.call(window, type, listener, options);
    }
  }
  for (const observer of activeObservers) observer.disconnect();
  if (vi.isFakeTimers()) vi.clearAllTimers();
  vi.useRealTimers();
  document.body.innerHTML = '';
});