var AdvancedSearch = Vue.component('advanced-search', {
    template: "#advancedSearch",
    name: "AdvanceSearch",

    data: function () {
        return {

            filters: {
                mobileNumber: "",
                status: "",
                startDate: "",
                endDate: ""
            },

            configStartDate: {
                disableMobile: true
            }
        };
    },

    computed: {

        config: function () {
            var self = this;
            var configForEndDate = {
                disableMobile: true
            }

            if (self.filters.startDate) {
                configForEndDate["minDate"] = self.filters.startDate;
                return configForEndDate;
            }

            return configForEndDate;
        },

        showClearButton: function () {
            var self = this;
            var filters = self.filters;

            return !!(filters.mobileNumber || filters.status || filters.startDate || filters.endDate);
        }

    },

    watch: {
        'filters': {
            handler: function () {
                this.updateFilters(this.filters);
            },
            deep: true
        }
    },

    props: {
        "updateFilters": {
            type: Function,
            required: true
        }
    },

    methods: {

        formatText: function (text) {
            return _.startCase(_.toLower(text));
        },

        clearAdvanceSearch: function () {
            var self = this;
            var filters = self.filters;
            filters.mobileNumber = "";
            filters.status = "";
            filters.startDate = "";
            filters.endDate = "";
        }

    }
})