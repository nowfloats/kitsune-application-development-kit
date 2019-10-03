var lsItemKeys = Object.freeze({
    'IS_SIDEBAR_COLLAPSED': 'IS_SIDEBAR_COLLAPSED'
});

var _sideBarResizeCallback = null;

var tooltips = null;

function toggleSidebarCollapse(currentState) {
    var containerElement = document.getElementById('wrapper');
    var sidebarElement = document.getElementById('sidebar-wrapper');
    var containerClassName = 'sidebar-collapsed-container';
    var sidebarClassName = 'collapsed';

    var isCollapsed = localStorage.getItem(lsItemKeys['IS_SIDEBAR_COLLAPSED']);

    if (currentState !== null && currentState !== undefined) {
        toggleSidebar(currentState);
    } else {
        var state = isCollapsed === 'false' ? 'true' : 'false';
        toggleSidebar(state);
        localStorage.setItem(lsItemKeys['IS_SIDEBAR_COLLAPSED'], state);
    }

    function toggleSidebar(toggled) {
        toggleTooltips(toggled);
        if (!sidebarElement) {
            return;
        }
        if (toggled === 'false') {
            containerElement.classList.remove(containerClassName);
            sidebarElement.classList.remove(sidebarClassName);
        } else {
            containerElement.classList.add(containerClassName);
            sidebarElement.classList.add(sidebarClassName);
        }
    }

    if (_sideBarResizeCallback) {
        _sideBarResizeCallback();
    }
    
}

function toggleTooltips(create) {
    try {
        if (create !== 'false') {
            tooltips = Tippy('[data-tippy]', {
                placement: 'right',
                distance: -80,
                duration: [200, 200],
                animation: 'fade'
            })
        } else {
            if (tooltips && tooltips.destroyAll) {
                tooltips.destroyAll();
            }
        }
    }
    catch (e) {
        console.log('tooltip :', e);
    }

}

function initializeSidebar() {
    var sidebarState = lsItemKeys['IS_SIDEBAR_COLLAPSED'];
    var currentState = localStorage.getItem(sidebarState);

    if (currentState === null || currentState === undefined) {
        currentState = false;
        localStorage.setItem(lsItemKeys['IS_SIDEBAR_COLLAPSED'], currentState);
    }
    toggleSidebarCollapse(currentState);

    //$(function () {
    //    $('[data-toggle="tooltip"]').tooltip({ container: 'body'});
    //});
}

initializeSidebar();