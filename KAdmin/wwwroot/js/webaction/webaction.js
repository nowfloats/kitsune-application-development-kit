const WebactionsEndpoints = {
    getAll: 'Inbox/GetWebActionsList',
    data: 'Inbox/GetWebActionsData?webActionName={name}',
    addData: 'Inbox/AddWebActionsData?webActionName={name}',
    updateData: 'Inbox/UpdateWebActionsData?name={name}&objectid={objectid}',
    uploadFile: CookieHelper.isAliCloud() ?
        'https://kadmin.getkitsune-alicloud.com/k-admin/Inbox/WebActionDataUploadV2?name={name}'
        : 'Inbox/WebActionDataUpload?name={name}',
    GetWebsiteUserDetails: 'Settings/GetWebsiteUserDetails',
    GetDeveloperDetails: '/k-admin/ManageWebsiteContent/GetDeveloperDetails',
    UploadFileToSystemWebaction: '/k-admin/Inbox/UploadFileToSystemWebaction',
    SendEmail: '/k-admin/ManageWebsiteContent/SendEmail',
};

const Endpoints = Object.freeze({
    GetCustomerName: 'Inbox/GetCustomerName'
});

const isWebaction = true;

Vue.component("VueFormGenerator", VueFormGenerator.component);
Vue.component('KModal', KModal);
Vue.component('k-header', KHeader);

const selectorsRegex = /(name)|(title)|(header)/i;

const PropertyVisiblityStatus = {
    "websiteid": false,
    "createdon": false,
    "updatedon": false
}

// enum for webactions PropertyTypes
var WAPropertyEnum = Object.freeze({
    'str': 'STR',
    'array': 'ARRAY',
    'number': 'NUMBER',
    'boolean': 'BOOLEAN',
    'date': 'DATE',
    'image': 'IMAGE',
    'link': 'LINK',
    'webaction': 'WEBACTION'
})

