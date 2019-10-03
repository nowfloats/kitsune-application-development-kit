"use strict";

const Endpoints = Object.freeze({
    GetVirtualMobileNumbers: '/k-admin/CallTrackerLog/GetVMNDetails',
    GetLogs: '/k-admin/CallTrackerLog/GetCallLogs',
    GetCustomerName: '/k-admin/CallTrackerLog/GetCustomerName',
    GetWebsiteUserDetails: '/k-admin/Settings/GetWebsiteUserDetails',
    GetDeveloperDetails: '/k-admin/ManageWebsiteContent/GetDeveloperDetails',
    UploadFileToSystemWebaction: '/k-admin/Inbox/UploadFileToSystemWebaction',
    SendEmail: '/k-admin/ManageWebsiteContent/SendEmail',
});

Vue.component('advanced-search', AdvancedSearch);
Vue.component('tracker-log', TrackerLog);
Vue.component('flat-pickr', VueFlatpickr);
Vue.component('calls-detail', CallDetails);
Vue.component('k-header', KHeader);
Vue.use('KModal', KModal);
Vue.use(VuePaginate);

var vm = new Vue({
    el: "#app",

    data: function () {
        return {
            virtualMobileNumbers: [],
            selectedVirtualMobileNumber: '',
            callLogs: [],

            filters: {
                mobileNumber: "",
                startDate: "",
                endDate: "",
                status: ""
            },

            vmnFetchStatus: {
                fetching: true,
                success: false,
                error: false
            },

            showWarningForDisabledVMN: true,

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
        };
    },

    created: function () {
        this.fetchVMNDetails();
        //this.fetchCallLogs();
        this.getUserData();
    },

    computed: {

        getFilters: function () {
            return this.filters;
        },

        filteredLogs: function () {
            var self = this;
            var filters = self.getFilters;
            var mobileNumberRegex = filters.mobileNumber ? new RegExp(filters.mobileNumber, "i") : "";
            var statusRegex = filters.status ? new RegExp(filters.status, 'i') : "";
            var startDate = self.getTimeFromDate(filters.startDate, false);
            var endDate = self.getTimeFromDate(filters.endDate, true);
            var callLogs = self.callLogs;

            // on change of selected mobile number
            var selectedVMN = self.selectedVirtualMobileNumber;
            var number = self.selectedVirtualMobileNumber.orignalNumber;
            var orignalNumberRegex = number ? new RegExp(number, 'i') : "";

            var filterLogs = function (log) {

                var isMatched = true;
                var momentDate = new moment(log.callDateTime);
                var date = (new Date(momentDate.toDate())).getTime();

                if (mobileNumberRegex) {
                    isMatched = isMatched && mobileNumberRegex.test(log.callerNumber);
                }

                if (statusRegex) {
                    isMatched = isMatched && statusRegex.test(log.callStatus);
                }

                if (startDate) {
                    isMatched = isMatched && date >= startDate;
                }

                if (endDate) {
                    isMatched = isMatched && date <= endDate;
                }

                if (orignalNumberRegex) {
                    isMatched = isMatched && orignalNumberRegex.test(log.actualNumber);
                }

                return isMatched;
            }

            return callLogs ? callLogs.filter(filterLogs) : [];

        },

        missedCallsCount: function () {
            var filteredLogs = this.filteredLogs;
            var missedCallRegex = /missed/i;
            var totalMissedCalls = 0;

            var missedCalls = function (log) {
                if (missedCallRegex.test(log.callStatus)) {
                    totalMissedCalls++;
                }
            }

            filteredLogs.forEach(missedCalls);

            return totalMissedCalls;
        },

        connectedCallsCount: function () {
            var filteredLogs = this.filteredLogs;
            var connectedCallRegex = /connected/i;
            var totalConnectedCalls = 0;

            var connectedCalls = function (log) {
                if (connectedCallRegex.test(log.callStatus)) {
                    totalConnectedCalls++;
                }
            }

            filteredLogs.forEach(connectedCalls);

            return totalConnectedCalls;
        },

        totalCallsCount: function () {
            var self = this;
            return self.filteredLogs ? self.filteredLogs.length : 0;
        },

        showDefaultCase: function () {
            var self = this;
            var VMNumbers = self.virtualMobileNumbers;
            return (VMNumbers ? VMNumbers.length <= 0 : false);
        },

        vmnEnabled: function () {
            var self = this;
            var virtualMobileNumbers = self.virtualMobileNumbers;
            var hasVMNumbers = virtualMobileNumbers ? virtualMobileNumbers.length > 0 : false;
            return hasVMNumbers;
        },

        isFetching: function () {
            var self = this;
            var vmnFetchStatus = self.vmnFetchStatus;
            return vmnFetchStatus.fetching;
        },

        showWarning: function () {
            var self = this;
            var selectedVMN = self.selectedVirtualMobileNumber;
            var warningVisibility = self.showWarningForDisabledVMN;

            if (selectedVMN) {
                return !selectedVMN.isActive && warningVisibility;
            }

            return false;
        }
    },

    methods: {

        fetchVMNDetails: function () {
            var self = this;
            var VMNFetchStatus = self.vmnFetchStatus;

            self.showHideLoader(true);
            VMNFetchStatus.fetching = true;

            axios.get(Endpoints.GetVirtualMobileNumbers, { responseType: 'json' })
                .then(function (res) {
                    self.updateVirtualMobileNumbers(res.data);
                    apiFetchsuccess();
                })
                .catch(function (err) {
                    toastr.error('', 'Unbale to fetch Details.');
                    self.showHideLoader(false);
                    apiFetchError();
                });

            // helper functions
            var apiFetchsuccess = function () {
                VMNFetchStatus.fetching = false;
                VMNFetchStatus.success = true;
            }

            var apiFetchError = function () {
                VMNFetchStatus.fetching = false;
                VMNFetchStatus.error = true;
            }

        },

        fetchCallLogs: function (allLogs = true, mobileNumber, limit = 10000) {
            var self = this;

            var requestObj = {
                DataforAllNumbers: true,
                Limit: limit
            };


            if (!allLogs) {
                requestObj.DataforAllNumbers = false,
                    requestObj.Number = mobileNumber
            }
            self.showHideLoader(true);

            axios.post(Endpoints.GetLogs, requestObj, { responseType: 'json' })
                .then(function (res) {
                    self.updateCallLogs(res.data ? res.data : []);
                    self.showHideLoader(false);
                })
                .catch(function (err) {
                    toastr.error('', 'Error in getting call logs.')
                    self.showHideLoader(false);
                });
        },

        updateVirtualMobileNumbers: function (data) {
            var self = this;

            if (data && data.Data) {
                var uniqueNumbers = _.uniqBy(data.Data, 'calltrackernumber');

                var processVirtualMobileNumbers = function (mobileNumber) {
                    return {
                        number: mobileNumber.calltrackernumber,
                        isActive: mobileNumber.isactive,
                        orignalNumber: mobileNumber.contactnumber
                    }
                };

                var virtualMobileNumbers = uniqueNumbers.map(processVirtualMobileNumbers);

                if (virtualMobileNumbers ? virtualMobileNumbers.length > 0 : false) {
                    self.fetchCallLogs();
                } else {
                    self.showHideLoader(false);
                }

                Vue.set(self, 'virtualMobileNumbers', virtualMobileNumbers);
            }

        },

        updateCallLogs: function (callLogs) {

            var processCallLog = function (callDetail) {
                return {
                    callDateTime: callDetail.callDateTime,
                    recordingUrl: callDetail.callRecordingUri,
                    callStatus: callDetail.callStatus,
                    callerNumber: callDetail.callerNumber,
                    calleeNumber: callDetail.virtualNumber,
                    actualNumber: callDetail.merchantActualNumber
                }
            }

            var processedCallLogs = callLogs.map(processCallLog);

            Vue.set(this, 'callLogs', processedCallLogs);
        },

        // not in use
        selectedVMNChangeHandler: function () {
            var selectedVMN = this.selectedVirtualMobileNumber,
                fetchAllLogs = selectedVMN == '';

            this.showWarningForDisabledVMN = true;
            this.fetchCallLogs(fetchAllLogs, '');
        },

        updateFilters: function (filters) {
            var self = this;
            Vue.set(self.$data, "filters", filters);
        },

        getTimeFromDate: function (date, isEndOfDay) {

            if (Date.parse(date)) {
                date = new Date(date);

                if (isEndOfDay) {
                    date.setHours(23, 59, 59, 999);
                } else {
                    date.setHours(0, 0, 0, 0);
                }

                return date.getTime();
            }

            return '';

        },

        showHideLoader: function (show) {
            var app = document.getElementById('app');
            if (app) {

                if (show) {
                    showLoader(app);
                } else {
                    removeLoader(app);
                }

            }
        },

        closeVMNWarning: function () {
            var self = this;
            Vue.set(self, 'showWarningForDisabledVMN', false);
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
    }
});