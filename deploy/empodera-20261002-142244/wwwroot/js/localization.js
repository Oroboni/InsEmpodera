(() => {
    let catalog = {};
    let entries = [];
    const localizableUiSelector = '[data-i18n-text], button, a.wizard-btn';
    const nativeAlert = window.alert.bind(window);
    const nativeConfirm = window.confirm.bind(window);

    function translate(text) {
        if (typeof text !== 'string') return text;
        return entries.reduce((result, [source, translation]) =>
            result.replaceAll(source, translation), text);
    }

    window.translateText = translate;
    window.alert = message => nativeAlert(translate(message));
    window.confirm = message => nativeConfirm(translate(message));

    const catalogUrl = `/Language/Catalog?page=${encodeURIComponent(window.location.pathname)}`;
    fetch(catalogUrl, { credentials: 'same-origin' })
        .then(response => response.ok ? response.json() : {})
        .then(translations => {
            catalog = translations;
            entries = Object.entries(catalog)
                .sort(([left], [right]) => right.length - left.length);

            const translateNode = node => {
                if (node.nodeType === Node.TEXT_NODE) {
                    if (node.parentElement?.closest(localizableUiSelector))
                        node.textContent = translate(node.textContent);
                    return;
                }

                if (node.nodeType === Node.ELEMENT_NODE) {
                    ['placeholder', 'title', 'aria-label', 'alt', 'data-tooltip'].forEach(attribute => {
                        if (node.hasAttribute(attribute)) {
                            const current = node.getAttribute(attribute);
                            const localized = translate(current);
                            if (localized !== current) node.setAttribute(attribute, localized);
                        }
                    });
                    if (node.matches(localizableUiSelector))
                        node.childNodes.forEach(translateNode);

                    node.querySelectorAll?.(localizableUiSelector).forEach(element =>
                        element.childNodes.forEach(translateNode));
                }
            };

            const observer = new MutationObserver(mutations => {
                for (const mutation of mutations) {
                    if (mutation.type === 'attributes') {
                        const attribute = mutation.attributeName;
                        const current = mutation.target.getAttribute(attribute);
                        const localized = translate(current);
                        if (localized !== current) mutation.target.setAttribute(attribute, localized);
                    }
                    mutation.addedNodes.forEach(translateNode);
                }
            });

            // Only explicitly marked UI labels are translated here. Form values,
            // user content and data attributes are deliberately left untouched.
            document.querySelectorAll(localizableUiSelector).forEach(element =>
                element.childNodes.forEach(translateNode));

            observer.observe(document.body, {
                childList: true,
                subtree: true,
                attributes: true,
                attributeFilter: ['placeholder', 'title', 'aria-label', 'alt', 'data-tooltip']
            });
        })
        .catch(() => { /* Portuguese source text remains as a safe fallback. */ });
})();