var vm = new Vue({
    el: '#app',

    data: {
        webActions: [],

        currentWebaction: null,

        currentWebactionData: {
            data: null
        },

        schema: null,
        webactionData: {
            data: null
        },

        areFieldsEditable: false,
        currentClassName: '',
        currentModel: '',
        currentObjectId : '',
       
        isCurrentElementArray: false,
        isBase: false,

        showTable: false,
        isGettingData: false,

        history: [],

        webactionDatatypeMapping : {
            'str': 'text',
            'array': 'dynamicwa',
            'number': 'number',
            'boolean': 'boolean',
            'date': 'date',
            'image': 'obj',
            'link': 'obj',
            'webaction': 'webaction'
        },

        nativeClasses: [
            "str",
            "number",
            "boolean",
            "date",
            "array"
        ],

        classesProcessed: [],

        configure: {
            maxChars: 10,
            defaultPlaceholder: '-- -- --',
            propertyMaxChars: 25
        },

        currentModel: 'name',

        delete: {
            that: null,
            index: null
        },

        defaultImageLink: 'https://s3.ap-south-1.amazonaws.com/kitsune-buildtest-resources/kadmin/No_image_available.svg',

        upload: {
            isImage: false,
            isError: false
        },

        searchQuery: "",

        modalShowStatus: {
            upload: false,
            delete: false
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

    },

    created: function() {
        var self = this;
        self.getWebactionsList();
        self.getUserData();
    },

    computed: {

        webactions() {
            var self = this;
            return self.$data.webActions.WebActions;
        },
        
        currentWebactionName() {
            var self = this,
                currentWebaction = self.$data.currentWebaction;
            return currentWebaction ? currentWebaction.DisplayName : '';
        },

        currentClassProperties: function () {
            var self = this,
                data = self.$data,
                className = self.$data.currentClassName,
                kclass = null,
                properties = null;

            className = className.trim().toLowerCase();

            kclass = _.find(data.classesProcessed, function (cls) {
                return cls.Name.trim().toLowerCase() == className;
            });

            properties = _.get(kclass.Class, "schema.groups[0].fields");
            return properties;
        },

        isNative() {
            var self = this;
            return self.isClassNative(self.$data.currentClassName);
        },

        deleteElementIndex() {
            return this.delete.index + 1;
        },

        modelNameForUpdate() {
            return this.currentModel;
        },

        isErrorInUploading() {
            let self = this;
            return self.upload.isError;
        },

        isFileImage() {
            return this.upload.isImage;
        },

        filterWebactions() {
            var self = this,
                nameRegex = new RegExp(self.searchQuery, 'i');

            var filter = function (webaction) {
                return nameRegex.test(webaction.DisplayName);
            }

            return self.webactions ? self.webactions.filter(filter) : [];

        },

        showWebactions() {
            return this.filterWebactions ? (this.filterWebactions.length > 0) : false;
        }
        
    }, 

    methods: {

        getWebactionsList: function () {
            var self = this;
            self.showLoader(true);
            var request = _createCORSRequest('POST', WebactionsEndpoints.getAll);
            request.onload = function (res) {
                if (request.response) {
                    Vue.set(self.$data, 'webActions', JSON.parse(request.response));
                }
                self.showLoader(false);
            }
            request.onerror = function (error) {
                console.log(error);
                self.showLoader(false);
            }
            request.send();
        },

        getWebactionData: function (webaction) {
            var self = this;
            Vue.set(self.$data.currentWebactionData, 'data', {});
            if(webaction) {
              
                var request = _createCORSRequest('POST', WebactionsEndpoints.data.replace('{name}', webaction.Name));

                request.onload = function() {
                    var response = JSON.parse(request.response);
                    Vue.set(self.$data.currentWebactionData, 'data', response.Data);
                    Vue.set(self.$data.webactionData, 'data', response.Data);
                    Vue.set(self.$data, 'isGettingData', false);
                }

                request.onerror = function() {
                    console.log('error');
                    Vue.set(self.$data, 'isGettingData', false);
                }

                request.onloadend = function () {
                    
                    self.showLoader(false);
                }

                request.send();
                Vue.set(self.$data, 'isGettingData', true);
            }
        },

        addWebactionData: function () {
            var self = this,
                apiPayload = self.$data.currentWebactionData,
                xhr = null,
                webactionName = '',
                currentWebaction = self.$data.currentWebaction,
                data = {
                    ActionData: self.$data.currentWebactionData,
                    WebsiteId: ''
                }

            webactionName = currentWebaction.Name;

          

            $.ajax({
                type: 'POST',
                url: WebactionsEndpoints.addData.replace("{name}", webactionName),
                data: JSON.stringify(data),
                success: function (response) {
                    self.updateBaseResponseArray(response);
                },
                error: function (err) {
                    self.showLoader(false);
                    toastr.error('error while updating.');
                },
                contentType: "application/json"
            });

        },

        updateBaseResponseArray: function (objectId) {
            var self = this,
                currentData = self.$data.currentWebactionData,
                className = self.currentClassName,
                currentModel = self.currentModel,
                length = 0;

            Vue.set(currentData, "_id", objectId);
            Vue.set(self.$data, "currentObjectId", objectId);
            self.$data.webactionData.data.push(currentData);

            length = self.$data.webactionData.data.length;
            self.updateHistory(className, currentModel, currentModel, length-1);

            if (self.areFieldsEditable) {
                self.togglePropertiesButtons();
            }

            self.showLoader(false);
            toastr.success('success');
        },

        updateDataForWebaction: function () {
            var self = this,
                objectId = self.$data.currentObjectId,
                webactionName = '',
                currentWebaction = self.$data.currentWebaction,
                localHistory = null,
                history = self.$data.history,
                path = null,
                data = null,
                updateObj = {
                    UpdateValue: null
                },
                subUpdateObj = {
                    $set: null
                },
                url = WebactionsEndpoints.updateData;

            if (history.length > 0) {
                localHistory = Object.assign({}, history[0]);
                path = self.getPath([localHistory]);
                data = _.get(self.$data.webactionData, path);
                webactionName = currentWebaction.Name;
                url = url.replace("{name}", webactionName).replace("{objectid}", objectId);

                if (data) {
                    data = self.processCurrentObjectForArray(Object.assign({}, data));
                    data = self.deleteUnnecessaryProperties(data);
                    subUpdateObj.$set = data;
                    updateObj.UpdateValue = JSON.stringify(subUpdateObj);
                    $.ajax({
                        type: 'POST',
                        url: url,
                        data: JSON.stringify(updateObj),
                        success: function (response) {
                            if (self.areFieldsEditable) {
                                self.togglePropertiesButtons();
                            }
                            toastr.success('success');
                            self.showLoader(false);
                        },
                        error: function (err) {
                            toastr.error('error');
                            console.log('updated fail', err);
                            self.showLoader(false);
                        },
                        contentType: "application/json"
                    });
                }

            } else {
                console.error('update data error in path');
            }
        },

        deleteUnnecessaryProperties: function (data) {
            var self = this,
                propertiesToRemove = ['ActionId', 'CreatedOn', 'UpdatedOn', "_id", "WebsiteId", "IsArchived", "UserId"];

            _.forEach(propertiesToRemove, function (property) {
                delete data[property]
            })

            return data;
        },

        showLoader: function(show) {
            var element = document.getElementById('app');
            if (show) {
                showLoader(element);
            } else {
                removeLoader(null, true);
            }
        },

        selectedWebactionChange: function (webAction) {
            var self = this,
                propertiesNameList = [],
                data = self.$data;

            self.showLoader(true);
            Vue.set(data, 'showTable', true);
            Vue.set(data, 'currentWebaction', webAction);

            var baseClass = self.getSchemaForWebaction(webAction);
            self.updateProcessedClasses(baseClass);

            var schema = self.getArraySchemaObject(root_class.Class, 'data');

            Vue.set(data, 'schema', schema);
            Vue.set(data, 'currentClassName', 'root');
            Vue.set(data, 'currentModel', 'data');
            Vue.set(data, 'isCurrentElementArray', true);
            Vue.set(data, 'isBase', true);

            Vue.set(data, 'currentObjectId', '');
            
            self.getWebactionData(webAction);
        },

        updateProcessedClasses: function (schema) {
            var self = this;
            Vue.set(self.$data, 'classesProcessed', []);

            self.$data.classesProcessed.push(schema);
            self.$data.classesProcessed.push(root_class);
            self.$data.classesProcessed.push(link_class);
            self.$data.classesProcessed.push(image_class);

            _.forEach(self.$data.nativeClasses, function (nativeClassName) {

                var obj = {
                    Class: self.getNativeClassObject(nativeClassName),
                    Name: nativeClassName
                }

                self.$data.classesProcessed.push(obj);
            });
        },

        getDatatypeMapping: function (type) {
            var self = this;
            if (type) {
                type = type.trim().toLowerCase()
                return self.webactionDatatypeMapping[type];
            }
            return null;
        },

        // -- start -- schema base objects -- //
        getPropertyObject: function(property) {
            var self = this;
            if (!property) {
                return null;
            }
            var obj = {
                type: "input",
                inputType: self.getDatatypeMapping(property.DataType),
                label: property.DisplayName,
                model: property.PropertyName,
                className: property.DataType,
                disabled: true,
                buttons: [
                    {
                        classes: "btn btn-default fa fa-pencil",
                        label: "",
                        title: "edit",
                        onclick: function (model, field) {
                            let btn = field.buttons[0];
                            if (field.disabled) {
                                Vue.set(btn, 'classes', 'btn btn-success fa fa-check');
                                self.areFieldsEditable = true;
                            } else {
                                Vue.set(btn, 'classes', 'btn btn-default fa fa-pencil');
                                self.addOrUpdateData();
                            }
                            Vue.set(field, 'disabled', !field.disabled);
                        }
                    }
                ]
            }
            return Object.assign({}, obj);
        },

        getObjectSchema: function (property, isArray) {
            let self = this;
            let classObj = {
                type: isArray ? 'arrobjwa' : 'objwa',
                model: property.PropertyName,
                //label: property.DisplayName,
                className: property.DataType,
                schema: {
                    groups: [{
                        legend: '',
                        fields: []
                    }]
                }
            }
            return Object.assign({}, classObj);
        },

        getNativeClassObject: function (className) {
            let obj = {
                model: className,
                schema: {
                    groups: [
                        {
                            fields: [
                                {
                                    disabled: false,
                                    inputType: className.toLowerCase() == 'array' ?
                                        this.getDatatypeMapping['str']
                                        : this.getDatatypeMapping[className], // needs to be updated
                                    label: className,
                                    model: "val",
                                    type: "input",
                                    visible: true
                                },
                                {
                                    disabled: false,
                                    inputType: "text",
                                    label: "_kid",
                                    model: "_kid",
                                    type: "input",
                                    visible: false
                                },
                                {
                                    disabled: false,
                                    inputType: "text",
                                    label: "k_referenceid",
                                    model: "k_referenceid",
                                    type: "input",
                                    visible: false
                                },
                                {
                                    disabled: false,
                                    inputType: "text",
                                    label: "isarchived",
                                    model: "isarchived",
                                    type: "checkbox ",
                                    visible: false
                                }

                            ],
                            legend: className
                        }
                    ]
                },
                type: 'object'
            }

            if (className.toLowerCase() == 'boolean') {
                obj.schema.groups[0].fields[0] = {
                    type: "switch",
                    label: className,
                    model: "val",
                    textOn: "True",
                    textOff: "False"
                }
            }

            if (className.toLowerCase() == 'datetime') {
                obj.schema.groups[0].fields[0]['format'] = 'YYYY-MM-DD'
            }

            return Object.assign({}, obj);
        },
        // -- end -- schema base objects -- //

        getSchemaForWebaction: function (webaction) {
            // check for webaction
            var self = this,
                propertyList = webaction.Properties,
                schemaPropertyList = [],
                baseClassObj = Object.assign({}, base_class),
                showProperty = true;

            _.forEach(propertyList, function (property) {
                showProperty = self.propertyVisibilityStatus(property.PropertyName);

                if (showProperty) {

                    if (_compareString(property.DataType, WAPropertyEnum.array)) {
                        schemaPropertyList.push(self.getObjectSchema(property, true));
                    } else if (!self.isClassNative(property.DataType)) {
                        schemaPropertyList.push(self.getObjectSchema(property, false));
                    } else {
                        schemaPropertyList.push(self.getPropertyObject(property));
                    }


                }
                

            })

            
            baseClassObj.Class.schema.groups[0].fields = schemaPropertyList;

            return baseClassObj;
        },

        getArraySchemaObject: function (kclass, modelName) {
            kclass = Object.assign({}, kclass);
            var arraySchema = {
                groups: [
                    {
                        fields: [
                            {
                                model: modelName,
                                schema: {
                                    fields: []
                                },
                                type: 'dynamicwa'
                            }
                        ],
                        legend: null
                    }
                ]
            }
            var fields = kclass.schema.groups[0].fields;
            var legend = kclass.schema.groups[0].legend;
            arraySchema.groups[0].fields[0].schema.fields = fields;
            arraySchema.groups[0].legend = modelName;
            return arraySchema;
        },

        getPropertyClassName: function (propertyName) {
            var self = this,
                currentClassProperties = self.currentClassProperties,
                property = null;

            if (currentClassProperties && currentClassProperties.length > 0) {

                property = _.find(currentClassProperties, function (property) {
                    return property.model == propertyName;
                });

                if (property) {
                    return property.className;
                }
            }

            return null;
        },

        getSchemaForClass: function (className) {
            var self = this,
                classesProcessed = self.$data.classesProcessed,
                kclass = null;

            if (className) {
                kclass = _.find(classesProcessed, function (cls) {
                    return className.trim().toLowerCase() == cls.Name.trim().toLowerCase();
                })

                if (kclass) {
                    return kclass.Class.schema;
                }
            }

            return null;
        },

        getClassFromClassesProcessed: function (className) {
            var self = this;

            var cls = _.find(self.$data.classesProcessed, function (kclass) {
                return className.trim().toLowerCase() == kclass.Name.trim().toLowerCase();
            });

            return cls.Class;
        },

        // -- start -- history : breadcrumbs //
        updateHistory: function (className, modelName, displayName, index) {
            var historyObject = {
                    className: "",
                    displayName: "",
                    index: -1,
                    modelName: "",
                    path: ""
                },
                obj = Object.assign({}, historyObject),
                self = this;

            // templates => for more info : https://lodash.com/docs/4.17.5#template
            var displayTemplate = _.template("<%= displayName %> [<%= index %>]"),
                pathTemplate = _.template("[<%= index %>]");



            // updating current object
            obj.className = className;
            obj.modelName = modelName;
            obj.displayName = _.isNumber(index) ? displayTemplate({ displayName: displayName, index: index+1 }) : displayName;
            obj.index = _.isNumber(index) ? index : -1;
            obj.path = modelName
            //

            if (_.isNumber(index)) {
                obj.path = pathTemplate({ index: index });
            }


            self.$data.history.push(obj);

        },

        getPath: function (history) {
            let self = this;
            let path = '';
            let length = history.length;
            for (let i = 0; i < length; i++) {
                let ele = history[i];
                path = path + ele.path + '.';
            }
            path = path.replace(/(^\s*\.*\s*)|(\s*\.*\s*$)/gi, '');
            return "data"+path;
        },
        // -- end -- history : breadcrumbs //


        // -- start -- back button //

        backButton: function () {
            var self = this,
                history = self.$data.history;

            if (self.areFieldsEditable) {
                self.unsavedChangesModal(true);
                return;
            }   

            history.pop();
            self.goToInHistory();
        },

        goToInHistory: function () {
            var self = this,
                sourceData = self.$data.webactionData,
                data = null,
                path = '',
                history = self.$data.history,
                lastPoint = null;

            if (history.length > 0) {

                path = self.getPath(history); // Pending null checks
                lastPoint = _.last(history); // Pending null checks
                data = _.get(sourceData, path);
                schema = self.getSchemaForClass(lastPoint.className);
                

                Vue.set(self.$data, 'currentClassName', lastPoint.className);
                Vue.set(self.$data, 'currentModel', lastPoint.modelName);
                Vue.set(self.$data, 'currentWebactionData', data);
                Vue.set(self.$data, 'schema', schema);

                if (_compareString(lastPoint.className, 'array')) {
                    Vue.set(self.$data, 'isCurrentElementArray', true);
                } else {
                    Vue.set(self.$data, 'isCurrentElementArray', false);
                }

            } else {
                self.selectedWebactionChange(self.currentWebaction);
            }
        },

        breadcrumbsHistoryJump: function (index) {
            let self = this;

            if (self.areFieldsEditable) {
                self.unsavedChangesModal(true);
                return;
            }

            if (index == -1) {
                self.selectedWebactionChange(self.currentWebaction);
            }

            self.history.splice(index + 1);
            self.goToInHistory();
        },

        // -- end -- back button //

        isClassNative(className) {
            var self = this,
                listOfNativeClasses = self.$data.nativeClasses;
            var cls = _.find(listOfNativeClasses, function (nativeClassName) {
                return _compareString(className, nativeClassName);
            });

            return !!cls;
        },

        // -- start -- naive array processing
        convertNativeArrayToObjectArray: function (arr) {
            var self = this,
                processedArray = [],
                obj = null;

            _.forEach(arr, function (ele) {

                obj = null;

                if (_.isObject(ele)) {

                    if (!ele.isarchived) {
                        obj = ele;
                    }

                } else {
                    obj = {
                        val: ele,
                        isarchived: false,
                        disabled: true
                    }
                }
                if (obj) {
                    processedArray.push(obj);
                }
            })

            return processedArray;
        },

        convertObjectArrayToNativeArray: function (arr) {
            var self = this,
                processedArray = [],
                val = null;

            _.forEach(arr, function (ele) {

                val = null;

                if (_.isObject(ele)) {

                    if (!ele.isarchived) {
                        val = ele.val;
                    }

                } else {
                    val = ele
                }

                if (val) {
                    processedArray.push(val);
                }
            })

            return processedArray;
        },
        // -- end -- naive array processing

        manage: function (thisn, isElementFromArray, index, isAddingNewToArray, isAddingFromList) {
            var self = thisn.$root,
                model = thisn.model,
                field = thisn.schema,
                isArray = self.$data.isCurrentElementArray,
                dataToSet = null,
                className = '', // className of the new element clicked
                schema = null,
                kclass = null,
                history = self.$data.history,
                currentObjectId = self.$data.currentObjectId,
                isBase = self.$data.isBase;


            if (self.areFieldsEditable) {
                self.unsavedChangesModal(true);
                return;
            }


            currentObjectId = currentObjectId.trim();

            if (!currentObjectId && !isBase) {
                toastr.warning("please save current changes.");
                return;
            }

            className = self.getPropertyClassName(field.model);
            schema = self.getSchemaForClass(className);

            if (isArray) {

                if (!model[field.model][index]) {
                    Vue.set(model[field.model], index, {})
                }
                dataToSet = model[field.model][index] 

            } else {

                if (!model[field.model]) { // check if object is available else initialize
                    Vue.set(model, field.model, {});
                }
                dataToSet = model[field.model];
            }


            if (history.length == 0 && !isAddingNewToArray) {
                Vue.set(self.$data, 'currentObjectId', dataToSet['_id']);
            }

            // next object to be rendered
            if (_compareString(className, 'array')) {
                Vue.set(self.$data, 'isCurrentElementArray', true);
                kclass = self.getClassFromClassesProcessed(className);
                schema = self.getArraySchemaObject(kclass, field.model);

                if (self.isClassNative(className)) {
                    model[field.model] = self.convertNativeArrayToObjectArray(model[field.model]);
                }

                dataToSet = model; // arayExists ? nothing : initialize
            } else {
                Vue.set(self.$data, 'isCurrentElementArray', false);
            }

            Vue.set(self.$data, 'currentClassName', className);
            Vue.set(self.$data, 'currentModel', field.model);
            Vue.set(self.$data, 'schema', schema);
            Vue.set(self.$data, 'currentWebactionData', dataToSet);
            Vue.set(self.$data, 'isBase', false);

            if (!isAddingNewToArray) {
                self.updateHistory(className, field.model, field.model, index);
            }
        },

        // -- start -- save or add data to webaction //
        addOrUpdateData: function () {
            var self = this,
                dataToSubmit = self.$data.currentWebactionData,
                isBaseObject = false,
                className = self.$data.currentClassName,
                propertyName = self.$data.currentModel,
                currentObjectId = self.$data.currentObjectId;

            self.showLoader(true);
            
            if (currentObjectId && currentObjectId.trim().length > 0) {
                self.updateDataForWebaction();
            } else {
                self.addWebactionData();
            }

        },
        // -- end -- save or add data to webaction //

        getArrayProperties: function () {
            var self = this,
                properties = [],
                arrProperties = [];

            properties = self.getClassProperties('base');

            _.forEach(properties, function (property) {

                if (_compareString(property.className, 'array')) {
                    arrProperties.push(property.model);
                }

            });

            return arrProperties;

        },

        processCurrentObjectForArray: function (data) {
            var self = this,
                arrProperties = [],
                arr = [];

            arrProperties = self.getArrayProperties();

            _.forEach(arrProperties, function (propertyName) {
                arr = [];
                if (data[propertyName]) {
                    arr = self.convertObjectArrayToNativeArray(data[propertyName]);
                    Vue.set(data, propertyName, arr);
                }

            })

            return data;
        },

        getClassProperties: function (className) {
            var self = this,
                data = self.$data,
                kclass = null,
                properties = null;

            kclass = _.find(data.classesProcessed, function (cls) {
                return _compareString(cls.Name, className);
            });

            properties = _.get(kclass.Class, "schema.groups[0].fields");
            return properties;
        },

        // delete confirmation modal
        deleteConfirm: function () {
            var self = this,
                obj = self.delete,
                modalStatus = self.modalShowStatus;
            Vue.set(obj.that.value[obj.index], 'isarchived', true);

            if (self.isBase) {
                self.deleteDataFromWebaction(obj.that.value[obj.index]);
            } else {
                self.addOrUpdateData();
            }

            Vue.set(modalStatus, 'delete', false);
        },

        deleteDataFromWebaction: function (data) {


            var self = this,
                objectId = data['_id'],
                url = WebactionsEndpoints.updateData,
                webaction = self.$data.currentWebaction,
                webactionName = '',
                updateObj = {
                    UpdateValue: null
                },
                removeObj = {
                    $set: {
                        IsArchived: true
                    }
                }

            webactionName = webaction.Name;

            url = url.replace("{name}", webactionName).replace("{objectid}", objectId);
            updateObj.UpdateValue = JSON.stringify(removeObj);

            $.ajax({
                type: 'POST',
                url: url,
                data: JSON.stringify(updateObj),
                success: function (response) {
                    self.showLoader(false);
                },
                error: function (err) {
                    console.log('updated fail', err);
                    self.showLoader(false);
                },
                contentType: "application/json"
            });

        },


        // managing object selectors --start
        findSelectors: function (propertyList) {
            var propertyNames = [];
            for (let i = 0; i < propertyList.length; i++) {

                let property = propertyList[i];
                let name = property.model;
                let valid = selectorsRegex.test(name) && (propertyNames.length <= 3);
                if (valid) {
                    propertyNames.push(name);
                }
            }
            return propertyNames;
        },

        findImages: function (propertyList) {
            var propertyNames = [],
                className = '';
            _.forEach(propertyList, function (property) {
                className = property.className;
                if (className && _compareString(className, 'image')) {
                    propertyNames.push(property.model);
                }
            });

            return propertyNames;
        },

        containProperties: function (propertyList) {
            var propertyNames = [];
            var length = propertyList.length >= 3 ? 3 : propertyList.length; // 3 -> minimum number of properties to show
            for (var i = 0; i < length; i++) {
                propertyNames.push(propertyList[i].model);
            }
            return propertyNames;
        },
        //managing object selectors --end

        // returns object/array from the given "data" object as "modelName.propertyName"
        getDataForPropertyNames: function (data, modelName, propertyName) {
            var obj = _.get(data, modelName + '.' + propertyName);
            return obj;
        },

        togglePropertiesButtons: function (active) {
            let self = this;
            let fields = self.currentClassProperties;
            let btn = null;

            if (!self.areFieldsEditable) {

                if (self.isNative && self.isCurrentElementArray) {
                    let data = self.currentWebactionData[self.currentModel];
                    _.forEach(data, function (ele) {
                        ele.disabled = false;
                    })

                } else {

                    _.forEach(fields, function (field) {
                        if (field.disabled) {
                            field.disabled = !field.disabled;
                            btn = field.buttons[0];
                            Vue.set(btn, 'classes', 'btn btn-success fa fa-check');
                        }
                    });
                }

                Vue.set(self, 'areFieldsEditable', true);

            } else {
                if (self.isNative && self.isCurrentElementArray) {
                    let data = self.currentWebactionData[self.currentModel];
                    _.forEach(data, function (ele) {
                        ele.disabled = true;
                    })

                } else {
                    _.forEach(fields, function (field) {
                        btn = field.buttons;
                        if (!field.disabled && btn) {
                            field.disabled = !field.disabled;
                            btn = field.buttons[0];
                            Vue.set(btn, 'classes', 'btn btn-default fa fa-pencil');
                        }
                    });
                }

                Vue.set(self, 'areFieldsEditable', false);
                
            }
        },


        // pending changes modal --start
        unsavedChangesModal: function (open) {
            var modalStatus = this.modalShowStatus;

            if (open) {
                Vue.set(modalStatus, 'update', true);
            } else {
                Vue.set(modalStatus, 'update', false);
            }

        },

        saveUnsavedChanges: function () {
            let self = this;
            self.addOrUpdateData();
            self.unsavedChangesModal(false);
        },

        discardUnsavedChanges: function () {
            let self = this;
            self.togglePropertiesButtons();
            self.unsavedChangesModal(false);
        },
        // pending changes modal --end


        // --- start --- upload file methods 

        startUploadFile: function () {
            var self = this,
                currentWebaction = self.$data.currentWebaction,
                webactionName = currentWebaction.Name;

            self.showLoader(true);
            // update webaction name in the query parameter
            myDropzone.options.url = myDropzone.options.url.replace("{name}", webactionName); 
            myDropzone.options.headers = Object.assign({}, myDropzone.options.headers,
                { WebsiteId: self.customerData.websiteId, DeveloperId: self.customerData.developerId });
            myDropzone.processQueue();
        },

        updateLinkOfImgaeObject: function (link) {
            var self = this;
            // check for empty link
            if (link && link.length > 0) {
                Vue.set(self.$data.currentWebactionData, "url", link);
                self.addOrUpdateData();
            } else {
                self.setIsErrorInUploading(true);
                toastr.error("Error", "file upload failed");
            }
            
        },

        setIsImageForUploading: function (isImage) {
            var self = this;
            Vue.set(self.upload, "isImage", isImage);
        },

        setIsErrorInUploading: function (isError) {
            var self = this;
            Vue.set(self.upload, 'isError', isError);
        },

        // --- end --- upload file methods 

        gotoInbox: function () {
            Vue.set(this.$data, "showTable", false);
            // reset all properties related to webaction rendered.
            Vue.set(this.$data, "currentClassName", "");
            Vue.set(this.$data, "currentModel", "");
            Vue.set(this.$data, "currentObjectId", "");
            Vue.set(this.$data, "currentWebaction", {});
            Vue.set(this.$data, "currentWebactionData", {});
        },

        formatPropertyName: function (propertyName) {
            if (propertyName) {
                propertyName = _.lowerCase(propertyName);
                return _.startCase(propertyName);
            }
            return propertyName;
        },

        hideAllModals: function () {
            var self = this,
                modalStatus = self.modalShowStatus;

            for (propertyName in modalStatus) {
                Vue.set(modalStatus, propertyName, false)
            }
        },

        propertyVisibilityStatus(propertyName) {
            var propertyExists = true;

            if (propertyName && propertyName.trim()) {
                propertyName = propertyName.trim().toLowerCase();
                propertyExists = PropertyVisiblityStatus.hasOwnProperty(propertyName);
                return propertyExists ? !!PropertyVisiblityStatus[propertyName] : true;
            }

            return true;
        },

        closeActiveModal: function () {
            closeActiveModals();
        },

        processDescription: function (description, trim) {
            var maxChars = 30;

            description = description ? description : "";

            if (description.length > maxChars && trim) {
                return (description.substr(0, maxChars) + "...");
            }

            return description;
        },

        getUserData: function() {
            var self = this;

            axios.get(WebactionsEndpoints.GetWebsiteUserDetails, { responseType: 'json' })
                .then(function(response) {
                    self.customerData = response.data;
                    self.genericHelpModal.supportEmailForm.to.push(self.customerData.contact.email);
                    self.getDeveloperDetails(self.customerData.developerId, function (email) {
                        self.genericHelpModal.supportEmailForm.to.push(email);
                    });
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
                url: WebactionsEndpoints.UploadFileToSystemWebaction,
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
                url: WebactionsEndpoints.SendEmail,
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
                url: WebactionsEndpoints.GetDeveloperDetails,
                data: { developerId: developerId }
            }).then(function (response) {
                callback(response.data);
            });
        },
    }
})
