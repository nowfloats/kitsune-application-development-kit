var KCollapse = Vue.component('k-collapse', {
    template: '#k-collapse-component',
    name: KCollapse,
    data: function () {
        return {
            isCollapsed: true,
        }
    },
    props: {
        'headerText': {
            type: Function,
        },
        'colors': {
            type: Object,
        },
    },
    computed: {
        getHeaderStyle: function () {
            var textColor = '#323d4a';
            var backgroundColor = '#f7f7f7';
            var iconColor = '#ff9100';
            return { color: headerTextColor, backgroundColor: backgroundColor, iconColor: iconColor };
        }
    },
    methods: {
        toggleCollapse: function () {
            var self = this;
            self.isCollapsed = !self.isCollapsed;
        }
    },
})