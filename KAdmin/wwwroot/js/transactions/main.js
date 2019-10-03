const APIEndpoints = Object.freeze({
    GetData: 'Orders/GetOrders',
    GetGateways: 'Orders/GetGateways',
    GetWebsiteUserDetails: '/k-admin/Settings/GetWebsiteUserDetails',
    GetDeveloperDetails: '/k-admin/ManageWebsiteContent/GetDeveloperDetails',
    UploadFileToSystemWebaction: '/k-admin/Inbox/UploadFileToSystemWebaction',
    SendEmail: '/k-admin/ManageWebsiteContent/SendEmail',
})

const Endpoints = Object.freeze({
    GetCustomerName: 'Orders/GetCustomerName'
});

Vue.component('k-header', KHeader);
Vue.use('KTable', KTable);
Vue.use('KModal', KModal);
Vue.use(VuePaginate);
Vue.component('flat-pickr', VueFlatpickr);

var vm = new Vue({
    el: "#app",

    data: {

        orders: [],

        table: {
            columns: ["Date", "transaction_id", "amount", "buyer_name", "status", "_payment_gateway", "description"],
            ordersToShow: 10
        },

        columnsForSearch: ["transaction_id", "amount", "buyer_name", "status", "_payment_gateway", "buyer_email", "buyer_phone"],

        paymentStatus: ["Success", "Fail", "Inprogress"],

        searchQuery: "",
        gateways: [],
        isFetchingData: true,
        isFetchingGateways: true,
        showWarning: true,
        modalShowStatus: {
            transactionDetails: false
        },
        selectedTransaction: null,

        advancedSearch: {
            query: "",
            field: "",
            startDate: "",
            endDate: "",
            gateway: "",
            paymentStatus: ""
        },

        configStartDate: {
            disableMobile: true
        },

        isAdvancedSearchVisible: false,
        isAdvanceSearchEnabled: false,

        genericHelpModal: {
            modalShowStatus: false,
            supportEmailForm: {
                subject: 'Reporting an issue for: ' + localStorage.getItem('DOMAIN'),
                message: '',
                image: null,
                to: [],
                clientId: null
            },
        },

        customerData: null,

        systemWebactions: {
            supportEmail: { name: 'kadminsupportemail', authId: '5959ec985d643701d48ee8ab' }
        },
    },

    created: function () {
        //this.getGatewaysFromAPI();
        this.getOrdersFromAPI();
        this.getUserData();
    },

    computed: {

        totalAmount: function () {

            var self = this,
                orders = self.orders,
                total = 0;

            var computeTotalAmount = function (order) {
                var amount = 0;

                if (compareString(order.status, "success")) {
                    amount = parseFloat(order.amount);
                    if (amount != NaN) {
                        total += amount;
                    }
                }
            }

            orders.forEach(computeTotalAmount);

            return accounting.formatMoney(total, { symbol: '₹', format : "%s %v" });

        },

        totalFilteredAmount: function () {
            var self = this,
                total = 0,
                orders = self.advancedSearchResult;

            var computeTotalAmount = function (order) {
                var amount = 0;

                if (compareString(order.status, "success")) {
                    amount = parseFloat(order.amount);
                    if (amount != NaN) {
                        total += amount;
                    }
                }
            }

            orders.forEach(computeTotalAmount);

            return accounting.formatMoney(total, { symbol: '₹', format: "%s %v" });
        },

        filteredOrders: function () {
            var self = this,
                columnsNames = self.table.columns,
                searchQuery = self.searchQuery,
                regexForSearch = new RegExp(searchQuery, 'i');

            var filterorders = function (order) {
                var isMatched = false;

                columnsNames.forEach(function (columnName) {
                    isMatched = isMatched || regexForSearch.test(order[columnName]);
                });

                return isMatched;
            };

            return self.orders.filter(filterorders);
        },

        isDataAvailable: function () {
            var self = this;
            return self.orders ? (self.orders.length ? true : false) : false;
        },

        isKpayEnabled: function () {
            var self = this;
            return self.gateways ? (self.gateways.length ? true : false) : false;
        },

        gatewaysEnabled: function () {
            var gateways = this.gateways;
            return gateways;
        },

        advancedSearchResult: function () {
            var self = this,
                advancedProperties = self.advancedSearch,
                orders = self.orders;

            var queryRegex = self.getStringRegex(advancedProperties.query);
            var startDate = self.getTimeFromDate(advancedProperties.startDate, false);
            var endDate = self.getTimeFromDate(advancedProperties.endDate, true);
            var selectedField = advancedProperties.field;
            var selectedGateway = advancedProperties.gateway ? _.lowerCase(advancedProperties.gateway) : "";
            var paymentStatus = advancedProperties.paymentStatus ? _.lowerCase(advancedProperties.paymentStatus) : "";

            var filterorders = function (order) {
                var isMatched = true,
                    date = Date.parse(order.CreatedOn) ? (new Date(order.CreatedOn)).getTime() : false;


                if (advancedProperties.query || selectedField) {
                    
                    if (advancedProperties.query && selectedField) {
                        isMatched = isMatched && (queryRegex.test(order[selectedField]));
                    } else if (advancedProperties.query) {
                        isMatched = isMatched && self.findInAllColumns(order, queryRegex);
                    }

                }

                if (startDate && date) {
                    isMatched = isMatched && (date >= startDate);
                }

                if (endDate && date) {
                    isMatched = isMatched && (date <= endDate);
                }

                if (selectedGateway) {
                    isMatched = isMatched && (_.lowerCase(order._payment_gateway) == selectedGateway);
                }

                if (paymentStatus) {
                    isMatched = isMatched && (_.lowerCase(order.status) == paymentStatus);
                }
                
                return isMatched;
            };

            return orders.filter(filterorders);

        },

        config: function () {
            var self = this,
                startDate = self.advancedSearch.startDate;

            var configForEndDate = {
                disableMobile: true
            }

            if (startDate) {
                configForEndDate["minDate"] = startDate;
                return configForEndDate;
            }

            return configForEndDate;
        },

        showClearAllButton: function () {
            var self = this,
                advancedSearch = self.advancedSearch;

            var query = advancedSearch.query ? advancedSearch.query.trim() : "",
                field = advancedSearch.field ? advancedSearch.field.trim() : "",
                startDate = advancedSearch.startDate ? advancedSearch.startDate.trim() : "",
                endDate = advancedSearch.endDate ? advancedSearch.endDate.trim() : "",
                gateway = advancedSearch.gateway ? advancedSearch.gateway.trim() : "",
                paymentStatus = advancedSearch.paymentStatus ? advancedSearch.paymentStatus.trim() : "";

            return !!(query || field || startDate || endDate || gateway || paymentStatus);
        }

    },

    methods: {
        getGatewaysFromAPI: function () {
            var self = this;

            self.showHideLoader(true);
            axios.post(APIEndpoints.GetGateways)
                .then(function (response) {
                    self.getGateways(null, response.data);
                })
                .catch(function (error) {
                    self.getGateways(error, null);
                })

        },

        getGateways: function (error, response) {
            this.showHideLoader(false);
            Vue.set(this, 'isFetchingGateways', false);

            if (error) {
                console.log('error on fetching gateways');
                return;
            }

            if (compareString(response.status, 'success')) {
                Vue.set(this, 'gateways', response.gateways);
            }
            
        },

        getOrdersFromAPI: function () {
            var self = this;

            self.showHideLoader(true);

            axios.post(APIEndpoints.GetData)
                .then(function (res) {
                    self.getOrders(null, res.data);
                    self.getGatewaysFromAPI();
                })
                .catch(function (err) {
                    self.getOrders(err);
                })

        },

        getOrders: function (err, response) {
            var self = this;

            Vue.set(self, 'isFetchingData', false);
            self.showHideLoader(false);

            if (err) {
                console.log('error in getting Orders');
                return;
            }

            var orders = response.Data ? response.Data : [];
            orders = _.orderBy(orders, "CreatedOn", 'desc');
            Vue.set(this, 'orders', response.Data);


            setTimeout(function () {
                flatpickr('#fromDate', {});
                flatpickr('#toDate', {
                    "disable": [
                        function (date) {

                            var startDate = self.advancedSearchI.startDate;

                            if (startDate && Date.parse(startDate)) {
                                startDate = (new Date(startDate)).getTime();
                                date = (new Date(date)).getTime();
                                return (date < startDate);
                            }
                            return false;

                        }]
                 });
            }, 500);
            
        },

        formatPropertyName: function (propertyName) {
            if (propertyName) {
                return _.startCase(_.lowerCase(propertyName));
            }
            return propertyName;
        },

        showHideLoader: function (show) {
            var app = document.getElementById('app');
            show ? showLoader(app) : removeLoader(app);
        },

        closeWarningModal: function () {
            Vue.set(this, 'showWarning', false)
        },

        showHideTransactionsModal: function (show) {
            var self = this;
            if (show) {
                Vue.set(self.modalShowStatus, 'transactionDetails', true);
            } else {
                Vue.set(self.modalShowStatus, 'transactionDetails', false);
            }
        },

        setSelectedTransaction: function (transaction) {
            var self = this;
            Vue.set(self, 'selectedTransaction', transaction);
        },

        selectTransaction: function (transaction) {
            var self = this;
            self.setSelectedTransaction(transaction);
            self.showHideTransactionsModal(true);
        },

        hideAllModals: function () {
            var self = this;
            self.showHideTransactionsModal(false);
        },

        /*
         * Processing Date for Advanced Properties
         */
        getTimeFromDate: function (date, endOfDay) {

            if (Date.parse(date)) {
                date = new Date(date);

                if (endOfDay) {
                    date.setHours(23, 59, 59, 999); // end of day
                } else {
                    date.setHours(0, 0, 0, 0); // start of day
                }

                return date.getTime();
            }

            return false;
        },

        /*
         * Processing  for Advanced Properties
         */
        getStringRegex: function (text) {
            if (text && text.trim()) {
                return new RegExp(text, "i");
            }
            false;
        },


        resetAdvanceProperty: function () {
            var self = this,
                advancedProperties = self.advancedSearch;
            
            Vue.set(advancedProperties, 'query', '');
            Vue.set(advancedProperties, 'startDate', '');
            Vue.set(advancedProperties, 'endDate', '');
            Vue.set(advancedProperties, 'paymentStatus', '');
            Vue.set(advancedProperties, 'gateway', '');
            Vue.set(advancedProperties, 'field', '');
        },

        findInAllColumns: function (order, regex) {
            var columnsForSearch = this.columnsForSearch,
                isMatch = false;

            var findData = function (columnName) {
                isMatch = isMatch || regex.test(order[columnName]);
            }

            columnsForSearch.forEach(findData)

            return isMatch;
        },

        resetAndHideGenericHelpModal: function () {
            this.genericHelpModal.modalShowStatus = false;
            this.genericHelpModal.supportEmailForm = {
                subject: 'Reporting an issue for: ' + localStorage.getItem('DOMAIN'),
                message: '',
                image: null,
                to: [],
                clientId: null
            };
        },

        sendGenericSupportEmail: function () {
            var self = this;
            self.genericHelpModal.modalShowStatus = false;

            if (!self.isGenericSupportEmailFormValid()) {
                event.preventDefault();
                event.stopPropagation();
                return;
            }

            var emailRequest = {
                To: self.genericHelpModal.supportEmailForm.to,
                Subject: self.genericHelpModal.supportEmailForm.subject,
                EmailBody: self.genericHelpModal.supportEmailForm.message,
                Attachments: [],
                clientId: self.genericHelpModal.supportEmailForm.clientId
            };

            if (self.genericHelpModal.supportEmailForm.image) {
                self.uploadSupportEmailImage(self.genericHelpModal.supportEmailForm.image, function (uploadedUrl) {
                    emailRequest.Attachments = uploadedUrl.error ? [] : [uploadedUrl];
                    self.sendEmail(emailRequest);
                    self.resetAndHideGenericHelpModal();
                });
            } else {
                self.sendEmail(emailRequest);
                self.resetAndHideGenericHelpModal();
            }
        },

        uploadSupportEmailImage: function (image, callback) {
            var self = this;
            var formData = new FormData();
            formData.append('File', image);
            formData.append('WebactionName', self.systemWebactions.supportEmail.name);
            formData.append('AuthId', self.systemWebactions.supportEmail.authId);
            formData.append('FileName', image.name);
            axios({
                method: 'post',
                url: APIEndpoints.UploadFileToSystemWebaction,
                config: { headers: { 'Content-Type': 'multipart/form-data' } },
                data: formData
            }).then(function (response) {
                callback(response.data);
            }).catch(function (error) {
                callback({ error: 'error' });
            });
        },

        isGenericSupportEmailFormValid: function () {
            var self = this;
            return (self.genericHelpModal.supportEmailForm.message != null && self.genericHelpModal.supportEmailForm.message.length > 0);
        },

        genericHelpModalProcessUploadedFile: function (event) {
            var self = this;
            var files = event.target.files || event.dataTransfer.files;
            if (!files.length) {
                return;
            }
            self.genericHelpModal.supportEmailForm.image = files[0];
        },

        sendEmail: function (request) {
            if (request.To.length < 2) {
                return;
            }
            axios({
                method: 'post',
                url: APIEndpoints.SendEmail,
                data: request
            }).then(function (response) {
                toastr.success("", "Successfully sent the email");
            }).catch(function (error) {
                toastr.error("", "Error sending support email");
            });
        },

        getUserData: function () {
            var self = this;

            axios.get(APIEndpoints.GetWebsiteUserDetails, { responseType: 'json' })
                .then(function (response) {
                    self.customerData = response.data;
                    self.genericHelpModal.supportEmailForm.to.push(self.customerData.contact.email);
                    self.getDeveloperDetails(self.customerData.developerId, function (email) {
                        self.genericHelpModal.supportEmailForm.to.push(email);
                    });
                });
        },

        getDeveloperDetails: function (developerId, callback) {
            axios({
                method: 'post',
                url: APIEndpoints.GetDeveloperDetails,
                data: { developerId: developerId }
            }).then(function (response) {
                callback(response.data);
            });
        },
    }
})