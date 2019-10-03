var Endpoints = Object.freeze({
    GetDetailedAnalyticsForDate: '/k-admin/GWTAnalytics/GetDetailedAnalyticsForDate',
    GetDetailedAnalyticsForDateRange: '/k-admin/GWTAnalytics/GetDetailedAnalyticsForDateRange',
    GetDailySearchAnalytics: '/k-admin/GWTAnalytics/GetDailySearchAnalytics',
    GetKitsunePaymentsData: '/k-admin/GWTAnalytics/GetKitsunePaymentsData',
    GetWebActionCount: '/k-admin/Inbox/GetWebActionDataCount',
    GetCustomerName: '/k-admin/ManageWebsiteContent/GetCustomerName',
    GetWebsiteUserDetails: '/k-admin/Settings/GetWebsiteUserDetails',
    GetDeveloperDetails: '/k-admin/ManageWebsiteContent/GetDeveloperDetails',
    UploadFileToSystemWebaction: '/k-admin/Inbox/UploadFileToSystemWebaction',
    SendEmail: '/k-admin/ManageWebsiteContent/SendEmail',
    GetVisitors: '/k-admin/Analytics/GetVisitors',
    GetTrafficSources: '/k-admin/Analytics/GetTrafficSources',
    GetDevices: '/k-admin/Analytics/GetDevices',
    GetBrowsers: '/k-admin/Analytics/GetBrowsers',
});

var CardTypes = Object.freeze({
    orders: 'ORDERS',
    visitors: 'VISITORS',
    sales: 'SALES',
    inbox: 'INBOX'
});

var datepickerOptions = {
    triggerElementId: 'trigger-range',
    sundayFirst: true,
    texts: {
        apply: 'confirm'
    },
    colors: {
        selected: '#ff9100',
        inRange: '#FFC51F',
        inRangeBorder: '#FFFFFF'
    },
    endDate: new Date(),
};

Vue.use(window.AirbnbStyleDatepicker, datepickerOptions);
Vue.use(VueTippy);
Vue.component('k-header', KHeader);
Vue.use(VuePaginate);
Vue.component(KLoader);
Vue.component('KModal', KModal);

