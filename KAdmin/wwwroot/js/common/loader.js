function getLoader() {
    var loader = document.createElement('div');
    loader.classList = 'loader loader-default is-active';
    loader.id = 'loader';
    return loader;
}

function showLoader(element) {
    if (!element) {
        console.log('loader : element not valid', element);
        return;
    }

    removeLoader(element);
    element.append(getLoader());
}

function removeLoader(element, removeAllLoaders) {
    var loader = getLoader();

    if (element && !removeAllLoaders) {

        var loaderInElement = element.querySelector('#' + loader.id);

        if (loaderInElement) {
            loaderInElement.remove();
        }

    }

    if (removeAllLoaders) {

        var loaders = document.querySelectorAll('#' + loader.id);
        var removeLoader = function (element) {
            element.remove();
        }

        if (loaders && loaders.length) {
            _.forEach(loaders, removeLoader);
        }
    }
}