var Endpoints = Object.freeze({
    GetWebsiteUserDetails: 'Settings/GetWebsiteUserDetails',
    UpdateCustomerDetails: 'Settings/UpdateCustomerDetails',
    UpdatePassword: 'Settings/UpdatePassword',
    GetCustomerName: 'Settings/GetCustomerName',
    GetDeveloperDetails: '/k-admin/ManageWebsiteContent/GetDeveloperDetails',
    UploadFileToSystemWebaction: '/k-admin/Inbox/UploadFileToSystemWebaction',
    SendEmail: '/k-admin/ManageWebsiteContent/SendEmail',
    GetWebActionListWithConfig: '/k-admin/Inbox/GetWebActionListWithConfig',
    CreateOrUpdateWebaction: '/k-admin/Inbox/CreateOrUpdateWebaction',
});

Vue.component('k-header', KHeader);

var vm = new Vue({
    el: "#app",
    created: function () {
        this.getUserData();
        toastr.options = {
            "positionClass": "toast-bottom-right"
        };
        this.getCustomerDetails();
    },
    components: {
        'KModal': KModal
    },
    data: function () {
        return {
            inputname: '',
            inputemail: '',
            inputphone: '',
            inputpassword: '',
            status: '',
            isReadOnly: true,
            password: {
                oldPassword: '',
                newPassword: '',
                retypePassword: '',
            },
            modalsShowStatus: {
                isEditPassword: false,
                isEditWebaction: false,
            },

            customerData: {},

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

            systemWebactions: {
                supportEmail: { name: 'kadminsupportemail', authId: '5959ec985d643701d48ee8ab' }
            },

            webactionList: [],
            activeWebActionForUpdate: null,
        };
    },
    methods: {
        getUserData: function () {
            var self = this;
            self.showLoader(true);

            axios.get(Endpoints.GetWebsiteUserDetails, { responseType: 'json' })
                .then(function (response) {
                    var userData = response.data || {};
                    if (self.isUserDataValid(userData)) {
                        self.setUserDetails(userData);
                    } else {
                        console.error('User data not valid');
                    }
                    self.showLoader(false);

                    self.genericHelpModal.supportEmailForm.to.push(userData.contact.email);
                    Vue.set(self.customerData, 'websiteId', userData.websiteId);
                    self.getWebactionDetails();
                    self.getDeveloperDetails(userData.developerId, function (email) {
                        self.genericHelpModal.supportEmailForm.to.push(email);
                    });
                })
                .catch(function (err) {
                    self.showLoader(false);
                    toastr.error('Error while getting details');
                });
        },

        updateUserData: function () {
            var self = this;
            var name = self.inputname;
            var email = self.inputemail;
            var phone = self.inputphone;
            var password = self.inputpassword;

            var userObj = {
                AccessType: null,
                ContactDetails: {
                    FullName: name,
                    Email: email,
                    PhoneNumber: phone,
                    Password: password
                }
            };

            vm.showLoader(true);

            axios.post(Endpoints.UpdateCustomerDetails, userObj)
                .then(function (response) {
                    if (response && response.data) {
                        toastr.success('Save successful');
                    }
                    self.emittHeaderRefreshEvent();
                    self.showLoader(false);
                })
                .catch(function (error) {
                    if (error && error.response && error.response.data && error.response.data.errorMessage) {
                        toastr.error(error.response.data.errorMessage);
                    } else {
                        toastr.error('An error occoured when updating the details.');
                    }
                    self.showLoader(false);
                    self.getUserData();
                });
        },

        isUserDataValid: function (userData) {
            return (userData && userData.lastLoginTimeStamp != undefined && userData.contact != undefined && userData.contact.email != undefined);
        },

        setUserDetails: function (userData) {
            var self = this;
            var lastLoggedInTime = moment(userData.lastLoginTimeStamp).format('D MMMM, h:mm a');
            self.inputname = userData.contact.fullName;
            self.inputemail = userData.contact.email;
            self.inputphone = userData.contact.phoneNumber;
            self.inputpassword = userData.password;
            self.status = 'Session Logged In ' + lastLoggedInTime;
        },

        toggleReadonly: function (isReadOnly) {
            var self = this;
            this.$data.isReadOnly = isReadOnly;
        },

        editOrSaveUserData: function () {
            var self = this;
            self.$data.isReadOnly = !self.$data.isReadOnly;

            if (self.$data.isReadOnly) {
                self.updateUserData();
            }
        },

        onChangePasswordClick: function () {
            var self = this;
            self.modalsShowStatus.isEditPassword = true;
        },

        changePassword: function () {
            var self = this;
            var request = {
                NewPassword: self.password.newPassword,
                OldPassword: self.password.oldPassword
            };

            if (!self.isPasswordValid()) {
                return;
            }

            axios.post(Endpoints.UpdatePassword, request, { responseType: 'json' })
                .then(function (response) {
                    if (response.data == "") {
                        toastr.success('Save successful');
                        self.inputpassword = self.password.newPassword;
                    } 
                    self.modalsShowStatus.isEditPassword = false;
                    self.clearPasswordObj();
                })
                .catch(function (err) {
                    if (err && err.response && err.response.data && err.response.data.errorMessage) {
                        toastr.error(err.response.data.errorMessage);
                    } else {
                        toastr.error('There was an error changing your password');
                    }
                });
        },

        cancelChangePassword: function () {
            var self = this;
            self.modalsShowStatus.isEditPassword = false;
            self.clearPasswordObj();
        },

        clearPasswordObj: function () {
            var self = this;
            self.password = {
                oldPassword: '',
                newPassword: '',
                retypePassword: '',
            };
        },

        cancelEdit: function () {
            var self = this;
            self.toggleReadonly(true);
            self.getUserData();
        },

        isPasswordValid: function () {
            var self = this;
            if (self.password.oldPassword == null || !self.password.oldPassword.trim()) {
                toastr.error('Old Password cannot be empty');
                return false;
            }

            if (self.password.newPassword == null || !self.password.newPassword.trim()) {
                toastr.error('New Password cannot be empty');
                return false;
            }

            if (self.password.retypePassword == null || !self.password.retypePassword.trim()) {
                toastr.error('Retyped Password cannot be empty');
                return false;
            }

            if (self.password.retypePassword !== self.password.newPassword) {
                toastr.error('New Password and retyped password do not match');
                return false;
            }

            return true;
        },

        showLoader: function (show) {
            var container = document.querySelector('.container-fluid.p-5');
            if (container) {

                if (show) {
                    showLoader(container);
                } else {
                    removeLoader(null, true);
                }

            }
        },

        hideAllModals: function () {
            var modalsStatus = this.modalsShowStatus;
            Object.keys(this.modalsShowStatus).map(function (modalName) {
                Vue.set(modalsStatus, modalName, false);
            });
        },

        openWindowWithUrl: function (url) {
            url = 'http://' + url;
            window.open(url);
        },

        emittHeaderRefreshEvent: function () {
            var event = new Event('customerRefreshed');
            window.dispatchEvent(event);
        },

        getCustomerDetails: function () {
            var self = this;
            axios({
                method: 'POST',
                url: Endpoints.GetCustomerName,
            }).then(function (response) {
                self.customerData = response.data;
            }).catch(function (error) {
                console.error(error);
            });

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

        getDeveloperDetails: function (developerId, callback) {
            axios({
                method: 'post',
                url: Endpoints.GetDeveloperDetails,
                data: { developerId: developerId }
            }).then(function (response) {
                callback(response.data);
            });
        },

        getWebactionDetails: function () {
            var self = this;
            self.showLoader(true);
            axios({
                method: 'post',
                url: Endpoints.GetWebActionListWithConfig,
                data: {},
            }).then(function (response) {
                self.showLoader(false);
                if (response && response.data) {
                    self.setWebActionForWebsite(response.data.WebActions);
                }
            }).catch(function (response) {
                self.showLoader(false);
                if (response && response.error) {
                    toastr.error('Error getting webaction data');
                }
            });
        },

        setWebActionForWebsite: function (webActionList) {
            var self = this;
            if (!webActionList || !Array.isArray(webActionList)) {
                return;
            }
            Vue.set(self, 'webactionList', webActionList);
        },

        isEmailConfiguredForWebAction: function (configs) {
            var self = this;
            var isConfigured = false;
            configs.map(function (config) {
                if (config.WebsiteId === self.customerData.websiteId) {
                    isConfigured = true;
                }
            });
            return isConfigured;
        },

        editWebActionConfig: function (action) {
            var self = this;
            if (self.activeWebActionForUpdate) {
                self.modalsShowStatus.isEditWebaction = true;
                return;
            }
            self.activeWebActionForUpdate = action;
        },

        getFormattedWebationName: function (webactionName) {
            return webactionName.replace(/[-_]/g, ' ');
        },

        configureWebActionEmail: function (action) {
            var self = this;
            if (self.activeWebActionForUpdate) {
                self.modalsShowStatus.isEditWebaction = true;
                return;
            }
            action.WebsiteConfig = [{
                WebsiteId: self.customerData.websiteId,
                Configuration: {
                    NotificationEmail: '',
                },
            }];
            Vue.set(self, 'activeWebActionForUpdate', action);
            vm.$forceUpdate();
        },

        updateWebAction: function () {
            var self = this;
            self.showLoader(true);
            axios({
                method: 'post',
                url: Endpoints.CreateOrUpdateWebaction,
                data: self.activeWebActionForUpdate,
            }).then(function (response) {
                self.showLoader(false);
                self.modalsShowStatus.isEditWebaction = false;
                toastr.success("Update successful");
                self.activeWebActionForUpdate = null;
                self.getWebactionDetails();
            }).catch(function (err) {
                toastr.error("Error");
                self.showLoader(false);
                self.modalsShowStatus.isEditWebaction = false;
                self.activeWebActionForUpdate = null;
                self.getWebactionDetails();
            });
        },

        cancelWebActionUpdate: function () {
            var self = this;
            self.modalsShowStatus.isEditWebaction = false;
            self.activeWebActionForUpdate = null;
            self.getWebactionDetails();
        },
    },
    computed: {
        getSaveOrEditBtnText: function () {
            var self = this;
            var btnText = self.isReadOnly ? 'Edit' : 'Save';
            return btnText;
        },

        getSiteMapUrl: function () {
            var self = this;
            return (self.customerData.websiteUrl)
                ? ((self.customerData.websiteUrl).toLowerCase() + '/sitemap.xml') : '';
        },

        getRobotsTxtUrl: function () {
            var self = this;
            return (self.customerData.websiteUrl)
                ? ((self.customerData.websiteUrl).toLowerCase() + '/robots.txt') : '';
        }
    },

});