var GWTAnalytics = new Vue({
    el: '#app',
    data: function () {
        return {
            overviewCardTypes: {
                orders: 0,
                visitors: 1,
                sales: 2,
                inbox: 3
            },

            overviewCards: {
                "visitors": {
                    title: 'Monthly Visitors',
                    figure: 0,
                    iconClass: 'user-icon',
                    conversionClass: 'conversion-down'
                },
                "orders": {
                    title: 'Monthly Orders',
                    figure: 0,
                    iconClass: 'graph-icon',
                    conversionClass: 'conversion-up'
                },
                "sales": {
                    title: 'Monthly Sales',
                    figure: 0,
                    iconClass: 'money-bag-icon',
                    conversionClass: 'conversion-up'
                },
                "inbox": {
                    title: 'Inbox',
                    figure: 0,
                    iconClass: 'mail-icon',
                    conversionClass: 'conversion-up'
                }
            },

            selectedDateRange: {
                startDate: moment(new Date()).subtract(28, 'days').format("YYYY-MM-DD"),
                endDate: moment(new Date()).format("YYYY-MM-DD"),
                startDateApplied: moment(new Date()).subtract(28, 'days').format("YYYY-MM-DD"),
                endDateApplied: moment(new Date()).format("YYYY-MM-DD")
            },

            searchAnalyticsData: {
                tableData: [],
                cardsAggregateData: {},
            },

            paginate: ['searchAnalyticsTableData'],

            dateFilter: {
                '7days': {
                    startDate: moment(new Date()).subtract(7, 'days').format("YYYY-MM-DD"),
                    endDate: moment(new Date()).format("YYYY-MM-DD"),
                },
                '28days': {
                    startDate: moment(new Date()).subtract(28, 'days').format("YYYY-MM-DD"),
                    endDate: moment(new Date()).format("YYYY-MM-DD"),
                },
                '90days': {
                    startDate: moment(new Date()).subtract(90, 'days').format("YYYY-MM-DD"),
                    endDate: moment(new Date()).format("YYYY-MM-DD"),
                },
            },

            detailedAnalyticsSort: {
                sortColumn: null,
                sortOrder: true,
            },

            pastMonthlyConversion: {
                visitors: null,
                orders: null,
                sales: null,
                inbox: null,
            },

            datePickerTrigger: true,

            loading: {
                basicOverview: {
                    visitors: true,
                    orders: null,
                    sales: null,
                    inbox: null,
                },
                searchAnalytics: null,
            },

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

            visitors: [],
            traffic: [],
            browser: [],
            devices: [],
            visitors_geo: [['Country', 'Visitors']],
            headers: {
                responseType: 'json',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded'
                }
            },
            isBrowserDataNotAvailable: false,
            isDeviceDataNotAvailable: false,
            isTrafficDataNotAvailable: false
        };
    },

    created: function () {
        var self = this;
        // self.getDetailedAnalyticsForCurrentDate();
        // self.getSearchAnalyticsForCurrentDate();
        self.getMonthlyData();

        self.getDetailedAnalyticsDataForDateRange(self.dateFilter['28days'], function (analyticsData) {
            self.onSuccessDetailedAnalyticsDataForDateRange(analyticsData);
        });

        self.getPastMonthConversion();

        self.getUserData();
    },

    mounted: function () {
        var self = this;
        self.getVistorsData(1);
        self.getGeoVisitors();
        self.getBrowsers();
        self.getDevices();
        self.getTrafficSource();
    },

    filters: {
        'numeralFormat': function (value) {
            return numeral(value).format('0 a');
        },
    },

    computed: {

        maxDateForDatePicker: function () {
            return moment(new Date()).format("YYYY-MM-DD");
        },

        getOverviewCards: function () {
            var self = this;
            return self.overviewCards;
        },

        searchAnalyticsTableData: function () {
            var self = this;
            var tableData = self.searchAnalyticsData.tableData.slice();
            if (self.detailedAnalyticsSort.sortColumn) {
                if (self.detailedAnalyticsSort.sortOrder) {
                    return _.sortBy(tableData, [self.detailedAnalyticsSort.sortColumn]);
                } else {
                    return _.reverse(_.sortBy(tableData, [self.detailedAnalyticsSort.sortColumn]));
                }
            }
            return tableData;
        },

        searchAnalyticsCardData: function () {
            var self = this;
            var cardData = Object.assign({}, self.searchAnalyticsData.cardsAggregateData);
            if (cardData.hasOwnProperty('ctr')) {
                cardData['ctr'] = parseFloat(cardData['ctr']) * 100;
                cardData.ctr = numeral(cardData.ctr).format('0.0');
            }
            return cardData;
        },

        selectedDateRangeText: function () {
            var self = this;
            var dateRangeText = '';
            var startDate = moment(self.selectedDateRange.startDateApplied).format('DD MMMM YYYY');
            var endDate = moment(self.selectedDateRange.endDateApplied).format('DD MMMM YYYY');
            var difference = moment(self.selectedDateRange.endDateApplied).diff(moment(self.selectedDateRange.startDateApplied), 'days');
            if (difference == 7) {
                dateRangeText = 'Last 7 days';
            } else if (difference == 28) {
                dateRangeText = 'Last 28 days';
            } else if (difference == 90) {
                dateRangeText = 'Last 90 days';
            } else if (startDate == endDate) {
                dateRangeText = startDate;
            } else if (moment(self.selectedDateRange.startDateApplied).format('YYYY') == moment(self.selectedDateRange.endDateApplied).format('YYYY')) {
                dateRangeText = moment(self.selectedDateRange.startDateApplied).format('DD MMMM') + ' - ' + moment(self.selectedDateRange.endDateApplied).format('DD MMMM');
            } else {
                dateRangeText = startDate + ' - ' + endDate;
            }
            return dateRangeText;
        },

        monthlyConversion: function () {
            var self = this;
            var inboxConversion = null;
            var salesConversion = null;
            var visitorsConversion = null;
            var ordersConversion = null;

            function calculateIncrease(oldValue, newValue) {
                if (oldValue == 0 && newValue != 0) {
                    return 100;
                }
                return ((newValue - oldValue) / oldValue) * 100;
            }

            if (self.pastMonthlyConversion.inbox != null) {
                if (self.overviewCards.inbox.figure != null) {
                    inboxConversion = calculateIncrease(self.pastMonthlyConversion.inbox, self.overviewCards.inbox.figure);
                }
            }

            if (self.pastMonthlyConversion.sales != null) {
                if (self.overviewCards.sales.figure != null) {
                    salesConversion = calculateIncrease(self.pastMonthlyConversion.sales, self.overviewCards.sales.figure);
                }
            }

            if (self.pastMonthlyConversion.orders != null) {
                if (self.overviewCards.orders.figure != null) {
                    ordersConversion = calculateIncrease(self.pastMonthlyConversion.orders, self.overviewCards.orders.figure);
                }
            }

            if (self.pastMonthlyConversion.visitors != null) {
                if (self.overviewCards.visitors.figure != null) {
                    visitorsConversion = calculateIncrease(self.pastMonthlyConversion.visitors, self.overviewCards.visitors.figure);
                }
            }

            inboxConversion = (isNaN(inboxConversion) || Infinity == inboxConversion || -Infinity == inboxConversion) ? 0 : inboxConversion;
            salesConversion = (isNaN(salesConversion) || Infinity == salesConversion || -Infinity == salesConversion) ? 0 : salesConversion;
            visitorsConversion = (isNaN(visitorsConversion) || Infinity == visitorsConversion || -Infinity == visitorsConversion) ? 0 : visitorsConversion;
            ordersConversion = (isNaN(ordersConversion) || Infinity == ordersConversion || -Infinity == ordersConversion) ? 0 : ordersConversion;

            return {
                inbox: numeral(inboxConversion).format('0.0'),
                sales: numeral(salesConversion).format('0.0'),
                visitors: numeral(visitorsConversion).format('0.0'),
                orders: numeral(ordersConversion).format('0.0'),
            };
        },

        getMonthAndYearText: function () {
            return moment().format('MMMM YYYY');
        },
    },

    methods: {

        getDetailedAnalyticsForCurrentDate: function () {
            var self = this;
            var currentDate = new Date();
            var dateObj = {
                year: currentDate.getFullYear(),
                month: (currentDate.getMonth() + 1),
                day: (currentDate.getDay() + 1),
            };

            self.getDetailedAnalyticsDataForDate(dateObj, function (analyticsData) {
                Vue.set(self.searchAnalyticsData, 'tableData', analyticsData);
            });
        },

        getSearchAnalyticsForCurrentDate: function () {
            var self = this;
            var currentDate = new Date();
            var dateObj = {
                year: currentDate.getFullYear(),
                month: (currentDate.getMonth() + 1)
            };

            self.getSearchAnalyticsDataForDate(dateObj, function (analyticsData) {
                if (analyticsData && Array.isArray(analyticsData)) {
                    analyticsData.map(function (dailyAnalytics) {
                        if (dailyAnalytics.hasOwnProperty('day') && dailyAnalytics.day == (currentDate.getDate + 1)) {
                            self.setSearchAnalyticsCardData(dailyAnalytics);
                        }
                    });
                }
            });
        },

        setSearchAnalyticsCardData: function (data) {
            var self = this;
            var cardData = _.pick(data, ["totalNoOfSearchQueries", "totalNoOfImpressions", "totalNoOfClicks", "ctr"]);
            Vue.set(self.searchAnalyticsData, 'cardsAggregateData', cardData);
        },

        onDateRangeSelected: function () {
            var self = this;
            if (self.selectedDateRange.startDate) {
                self.selectedDateRange.startDateApplied = self.selectedDateRange.startDate;
            }

            if (self.selectedDateRange.endDate && self.selectedDateRange.endDate.trim().length > 0) {
                self.selectedDateRange.endDateApplied = self.selectedDateRange.endDate;
            } else {
                self.selectedDateRange.endDateApplied = self.selectedDateRange.startDateApplied;
            }


            if (self.selectedDateRange.startDateApplied == self.selectedDateRange.endDateApplied) {
                var currentDate = new Date(self.selectedDateRange.startDateApplied);
                var dateObj = {
                    year: currentDate.getFullYear(),
                    month: (currentDate.getMonth() + 1),
                    day: (currentDate.getDay() + 1),
                };
                self.getDetailedAnalyticsDataForDate(dateObj, function (analyticsData) {
                    Vue.set(self.searchAnalyticsData, 'tableData', analyticsData);
                });
            } else {
                var dateRange = {
                    startDate: self.selectedDateRange.startDateApplied,
                    endDate: self.selectedDateRange.endDateApplied,
                };
                self.getDetailedAnalyticsDataForDateRange(dateRange, function (analyticsData) {
                    self.onSuccessDetailedAnalyticsDataForDateRange(analyticsData);
                });
            }

        },

        onSuccessDetailedAnalyticsDataForDateRange: function (analyticsData) {
            var self = this;
            var searchQueries = analyticsData.length || 0;
            var impressions = 0;
            var clicks = 0;
            var avgCTR = 0;
            var a = _.forEach(analyticsData, function (data) {
                impressions += data.impressions;
                clicks += data.clicks;
            });
            var cardData = {
                totalNoOfSearchQueries: searchQueries,
                totalNoOfImpressions: impressions,
                totalNoOfClicks: clicks,
                ctr: (clicks / impressions),
            };
            Vue.set(self.searchAnalyticsData, 'tableData', analyticsData);
            Vue.set(self.searchAnalyticsData, 'cardsAggregateData', cardData);
        },

        getDetailedAnalyticsDataForDate: function (dateObj, callback) {
            axios({
                method: 'post',
                url: Endpoints.GetDetailedAnalyticsForDate,
                data: dateObj
            }).then(function (response) {
                //self.showLoader(false);
                if (callback && typeof callback == 'function') {
                    callback(response.data);
                }
            }).catch(function (error) {
                //self.showLoader(false);
            });
        },

        getDetailedAnalyticsDataForDateRange: function (dateObj, callback) {
            var self = this;
            self.loading.searchAnalytics = false;
            axios({
                method: 'post',
                url: Endpoints.GetDetailedAnalyticsForDateRange,
                data: dateObj
            }).then(function (response) {
                self.loading.searchAnalytics = true;
                if (callback && typeof callback == 'function') {
                    callback(response.data);
                }
            }).catch(function (error) {
                self.loading.searchAnalytics = true;
            })
        },

        getDetailedSearchAnalyticsDataForDateFilter: function (dateFilter) {
            var self = this;
            var dateObj = self.dateFilter[dateFilter];
            if (!dateObj || !dateObj.startDate || !dateObj.endDate) {
                return;
            }
            self.selectedDateRange.startDateApplied = dateObj.startDate;
            self.selectedDateRange.endDateApplied = dateObj.endDate;
            self.getDetailedAnalyticsDataForDateRange(dateObj, function (analyticsData) {
                self.onSuccessDetailedAnalyticsDataForDateRange(analyticsData);
            });
        },

        getSearchAnalyticsDataForDate: function (dateObj, callback) {
            axios({
                method: 'post',
                url: Endpoints.GetDailySearchAnalytics,
                data: dateObj
            }).then(function (response) {
                if (callback && typeof callback == 'function') {
                    callback(response.data);
                }
            }).catch(function (error) {

            });
        },

        getMonthlySearchAnalytics: function (year, callback) {
            axios({
                method: 'post',
                url: Endpoints.GetMonthlySearchAnalytics,
                data: year
            }).then(function (response) {

            }).catch(function (error) {

            });
        },

        onStartDateSelected: function (selectedDate) {
            var self = this;
            Vue.set(self.selectedDateRange, 'startDate', selectedDate);
        },

        onEndDateSelected: function (selectedDate) {
            var self = this;
            Vue.set(self.selectedDateRange, 'endDate', selectedDate);
        },

        getMonthlyData: function () {
            var self = this;
            var fromDate = moment().date(1).format("YYYY-MM-DD");
            var toDate = moment().format("YYYY-MM-DD");
            var type = 'webform';

            self.loading.basicOverview.visitors = true;
            self.loading.basicOverview.orders = true;
            self.loading.basicOverview.sales = true;
            self.loading.basicOverview.inbox = true;

            self.getKitsunePaymentsData({ from_date: fromDate, to_date: toDate }, function (paymentsData) {
                if (!paymentsData || !paymentsData.hasOwnProperty('status') || paymentsData.status != 'success') {
                    return;
                }
                var orderCount = (paymentsData.SUCCESS && paymentsData.SUCCESS.hasOwnProperty('count')) ? paymentsData.SUCCESS.count : 0;
                orderCount += (paymentsData.FAIL && paymentsData.FAIL.hasOwnProperty('count')) ? paymentsData.FAIL.count : 0;
                orderCount += (paymentsData.INPROGRESS && paymentsData.INPROGRESS.hasOwnProperty('count')) ? paymentsData.INPROGRESS.count : 0;
                var totalAmount = (paymentsData.SUCCESS && paymentsData.SUCCESS.hasOwnProperty('amounts')) ? paymentsData.SUCCESS.amounts['INR'] : 0;
                Vue.set(self.overviewCards.orders, 'figure', orderCount);
                Vue.set(self.overviewCards.sales, 'figure', totalAmount);
            });

            self.getVisitorsData({ fromDate: fromDate, toDate: toDate, filterType: 1 }, function (visitorsData) {
                var montlyVisitors = _.reduce(visitorsData, function (visitors, item) {
                    return parseFloat(visitors) + parseFloat(item.DataCount);
                }, 0);
                montlyVisitors = !montlyVisitors || isNaN(montlyVisitors) ? 0 : montlyVisitors;
                Vue.set(self.overviewCards.visitors, 'figure', montlyVisitors);
            });

            self.getWebActionCount({ startDate: fromDate, endDate: toDate, type }, function (webactionData) {
                if (webactionData && webactionData.Actions) {
                    Vue.set(self.overviewCards.inbox, 'figure', _.reduce(webactionData.Actions, function (sum, actionItem) {
                        return sum + actionItem.Count;
                    }, 0));
                }
            });
        },

        getPastMonthConversion: function () {
            var self = this;
            var startDate = moment().subtract(1, 'months').startOf('month').format("YYYY-MM-DD");
            var endDate = moment().subtract(1, 'months').endOf('month').format("YYYY-MM-DD");
            var type = 'webform';

            self.getKitsunePaymentsData({ from_date: startDate, to_date: endDate }, function (paymentsData) {
                if (!paymentsData || !paymentsData.hasOwnProperty('status') || paymentsData.status != 'success') {
                    return;
                }
                var orderCount = (paymentsData.SUCCESS && paymentsData.SUCCESS.hasOwnProperty('count')) ? paymentsData.SUCCESS.count : 0;
                orderCount += (paymentsData.FAIL && paymentsData.FAIL.hasOwnProperty('count')) ? paymentsData.FAIL.count : 0;
                orderCount += (paymentsData.INPROGRESS && paymentsData.INPROGRESS.hasOwnProperty('count')) ? paymentsData.INPROGRESS.count : 0;
                var totalAmount = (paymentsData.SUCCESS && paymentsData.SUCCESS.hasOwnProperty('amounts')) ? paymentsData.SUCCESS.amounts['INR'] : 0;
                Vue.set(self.pastMonthlyConversion, 'orders', orderCount);
                Vue.set(self.pastMonthlyConversion, 'sales', totalAmount);
            });

            self.getVisitorsData({ fromDate: startDate, toDate: endDate, filterType: 1 }, function (visitorsData) {
                var montlyVisitors = _.reduce(visitorsData, function (visitors, item) {
                    return parseFloat(visitors) + parseFloat(item.DataCount);
                }, 0);
                montlyVisitors = !montlyVisitors || isNaN(montlyVisitors) ? 0 : montlyVisitors;
                Vue.set(self.pastMonthlyConversion, 'visitors', montlyVisitors);
            });

            self.getWebActionCount({ startDate: startDate, endDate: endDate, type }, function (webactionData) {
                if (webactionData && webactionData.Actions) {
                    Vue.set(self.pastMonthlyConversion, 'inbox', _.reduce(webactionData.Actions, function (sum, actionItem) {
                        return sum + actionItem.Count;
                    }, 0));
                }
            });
        },

        getKitsunePaymentsData: function (requestObj, callback) {
            var self = this;
            axios({
                method: 'post',
                url: Endpoints.GetKitsunePaymentsData,
                data: requestObj,
            }).then(function (response) {
                self.loading.basicOverview.orders = false;
                self.loading.basicOverview.sales = false;
                if (callback && typeof callback == 'function') {
                    callback(response.data);
                }
            }).catch(function (error) {
                self.loading.basicOverview.orders = false;
                self.loading.basicOverview.sales = false;
                console.error('error getKitsunePaymentsData', error);
            });
        },

        getVisitorsData: function (requestObj, callback) {
            var self = this;
            axios({
                method: 'post',
                url: Endpoints.GetVisitors,
                data: requestObj,
            }).then(function (response) {
                self.loading.basicOverview.visitors = false;
                if (callback && typeof callback == 'function') {
                    callback(response.data);
                }
            }).catch(function (error) {
                self.loading.basicOverview.visitors = false;
                console.error('error getVisitorsData', error);
            });
        },

        getWebActionCount: function (requestObj, callback) {
            var self = this;
            axios({
                method: 'post',
                url: Endpoints.GetWebActionCount,
                data: requestObj
            }).then(function (response) {
                self.loading.basicOverview.inbox = false;
                if (callback && typeof callback == 'function') {
                    callback(response.data);
                }
            }).catch(function (error) {
                self.loading.basicOverview.inbox = false;
                console.error(error);
            });
        },

        sortDataBy: function (sortColumn) {
            var self = this;
            if (!self.detailedAnalyticsSort.sortColumn || self.detailedAnalyticsSort.sortColumn != sortColumn) {
                Vue.set(self.detailedAnalyticsSort, 'sortColumn', sortColumn);
                Vue.set(self.detailedAnalyticsSort, 'sortOrder', false);
            } else if (self.detailedAnalyticsSort.sortColumn && self.detailedAnalyticsSort.sortColumn == sortColumn) {
                var sortOrder = self.detailedAnalyticsSort.sortOrder;
                Vue.set(self.detailedAnalyticsSort, 'sortOrder', !sortOrder);
            }

            var paginationLinkComponent = self.$refs.paginationLinks;
            paginationLinkComponent.$data.target.currentPage = 0;
        },

        onOverviewCardClick: function (cardType) {
            if (cardType == 'orders' || cardType == 'sales') {
                window.location.href = '/k-admin/Orders';
            } else if (cardType == 'inbox') {
                window.location.href = '/k-admin/Inbox';
            }
        },

        onDatePickerClosed: function () {
            var self = this;
            self.datePickerTrigger = false;
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
                url: Endpoints.UploadFileToSystemWebaction,
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
                url: Endpoints.SendEmail,
                data: request
            }).then(function (response) {
                toastr.success("", "Successfully sent the email");
            }).catch(function (error) {
                toastr.error("", "Error sending support email");
            });
        },

        getUserData: function () {
            var self = this;

            axios.get(Endpoints.GetWebsiteUserDetails, { responseType: 'json' })
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
                url: Endpoints.GetDeveloperDetails,
                data: { developerId: developerId }
            }).then(function (response) {
                callback(response.data);
            });
        },


        // graphs START
        getVistorsData: function (filterType, fromDate, toDate) {
            var self = this,
                container = document.getElementById('visitorMonthGraphContainer');

            fromDate = (fromDate === undefined) ? moment().date(1).format("YYYY-MM-DD") : fromDate;
            toDate = (toDate === undefined) ? moment().format("YYYY-MM-DD") : toDate;

            var requestObj = 'filterType=' + filterType + '&fromDate=' + fromDate + "&toDate=" + toDate;
            showLoader(container);
            axios.post(Endpoints.GetVisitors, requestObj, self.headers)
                .then(function (response) {
                    var visitorList = response.data;

                    visitorList = visitorList ? visitorList : self.getDefaultVisitorData();

                    visitorList.map(function (visitor) {
                        self.$data.visitors.push({
                            date: visitor.Key1,
                            count: visitor.DataCount
                        });
                    });
                    self.loadVistorsChart(container);
                })
                .catch(function (err) {
                    toastr.error("Error", "Something bad happened!");
                    removeLoader(container);
                });
        },
        loadVistorsChart: function (container) {
            var self = this;

            var graphParent = document.getElementById('visitor-chart');
            graphParent.innerHTML = "";

            Morris.Line({
                element: 'visitor-chart',
                data: self.$data.visitors,
                xkey: 'date',
                ykeys: ['count'],
                labels: ['count'],
                parseTime: true,
                lineColors: ['#1976D2'],
                resize: true,
                redraw: true
            });
            removeLoader(container);
        },

        getGeoVisitors: function () {
            var self = this,
                container = document.getElementById('visitorCountryGraphContainer'),
                requestObj = 'filterType=3&fromDate=' + moment().date(1).format("YYYY-MM-DD") + '&toDate=' + moment().format("YYYY-MM-DD");

            showLoader(container);
            axios.post(Endpoints.GetVisitors, requestObj, self.headers)
                .then(function (response) {
                    var visitorList = response.data;

                    visitorList = visitorList ? visitorList : self.getDefaultGeoVisitors();

                    visitorList.map(function (visitor) {
                        self.$data.visitors_geo.push([visitor.Key1, visitor.DataCount]);
                    });
                    self.loadGeoVisitorsChart(container);
                })
                .catch(function (err) {
                    toastr.error("Error", "Something bad happened!");
                    removeLoader(container);
                });
        },
        loadGeoVisitorsChart: function (container) {
            var self = this,
                graphParent = document.getElementById('visitor-geo');

            graphParent.innerHTML = "";

            google.charts.load('current', {
                'packages': ['geochart'],
                'mapsApiKey': 'AIzaSyAW2lmRa8fIb-BtCNGnniClLIeSuULVGMU'
            });
            google.charts.setOnLoadCallback(drawRegionsMap);

            function drawRegionsMap() {
                var data = google.visualization.arrayToDataTable(self.$data.visitors_geo);
                var options = {
                    backgroundColor: 'white',
                    colorAxis: { colors: ['#E3F2FD', '#0D47A1'] }
                };
                var chart = new google.visualization.GeoChart(document.getElementById('visitor-geo'));
                chart.draw(data, options);
            }
            removeLoader(container);
        },

        getBrowsers: function () {
            var self = this,
                browserContainer = document.getElementById('browserGraphContainer'),
                requestObj = 'fromDate=' + moment().date(1).format("YYYY-MM-DD") + '&toDate=' + moment().format("YYYY-MM-DD");
            self.isBrowserDataNotAvailable = false;

            showLoader(browserContainer);
            axios.post(Endpoints.GetBrowsers, requestObj, self.headers)
                .then(function (response) {
                    var browserList = response.data;
                    browserList = _.filter(browserList, function (o) { return o.Key1.toLowerCase().indexOf('bot') == -1; });
                    browserList = _.take(browserList, 5);
                    browserList.map((browser) => {
                        self.$data.browser.push({
                            label: browser.Key1,
                            value: browser.DataCount
                        });
                    });

                    if (!browserList || !browserList.length) {
                        self.isBrowserDataNotAvailable = true;
                        removeLoader(browserContainer);
                    } else {
                        self.loadBrowsersChart(browserContainer);
                    }
                })
                .catch(function (error) {
                    toastr.error("Error", "Something bad happened!");
                    removeLoader(browserContainer);
                    self.isBrowserDataNotAvailable = true;
                });
        },
        loadBrowsersChart: function (container) {
            var self = this,
                graphParent = document.getElementById('browser');

            graphParent.innerHTML = "";

            Morris.Donut({
                element: 'browser',
                data: self.$data.browser,
                colors: ['#0D47A1', '#1976D2', '#2196F3', '#64B5F6'],
                resize: true,
                formatter: function (y, data) { return y + ' requests' }
            });
            removeLoader(container);
            //toastr.success("Success", "Vistor Data loaded browser")
        },

        getDevices: function () {
            var self = this,
                devicesContainer = document.getElementById('devicesGraphContainer');
            requestObj = 'fromDate=' + moment().date(1).format("YYYY-MM-DD") + '&toDate=' + moment().format("YYYY-MM-DD");
            self.isDeviceDataNotAvailable = false;

            showLoader(devicesContainer);
            axios.post(Endpoints.GetDevices, requestObj, self.headers)
                .then(function (response) {
                    var devicesList = response.data;
                    devicesList = _.take(devicesList, 5);
                    devicesList.map(function (device) {
                        self.$data.devices.push({
                            label: device.Key1,
                            value: device.DataCount
                        });
                    });
                    if (!devicesList.length) {
                        self.isDeviceDataNotAvailable = true;
                        removeLoader(devicesContainer);
                    } else {
                        self.loadDevicesChart(devicesContainer);
                    }
                })
                .catch(function (err) {
                    toastr.error("Error", "Something bad happened!");
                    removeLoader(devicesContainer);
                    self.isDeviceDataNotAvailable = true;
                });
        },
        loadDevicesChart: function (container) {
            var self = this,
                graphParent = document.getElementById('devices');

            graphParent.innerHTML = "";

            Morris.Donut({
                element: 'devices',
                data: self.$data.devices,
                colors: ['#0D47A1', '#1976D2', '#2196F3', '#64B5F6'],
                resize: true,
                formatter: function (y, data) { return y + ' requests' }
            });
            removeLoader(container);
            //toastr.success("Success", "Vistor Data loaded devices")
        },

        getTrafficSource: function () {
            var self = this,
                trafficContainer = document.getElementById('trafficGraphContainer'),
                requestObj = 'fromDate=' + moment().date(1).format("YYYY-MM-DD") + '&toDate=' + moment().format("YYYY-MM-DD");

            self.isTrafficDataNotAvailable = false;

            showLoader(trafficContainer);
            axios.post(Endpoints.GetTrafficSources, requestObj, self.headers)
                .then(function (response) {
                    var data = response.data;
                    var re_search = new RegExp(/(google|bing|yandex|yahoo)/);
                    var re_social = new RegExp(/(insta|facebook)/);
                    var re_rest = new RegExp(re_search.source + "|" + re_social.source);

                    var search = _.filter(data, function (o) { return re_search.test(o.Key1) });
                    var social = _.filter(data, function (o) { return re_social.test(o.Key1) });
                    var rest = _.filter(data, function (o) { return !re_rest.test(o.Key1) });

                    self.$data.traffic = [
                        { label: 'Social', value: _.sumBy(social, function (o) { return o.DataCount; }) },
                        { label: 'Rest', value: _.sumBy(rest, function (o) { return o.DataCount; }) },
                        { label: 'Search', value: _.sumBy(search, function (o) { return o.DataCount; }) }];

                    if (!rest.length && !social.length && !search.length) {
                        self.isTrafficDataNotAvailable = true;
                        removeLoader(trafficContainer);
                    } else {
                        self.loadTrafficSourceChart(trafficContainer);
                    }
                })
                .catch(function (error) {
                    toastr.error("Error", "Something bad happened!");
                    removeLoader(trafficContainer);
                    self.isTrafficDataNotAvailable = true;
                });
        },
        loadTrafficSourceChart: function (container) {
            var self = this,
                graphParent = document.getElementById('referal');
            graphParent.innerHTML = "";

            var m = Morris.Donut({
                element: 'referal',
                data: self.$data.traffic,
                colors: ['#0D47A1', '#1976D2', '#2196F3', '#64B5F6'],
                resize: true,
                formatter: function (y, data) { return y + ' requests' }
            });
            m.select(2);
            removeLoader(container);
            //toastr.success("Success", "Vistor Data loaded traffic")
        },

        getDefaultVisitorData: function () {
            var date = new moment();
            date = date.format('YYYY-MM-DD');
            return [{
                Key1: date,
                DataCount: 0
            }]
        },

        getDefaultGeoVisitors: function () {
            return [
                {
                    DataCount: 0,
                    Key1: "India",
                    Key2: "IN"
                }
            ]
        },

        resetGraphs() {
            var self = this,
                chartsContainersId = [
                    "visitorMonthGraphContainer",
                    "visitorCountryGraphContainer",
                    "browserGraphContainer",
                    "devicesGraphContainer",
                    "trafficGraphContainer"];

            var reRenderGraph = function (chartContainerId) {
                var container = document.getElementById(chartContainerId);

                if (container) {
                    showLoader(container);
                    self.renderGraph(chartContainerId, container);
                }
            }

            chartsContainersId.forEach(reRenderGraph);
        },

        renderGraph: function (containerId, container) {
            var self = this;
            switch (containerId) {
                case "visitorMonthGraphContainer":
                    self.loadVistorsChart(container);
                    break;
                case "visitorCountryGraphContainer":
                    self.loadGeoVisitorsChart(container);
                    break;
                case "browserGraphContainer":
                    self.loadBrowsersChart(container);
                    break;
                case "devicesGraphContainer":
                    self.loadDevicesChart(container);
                    break;
                case "trafficGraphContainer":
                    self.loadTrafficSourceChart(container);
                    break;
            }

        }
        // graphs END
    },
});

// callback executed when sidebar resizes
_sideBarResizeCallback = function () {
    setTimeout(function () {
        GWTAnalytics.resetGraphs();
    }, 350);
}