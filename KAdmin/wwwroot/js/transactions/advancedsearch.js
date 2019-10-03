var KTable = Vue.component('k-table', {
    template: "#transaction-table",
    name: "KTable",

    props: {
        "columns": {
            type: Array,
            required: true
        },
        "orders": {
            type: Array,
            required: true
        },
        "ordersToShow": {
            type: Number,
            required: true
        },
        "searchQuery": {
            type: String,
            required: true
        },
        "selectTransaction": {
            type: Function,
            required: true
        }
    },

    data: function () {
        return {
            paginate: ['filteredList'],
            isAscending: false,
            ordersList: this.orders
        };
    },

    watch: {

        'orders': function () {
            this.ordersList = this.orders;
        }

    },

    computed: {

        columnNames: function () {

            var updateColumn = function (columnName) {
                return {
                    columnName: columnName,
                    isAscending: true,
                    isActive: false
                }
            }

            return this.columns.map(updateColumn);

        },

        filteredList: function () {
            var self = this,
                columnsNames = self.columns,
                searchQuery = self.searchQuery,
                regexForSearch = new RegExp(searchQuery, 'ig');

            var filterorders = function (order) {
                var isMatched = false;

                columnsNames.forEach(function (columnName) {
                    isMatched = isMatched || regexForSearch.test(order[columnName]);
                });

                return isMatched;
            };

            return self.ordersList.filter(filterorders);
        },

        showPaginate: function () {
            var filteredList = this.filteredList;
            return filteredList ? filteredList.length > 0 : false;
        }

    },

    methods: {

        formatName: function (propertyName) {
            return _.startCase(propertyName);
        },

        sort: function (column) {
            var self = this,
                isAscending = column.isAscending,
                type = isAscending ? 'asc' : 'desc';


            self.resetAllColumnsState();

            Vue.set(column, 'isActive', true);

            var isDate = compareString(column.columnName, 'date');

            var orderedList = _.orderBy(self.ordersList, isDate ? 'CreatedOn' : column.columnName, type);

            Vue.set(column, 'isAscending', !isAscending);

            Vue.set(self, 'ordersList', orderedList);
        },

        setColumnForSorting: function (column) {
            debugger;
            column.random = new Date();
            Vue.set(this, 'sortColumn', column);

        },

        resetAllColumnsState: function () {
            var columns = this.columnNames;

            var resetIsActive = function (column) {
                column.isActive = false;
                column.isAscending = true;
            }

            columns.forEach(resetIsActive);
        },

        formatDate: function (date) {
            date = new moment(date).format('llll');
            return date;
        },

        formatTransactionId: function (transactionId) {

            if (transactionId && !compareString(transactionId, "bsonnull")) {
                return transactionId;
            }

            return "-- -- --";

        },

        formatAmount: function (amount) {
            if (amount != null && amount != undefined) {
                return accounting.format(amount, 2);
            }
            return 0.00;
        }
    }

});