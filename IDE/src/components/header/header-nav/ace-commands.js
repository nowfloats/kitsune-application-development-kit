const isMac = navigator.appVersion.indexOf('Mac') !== -1;
export const controlKey = isMac ? '⌘' : 'ctrl+';
export const altControlKey = isMac ? '⌃' : 'ctrl+';
export const shiftKey = isMac ? '⇧' : 'shift+';
export const altKey = isMac ? '⌥' : 'alt+';

export const supportedAceCommands = [
    [{
        desc: 'go to start',
        command: 'gotostart',
        shortcut: `${controlKey}Home`
    },
    {
        desc: 'go to line',
        command: 'gotoline',
        shortcut: `${controlKey}L`
    },
    {
        desc: 'go to matching',
        command: 'jumptomatching',
        shortcut: isMac ? '' : `${controlKey}P`
    }],
    [{
        desc: 'fold/expand',
        command: 'fold',
        shortcut: `${isMac ? controlKey : ''}${altKey}L`
    },
    {
        desc: 'fold all',
        command: 'foldall',
        shortcut: `${isMac ? controlKey : ''}${altControlKey}${altKey}0`
    },
    {
        desc: 'expand all',
        command: 'unfoldall',
        shortcut: `${isMac ? controlKey : ''}${shiftKey}${altKey}0`
    }],
    [{
        desc: 'indent',
        command: 'indent',
        shortcut: `Tab`
    },
    {
        desc: 'block indent',
        command: 'blockindent',
        shortcut: `${altControlKey}]`
    }],
    {
        desc: 'remove line',
        command: 'removeline',
        shortcut: `${controlKey}D`
    },
];