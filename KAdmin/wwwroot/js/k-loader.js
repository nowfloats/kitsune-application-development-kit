var KLoader = Vue.component('k-loader', {
    template: '#k-loader-component',
    name: 'KLoader',
    data: function () {
        return {
            loaderSize: null,
        }
    },
    props: [
        'size',
        'color',
    ],
    computed: {
        getLoaderSize: function () {
            var self = this;

        }
    }
});