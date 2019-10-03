var KHeader = Vue.component("calls-detail", {
    template: "#kheader",
    name: "KHeader",

    data: function () {
        return {
            customerName: "",
            websiteUrl: ""
        };
    },

    mounted() {
        this.fetchCustomerDetails();
        this.subscribeToEventForRefresh();
    },

    computed: {

        getCustomerName: function () {
            var name = this.customerName;
            if (name && name.trim()) {
                return name;
            }
            return ' ... ';
        },

        getWebsiteUrl: function () {
            var domainUrl = this.websiteUrl;

            if (domainUrl && domainUrl.trim()) {
                domainUrl = _.toLower(domainUrl);
                return domainUrl;
            }
            return "";
        }
    },

    methods: {

        fetchCustomerDetails: function () {
            
            var successCallback = function (res) {
                if (res && res.data) {
                    this.updateCustomerDetails(res.data);
                }
            }.bind(this);

            var errorCallback = function (err) {
                console.log(err);
            }.bind(this);

            axios.get(Endpoints.GetCustomerName, { responseType: 'json' })
                .then(successCallback)
                .catch(errorCallback);
        },

        updateCustomerDetails: function (customerDetails) {
            Vue.set(this, 'customerName', customerDetails.customerName);
            Vue.set(this, 'websiteUrl', customerDetails.websiteUrl);
        },

        previewHandler: function (event) {
            event.preventDefault();
            event.stopPropagation();
            
            var websiteUrl = this.getWebsiteUrl;
            var date = (new Date()).getTime();
            var protocol = window.location.protocol;

            if (!websiteUrl) {
                websiteUrl = window.location.origin;
            }

            window.open(protocol + "//" + websiteUrl + "?v=" + date);
        },

        subscribeToEventForRefresh: function () {
            var eventHandler = this.fetchCustomerDetails.bind(this);
            window.addEventListener('customerRefreshed', eventHandler);
        },

        showGenericHelpModal: function () {
            var element = document.querySelector('.tippy-popper');
            element.parentNode.removeChild(element);
            if (window.hasOwnProperty('GWTAnalytics')) {
                GWTAnalytics.genericHelpModal.modalShowStatus = true;
            } else if (window.hasOwnProperty('vm')) {
                if (vm.hasOwnProperty('hasSchema')) {
                    vm.showGenericHelpModal();
                } else {
                    vm.genericHelpModal.modalShowStatus = true;
                }

                /*else if (vm.hasOwnProperty('orders')) {
                    vm.genericHelpModal.modalShowStatus = true;
                } else if (vm.hasOwnProperty('callLogs')) {
                    vm.genericHelpModal.modalShowStatus = true;
                }*/
            } 
        },
    }
});

var KConsoleModeHeader = Vue.component('k-console-mode-header', {
    template: '#console-mode-header-component',
    name: 'KConsoleModeHeader',
});