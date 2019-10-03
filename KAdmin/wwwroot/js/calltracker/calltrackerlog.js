var TrackerLog = Vue.component('k-table', {
    template: "#callTracker",
    name: "TrackerLog",

    data: function () {
        return {
            dateTimeColumn: {
                isActive: false,
                isAscending: true
            },
            logsPerPage: 10,
            paginate: ['LogsForAllCalls'],
            callLogs: this.callsLog
        };
    },

    props: ['callsLog'],

    computed: {

        LogsForAllCalls: function () {
            return this.callLogs;
        },

        showData: function () {
            var callsLog = this.callsLog;
            return callsLog ? (callsLog.length > 0) : false;
        }

    },

    watch: {

        'callsLog': function () {
            this.callLogs = this.callsLog;
        }

    },

    methods: {

        formatText: function (text) {
            return _.startCase(_.toLower(text));
        },

        formatDate: function (date) {
            if (date) {
                return new moment(date).format('llll');
            }
            return "";
        },

        sortDateTimeColumn() {

            var self = this;
            var column = self.dateTimeColumn;
            var sortType = column.isAscending ? 'asc' : 'desc'

            Vue.set(column, 'isActive', true);

            var orderedList = _.orderBy(self.callLogs, 'callDateTime', sortType);
            
            Vue.set(column, 'isAscending', !column.isAscending);

            Vue.set(self, 'callLogs', orderedList);
        },

        isValidUrl(url) {
            // api returns None is call tracker logs

            var isValid = url && _.toLower(url) != "none";
            if (isValid) {
                return url;
            }

            return "";
        }

    }
})