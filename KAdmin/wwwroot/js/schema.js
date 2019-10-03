"use strict";

const Endpoints = Object.freeze({
    GetFormSchema: 'ManageWebsiteContent/GetFormSchema',
    GetLanguageSchema: 'ManageWebsiteContent/GetLanguageSchemaById',
    AddDataForSchema: 'ManageWebsiteContent/AddDataForSchema',
    UpdateDataForSchema: 'ManageWebsiteContent/UpdateDataForSchema',
    GetDataForSchema: 'ManageWebsiteContent/GetDataForSchema',
    GetDataFormReferenceId: 'ManageWebsiteContent/GetDataWithReferenceId',
    DeleteDataForSchema: 'ManageWebsiteContent/DeleteDataForSchema',
    UploadImageForSchema:
        CookieHelper.isAliCloud() ?
        'https://kadmin.getkitsune-alicloud.com/k-admin/ManageWebsiteContent/SaveUploadedFileV2':
        'ManageWebsiteContent/SaveUploadedFile',
    GetClassesByClassType: 'ManageWebsiteContent/GetClassesByClassType',
    GetDataByClassName: 'ManageWebsiteContent/GetDataByClassType',
    UploadFileToSystemWebaction: 'Inbox/UploadFileToSystemWebaction',
    AddDataToSystemWebaction: 'Inbox/AddDataToSystemWebaction',
    SendEmail: 'ManageWebsiteContent/SendEmail',
    GetWebsiteUserDetails: 'Settings/GetWebsiteUserDetails',
    GetDeveloperDetails: 'ManageWebsiteContent/GetDeveloperDetails',
    GetClientId: 'ManageWebsiteContent/GetWebsiteDetails',
    IsCallTrackerEnabled: 'ManageWebsiteContent/IsCallTrackerEnabled',
    GetCustomerName: 'ManageWebsiteContent/GetCustomerName',
    GetDataByProperty: 'ManageWebsiteContent/GetDataByProperty',
    GetDataByPropertyBulk: 'ManageWebsiteContent/GetDataByPropertyBulk',
});

const isWebaction = false;

Vue.component("VueFormGenerator", VueFormGenerator.component);
Vue.component('KModal', KModal);
Vue.component('k-header', KHeader);
Vue.use(VueTippy);
Vue.component('KConsoleModeHeader', KConsoleModeHeader);

const InputTypeMapping = {
    0: 'text',
    1: 'complex',
    2: 'number',
    3: 'checkbox',
    4: 'date',
    5: 'complex',
    6: 'complex'
}

const selectorsRegex = /(name)|(title)|(header)/i;

const advancedGroupName = "_advanced";

const PropertyVisibliityStatus = {
    "_kid": true,
    "k_referenceid": true,
    "createdon": true,
    "updatedon": true,
    "isarchived": true,
    "userid": true,
    "schemaid": true,
    "websiteid": true,
    "rootaliasurl": true,
    "_propertyName": true,
    "_parentClassName": true,
    "_parentClassId": true
};

const nativeMapping = {
    'str': 'text',
    'boolean': 'switch',
    'number': 'number',
    'datetime': 'date'
};

const nativeArrayTypes = ['str', 'boolean', 'number', 'datetime'];

const primitiveTypes = ['str', 'boolean', 'number', 'datetime'];

const Modal = Object.freeze({
    'DELETE': 'delete',
    'UPDATE': 'update',
    'UPLOAD': 'upload',
    'IMAGEPROCESSOR': 'imageProcessor',
    'RICHTEXTEDITOR': 'showRichTextEditorModal',
    'HAVINGISSUES': 'havingIssues',
    'VMNUPDATE': 'vmnUpdate',
    'DELETEOBJECT': 'deleteObject',
    'TEXTAREA': 'showTextAreaModal'
});

const SlidingPanelLevel = Object.freeze({
    "HIDDEN": 0,
    "CLASSES": 1,
    "PROPERTIES": 2,
    "OBJECTS": 3
});

const COMPLEX_OBJECT_TYPE = 5;
const KSTRING_OBJECT_TYPE = 7;
const ARRAY_TYPE = 1;

const OBJECT_TYPES = [COMPLEX_OBJECT_TYPE, KSTRING_OBJECT_TYPE];

const systemProperties = ['_id', '_kid', '_parentClassId', '_parentClassName', '_propertyName', 'k_referenceid', 'createdon', 'isarchived', 'updatedon', 'websiteid', '_reflectionId', 'schemaid', 'userid', ];

var referenceDataObjectTemplate = Object.freeze({
    data: null,
    selectedClassName: null,
    selectedPropertyName: null,
    level: SlidingPanelLevel.HIDDEN,
    requestedClassName: null,
    isForReverseReference: false,
    selectedForwardReferenceItem: null,
    forwardReference: {
        isMultipleSelect: false,
        selectedItems: [],
    },
    reverseReference: {
        relatedClassTypes: [],
        selectedItems: [],
        selectedPath: null,
        items: [],
        propertyToRefer: null,
    },
    isfetchingData: false,
});


var vm = new Vue({
    el: "#app",

    components: {
        "SlidingHeader": SlidingHeader,
        "SlidingBody": SlidingBody,
        "SlidingFooter": SlidingFooter,
        "SlidingPanel": SlidingPanel,
        "ContextPopoverMenu": ContextPopoverMenu
    },

    data: {
        hasSchema: true,
        new_site: false,

        model: {},
        schemaName: '',
        schemaData: {},

        legends: [],
        spath: [],
        reference_id: '',
        isArray: false,
        isNative: false,
        isArrayForRendering: false,
        history: [],

        currentPropertyList: [],
        currentModel: '',
        currentClassName: '',
        isAddingNewToArray: false,

        languageSchema: [],
        renderedForm: { groups: [] },
        schema: {},
        formOptions: {
            validateAfterLoad: true,
            validateAfterChanged: true,
            isDataConsoleMode: false,
            systemProperties: systemProperties,
            propertyMaxChars: 25,
            defaultPlaceholder: '-- -- --',
            objectPropertyPlaceholder: '*******',
            valueMaxChars: 50,
            defaultImageLink: 'https://s3.ap-south-1.amazonaws.com/kitsune-buildtest-resources/kadmin/no-image-square-placeholder.svg',
        },

        classesProcessed: [],

        configure: {
            maxChars: 50,
            defaultPlaceholder: '-- -- --',
            propertyMaxChars: 25,
            datetimePlaceholder: 'YYYY-MM-DD'
        },

        areFieldsEditable: false,

        delete: {
            that: null,
            index: null,
            displayIndex: null
        },

        nativearray: {
            arr: null
        },

        upload: {
            isImage: false,
            isError: false
        },

        currentPropertyGroup: {
            name: '',
            index: -1
        },

        showRemainingBreadCrumbs: false,

        modalShowStatus: {
            delete: false,
            update: false,
            upload: false,
            imageProcessor: false,
            showRichTextEditorModal: false,
            havingIssues: false,
            vmnUpdate: false,
            maxArrayCountForProperty: false,
            deleteObject: false,
            showTextAreaModal: false,
        },

        richText: {
            elementId: 'froalaEditor',
            content: '',
            requestedPropertyName: '',
            froala: null
        },

        textArea: {
            content: '',
            requestedPropertyName: '',
            froala: null,
        },

        richTextEditorOptions: {
            modules: {
                toolbar: [
                    ['bold', 'italic', 'underline', 'strike'],
                    [{ 'list': 'ordered' }, { 'list': 'bullet' }],
                    [{ 'align': [] }],
                    ['clean']
                ],
            }
        },

        referenceData: _.cloneDeep(referenceDataObjectTemplate),

        baseSelectedGroup: null,

        supportEmailForm: {
            subject: 'Reporting an issue for: ' + localStorage.getItem('DOMAIN'),
            message: '',
            image: null,
            to: [],
            clientId: null
        },

        systemWebactions: {
            supportEmail: { name: 'kadminsupportemail', authId: '5959ec985d643701d48ee8ab' }
        },

        customerData: null,
        isCallTrackerEnabledForWebsite: null,

        vmnData: {
            sameVMNNumberList: []
        },

        arrayPropertyMaxCountModal: {
            className: null,
            propertyName: null,
            maxLength: null,
        },

        isDataConsoleMode: false,

        deleteObject: {
            propertyName: null,
        },

        currentSchemaDataPath: [],

        oldModel: {}, // the oldModel can be used to get the old state of model before user edits the properties,

        arrayItemPagination: {
            limit: 10,
        },

        maxNavigationItemNumber: 3,

        propertyToDelete: {},

        maxArraySizeForDataFetching: 10000,

        arrayPropertyFilters: {
            searchFilter: null,
            sortFilter: null,
            ascDesc: 1,
        }
    },

    created: function () {
        var self = this;
        if (isConsoleMode) {
            self.isDataConsoleMode = true;
            self.removeSidebar();
            self.formOptions.isDataConsoleMode = true;
        }
        self.getUserData();
        self.loadLanguageSchema();
        self.isCallTrackerEnabled();

        self.isCallTrackerEnabled();
        toastr.options = {
            "positionClass": "toast-bottom-right"
        };
    },

    computed: {

        currentPropertyName() {
            var currentProperty = _.last(this.history);
            if (currentProperty) {
                return this.formatPropertyName(currentProperty.displayName);
            }
            return "Home";
        },

        deleteElementIndex() {
            return this.delete.displayIndex;
        },

        haskid() {
            return !!this.model['_kid'];
        },

        kidForReportingIssue() {
            var reportingId = this.model._kid;
            if (!reportingId && this.currentSchemaDataPath.length > 1) {
                var parentSegment = this.currentSchemaDataPath[this.currentSchemaDataPath.length - 2];
                reportingId = parentSegment._meta.parentClassId;
            }
            return reportingId;
        },

        modelNameForUpdate() {
            let model = this.currentModel;
            return (model ? model : this.legends[0]);
        },

        isFileImage() {
            return this.upload.isImage;
        },

        showButtonsForEditing() {
            var self = this,
                currentClassName = self.currentClassName;
            var showEdit = true;

            if (self.isAddingNewToArray && self.isArrayForRendering) {
                showEdit = true;
            } else if (self.isArrayForRendering) {
                showEdit = false;
            }

            showEdit = (self.isDataConsoleMode && !self.model._kid) ? false : showEdit;

            return showEdit;
        },

        isErrorInUploading() {
            let self = this;
            return self.upload.isError;
        },

        propertyGroups() {
            var self = this,
                className = self.currentClassName ? self.currentClassName : self.languageSchema.EntityName,
                currentClassProperties = [],
                uniqueGroups = [],
                isCurrentElementArray = self.isRenderingComplexArray;
            
            if (className) {
                currentClassProperties = self.getPropertyListFromClass(className);
                currentClassProperties.forEach(function (property) {
                    var groupName = property.GroupName;

                    if (groupName && (uniqueGroups.indexOf(groupName) == -1)) {
                        uniqueGroups.push(groupName);
                    }
                });
            }

            uniqueGroups = uniqueGroups ? uniqueGroups.sort() : uniqueGroups;

            return isCurrentElementArray ? [] : uniqueGroups;
        },

        currentGroupName() {
            return this.currentPropertyGroup.name;
        },

        saveBtnShow() {
            var self = this;
            return (self.areFieldsEditable || self.isRenderingNativeArray);
        },

        isBaseClass() {
            var self = this,
                currentClassName = self.$data.currentClassName;
            if (currentClassName == "") {
                return true;
            }
            return false;
        },

        isRenderingNativeArray() {
            var self = this;
            var lastSegment = _.last(self.currentSchemaDataPath);
            return (lastSegment && lastSegment.Type === ARRAY_TYPE && lastSegment.hasOwnProperty('NativeArrayProperty'));
        },


        /**
        * Referencing 
        **/
        getReferenceClassNames() {
            var self = this,
                data = self.referenceData.data,
                groups = data ? data.GroupCount : [],
                classNames = [];

            if (groups && groups.length > 0) {
                var groupsNameAndCount = function (group) {
                    return {
                        name: group.Name,
                        matchedProperties: group.Count
                    }
                }
                classNames = groups.map(groupsNameAndCount);
            }
            return classNames;
        },

        /**
        * Referencing 
        **/
        getPropertyNamesByClassName: function () {
            var self = this,
                data = self.referenceData.data,
                groups = (data && data.GroupCount) ? data.GroupCount : [],
                propertyNames = [],
                className = self.referenceData.selectedClassName;

            groups.map(function (group) {
                if (group.Name === className) {
                    group.SubGroupCounts.map(function (subGroup) {
                        propertyNames.push({
                            name: subGroup.Name,
                            matchedProperties: subGroup.Count
                        });
                    });
                }
            });

            return propertyNames;
        },

        /**
        * Referencing: data in Level 3 Sliding panel
        **/
        getDataForClassAndProperty: function () {
            var self = this,
                properties = [],
                className = self.referenceData.selectedClassName,
                propertyName = self.referenceData.selectedPropertyName,
                data = self.referenceData.data ? self.referenceData.data : {},
                dataObjects = data.Data ? data.Data : [];

            if (!self.referenceData.isForReverseReference) {
                if (className && className.trim() && propertyName && propertyName.trim()) {
                    var selectedClassName = className.trim().toLowerCase();
                    var selectedPropertyName = propertyName.trim().toLowerCase();

                    var selectedData = function (obj) {
                        var propertyClassName = obj._parentClassName ? obj._parentClassName.toLowerCase() : "",
                            propertyName = obj._propertyName ? obj._propertyName.toLowerCase() : "";

                        return (propertyClassName === selectedClassName && selectedPropertyName === propertyName);
                    };

                    var isNotEmpty = function (item) {
                        var isNonEmpty = false;

                        Object.keys(item).map(function (property) {
                            if (!self.getSystemProperties.includes(property) && item[property] != null && isNonEmpty == false) {
                                isNonEmpty = true;
                            }
                        });
                        return isNonEmpty;
                    };

                    properties = dataObjects.filter(selectedData).filter(isNotEmpty);
                } else {
                    //console.info("getDataForClassAndProperty, className or propertyName not valid : ", className, propertyName);
                }
            }

            return properties;
        },

        /**
        * Referencing - get the properties of the class to display in the sidebar level 3
        **/
        getPropertiesToDisplay: function () {

            var self = this;
            var className = self.referenceData.requestedClassName;
            var propertiesToDisplay = [];

            if (className && className.trim()) {

                var propertiesList = self.getPropertyListFromClass(className);
                if (propertiesList && propertiesList.length > 0) {
                    var selectorsProperties = self.findSelectors(propertiesList); // gets preferred properties
                    var basicProperties = self.containProperties(propertiesList); // properties to display excluding system properties
                    if (selectorsProperties && selectorsProperties.length > 0) {
                        propertiesToDisplay = selectorsProperties;
                    } else if (basicProperties && basicProperties.length > 0) {
                        propertiesToDisplay = basicProperties;
                    }

                } else {
                    //console.error("No properties found for className : ", className);
                }

            } else {
                //console.error("getPropertiesToDiplay : className not valid ", className);
            }
            return propertiesToDisplay;
        },

        /**
        * Referencing get data from the head
        **/
        getReferenceHeaderText: function () {
            var self = this;
            var referenceData = self.referenceData;

            if (referenceData.selectedPropertyName == null && referenceData.selectedClassName == null) {
                return '';
            } else if (referenceData.selectedPropertyName == null) {
                return referenceData.selectedClassName;
            } else {
                return referenceData.selectedPropertyName + ' of ' + referenceData.selectedClassName;
            }
        },

        isForReverseReference: function () {
            return this.referenceData.isForReverseReference;
        },

        getContextMenu: function () {
            var self = this;
            return [{ name: 'Add to', action: self.getSimilarPropertiesToReferTo }];
        },

        isContextMenuVisible: function () {
            return (this.isBaseClass === false &&
                this.isArrayForRendering == false &&
                this.model._kid && !this.isAtRootLevel);
        },

        getSystemProperties: function () {
            return systemProperties;
        },

        isRenderingPhoneNumber: function () {
            return this.currentClassName.trim().toLowerCase() === 'phonenumber';
        },

        getArrayPropertyMaxCount: function () {
            var self = this;
            var className = self.getParentClassName();
            var propertyName = self.currentModel;
            var advancedProperties = self.getAdvancedPropertyByClassAndProperty(className, propertyName);
            var maxCount = null;
            if (advancedProperties) {
                if (advancedProperties.hasOwnProperty('ArrayMaxLength')) {
                    maxCount = advancedProperties.ArrayMaxLength;
                    if (typeof maxCount == 'string' && isNaN(parseInt(maxCount))) {
                        maxCount = _.get(self.schemaData, maxCount);
                    }
                }
            }
            return maxCount;
        },

        getPropertiesForActiveGroup: function () {
            var self = this;
            var propertyList = [];
            self.schema.groups[0].fields.map(function (field) {
                if ((self.currentGroupName == '' && !PropertyVisibliityStatus[field.model]) || (field.groupName === self.currentGroupName)) {
                    propertyList.push(field.model);
                }
            });
            return propertyList;
        },

        getPropertiesForCurrentRenderedProperty: function () {
            var self = this;
            if (!self.currentSchemaDataPath || !Array.isArray(self.currentSchemaDataPath) || !self.currentSchemaDataPath.length) {
                return [];
            }
            let length = (self.currentSchemaDataPath.length - 1);
            var currentPropertyDataType = self.currentSchemaDataPath[length].PropertyDataType;
            return self.getPropertyListFromClass(currentPropertyDataType);
        },

        getCurrentRenderedPropertyDetails: function () {
            var self = this;
            return self.currentSchemaDataPath[self.currentSchemaDataPath.length - 1];
        },

        isAtRootLevel: function () {
            return (this.currentSchemaDataPath &&
                Array.isArray(this.currentSchemaDataPath) &&
                this.currentSchemaDataPath.length === 1);
        },

        schemaNavigationPathForRender: function () {
            var self = this;
            var navigationSegments = [];
            self.currentSchemaDataPath.map(function (segment, index) {
                if (segment.Type == ARRAY_TYPE && segment.Filter && segment.Filter.hasOwnProperty('_kid')) {
                    var arraySegment = _.cloneDeep(segment);
                    var arrayItemSegment = _.cloneDeep(segment);
                    var displayText = _.startCase(arraySegment.PropertyName);
                    arraySegment.displayText = displayText;
                    delete arraySegment.Filter;
                    navigationSegments.push(arraySegment);
                    displayText = _.startCase(arraySegment.PropertyDataType) + ' # ' + arraySegment._meta.ClickedIndex;
                    arrayItemSegment.displayText = displayText;
                    arrayItemSegment.isArrayItem = true;
                    navigationSegments.push(arrayItemSegment);
                } else {
                    var pathSeg = _.cloneDeep(segment);
                    var displayText = _.startCase(segment.PropertyName);
                    if (index == 0) {
                        displayText = _.startCase('Home');
                    }
                    pathSeg.displayText = displayText;
                    navigationSegments.push(pathSeg);
                }
            });

            return navigationSegments;
        },

        getPopoverSchemaNavigationItems: function () {
            var self = this;
           return self.schemaNavigationPathForRender.slice(0, (self.schemaNavigationPathForRender.length - self.maxNavigationItemNumber));
        },

        getSchemaNavigationItemsWihoutPopover: function () {
            var self = this;
            var offset = self.schemaNavigationPathForRender.length - self.maxNavigationItemNumber > 0 ? self.schemaNavigationPathForRender.length - self.maxNavigationItemNumber : 0
            return self.schemaNavigationPathForRender.slice(offset);
        },

        showSaveEditBtns: function () {
            var self = this;
            return _.isEmpty(self.model._kid);
        },

        isAddingNewToComplexArray: function () {
            var self = this;
            var isAdding = false;
            var lastSegment = self.currentSchemaDataPath[self.currentSchemaDataPath.length - 1];
            if (lastSegment.Type === 1 && !lastSegment.hasOwnProperty('Filter') && !self.isNative) {
                isAdding = true;
            }
            return isAdding;
        },

        isRenderingComplexArray: function () {
            var self = this;
            var isRendering = false;
            var lastSegment = self.currentSchemaDataPath[self.currentSchemaDataPath.length - 1];
            if (lastSegment && lastSegment.Type === 1 && !lastSegment.hasOwnProperty('Filter') && lastSegment.hasOwnProperty('Index')
                && lastSegment.hasOwnProperty('Length') && lastSegment.hasOwnProperty('Limit')) {
                isRendering = true;
            }
            return isRendering;
        },

        getPaginationDataForCurrentArray: function () {
            var self = this;
            var lastSegment = _.last(self.currentSchemaDataPath);
            if (lastSegment.Type === ARRAY_TYPE && !lastSegment.hasOwnProperty('Filter')) {
                return {
                    length: lastSegment.Length,
                    index: lastSegment.Index,
                    pageSize: lastSegment.Limit
                };
            }
            return null;
        },

        getArrayPropertySortProps: function () {
            var self = this;
            var propertyClass = _.find(self.languageSchema.Classes, { Name: self.currentClassName });
            if (!propertyClass) {
                return null;
            }
            var requiredProps = [];
            propertyClass.PropertyList.map(function (prop) {
                if (!systemProperties.includes(prop.Name) && ((prop.DataType.Name.toLowerCase() === 'str') || prop.DataType.Name.toLowerCase() === 'number')) {
                    requiredProps.push(prop.Name);
                }
            });

            if (requiredProps.length > 0) {
                return requiredProps;
            } else {
                return null;
            }
        },
    },

    methods: {

        loadLanguageSchema: function () {
            var self = this;
            self.getLanguageSchema(function (data) {
                self.showLoader(false);
                self.languageSchema = data;
                self.schemaName = data.EntityName;
                self.processAllClasses();
                self.getBaseClassProperties();
                let schema = self.getProcessedClass(data.EntityName);
                self.currentSchemaDataPath.push({ PropertyName: data.EntityName, PropertyDataType: data.EntityName, Type: 5 });
                self.getSchemaForActiveProperty();
                self.resetCurrentGroup();
                self.showLoader(true);
                self.getBaseClassIdForWebsite(function (response) {
                    self.new_site = false;
                    self.getSchemaDataForActiveProperty();
                    toastr.success('Load successful');
                }, function (err) {
                    self.new_site = true;
                    self.showLoader(false);
                });
            }, function (err) {
                self.showLoader(false);
            });
        },

        getUserData: function () {
            var self = this;

            axios.get(Endpoints.GetWebsiteUserDetails, { responseType: 'json' })
                .then(function (response) {
                    self.customerData = response.data;
                    self.supportEmailForm.to.push(self.customerData.contact.email);
                    self.getDeveloperDetails(self.customerData.developerId, function (email) {
                        self.supportEmailForm.to.push(email);
                    });
                    myDropzone.options.headers = Object.assign({}, myDropzone.options.headers,
                        { WebsiteId: self.customerData.websiteId, DeveloperId: self.customerData.developerId });
                });
        },

        getLanguageSchema: function (successCallback, errorCallback) {
            this.showLoader(true);
            axios({
                method: 'post',
                url: Endpoints.GetLanguageSchema
            }).then(function (response) {
                successCallback(response.data);
            }).catch(function (err) {
                errorCallback(err);
            });
        },

        isCallTrackerEnabled: function () {
            var self = this;
            axios({
                method: 'post',
                url: Endpoints.IsCallTrackerEnabled
            }).then(function (response) {
                if (response.data && response.data.hasOwnProperty('isActive')) {
                    self.isCallTrackerEnabledForWebsite = response.data.isActive;
                } else {
                    self.isCallTrackerEnabledForWebsite = false;
                }
            }).catch(function () {
                self.isCallTrackerEnabledForWebsite = false;
            });
        },

        // toggling schema loader
        showLoader: function (show) {
            var schemaConatiner = document.getElementById('app');
            if (show) {
                showLoader(schemaConatiner);
            } else {
                removeLoader(null, true);
            }
        },

        // preprocess all classes received from schema
        processAllClasses: function () {
            let self = this;
            _.forEach(self.languageSchema.Classes, (cls) => {
                if (cls.ClassType != 3 && cls.Name != 'kstring' && cls.Name != 'phonenumber') {
                    self.getSchemaFromClass(cls);
                } else if (cls.Name == 'kstring') {
                    let name = cls.Name;
                    let kstringProcessedClass = self.getKstringObject(name);
                    self.classesProcessed.push({ Class: kstringProcessedClass, Name: name });
                } else if (cls.Name == 'phonenumber') {
                    let name = cls.Name;
                    let phoneNumberProcessedClass = self.getPhoneNumberFormField(name);
                    self.classesProcessed.push({ Class: phoneNumberProcessedClass, Name: name });
                } else {
                    let name = cls.Name;
                    let obj = self.getNativeClassObject(name);
                    self.classesProcessed.push({ Class: obj, Name: name });
                }
            });
        },

        getProcessedClass: function (name, field) {
            let self = this;

            Vue.set(self, 'isNative', self.isClassNativeClass(name));

            let clas = _.find(self.classesProcessed, (cls) => {
                return cls.Name.trim().toLowerCase() === name.trim().toLowerCase();
            });

            if (name.trim().toLowerCase() == 'kstring' && field && field.isRichTextEnabled) {
                return (Object.assign({}, self.getKstringObject("kstring", true)));
            }

            return Object.assign({}, clas.Class);
        },

        formatPropertyName: function (propertyName) {
            if (propertyName) {
                return _.startCase(propertyName);
            }
            return propertyName;
        },

        isClassNativeClass: function (name) {
            let self = this;
            name = name.trim().toLowerCase();
            if (name == 'kstring' || name == 'phonenumber') {
                return false;
            }
            let classes = self.languageSchema.Classes;
            let clas = _.find(classes, function (cls) {
                return cls.Name.trim().toLowerCase() == name.trim().toLowerCase();
            });
            return (clas.ClassType == 3);
        },

        setCurrentGroup: function (groupName, index) {
            var self = this,
                currentPropertyGroup = self.currentPropertyGroup,
                areFieldsEditable = self.areFieldsEditable;

            if (areFieldsEditable) {
                self.unsavedChangesModal(true);
                return;
            }

            if (self.isBaseClass) {
                Vue.set(self, 'baseSelectedGroup', {
                    index: index,
                    groupName: groupName
                });
            }

            self.resetArrayPropertyFilter();
            Vue.set(currentPropertyGroup, 'index', index);
            Vue.set(currentPropertyGroup, 'name', groupName);
            var activeGroupObj = {
                index: index,
                name: groupName,
            };
            self.currentSchemaDataPath[self.currentSchemaDataPath.length - 1].CurrentGroupName = activeGroupObj;
            self.updateCurrentPointHistory(groupName);

        },

        resetCurrentGroup: function (groupName) {
            var self = this,
                currentPropertyGroup = self.currentPropertyGroup,
                propertyGroups = self.propertyGroups,
                index = 0;

            var firstGroupName = _.first(propertyGroups);

            groupName = (groupName !== undefined && groupName !== null) ? groupName : "";
            index = _.indexOf(propertyGroups, groupName);

            /*
             * if groupName is passed set that group
             * else set first groupName found
             * else set Advanced
             */
            if (groupName && index >= 0) {
                self.setCurrentGroup(groupName, index);
            } else if (groupName == advancedGroupName) {
                self.setCurrentGroup(advancedGroupName, -1);
            } else if (firstGroupName) {
                self.setCurrentGroup(firstGroupName, 0);
            } else {
                self.setCurrentGroup(advancedGroupName, -1);
            }
        },

        getDataForSchemaSlim: function (callback) {
            $.ajax({
                type: 'POST',
                url: Endpoints.GetDataForSchema,
                success: function (data) {
                    if (callback && typeof callback == 'function') {
                        callback(data);
                    }
                }
            });
        },


        isCurrentGroup: function (groupName) {
            var self = this;
            return (groupName == this.currentGroupName);
        },

        // ---- Upload ----
        startUploadFile: function () {
            let self = this;
            self.showLoader(true);
            myDropzone.processQueue();
        },

        // get the link for image and update the current object
        updateLinkOfImgaeObject: function (link) {
            var self = this;
            var model = {
                url: link
            };
            Vue.set(self, 'model', model);
            self.updateDataForSchema();
            self.showLoader(false);
        },

        setIsImageForUploading: function (isImage) {
            let self = this;
            Vue.set(self.upload, 'isImage', isImage);
        },

        setIsErrorInUploading: function (isError) {
            let self = this;
            Vue.set(self.upload, 'isError', isError);
        },

        closeUploadModal: function () {
            Vue.set(Modal.UPLOAD, false);
        },
        // ---- Upload End ----

        // makes all properties editable or uneditable
        editAllButton: function () {
            let self = this;
            let fields = self.schema.groups[0].fields;
            let btn = null;

            if (!self.areFieldsEditable) {

                if (self.isNative && self.isArrayForRendering) {

                    let data = self.model[self.currentModel];
                    _.forEach(data, function (ele) {
                        ele.readonly = false;
                    })

                } else {
                    _.forEach(fields, function (field) {
                        if (field.readonly && field.visible && !self.isKeywordPropertyInKstring(field.model)) {
                            if (!field.advancedProperties ||
                                (field.advancedProperties.hasOwnProperty('ReadOnly') && !field.advancedProperties.ReadOnly)) {
                                field.readonly = !field.readonly;
                            }
                        }
                    });
                }

                self.toggleFieldsEditable(true);

            } else {
                if (self.isNative && self.isArrayForRendering) {

                    let data = self.model[self.currentModel];
                    _.forEach(data, function (ele) {
                        ele.readonly = true;
                    })

                } else {
                    _.forEach(fields, function (field) {
                        if (!field.readonly && field.visible && !self.isKeywordPropertyInKstring(field.model)) {
                            field.readonly = !field.readonly;
                        }
                    });
                }
                self.toggleFieldsEditable(false);
            }
        },

        isKeywordPropertyInKstring(modelName) {
            var self = this,
                currentClassName = self.currentClassName,
                currentClassName = _.lowerCase(currentClassName);
            modelName = _.lowerCase(modelName);
            if (currentClassName == 'kstring' && modelName == 'keywords') {
                return true;
            }
            return false;
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

        getSchemaFromClass: function (klass, modelName) {
            let self = this;

            let klassObj = self.getClassObject(), // to get the class template
                { Name } = klass;

            klassObj.model = modelName ? modelName : Name.toLowerCase();
            klassObj.schema.groups[0].legend = Name.toLowerCase();

            klass.PropertyList = _.sortBy(klass.PropertyList, ['Type']); // sorts properties for displaying in ui

            _.forEach(klass.PropertyList, function (property) {

                let { Type, Name, DataType, Description, GroupName, _AdvanceProperties } = property;
                let className = DataType.Name;
                let prop = null;
                let isUrlPropertyInImage = false;

                var isStringRichText = (Type == 0 || className.toLowerCase() == 'kstring') ? self.isRichTextEnabled(property) : false;

                var isTextArea = self.isTextAreaEnabled(property);

                var isStringDropdown = self.isStringDropdownEnabled(property);

                var maxCharacterLimit = self.getMaxCharacterLimit(property);

                var isPhoneNumberType = (Type == 8 && DataType.Name === 'PHONENUMBER');

                // if not object, array or kstring or not phonenumber
                if (Type != 1 && Type != 5 && Type != 7 && !isStringRichText && !isPhoneNumberType && !isTextArea && !isStringDropdown) {

                    // checks if property is url in image class need upload button
                    if (klass.Name == 'image' && Name == 'url') {
                        isUrlPropertyInImage = true;
                    }

                    prop = self.getPropertyObject(Name, Type, Description, isUrlPropertyInImage, GroupName, DataType, maxCharacterLimit);

                    // url in image class doesnot need inputtype
                    if (!isUrlPropertyInImage) {
                        prop.inputType = InputTypeMapping[Type];
                        if (self.isDataConsoleMode) {
                            prop.advancedProperties = Object.assign({}, { ReadOnly: true });
                        }
                    }


                } else if (Type == 5 || Type == 1 || Type == 7 || isStringRichText || isPhoneNumberType || isStringDropdown || isTextArea) {
                    var config = {
                        isArray: Type == 1,
                        propertyName: Name,
                        groupName: GroupName,
                        type: Type,
                        isStringRichText: isStringRichText,
                        propertyDataType: className,
                        propertyType: property.Type,
                        placeholder: Description,
                        isStringDropdown: isStringDropdown,
                        isTextArea: isTextArea,
                    };
                    prop = self.getObjectSchema(config);
                    prop.model = Name;
                    prop.schema.groups[0].legend = Name;
                    prop.className = klass.Name;
                }

                if (prop) {
                    if (_AdvanceProperties) {
                        prop.advancedProperties = _AdvanceProperties;
                        if (self.isDataConsoleMode) {
                            if (prop.advancedProperties.hasOwnProperty('ReadOnly')) {
                                prop.advancedProperties.ReadOnly = false;
                            }
                        }
                    }

                    klassObj.schema.groups[0].fields.push(prop);
                }
            })

            self.classesProcessed.push({ Class: klassObj, Name: klass.Name });
        },

        getClassObject: function () {
            let classObj = {
                type: 'object',
                model: '',
                schema: {
                    groups: [{
                        legend: '',
                        fields: []
                    }]
                }
            }
            return Object.assign({}, classObj);
        },

        isRichTextEnabled: function (property) {
            var isRichText = false;
            var advanceProperties = property._AdvanceProperties;
            isRichText = advanceProperties ? advanceProperties.RichText : false;
            return isRichText ? true : false;
        },

        isStringDropdownEnabled: function (property) {
            var isStringDropdown = false;
            var advanceProperties = property._AdvanceProperties;
            isStringDropdown = advanceProperties ? advanceProperties.StringDropdown : false;
            return isStringDropdown;
        },

        getMaxCharacterLimit: function (property) {
            var charMaxLength = null;
            var advanceProperties = property._AdvanceProperties;
            charMaxLength = advanceProperties && advanceProperties.hasOwnProperty('CharMaxLength') ? advanceProperties.CharMaxLength : false;
            return charMaxLength;
        },

        isTextAreaEnabled: function (property) {
            var isTextArea = false;
            var advanceProperties = property._AdvanceProperties;
            isTextArea = advanceProperties ? advanceProperties.TextArea : false;
            return isTextArea;
        },

        getPropertyObject: function (propertyName, propertyType, description, isUrlInImageClass, groupName, dataType, charLimit) {
            let self = this;
            let propertyObj = {
                type: 'input',
                inputType: '',
                label: propertyName,
                model: propertyName,
                groupName: groupName,
                visible: !self.getPropertyVisibilityStatus(propertyName),
                placeholder: description ? description : self.configure.defaultPlaceholder,
                readonly: true,
            };

            if (charLimit) {
                propertyObj.maxlength = charLimit;
            }

            let switchObj = {
                type: "switch",
                label: propertyName,
                model: propertyName,
                groupName: groupName,
                textOn: "True",
                textOff: "False",
                visible: !self.getPropertyVisibilityStatus(propertyName),
                readonly: true,
                onChanged: function () {
                    var then = self;
                    if (!self.areFieldsEditable) {
                        Vue.set(then, 'areFieldsEditable', true);
                    }
                }
            };

            let urlPropertyInImageClass = {
                type: 'label',
                label: propertyName,
                model: propertyName,
                visible: !self.getPropertyVisibilityStatus(propertyName),
                placeholder: description ? description : self.configure.defaultPlaceholder,
                readonly: true,
                buttons: [
                    {
                        classes: "kitsune-btn-primary upload-btn",
                        label: "upload",
                        isUpload: true,
                        onclick: function (model, field) {
                            cancelUpload();
                            let btn = document.getElementById('uploadFileInit');
                            btn.click();
                        }
                    }
                ]
            };

            let dateTimeClass = {
                type: 'datetime',
                label: propertyName,
                model: propertyName,
                groupName: groupName,
                visible: !self.getPropertyVisibilityStatus(propertyName),
                placeholder: description,
                format: 'YYYY-MM-DD',
                readonly: true,
                fieldClasses: 'form-control col-md-6'
            };

            let phoneNumberClass = {
                type: 'phonenumber',
                label: propertyName,
                model: propertyName,
                groupName: groupName,
                visible: !self.getPropertyVisibilityStatus(propertyName),
                placeholder: description ? description : self.configure.defaultPlaceholder,
                readonly: true
            };

            if (self.isDataConsoleMode) {
                urlPropertyInImageClass.buttons = [];
            }


            // upload button for image class
            if (isUrlInImageClass) {
                return Object.assign({}, urlPropertyInImageClass);
            }

            if (dataType.Name === 'DATE') {
                return Object.assign({}, dateTimeClass);
            }

            return Object.assign({}, (propertyType == 3 ? switchObj : propertyObj));
        },

        getPropertyVisibilityStatus: function (propertyName) {
            propertyName = propertyName.trim().toLowerCase();
            return PropertyVisibliityStatus[propertyName];
        },

        getObjectSchema: function (config) {
            var self = this;
            var fieldType = (config.type == 0) ? 'richtext' : (config.isArray ? 'arrobj' : 'obj');

            if (config.isTextArea) {
                fieldType = 'textMultiLine';
            } else if (config.isStringDropdown) {
                fieldType = 'select';
            }

            var classObj = {
                visible: !self.getPropertyVisibilityStatus(config.propertyName),
                type: fieldType,
                model: '',
                className: '',
                groupName: config.groupName,
                isRichTextEnabled: config.isStringRichText,
                isTextArea: config.isTextArea,
                schema: {
                    groups: [{
                        legend: '',
                        fields: []
                    }]
                },
                propertyDataType: config.propertyDataType,
                propertyType: config.propertyType,
                placeholder: config.placeholder || null,
            };

            if (fieldType === 'arrobj') {
                Object.assign(classObj, {
                    addNewItemToArray: self.onClickAddToArrayProperty,
                    showArrayItems: self.onClickShowArrayItems,
                    nativeArrayTypes: nativeArrayTypes,
                });
            } else if (fieldType === 'obj') {
                Object.assign(classObj, {
                    showPropertyDetail: self.onClickShowObjectProperty,
                    initializeCurrentObj: self.saveCurrentObj,
                    deleteImage: self.confirmDeleteProperty,
                    selectFromExisting: self.selectFromExisting,
                });
            } else if (fieldType === 'select') {
                Object.assign(classObj, {
                    label: config.propertyName,
                    values: JSON.parse(config.isStringDropdown),
                    selectOptions: {
                        name: 'displayText',
                        value: 'value'
                    },
                    disabled: self.isDataConsoleMode,
                    onChanged: function () {
                        if (!self.areFieldsEditable) {
                            Vue.set(self, 'areFieldsEditable', true);
                        }
                    }
                });
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
                                    readonly: false,
                                    inputType: nativeMapping[className],
                                    label: className,
                                    model: "val",
                                    type: "input",
                                    visible: true
                                },
                                {
                                    readonly: false,
                                    inputType: "text",
                                    label: "_kid",
                                    model: "_kid",
                                    type: "input",
                                    visible: false
                                },
                                {
                                    readonly: false,
                                    inputType: "text",
                                    label: "k_referenceid",
                                    model: "k_referenceid",
                                    type: "input",
                                    visible: false
                                },
                                {
                                    readonly: false,
                                    inputType: "text",
                                    label: "_propertyName",
                                    model: "_propertyName",
                                    type: "input",
                                    visible: false
                                },
                                {
                                    readonly: false,
                                    inputType: "text",
                                    label: "_parentClassName",
                                    model: "_parentClassName",
                                    type: "input",
                                    visible: false
                                },
                                {
                                    readonly: false,
                                    inputType: "text",
                                    label: "_parentClassId",
                                    model: "_parentClassId",
                                    type: "input",
                                    visible: false
                                },
                                {
                                    readonly: false,
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
                obj.schema.groups[0].fields[0] = {
                    type: 'datetime',
                    label: className,
                    format: 'YYYY-MM-DD',
                    model: "val",
                    readonly: false,
                    //styleClasses: 'col-md-6 col-xs-12'
                };
            }

            return Object.assign({}, obj);
        },

        getNativeClassObjectForArray: function (val) {
            let self = this;
            let nativeObj = {
                val: null,
                isarchived: false,
                readonly: true
            }
            let local = Object.assign({}, nativeObj);
            if (val && self.currentClassName == 'datetime') {
                val = moment(new Date(val)).format('YYYY-MM-DD');
                local.val = val;
            } else if (val) {
                local.val = val;
            }

            return local;
        },

        getKstringObject: function (propertyName, isRichText) {
            var self = this;

            var KstringRichTextEditor = {
                visible: true,
                type: 'richtext',
                model: 'text',
                schema: {
                    groups: [{
                        legend: '',
                        fields: []
                    }]
                }
            };

            var KstringPlainText = {
                type: 'input',
                inputType: 'text',
                label: "text",
                model: 'text',
                readonly: true,
                visible: true,
                placeholder: self.configure.defaultPlaceholder,
                advancedProperties: self.isDataConsoleMode ? Object.assign({}, { ReadOnly: true }) : null,
            }

            let kstring =
                {
                    type: 'object',
                    model: propertyName,
                    schema:
                    {
                        groups: [{
                            legend: propertyName,
                            fields: [
                                isRichText ? KstringRichTextEditor : KstringPlainText,
                                {
                                    type: 'input',
                                    inputType: 'select',
                                    label: "keywords",
                                    model: 'keywords',
                                    readonly: true,
                                    visible: true,
                                    placeholder: self.configure.defaultPlaceholder
                                },
                                {
                                    type: 'input',
                                    inputType: 'string',
                                    label: "_kid",
                                    model: '_kid',
                                    readonly: true,
                                    visible: false,
                                }
                            ]
                        }]
                    }
                }
            return Object.assign({}, kstring);
        },

        getPhoneNumberFormField: function (propertyName) {
            var self = this;

            var InputField = {
                type: 'input',
                inputType: 'number',
                label: 'text',
                model: 'text',
                visible: true,
                readonly: true,
                advancedProperties: self.isDataConsoleMode ? Object.assign({}, { ReadOnly: true }) : null,
                placeholder: self.configure.defaultPlaceholder
            };

            var countryCodeField = Object.assign({}, InputField, {
                label: 'Country Code',
                model: 'countrycode',
                inputType: 'tel',
                onChanged: function () {
                    if (this.value != '+91' || this.value != '91') {
                        self.model.isactive = false;
                        Vue.set(self, 'areFieldsEditable', true);
                    }
                },
            });

            var contactNumberField = Object.assign({}, InputField, { label: 'Contact Number', model: 'contactnumber' });

            var callTrackerNumberField = Object.assign({}, InputField, {
                type: 'label',
                label: 'Call Tracker Number',
                model: 'calltrackernumber',
                get: function (model) {
                    return model.calltrackernumber || self.configure.defaultPlaceholder;
                },
                visible: function () { return self.isCallTrackerEnabledForWebsite }
            });

            var isCallTrackerActive = {
                type: 'switch',
                label: self.model.isactive == true ? 'Deactivate Call Tracker' : 'Activate Call Tracker',
                model: 'isactive',
                textOn: "True",
                textOff: "False",
                visible: function () {
                    return (this.model.countrycode == '+91' || this.model.countrycode == '91')
                        && self.isCallTrackerEnabledForWebsite;
                },
                readonly: false,
                validator: function () {
                    return true;
                },
                onValidated: function (model, errors, field) {
                    if (model.isactive == true) {
                        field.label = 'Deactivate Call Tracker';
                    } else {
                        field.label = 'Activate Call Tracker';
                    }
                    return true;
                },
                onChanged: function (model, newVal, oldVal, field) {
                    var then = self;
                    if (!self.areFieldsEditable) {
                        Vue.set(then, 'areFieldsEditable', true);
                    }

                    if (model.isactive === true) {
                        field.label = 'Deactivate Call Tracker';
                    } else {
                        field.label = 'Activate Call Tracker';
                    }
                }
            };

            var phoneNumberObject = {
                type: 'object',
                model: propertyName,
                schema: {
                    groups: [{
                        legend: propertyName,
                        fields: [countryCodeField, contactNumberField, callTrackerNumberField, isCallTrackerActive]
                    }]
                }
            };

            return Object.assign({}, phoneNumberObject);
        },

        getArrayListFieldSchema: function (schema, arrProperty) {
            var self = this;
            let arraySchema = {
                groups: [
                    {
                        fields: [
                            {
                                model: arrProperty.PropertyName,
                                schema: {
                                    fields: []
                                },
                                type: 'arrayList',
                                propertyDataType: arrProperty.PropertyDataType,
                                propertyName: arrProperty.PropertyName,
                                primitiveTypes: primitiveTypes,
                                getNextItemsForArrayProperty: self.getNextItemsForArrayProperty,
                                getPreviousItemsForArrayProperty: self.getPreviousItemsForArrayProperty,
                                getArrayItem: self.getArrayItemWithId,
                                getCurrentPageIndex: function () {
                                    self.getCurrentRenderedPropertyDetails.Index
                                },
                                addNewItemToArray: self.onClickAddToArrayProperty,
                                removeArrayItem: self.confirmDeleteProperty,
                                isNativeArray: self.isNative,
                                getPaginationData: function () {
                                    return self.getPaginationDataForCurrentArray
                                },
                                addExistingItemsToArray: self.addExistingItemsToArray,
                                onSearch: self.onArrayPropertySearch,
                                onSortPropertyChanged: self.onArrayPropertySortPropertyChanged,
                                onSortOrderChanged: self.onArrayPropertySortOrderChanged,
                                filterProperties: self.arrayPropertyFilters,
                                sortProperties: self.getArrayPropertySortProps,
                            }
                        ],
                        legend: null
                    }
                ]
            };
            let fields = schema.schema.groups[0].fields;
            let legend = schema.schema.groups[0].legend;
            arraySchema.groups[0].fields[0].schema.fields = fields;
            arraySchema.groups[0].legend = arrProperty.PropertyName;
            return arraySchema;
        },

        getBaseClassProperties: function () {
            let self = this;
            let baseClass = _.find(self.languageSchema.Classes, { ClassType: 1 });
            let properties = baseClass.PropertyList;

            let className = baseClass.Name;
            let exists = _.find(self.legends, function (legend) { return (legend == className) });
            if (!exists) {
                self.legends.push(className);
            }
            self.currentPropertyList = properties;
        },

        getPropertyListFromClass: function (className) {
            if (!className) {
                return null;
            }

            let self = this;
            let classes = this.languageSchema.Classes;
            let currentClass = _.find(classes, function (cls) {
                return cls.Name.trim().toLowerCase() == className.trim().toLowerCase();
            });
            if (!_.isEmpty(currentClass)) {
                return currentClass.PropertyList;
            }
            return null;
        },

        updateCurrentPointHistory: function (groupName) {
            var self = this,
                history = self.history,
                last = _.last(history);

            if (last &&
                groupName !== undefined &&
                groupName !== null) {
                Vue.set(last, 'groupName', groupName);
            }
        },

        getBulkPropertyPathRequest: function (currentSchemaDataPath) {
            var self = this;
            var requiredKeysForRequest = ['PropertyName', 'PropertyDataType', 'Type', 'Index', 'Limit', 'ObjectKeys', 'Filter'];

            var lastPointInPath = currentSchemaDataPath[currentSchemaDataPath.length - 1];
            if (!lastPointInPath || !lastPointInPath.PropertyDataType) {
                return null;
            }

            var currentPropertyDataType = lastPointInPath.PropertyDataType;
            if (lastPointInPath.Type === KSTRING_OBJECT_TYPE) {
                currentPropertyDataType = currentPropertyDataType.toLowerCase();
            }
            var isArrayProperty = (lastPointInPath.Type === ARRAY_TYPE);
            var isObjectProperty = (COMPLEX_OBJECT_TYPE === lastPointInPath.Type);
            var isArrayItem = (lastPointInPath.Filter && lastPointInPath.Filter._kid != undefined);
            var isKstringType = (KSTRING_OBJECT_TYPE == lastPointInPath.Type);
            var isNativeArray = (ARRAY_TYPE === lastPointInPath.Type && lastPointInPath.hasOwnProperty('NativeArrayProperty'));

            if (currentPropertyDataType === 'LINK') {
                currentPropertyDataType = currentPropertyDataType.toLowerCase();
            }

            var propertyDataTypeObj = _.find(self.languageSchema.Classes, { Name: currentPropertyDataType });

            if (!propertyDataTypeObj || !propertyDataTypeObj.PropertyList) {
                return null;
            }

            var bulkRequestObject = { BulkPropertySegments: [] };

            if (isNativeArray) {
                var bulkPropertySegments = getPropertiesForNativeArray(currentSchemaDataPath);
                bulkRequestObject.BulkPropertySegments.push(bulkPropertySegments);
                return bulkRequestObject;
            } else if (isObjectProperty || isArrayItem) {
                var bulkPropertySegments = getPropertiesToRenderForObject(currentSchemaDataPath);
                bulkRequestObject.BulkPropertySegments = bulkPropertySegments;
                return bulkRequestObject;
            } else if (isKstringType) {
                var bulkPropertySegments = getPropertiesToForKstring(currentSchemaDataPath);
                bulkRequestObject.BulkPropertySegments = bulkPropertySegments;
                return bulkRequestObject;
            } else if (isArrayProperty) {
                var arrayItemsPreviewRequest = [];
                var objectKeys = getPropertiesNamesToRenderForObject(propertyDataTypeObj.PropertyList);
                arrayItemsPreviewRequest = _.map(currentSchemaDataPath,
                    function (segment) {
                        return _.pick(segment, requiredKeysForRequest);
                    });

                var size = (arrayItemsPreviewRequest.length - 1);
                arrayItemsPreviewRequest[size].ObjectKeys = {};
                objectKeys.map(function (key) {
                    arrayItemsPreviewRequest[size].ObjectKeys[key] = true;
                });
                var arrayFilterConfig = getArrayPropertySearchConfig();
                if (arrayFilterConfig) {
                    arrayItemsPreviewRequest[size].Filter = arrayFilterConfig;
                }

                var arraySortConfig = getArrayPropertySortConfig();
                if (arraySortConfig) {
                    arrayItemsPreviewRequest[size].Sort = arraySortConfig;
                }

                bulkRequestObject.BulkPropertySegments.push(arrayItemsPreviewRequest);
                return bulkRequestObject;
            }

            function getArrayPropertySearchConfig() {
                var stringProperties = [];
                var propertyClass = _.find(self.languageSchema.Classes, { Name: self.currentClassName });
                if (!self.arrayPropertyFilters.searchFilter) {
                    return;
                }
                propertyClass.PropertyList.map(function (property) {
                    if (!systemProperties.includes(property.Name) && property.DataType.Name.toLowerCase() === 'str') {
                        stringProperties.push(property.Name);
                    }
                });

                if (stringProperties.length < 1) {
                    return;
                }
                var config = { '$or': [] };
                stringProperties.map(function (prop) {
                    config.$or.push({ [prop]: { $regex: self.arrayPropertyFilters.searchFilter , $options: "i" } });
                });
                return config;
            }

            function getArrayPropertySortConfig() {
                if (!self.getArrayPropertySortProps || !self.arrayPropertyFilters.sortFilter) {
                    return null;
                }
                return { [self.arrayPropertyFilters.sortFilter]: self.arrayPropertyFilters.ascDesc };
            }

            function getPropertiesForNativeArray(schemaPathSegments) {
                var bulkPropertySegments = [];
                var currentSegments = _.cloneDeep(schemaPathSegments.slice(0, schemaPathSegments.length - 1));
                var baseSegments = [];

                var lastSegment = schemaPathSegments[schemaPathSegments.length - 1];
                var nativeArrayProperty = _.last(schemaPathSegments).NativeArrayProperty;
                var segmentSize = currentSegments.length;

                currentSegments.map(function (segment, index) {
                    Object.keys(segment).map(function (key) {
                        if (!requiredKeysForRequest.includes(key)) {
                            delete segment[key];
                        }

                        if (index == segmentSize - 1) {
                            segment.ObjectKeys = { [nativeArrayProperty]: true, _kid: true };
                        }
                    });
                    bulkPropertySegments.push(segment);
                });
                return bulkPropertySegments;
            }


            // to render a preview of object for item in array list
            function getPropertiesNamesToRenderForObject(propertyList, includeArrayProperties) {
                var stringProperties = [];
                var imageProperties = [];
                var propertiesToRender = [];
                var propertyNamesToRender = [];

                propertyList.map(function (property) {
                    if (property.DataType.Name == 'STR' || property.DataType.Name == 'NUMBER' && PropertyVisibliityStatus[property.DataType.Name] == undefined) {
                        stringProperties.push(property);
                    } else if (property.DataType.Name == 'image' && property.Type === COMPLEX_OBJECT_TYPE) {
                        imageProperties.push(property);
                    } else if (property.DataType.Name.toLowerCase() === 'str' &&
                        property.Type != ARRAY_TYPE && property.Type != COMPLEX_OBJECT_TYPE) {
                        propertiesToRender.push(property);
                    }
                });

                if (imageProperties.length > 0) {
                    propertiesToRender.push(imageProperties[0]);
                }

                stringProperties.map(function (stringProperty) {
                    var isValid = selectorsRegex.test(stringProperty.Name);
                    if (isValid && propertiesToRender.length < 3) {
                        propertiesToRender.push(stringProperty);
                    }
                });

                if (propertiesToRender.length < 3) {
                    stringProperties.map(function (stringProperty) {
                        if (!selectorsRegex.test(stringProperty.Name) && propertiesToRender.length < 3) {
                            propertiesToRender.push(stringProperty);
                        }
                    });
                }

                propertyNamesToRender = _.map(propertiesToRender, 'Name');

                propertyNamesToRender.unshift('_kid');

                return propertyNamesToRender;
            }

            // - for the root schema object 
            // - when clickcking an item in an array, 
            // - other object properties
            function getPropertiesToRenderForObject(schemaPathSegments) {
                var bulkPropertySegments = [];
                var lastSegment = schemaPathSegments[schemaPathSegments.length - 1];
                var classType = lastSegment.PropertyDataType;
                if (lastSegment.Type === KSTRING_OBJECT_TYPE) {
                    classType = classType.toLowerCase();
                } else if (classType === 'LINK') {
                    classType = classType.toLowerCase();
                }

                var propertyClass = _.find(self.languageSchema.Classes, { Name: classType });

                if (!propertyClass) {
                    console.error('propertyClass not found');
                    return;
                }
                 
                var arrayProperties = [];
                var objectProperties = [];
                var simpleTypeProperties = [];
                var kStringProperties = [];

                var basePathSegment = _.cloneDeep(schemaPathSegments);

                propertyClass.PropertyList.map(function (property) {
                    if (COMPLEX_OBJECT_TYPE === property.Type && PropertyVisibliityStatus[property.Name] == undefined) {
                        objectProperties.push(property);
                    } else if (KSTRING_OBJECT_TYPE === property.Type && PropertyVisibliityStatus[property.Name] == undefined) {
                        kStringProperties.push(property);
                    } else if (property.Type == ARRAY_TYPE && classType === 'kstring' && property.Name === 'keywords') {
                        simpleTypeProperties.push(property);
                    } else if (property.Type == ARRAY_TYPE && PropertyVisibliityStatus[property.Name] == undefined) {
                        arrayProperties.push(property);
                    } else if (property.Type != COMPLEX_OBJECT_TYPE && property.Type != ARRAY_TYPE) {
                        simpleTypeProperties.push(property);
                    }
                });


                if (simpleTypeProperties.length > 0) {
                    var simpleTypePropertyNames = _.map(simpleTypeProperties, 'Name');
                    var plainPropertiesSegment = _.cloneDeep(_.last(basePathSegment));
                    var bulkRequestItem = [];

                    plainPropertiesSegment.ObjectKeys = {};

                    simpleTypePropertyNames.map(function (name) {
                        plainPropertiesSegment.ObjectKeys[name] = true;
                    });

                    basePathSegment.map(function (segment, index) {
                        if (index < basePathSegment.length - 1) {
                            bulkRequestItem.push(segment);
                        }
                    });

                    bulkRequestItem.push(plainPropertiesSegment);

                    bulkPropertySegments.push(bulkRequestItem);
                }


                arrayProperties.map(function (property) {
                    var bulkRequestItem = [];

                    var arrPropertySegment = {
                        PropertyName: property.Name,
                        PropertyDataType: property.DataType.Name,
                        Type: ARRAY_TYPE,
                    };

                    var lengthPropertySegment = {
                        PropertyName: 'length',
                        PropertyDataType: 'function',
                        Type: 6,
                    };

                    basePathSegment.map(function (segment) {
                        bulkRequestItem.push(segment);
                    });
                    bulkRequestItem.push(arrPropertySegment);
                    bulkRequestItem.push(lengthPropertySegment);

                    bulkPropertySegments.push(bulkRequestItem);
                });

                kStringProperties.map(function (property) {
                    var bulkRequestItem = [];
                    var kstringPropertySegment = {
                        PropertyName: property.Name,
                        PropertyDataType: property.DataType.Name,
                        Type: KSTRING_OBJECT_TYPE,
                    };

                    basePathSegment.map(function (segment) {
                        bulkRequestItem.push(segment);
                    });

                    bulkRequestItem.push(kstringPropertySegment);
                    bulkPropertySegments.push(bulkRequestItem);
                });

                objectProperties.map(function (property) {
                    var objectPropertySegment = {};
                    var bulkRequestItem = [];

                    if (property.DataType.Name === 'image') {
                        objectPropertySegment = {
                            PropertyName: property.Name,
                            PropertyDataType: property.DataType.Name,
                            Type: COMPLEX_OBJECT_TYPE,
                        };

                    } else if (property.DataType.Name === 'phonenumber') {
                        objectPropertySegment = {
                            PropertyName: property.Name,
                            PropertyDataType: property.DataType.Name,
                            Type: COMPLEX_OBJECT_TYPE,
                        };
                    } else {
                        var propertyName = property.DataType.Name;
                        if (property.Type === KSTRING_OBJECT_TYPE) {
                            propertyName = propertyName.toLowerCase();
                        } else if (propertyName === 'LINK') {
                            propertyName = propertyName.toLowerCase();
                        }

                        var dataTypeObj = _.find(self.languageSchema.Classes, { Name: propertyName });

                        var stringPropertyList = dataTypeObj.PropertyList.filter(function (type) {
                            return type.DataType.Name === 'STR'
                        });

                        var imagePropertyList = dataTypeObj.PropertyList.filter(function (type) {
                            return type.DataType.Name === 'image';
                        });

                        var booleanPropertyList = dataTypeObj.PropertyList.filter(function (type) {
                            return type.DataType.Name === 'BOOLEAN';
                        });

                        var propertiesToRender = [];

                        if (imagePropertyList.length > 0) {
                            propertiesToRender = stringPropertyList.slice(0, 2);
                            propertiesToRender.unshift(imagePropertyList[0]);
                        } else {
                            propertiesToRender = stringPropertyList.slice(0, 3);
                        }

                        if (propertiesToRender.length < 3 && booleanPropertyList.length > 0) {
                            booleanPropertyList.map(function (property) {
                                if (propertiesToRender.length < 3) {
                                    propertiesToRender.push(property);
                                }
                            });
                        }

                        var objectKeys = { _kid: true };

                        propertiesToRender.map(function (property) {
                            objectKeys[property.Name] = true;
                        });

                        if (!_.isEmpty(objectKeys)) {
                            objectPropertySegment = {
                                PropertyName: property.Name,
                                PropertyDataType: property.DataType.Name,
                                Type: 5,
                                ObjectKeys: objectKeys,
                            };
                        }
                    }

                    if (!_.isEmpty(objectPropertySegment)) {
                        //bulkRequestItem.push(basePathSegment);
                        basePathSegment.map(function (segment) {
                            bulkRequestItem.push(segment);
                        });
                        bulkRequestItem.push(objectPropertySegment);
                        bulkPropertySegments.push(bulkRequestItem);
                    }
                });
                return bulkPropertySegments;
            }


            function getPropertiesToForKstring(schemaPathSegments) {
                var bulkPropertySegments = [];
                var lastSegment = _.last(schemaPathSegments);
                var basePathSegment = _.cloneDeep(schemaPathSegments);
                var classType = lastSegment.PropertyDataType.toLowerCase();

                var propertyClass = _.find(self.languageSchema.Classes, { Name: classType });



                if (propertyClass && propertyClass.PropertyList && propertyClass.PropertyList.length) {
                    var kStringPropertyNames = _.map(propertyClass.PropertyList, 'Name');
                    var propertySegment = _.cloneDeep(_.last(basePathSegment));
                    var bulkRequestItem = [];

                    propertySegment.ObjectKeys = {};

                    kStringPropertyNames.map(function (name) {
                        propertySegment.ObjectKeys[name] = true;
                    });

                    basePathSegment.map(function (segment, index) {
                        if (index < basePathSegment.length - 1) {
                            bulkRequestItem.push(segment);
                        }
                    });

                    bulkRequestItem.push(propertySegment);

                    bulkPropertySegments.push(bulkRequestItem);
                }
                return bulkPropertySegments;
            }
        },

        getSchemaDataForActiveProperty: function () {
            var self = this;
            self.showLoader(true);
            var bulkDataRequest = self.getBulkPropertyPathRequest(_.cloneDeep(self.currentSchemaDataPath));
            if (_.isEmpty(bulkDataRequest)) {
                self.showLoader(false);
                return;
            }
            self.getDataByPropertyPathBulk(bulkDataRequest, function (response) {
                self.onFetchBulkSchemaDataSuccess(bulkDataRequest, response);
                self.showLoader(false);
            }, function (err) {
                toastr.error('error fetching data.');
                self.showLoader(false);
            });
        },


        /**
        * Update this.model with the schema data 
        **/
        onFetchBulkSchemaDataSuccess: function (bulkDataRequest, data) {
            var model = {};
            var self = this;
            if (!bulkDataRequest || !data || _.isEmpty(data) || _.isEmpty(bulkDataRequest)) {
                return;
            }

            bulkDataRequest.BulkPropertySegments.map(function (propertySegments, index) {
                var propertyKey = getPropertyKey(propertySegments);
                var propertyValue = _.cloneDeep(data[index].Data);

                var valueKeysLength = propertyValue ? Object.keys(propertyValue).length : false;

                if (self.isNative && isPropertyOfTypeNativeArray(self.currentSchemaDataPath)) {
                    if (Array.isArray(propertyValue)) {
                        Object.assign(model, propertyValue[0]);
                    } else {
                        Object.assign(model, propertyValue);
                    }
                } else if (isPropertyTypeOfArray(propertySegments) && !Array.isArray(propertyValue)) {
                    propertyValue = [];
                    propertyValue.length = data[index].Data;
                    _.assign(model, { [propertyKey]: propertyValue });
                } else if (isPropertyTypeOfArray(propertySegments) && isArrayItem(propertySegments)) {
                    _.assign(model, data[index].Data[0]);
                } else if (isPropertyTypeOfArray(propertySegments)) {
                    _.assign(model, { [propertyKey]: propertyValue });
                } else if (isObjectProperty(propertySegments)) {
                    var currentSegment = self.currentSchemaDataPath[self.currentSchemaDataPath.length - 1];
                    var isParentProperty = currentSegment.PropertyName === propertyKey;
                    if (isParentProperty) {
                        Object.assign(model, propertyValue);
                    } else {
                        _.assign(model, { [propertyKey]: propertyValue });
                    }
                } else if (isKStringProperty(propertySegments)) {
                    if (isRederingInParentObject(model)) {
                        _.assign(model, { [propertyKey]: propertyValue });  
                    } else {
                        Object.assign(model, propertyValue);
                    }
                }
            });

            Vue.set(self, 'model', model);

            var lastPropertySegment = self.currentSchemaDataPath.slice(self.currentSchemaDataPath.length - 1)[0];

            if (lastPropertySegment.Type == COMPLEX_OBJECT_TYPE) { // check if is object
                var updatedPath = _.cloneDeep(self.currentSchemaDataPath);
                var parentSegment = updatedPath[updatedPath.length - 2];
                updatedPath[updatedPath.length - 1]._meta = {};
                updatedPath[updatedPath.length - 1]._meta.parentClassId = self.model._kid;
                if (parentSegment) {
                    updatedPath[updatedPath.length - 1]._meta.parentClassName = parentSegment.PropertyDataType;
                }
                Vue.set(self, 'currentSchemaDataPath', updatedPath);
            } 


            self.showLoader(false);

            function getPropertyKey(propertySegments) {
                var lastPropertySegment = getPropertySegmentFromSegmentsArray(propertySegments);
                return lastPropertySegment.PropertyName;
            }

            function isPropertyOfTypeNativeArray(propertySegments) {
                var lastPropertySegment = getPropertySegmentFromSegmentsArray(propertySegments);
                return (lastPropertySegment.Type === ARRAY_TYPE && lastPropertySegment.hasOwnProperty('NativeArrayProperty'));
            }

            function isPropertyTypeOfArray(propertySegments) {
                var lastPropertySegment = getPropertySegmentFromSegmentsArray(propertySegments);
                return (lastPropertySegment.Type === ARRAY_TYPE);
            }

            function isObjectProperty(propertySegments) {
                var lastPropertySegment = getPropertySegmentFromSegmentsArray(propertySegments);
                return (lastPropertySegment.Type === COMPLEX_OBJECT_TYPE);
            }

            function isKStringProperty(propertySegments) {
                var lastPropertySegment = getPropertySegmentFromSegmentsArray(propertySegments);
                return (lastPropertySegment.Type === KSTRING_OBJECT_TYPE);
            }

            function isArrayItem(propertySegments) {
                var lastPropertySegment = getPropertySegmentFromSegmentsArray(propertySegments);
                return (lastPropertySegment.Filter != undefined && lastPropertySegment.Filter._kid != undefined);
            }

            function getPropertySegmentFromSegmentsArray(propertySegments) {
                var lastPropertySegment = propertySegments[propertySegments.length - 1];
                if (lastPropertySegment && lastPropertySegment.Type == 6) {
                    lastPropertySegment = propertySegments[propertySegments.length - 2];
                }
                return lastPropertySegment;
            }

            function isRederingInParentObject(model) {
                return !!model._kid;
            }
        },

        // show list of array items with preview
        onClickShowArrayItems: function (fieldObj) {
            var self = this;
            self.resetArrayPropertyFilter();

            if (self.areFieldsEditable) {
                self.unsavedChangesModal(true);
                return;
            }

            if (self.new_site) {
                self.addDataForSchema(function () {
                    self.onClickShowArrayItems(fieldObj);
                });
                return;
            }

            if (!self.model._kid) {
                self.saveCurrentObj(function () {
                    self.onClickAddToArrayProperty(fieldObj);
                });
                return;
            }

            var isNativeArray = nativeArrayTypes.includes(fieldObj.propertyDataType);

            if (isNativeArray) {
                var currentSchemaDataPath = _.cloneDeep(self.currentSchemaDataPath);
                var lastSegment = currentSchemaDataPath[currentSchemaDataPath.length - 1];
                lastSegment.nativeArrayProperty = fieldObj.model;
                self.resetCurrentGroup(advancedGroupName);
                var arrPathSegment = {
                    PropertyDataType: fieldObj.propertyDataType,
                    PropertyName: fieldObj.model,
                    Type: fieldObj.propertyType,
                    Index: 0,
                    Limit: self.arrayItemPagination.limit,
                    Length: (self.model[fieldObj.model] && Array.isArray(self.model[fieldObj.model])) ?  self.model[fieldObj.model].length : 0,
                    NativeArrayProperty: fieldObj.model
                };

                self.currentClassName = fieldObj.propertyDataType;
                self.resetCurrentGroup(advancedGroupName);
                self.currentSchemaDataPath.push(arrPathSegment);
            } else {
                var arrPathSegment = {
                    PropertyDataType: fieldObj.propertyDataType,
                    PropertyName: fieldObj.model,
                    Type: fieldObj.propertyType,
                    Index: 0,
                    Limit: self.arrayItemPagination.limit,
                    Length: (self.model[fieldObj.model] && Array.isArray(self.model[fieldObj.model])) ? self.model[fieldObj.model].length : 0,
                };
                self.currentClassName = fieldObj.propertyDataType;
                self.currentSchemaDataPath.push(arrPathSegment);
                self.resetCurrentGroup(advancedGroupName);
            }
            
            self.showLoader(true);
            self.getSchemaForActiveProperty();
            self.getSchemaDataForActiveProperty();
        },

        // to add new item to array (complex obj array)
        onClickAddToArrayProperty: function (fieldObj) {
            var self = this;
            if (self.areFieldsEditable) {
                self.unsavedChangesModal(true);
                return;
            }

            if (self.new_site) {
                self.addDataForSchema(function () {
                    self.onClickAddToArrayProperty(fieldObj);
                });
                return;
            }

            var arrPathSegment = {
                PropertyDataType: fieldObj.propertyDataType,
                PropertyName: fieldObj.model,
                Type: fieldObj.propertyType,
                IsAddNew: true,
                _meta: {
                    Limit: self.arrayItemPagination.limit,
                    Length: (self.model[fieldObj.model] && Array.isArray(self.model[fieldObj.model])) ? self.model[fieldObj.model].length : 0,
                    Index: (self.model[fieldObj.model] && Array.isArray(self.model[fieldObj.model])) ? self.model[fieldObj.model].length : 0,
                }
            };

            if (fieldObj.type !== 'arrayList') {
                self.currentClassName = fieldObj.propertyDataType;
                self.currentSchemaDataPath.push(arrPathSegment);
                self.resetCurrentGroup();
            } else {
                var currentPathSegments = _.cloneDeep(self.currentSchemaDataPath);
                var lastSegment = currentPathSegments[currentPathSegments.length - 1];
                var meta = {};
                Object.keys(lastSegment).map(function (key) {
                    if (key != 'PropertyDataType' && key != 'PropertyName' && key != 'Type') {
                        meta[key] = lastSegment[key]
                        delete lastSegment[key];
                    }
                });

                lastSegment._meta = meta;
                lastSegment.IsAddNew = true;

                Vue.set(self, 'currentSchemaDataPath', currentPathSegments);
            }
            
            var objectField = self.getProcessedClass(fieldObj.propertyDataType);
            Vue.set(self, 'schema', objectField.schema);
            Vue.set(self, 'model', {});
        },

        onClickShowObjectProperty: function (fieldObj) {
            var self = this;
            self.resetArrayPropertyFilter();
            if (self.areFieldsEditable) {
                self.unsavedChangesModal(true);
                return;
            }

            if (!self.model._kid) {
                self.saveCurrentObj(function () {
                    self.onClickShowObjectProperty(fieldObj);
                });
                return;
            }

            var objectSegment = {
                PropertyDataType: fieldObj.propertyDataType,
                PropertyName: fieldObj.propertyName,
                Type: fieldObj.type,
            };
            self.currentClassName = fieldObj.propertyDataType;
            self.currentSchemaDataPath.push(objectSegment);
            self.resetCurrentGroup();
            self.showLoader(true);
            self.getSchemaForActiveProperty(fieldObj);
            self.getSchemaDataForActiveProperty();
        },

        // show array item with kid
        getArrayItemWithId: function (kid, propertyObj) {
            var self = this;
            if (self.areFieldsEditable) {
                self.unsavedChangesModal(true);
                return;
            }
            var pathSegments = _.cloneDeep(self.currentSchemaDataPath);
            var lastSegment = pathSegments[pathSegments.length - 1];
            var requiredKeys = ['PropertyDataType', 'PropertyName', 'Type'];
            Object.keys(lastSegment).map(function (key) {
                if (!requiredKeys.includes(key)) {
                    lastSegment._meta = _.isEmpty(lastSegment._meta) ? {} : lastSegment._meta;
                    lastSegment._meta[key] = lastSegment[key];
                    delete lastSegment[key];
                }
            });
            lastSegment.Filter = { _kid: kid };
            lastSegment.Index = 0;
            Object.assign(lastSegment._meta, { ClickedIndex: propertyObj.clickedIndex });

            Vue.set(self, 'currentSchemaDataPath', pathSegments);
            self.currentClassName = propertyObj.propertyDataType;
            self.resetCurrentGroup();
            self.showLoader(true);
            self.getSchemaForActiveProperty();
            self.getSchemaDataForActiveProperty();
        },


        // when clicking configure new or select from existing
        // from within an empty object / without _kid
        // case when add new in array and configure an object 
        // property in array
        saveCurrentObj: function (callback) {
            var self = this;
            self.showLoader(true);
            self.updateDataForSchema(function () {
                if (callback && typeof callback === 'function') {
                    callback();
                }
                self.showLoader(false);
            });
        },


        getNextItemsForArrayProperty: function () {
            var self = this;
            var currentPropertyPath = _.cloneDeep(self.currentSchemaDataPath);
            var depth = currentPropertyPath.length - 1;
            var currentSegment = currentPropertyPath[depth];

            if (currentSegment.Index + self.arrayItemPagination.limit < currentSegment.Length) {
                currentSegment.Index = (currentSegment.Index + self.arrayItemPagination.limit);
            }

            Vue.set(self, 'currentSchemaDataPath', currentPropertyPath);
            self.showLoader(true);
            self.getSchemaDataForActiveProperty();
        },

        getPreviousItemsForArrayProperty: function () {
            var self = this;
            var currentPropertyPath = _.cloneDeep(self.currentSchemaDataPath);
            var depth = currentPropertyPath.length - 1;
            currentPropertyPath[depth].Index = (currentPropertyPath[depth].Index - self.arrayItemPagination.limit);
            currentPropertyPath[depth].Index = (currentPropertyPath[depth].Index < 0) ? 0 : currentPropertyPath[depth].Index;
            Vue.set(self, 'currentSchemaDataPath', currentPropertyPath);
            self.showLoader(true);
            self.getSchemaDataForActiveProperty();
        },

        onArrayPropertySearch: function () {
            var self = this;
            var propertyClass = _.find(self.languageSchema.Classes, { Name: self.currentClassName });
            if (!propertyClass) {
                return;
            }
            self.showLoader(true);
            var bulkRequestObject = { BulkPropertySegments: [] };

            var currentSchemaDataPath = _.cloneDeep(self.currentSchemaDataPath);

            bulkRequestObject.BulkPropertySegments = getArrayPropertySearchItemsLength(currentSchemaDataPath.slice(0, currentSchemaDataPath.length - 1));

            self.getDataByPropertyPathBulk(bulkRequestObject, function (response) {
                if (!Array.isArray(response) || !response[0].hasOwnProperty('Data') || !Number.isInteger(response[0].Data)) {
                    self.showLoader(false);
                    self.getSchemaDataForActiveProperty();
                    return;
                }

                var updatedSchemaDataPath = _.cloneDeep(self.currentSchemaDataPath);
                updatedSchemaDataPath[updatedSchemaDataPath.length - 1].Length = response[0].Data;
                Vue.set(self, 'currentSchemaDataPath', updatedSchemaDataPath);
                self.getSchemaDataForActiveProperty();
                self.showLoader(false);
            }, function (err) {
                self.showLoader(false);
                self.getSchemaDataForActiveProperty();
            });


            function getArrayPropertySearchItemsLength(schemaPathSegments) {
                var bulkPropertySegments = [];
                var lastSegment = schemaPathSegments[schemaPathSegments.length - 1];
                var classType = lastSegment.PropertyDataType;
                if (lastSegment.Type === KSTRING_OBJECT_TYPE) {
                    classType = classType.toLowerCase();
                } else if (classType === 'LINK') {
                    classType = classType.toLowerCase();
                }

                var basePathSegment = _.cloneDeep(schemaPathSegments);
                var bulkRequestItem = [];

                var arrPropertySegment = {
                    PropertyName: _.last(self.currentSchemaDataPath).PropertyName,
                    PropertyDataType: self.currentClassName,
                    Type: ARRAY_TYPE,
                };

                var searchConfig = getArrayPropertySearchConfig();

                if (searchConfig) {
                    arrPropertySegment.Filter = searchConfig;
                }

                var lengthPropertySegment = {
                    PropertyName: 'length',
                    PropertyDataType: 'function',
                    Type: 6,
                };

                basePathSegment.map(function (segment) {
                    bulkRequestItem.push(segment);
                });

                bulkRequestItem.push(arrPropertySegment);
                bulkRequestItem.push(lengthPropertySegment);
                bulkPropertySegments.push(bulkRequestItem);

                return bulkPropertySegments;
            }


            function getArrayPropertySearchConfig() {
                var stringProperties = [];
                var propertyClass = _.find(self.languageSchema.Classes, { Name: self.currentClassName });
                if (!self.arrayPropertyFilters.searchFilter) {
                    return;
                }
                propertyClass.PropertyList.map(function (property) {
                    if (!systemProperties.includes(property.Name) && property.DataType.Name.toLowerCase() === 'str') {
                        stringProperties.push(property.Name);
                    }
                });

                if (stringProperties.length < 1) {
                    return;
                }
                var config = { '$or': [] };
                stringProperties.map(function (prop) {
                    config.$or.push({ [prop]: { $regex: self.arrayPropertyFilters.searchFilter, $options: "i" } });
                });
                return config;
            }
        },

        onArrayPropertySortPropertyChanged: function () {
            var self = this;
            if (self.arrayPropertyFilters.sortFilter) {
                self.getSchemaDataForActiveProperty();
            }
        },

        onArrayPropertySortOrderChanged: function () {
            var self = this;
            if (self.arrayPropertyFilters.ascDesc === 1) {
                self.arrayPropertyFilters.ascDesc = -1;
            } else {
                self.arrayPropertyFilters.ascDesc = 1;
            }
            if (self.arrayPropertyFilters.sortFilter) {
                self.getSchemaDataForActiveProperty();
            }
        },

        resetArrayPropertyFilter: function () {
            var self = this;
            self.arrayPropertyFilters.searchFilter = null;
            self.arrayPropertyFilters.sortFilter = null;
            self.arrayPropertyFilters.ascDesc = 1;
        },

        getDataByPropertyPathBulk: function (request, successCallback, errorCallback) {
            axios({
                method: 'post',
                url: Endpoints.GetDataByPropertyBulk,
                data: request,
            }).then(function (response) {
                if (successCallback && typeof successCallback === 'function') {
                    successCallback(response.data);
                }
            }).catch(function (err) {
                if (errorCallback && typeof errorCallback === 'function') {
                    errorCallback(err);
                }
            });
        },

        getNonSystemPropertiesFromPropertyList: function (properties) {
            return properties.filter(function (property) {
                return (PropertyVisibliityStatus[property] == undefined);
            });
        },


        // Find properties to render in fieldObj and fieldDynamic (rendering array of objects)
        // 
        findPropertiesToRenderForFieldObject: function (propertyList, count) {

            var selectedProperties = [];
            propertyList.map(function (property) {
                var propertyName = property.Name;
                var isOfRequiredType = (property.Type !== 6 && property.Type !== 1 && property.Type !== 5);
                var isNotSystemProperty = !!PropertyVisibliityStatus[propertyName];
                var valid = (selectorsRegex.test(name) && (selectedProperties.length <= 3) && isNotSystemProperty && isOfRequiredType);
                if (valid) {
                    selectedProperties.push(property);
                }
            });
            return selectedProperties;
        },

        goToPathInDataNavigation: function (index, isAddOffset) {
            var self = this;


            if (self.areFieldsEditable) {
                self.unsavedChangesModal(true);
                return;
            }

            var actualPath = _.cloneDeep(self.currentSchemaDataPath);
            var renderedPath = _.cloneDeep(self.schemaNavigationPathForRender);

            if (isAddOffset) {
                index += (renderedPath.length - self.maxNavigationItemNumber);
            }

            var popsToMake = renderedPath.length - (index + 1);
            var newRenderedSegment = renderedPath.slice(0, index);
            popsToMake = (popsToMake < 0) ? 0 : popsToMake;
            var isUpdateNeeded = (popsToMake > 0);

            while (popsToMake > 0 && actualPath.length > 1) {
                var lastSegment = _.last(actualPath);
                if (lastSegment.Type === ARRAY_TYPE && lastSegment.hasOwnProperty('Filter')) {
                    delete lastSegment.Filter;
                    if (lastSegment.hasOwnProperty('_meta')) {
                        Object.keys(lastSegment._meta).map(function (key) {
                            lastSegment[key] = lastSegment._meta[key];
                        });
                        delete lastSegment._meta;
                    }
                } else {
                    actualPath.pop();
                }
                popsToMake--;
            }
            
            if (isUpdateNeeded) {
                Vue.set(self, 'currentSchemaDataPath', actualPath);
                
                self.getSchemaForActiveProperty();
                self.getSchemaDataForActiveProperty();
                if (_.last(actualPath).hasOwnProperty('CurrentGroupName')) {
                    self.resetCurrentGroup(_.last(actualPath).CurrentGroupName.name);
                } else {
                    self.resetCurrentGroup();
                }
            }
        },

        getSchemaForActiveProperty: function (fieldObj) {
            var self = this;
            var lastSegment = self.currentSchemaDataPath[self.currentSchemaDataPath.length - 1];

            if (OBJECT_TYPES.includes(lastSegment.Type)) { // for object property
                var objectField = self.getProcessedClass(lastSegment.PropertyDataType, fieldObj);
                Vue.set(self, 'schema', objectField.schema);
                self.currentClassName = lastSegment.PropertyDataType;
            } else if (lastSegment.Type === 1 && !lastSegment.hasOwnProperty('Filter')) { // for array list
                var objectField = self.getProcessedClass(lastSegment.PropertyDataType /*fieldObj*/);
                var arrayListFieldSchema = self.getArrayListFieldSchema(objectField, lastSegment);
                Vue.set(self, 'schema', arrayListFieldSchema);
            } else if (lastSegment.Type === 1) { // for array item
                var objectField = self.getProcessedClass(lastSegment.PropertyDataType);
                Vue.set(self, 'schema', objectField.schema);
            } else {
                console.error('getSchemaForActiveProperty no schema');
            }
        },

        updateDataForSchema: function (callback, ignoreVMNcheck) {
            var self = this;
            var propertiesForUpdate = [];
            var fields = _.cloneDeep(self.schema.groups[0]).fields;
            self.showLoader(true);

            if (self.new_site) {
                self.addDataForSchema(function () {
                    self.updateDataForSchema(callback);
                });
                return;
            }

            if (self.isRenderingPhoneNumber && !ignoreVMNcheck && self.isCallTrackerEnabledForWebsite) {
                self.updateDataForVMN();
                return;
            }


            if (!self.isRenderingNativeArray) {
                // label for image url, switch for boolean
                fields.map(function (field) {
                    if (field.type === 'input' || field.type === 'label'
                        || field.type === 'textMultiLine' || field.type === 'select'
                        || field.type === 'switch' || field.type === 'richtext' || field.type === 'datetime'
                        && !systemProperties.includes(field.model)) {
                        propertiesForUpdate.push(field.model);
                    }
                });
            } else {
                propertiesForUpdate.push(fields[0].model);
            }

            
            var updateObject = {};
            
            propertiesForUpdate.map(function (property) {
                updateObject[property] = self.model[property];
            });

            var updateRequest = { BulkUpdates: [], Query: null, UpdateValue: null };
            var updateItem = {};

            /*if (self.isRenderingNativeArray) {
                updateItem.Query = { _kid: self.model._kid };
            } else */
            if (self.currentSchemaDataPath.length == 1) {
                updateItem.Query = { _kid: self.model._kid, _parentClassId: self.model._kid, _parentClassName: self.schemaName, _propertyName: self.schemaName };
            } else if (self.isAddingNewToComplexArray) {
                updateItem.Query = { _parentClassId: getParentClassId(), _parentClassName: getParentClassName(), _propertyName: getPropertyName() };
            } else if (!self.model._kid) { //adding new object property
                updateItem.Query = { _parentClassId: getParentClassId(), _parentClassName: getParentClassName(), _propertyName: getPropertyName() };
            } else if (self.isNative) {
                var parent = self.currentSchemaDataPath[self.currentSchemaDataPath.length - 2];
                var grandParent = self.currentSchemaDataPath.length > 2 ? self.currentSchemaDataPath[self.currentSchemaDataPath.length - 3] : null;
                updateItem.Query = {
                    _kid: self.model._kid,
                    _parentClassId: (parent && parent.Type === ARRAY_TYPE) ? ((grandParent && grandParent._meta) ? grandParent._meta.parentClassId : null) : parent._meta.parentClassId,
                    _parentClassName: (parent && parent.Type === ARRAY_TYPE) ? grandParent.PropertyDataType : ((parent.hasOwnProperty('_meta')) ? parent._meta.parentClassName : null),
                    _propertyName: parent.PropertyName,
                };

                if (updateItem.Query._parentClassId == null) {
                    delete updateItem.Query._parentClassId;
                }

                if (updateItem.Query._parentClassName == null) {
                    delete updateItem.Query._parentClassName;
                }

                if (updateItem.Query._propertyName == null) {
                    delete updateItem.Query._propertyName;
                }
            } else {
                updateItem.Query = { _kid: self.model._kid, _parentClassId: getParentClassId(), _parentClassName: getParentClassName(), _propertyName: getPropertyName() };
            }

            updateItem.UpdateValue = updateObject;
            updateRequest.BulkUpdates.push(updateItem);

            axios({
                method: 'post',
                url: Endpoints.UpdateDataForSchema,
                data: updateRequest,
            }).then(function (response) {
                if (response.data && Array.isArray(response.data) && response.data.length) {
                    if (!self.model._kid) { // for newly created obj
                        var newKid = response.data[0].Kid;
                        self.model._kid = newKid;
                        var currentSchemaDataPath = _.cloneDeep(self.currentSchemaDataPath);
                        var lastSchemaDataPath = currentSchemaDataPath[currentSchemaDataPath.length - 1];
                        if (lastSchemaDataPath.Type === ARRAY_TYPE && !lastSchemaDataPath.hasOwnProperty('Filter')) { // when adding new item to complex array
                            lastSchemaDataPath.Filter = { _kid: newKid };
                            var newLength = Number(lastSchemaDataPath._meta.Length) ? (lastSchemaDataPath._meta.Length + 1) : 1;
                            Object.assign(lastSchemaDataPath._meta, { ClickedIndex: newLength, Length: newLength });
                            lastSchemaDataPath.IsAddNew ? delete lastSchemaDataPath.IsAddNew : null;
                            Vue.set(self, 'currentSchemaDataPath', currentSchemaDataPath);
                        }
                    }
                }
                self.toggleFieldsEditable(false);
                self.resetFieldsToNonEditable();
                toastr.success('Save successful');

                if (self.isRenderingPhoneNumber) {
                    self.getSchemaDataForActiveProperty();
                } else {
                    self.showLoader(false);
                }
                

                if (callback && typeof callback === 'function') {
                    callback();
                }
            }).catch(function (response) {
                self.showLoader(false);
                self.toggleFieldsEditable(false);
                toastr.error('Error');
                console.error('err', response);
            });


            function getParentClassId() {
                var parentSegment = self.currentSchemaDataPath[self.currentSchemaDataPath.length - 2];
                if (parentSegment.Type === COMPLEX_OBJECT_TYPE) {
                    return parentSegment._meta.parentClassId;
                } else {
                    return parentSegment.Filter._kid;
                }
            }

            function getParentClassName() {
                var currentSegment = self.currentSchemaDataPath[self.currentSchemaDataPath.length - 2];
                return currentSegment.PropertyDataType;
            }

            function getPropertyName() {
                var currentSegment = self.currentSchemaDataPath[self.currentSchemaDataPath.length - 1];
                return currentSegment.PropertyName;
            }
        },


        updateDataForVMN: function () {
            var self = this;
            self.showLoader(true);
            self.showPhoneNumberUpdateModel();
        },

        addDataForSchema: function (callback) {
            var self = this;
            var requestObj = {
                WebsiteId: null,
                Data: {},
            };
            axios({
                method: 'post',
                url: Endpoints.AddDataForSchema,
                data: requestObj,
            }).then(function (response) {
                Vue.set(self.model, '_kid', response.data);
                Vue.set(self, 'new_site', false);
                var schemaNavigationPath = _.cloneDeep(self.currentSchemaDataPath);
                var baseSegment = schemaNavigationPath[0];
                baseSegment._meta = { parentClassId: response.data };
                Vue.set(self, 'currentSchemaDataPath', schemaNavigationPath);
                if (callback && typeof callback === 'function') {
                    callback();
                }
            }).catch(function (err) {
                toastr.error("Error", "Error");
            })
        },

        toggleFieldsEditable: function (toggle) {
            var self = this;
            self.areFieldsEditable = toggle;
        },


        // reset input fields to non editable
        resetFieldsToNonEditable: function () {
            let self = this;
            let fields = self.schema.groups[0].fields;
            let activebtns = _.filter(fields, function (field) {
                if (!field.readonly && field.visible) {
                    return true;
                }
            });
            _.forEach(activebtns, function (field) {
                Vue.set(field, 'readonly', !field.readonly);
            });
            self.areFieldsEditable = false;
        },


        setIsImageForUploading: function (isImage) {
            let self = this;
            Vue.set(self.upload, 'isImage', isImage);
        },


        // delete array item or image
        deleteProperty: function () {
            var self = this;
            var item = self.propertyToDelete;
            var requestObj = { BulkDelete: [] };

            var deleteItem = {
                _kid: item._kid,
                _parentClassId: item._parentClassId,
                _parentClassName: item._parentClassName,
                _propertyName: item._propertyName
            };

            requestObj.BulkDelete.push(deleteItem);
            self.showLoader(true);
            axios({
                method: 'post',
                url: Endpoints.DeleteDataForSchema,
                data: requestObj
            }).then(function (response) {
                self.showLoader(false);
                toastr.success('Delete Successful');
                self.cancelPropertyDelete();
                if (self.isRenderingComplexArray) {
                    var currentPath = _.cloneDeep(self.currentSchemaDataPath);
                    var lastSegment = currentPath[currentPath.length - 1];
                    lastSegment.Length--;
                    Vue.set(self, 'currentSchemaDataPath', currentPath);
                }
                self.getSchemaDataForActiveProperty();
            }).catch(function (response) {
                self.showLoader(false);
                self.cancelPropertyDelete();
                toastr.error("Error", "Error");
                self.getSchemaDataForActiveProperty();
            });
        },


        confirmDeleteProperty: function (item) {
            var self = this;
            var lastPathSegment = _.last(self.currentSchemaDataPath);

            if (lastPathSegment.Type === 1 && !lastPathSegment.Filter) { // is deleting from array
                var parentSegment = self.currentSchemaDataPath[self.currentSchemaDataPath.length - 2];
                item._parentClassId = parentSegment._meta.parentClassId;
                item._propertyName = lastPathSegment.PropertyName;
                item._parentClassName = parentSegment.PropertyDataType;
                item.displayText = lastPathSegment.PropertyDataType;
            }

            Vue.set(self, 'propertyToDelete', item);
            self.showDeleteObjModal();
        },

        cancelPropertyDelete: function () {
            var self = this;
            Vue.set(self, 'propertyToDelete', {});
            self.hideAllModals();
        },

        deleteConfirm: function () {
            var self = this;
            if (self.isNative) {
                var newArray = _.cloneDeep(self.model[self.currentModel]);
                newArray.splice(self.delete.index, 1);
                Vue.set(self.model, self.currentModel, newArray);
                self.updateDataForSchema(function () {
                    Vue.set(self, 'delete', {
                        displayIndex: null,
                        index: null,
                        that: null
                    });
                });
                self.hideAllModals();
            }
        },

        /**
        * Just receives the update object and calls the api and sends the response as callback
        **/
        updateDataForSchemaSlim: function (updateObj, callback, errorCallback) {
            if (!updateObj) {
                return;
            }
            let self = this;
            self.showLoader(true);

            if (self.new_site) {
                self.addDataForSchema(function () {
                    self.updateDataForSchemaSlim(updateObj, callback);
                });
                return;
            }

            axios({
                method: 'post',
                url: Endpoints.UpdateDataForSchema,
                data: updateObj
            }).then(function (response) {
                self.showLoader(false);
                toastr.success('Save successful');
                if (callback && typeof callback === 'function') {
                    callback(response.data);
                }
            }).catch(function (response) {
                self.showLoader(false);
                if (errorCallback && errorCallback === 'function') {
                    errorCallback(response);
                }
                toastr.error("", "Error");
            });
        },

        getDataByPropertySlim: function (requestObj, successCb, errorCb) {
            var self = this;
            axios({
                method: 'POST',
                url: Endpoints.GetDataByProperty,
                data: requestObj,
            }).then(function (response) {
                if (successCb && typeof successCb === 'function') {
                    successCb(response.data);
                }
            }).catch(function (error) {
                if (errorCb && typeof errorCb === 'function') {
                    errorCb(error.response.data);
                }
            });
        },

        getBaseClassIdForWebsite: function (successCb, errorCb) {
            var self = this;
            var requestObj = {
                PropertySegments: [
                    {
                        PropertyName: self.schemaName,
                        PropertyDataType: self.schemaName,
                        Type: 5
                    }
                ]
            };
            
            self.getDataByPropertySlim(requestObj, function (response) {
                if (successCb && typeof successCb === 'function') {
                    successCb(response);
                }
            }, function (err) {
                if (errorCb && typeof errorCb === 'function') {
                    errorCb(err);
                }
            });
        },



        // VMN/CallTracker - START

        isCallTrackerEnabled: function () {
            var self = this;
            axios({
                method: 'post',
                url: Endpoints.IsCallTrackerEnabled
            }).then(function (response) {
                if (response.data && response.data.hasOwnProperty('isActive')) {
                    self.isCallTrackerEnabledForWebsite = response.data.isActive;
                } else {
                    self.isCallTrackerEnabledForWebsite = false;
                }
            }).catch(function () {
                self.isCallTrackerEnabledForWebsite = false;
            });
        },

        showPhoneNumberUpdateModel: function () {
            var self = this;
            var classType = 'phonenumber';
            self.getDataForClassByClassType(classType, function (classData) {
                var otherOccourances = self.isMultipleOccouranceOfVMNPresent(self.oldModel, classData);
                if (otherOccourances && otherOccourances.length > 1) {
                    self.vmnData.sameVMNNumberList = otherOccourances;
                    self.modalShowStatus.vmnUpdate = true;
                } else {
                    self.updateDataForSchema(null, true);
                }
            });
        },

        isMultipleOccouranceOfVMNPresent: function (phoneNumberObj, classData) {
            var otherOccourances = [];

            if (!classData || !classData.Data || !Array.isArray(classData.Data)) {
                return otherOccourances;
            }

            classData.Data.map(function (item) {
                if (phoneNumberObj.contactnumber == item.contactnumber && item.isactive == true) {
                    otherOccourances.push(item);
                }
            });
            return otherOccourances;
        },


        updateAllVMN: function () {
            var self = this;
            var updateObj = self.getVMNBulkUpdateObj();
            self.updateDataForSchemaSlim(updateObj, function (successResponse) {
                self.resetFieldsToNonEditable();
                self.getSchemaDataForActiveProperty();
                self.showHideModal(Modal.VMNUPDATE, false);
                self.vmnData.sameVMNNumberList = [];
            }, function () {
                self.resetFieldsToNonEditable();
            });
        },

        getVMNBulkUpdateObj: function () {
            var self = this;
            var updateObj = {
                BulkUpdates: [],
                Query: null,
                UpdateValue: null
            };

            if (self.vmnData.sameVMNNumberList.length < 1) {
                return updateObj;
            }

            var newValueToUpdate = Object.assign({}, _.pick(self.model, [
                'calltrackernumber',
                'contactnumber',
                'countrycode',
                'isactive',
                'websiteid'
            ]));

            self.vmnData.sameVMNNumberList.map(function (phonenumber) {
                var updateItem = {
                    Query: _.pick(phonenumber, ['_parentClassId', '_parentClassName', '_propertyName', '_kid']),
                    UpdateValue: newValueToUpdate
                };
                updateObj.BulkUpdates.push(updateItem);
            });

            return updateObj;
        },

        // update this phone number and get the new calltracker number
        updateThisVMNAndGetNewCallTracker: function () {
            var self = this;

            var updateObj = {
                BulkUpdates: [],
                Query: {},
                UpdateValue: null
            };

            var nonActiveNumber = Object.assign({}, getVMNUpdateObj(self, false));
            var activeNumber = Object.assign({}, getVMNUpdateObj(self, true));

            updateObj.BulkUpdates.splice(0, 0, nonActiveNumber, activeNumber);

            self.updateDataForSchemaSlim(updateObj, function (response) {
                self.resetFieldsToNonEditable();
                self.getSchemaDataForActiveProperty();
                self.showHideModal(Modal.VMNUPDATE, false);
            });

            function getVMNUpdateObj(context, isActive) {
                var valueToUpdate = {
                    Query: {
                        _parentClassId: getParentClassId(),
                        _parentClassName: getParentClassName(),
                        _propertyName: getPropertyName(),
                        _kid: self.model._kid,
                    },
                    UpdateValue: Object.assign({}, context.model, { 'isactive': isActive }),
                };
                return valueToUpdate;
            }

            function getParentClassId() {
                var parentSegment = self.currentSchemaDataPath[self.currentSchemaDataPath.length - 2];
                if (parentSegment.Type === COMPLEX_OBJECT_TYPE) {
                    return parentSegment._meta.parentClassId;
                } else {
                    return parentSegment.Filter._kid;
                }
            }

            function getParentClassName() {
                var currentSegment = self.currentSchemaDataPath[self.currentSchemaDataPath.length - 2];
                return currentSegment.PropertyDataType || null;
            }

            function getPropertyName() {
                var currentSegment = self.currentSchemaDataPath[self.currentSchemaDataPath.length - 1];
                return currentSegment.PropertyName;
            }
        },

        // VMN / CallTracker - END




        // MODAL START

        showHideModal: function (modalName, show) {

            var modalStatus = this.modalShowStatus;
            Vue.set(modalStatus, modalName, show)

        },

        hideAllModals: function () {
            var self = this;
            var modalStatus = self.modalShowStatus;

            for (var key in modalStatus) {
                self.showHideModal(key, false);
            }

            self.resetArrayPropertyMaxCountModal();
        },

        resetArrayPropertyMaxCountModal: function () {
            var self = this;
            for (var key in self.arrayPropertyMaxCountModal) {
                self.arrayPropertyMaxCountModal[key] = null;
            }
        },

        showDeleteObjModal: function () {
            var self = this;
            self.modalShowStatus.deleteObject = true;
        },


        unsavedChangesModal: function (open) {
            var self = this;
            self.showHideModal(Modal.UPDATE, open ? true : false);
        },


        // save action in Modal
        saveUnsavedChanges: function () {
            var self = this;
            self.updateDataForSchema();
            self.unsavedChangesModal(false);
        },

        discardUnsavedChanges: function () {
            var self = this;
            self.resetFieldsToNonEditable();
            self.unsavedChangesModal(false);
        },

        showhavingIssuesModal: function () {
            var self = this;
            self.showHideModal(Modal.HAVINGISSUES, true);
            self.supportEmailForm.message = 'Hi, \n \n' +
                'I\'m facing issues while adding content to my website "' + localStorage.getItem("DOMAIN") +
                '". You can reach out to me at ' +
                self.customerData.contact.email + (self.customerData.contact.phoneNumber ? (' or ' + self.customerData.contact.phoneNumber) : "") +
                " ." + '\n \n Debug Id : ' + self.kidForReportingIssue;
        },

        // MODAL END




        // support email START

        isSupportEmailFormValid: function () {
            var self = this;
            return (self.supportEmailForm.message != null && self.supportEmailForm.message.length > 0);
        },

        cancelSendSupportEmail: function () {
            var self = this;
            self.hideAllModals();
            self.resetSupportEmailForm();
        },

        resetSupportEmailForm: function () {
            var self = this;
            self.supportEmailForm.subject = 'Reporting an issue for website:  ' + localStorage.getItem('DOMAIN');
            self.supportEmailForm.message = '';
            self.supportEmailForm.image = null;
        },

        processUploadedFile: function (event) {
            var self = this;
            var files = event.target.files || event.dataTransfer.files;
            if (!files.length) {
                return;
            }
            self.supportEmailForm.image = files[0];
            //to-do show image preview
        },


        sendSupportEmail: function (event) {
            var self = this;
            self.hideAllModals();

            if (!self.isSupportEmailFormValid()) {
                event.preventDefault();
                event.stopPropagation();
                return;
            }

            var emailRequest = {
                To: self.supportEmailForm.to,
                Subject: self.supportEmailForm.subject,
                EmailBody: self.supportEmailForm.message,
                Attachments: [],
                clientId: self.supportEmailForm.clientId
            };

            if (self.supportEmailForm.image) {
                self.uploadSupportEmailImage(self.supportEmailForm.image, function (uploadedUrl) {
                    emailRequest.Attachments = uploadedUrl.error ? [] : [uploadedUrl];
                    self.sendEmail(emailRequest);
                    self.logSupportEmailInWebAction(emailRequest);
                    self.resetSupportEmailForm();
                });
            } else {
                self.sendEmail(emailRequest);
                self.logSupportEmailInWebAction(emailRequest);
                self.resetSupportEmailForm();
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

        logSupportEmailInWebAction: function (emailObj) {
            var self = this;
            var payload = {
                webactionName: self.systemWebactions.supportEmail.name,
                authId: self.systemWebactions.supportEmail.authId,
                webactionData: {
                    ActionData: {
                        Subject: emailObj.Subject,
                        EmailBody: emailObj.EmailBody,
                        Attachments: emailObj.Attachments,
                        kid: self.kidForReportingIssue,
                        developerId: self.customerData.developerId || ''
                    },
                    WebsiteId: self.customerData.websiteId
                },
            };
            axios({
                method: 'post',
                url: Endpoints.AddDataToSystemWebaction,
                data: payload
            }).catch(function (error) {
                console.error('there is an error logging your webaction');
            });
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

        showGenericHelpModal: function () {
            this.showhavingIssuesModal();
        },

        // support email END


        // Rich Text Editor -- Start
        intializeFroala: function () {
            var self = this;
            var richText = self.richText;

            richText.froala = FroalaEditor(richText.elementId);
            richText.froala.initialize();
            richText.froala.setContent(richText.content);
        },

        onRichTextModalSave: function () {
            var self = this;
            var richText = self.richText;

            richText.content = richText.froala.getContent();

            Vue.set(self.model, richText.requestedPropertyName, richText.content);
            self.showHideModal(Modal.RICHTEXTEDITOR, false);

            self.updateDataForSchema();
        },

        onRichTextModalCancel: function () {
            var self = this;
            Vue.set(self.richText, 'content', '');
            self.showHideModal(Modal.RICHTEXTEDITOR, false);
        },
        // Rich Text Editor -- End


        // Text Area Start

        onTextAreaModalSave: function () {
            var self = this;
            var textArea = self.textArea;

            Vue.set(self.model, textArea.requestedPropertyName, textArea.content);
            self.showHideModal(Modal.TEXTAREA, false);

            self.updateDataForSchema();
        },

        onTextAreaModalCancel: function () {
            var self = this;
            Vue.set(self.textArea, 'content', '');
            self.showHideModal(Modal.TEXTAREA, false);
        },

        // Text Area End

        // data console mode
        removeSidebar: function () {
            var $el = document.getElementById('wrapper');
            $el.classList.remove('toggled');

        },




        // REFERENCING START --- 


        // managing object selectors --start
        findSelectors: function (propertyList, root) {
            var propertyNames = [];
            for (let i = 0; i < propertyList.length; i++) {
                let property = propertyList[i];
                let name = property.Name;
                let type = property.Type;
                let isDefaultProperty = !!PropertyVisibliityStatus[name];
                let valid = selectorsRegex.test(name) && (propertyNames.length <= 3) && (!isDefaultProperty);
                if (valid) {
                    propertyNames.push(name);
                }
            }
            return propertyNames;
        },

        containProperties: function (propertyList, root) {
            var propertyNames = [],
                length = propertyList.length >= 3 ? 3 : propertyList.length, // 3 -> minimum number of properties to show
                i = 0;
            while (propertyNames.length < length && i < propertyList.length) {
                let name = propertyList[i].Name;
                let isDefaultProperty = !!PropertyVisibliityStatus[name];
                if (!isDefaultProperty) {
                    propertyNames.push(name);
                }
                i++;
            }
            return propertyNames;
        },

        /**
        * Referencing Forward
        **/
        setSelectedclassNameInReferenceData: function (item) {
            var self = this,
                className = item ? item.name : null;

            if (className && className.trim()) {
                self.referenceData.selectedClassName = className;
                self.setSlidingPanelLevel(SlidingPanelLevel.PROPERTIES);
            } else {
                console.error("setSelectedclassNameInReferenceData: className can't be empty or null : ", className);
            }

        },

        /**
        * Referencing Forward
        **/
        setSelectedPropertyNameInReferenceData: function (item) {
            var self = this,
                propertyName = item ? item.name : null;

            if (propertyName && propertyName.trim()) {

                self.referenceData.selectedPropertyName = propertyName;
                self.setSlidingPanelLevel(SlidingPanelLevel.OBJECTS);

            } else {
                console.error("setSelectedPropertyInReferenceData: propertyName can't be null or empty : ", propertyName);
            }
        },


        // Referencing - get reverse referencing classes
        getSimilarPropertiesToReferTo: function () {
            var self = this;
            self.getClassesByClassType(self.currentClassName, function (referenceProperties) {
                if (referenceProperties && referenceProperties.length) {
                    referenceProperties = referenceProperties.filter(function (item) {
                        return self.isSelfReferencing(item) != true;
                    });
                    Vue.set(self.referenceData.reverseReference, 'relatedClassTypes', referenceProperties);
                    self.processReverseReferenceData(referenceProperties);
                    self.openReferenceSideBar(true);
                } else if (referenceProperties && referenceProperties.length === 0) {
                    toastr.error("something went wrong");
                }
            });
        },

        isSelfReferencing: function (item) {
            var self = this;
            var splitItems = item.split(':');
            var isSelfRefer = false;
            var dataPath = '';
            var lastSegment = _.last(self.currentSchemaDataPath);
            var lastSplitItem = _.last(splitItems);

            return (lastSegment.PropertyName === lastSplitItem.split('.')[1] && splitItems.length == self.currentSchemaDataPath.length - 1);
        },


        /**
        * Referencing reverse - process the reverse reference data and set props for Sliding Panel
        **/
        processReverseReferenceData: function (refData) {
            var self = this;
            var referencePanelList = [];
            var data = {
                Data: [],
                GroupCount: []
            };

            refData.map(parseResponse);
            Vue.set(self.referenceData, 'data', data);


            function parseResponse(item) {
                var listItemText = '';
                var splitItems = item.split(':');
                splitItems = splitItems.reverse();

                var obj = {
                    Count: 0,
                    Name: '',
                    SubGroupCounts: []
                };

                if (splitItems.length == 1) {
                    var path = splitItems[0].split('.')[1];
                    obj.Name += path;
                } else {
                    splitItems.map(function (splitItem, index) {
                        var path = splitItem.split('.');
                        if (path[0] == self.schemaName) {
                            obj.Name += ' of ' + path[1];
                        } else {
                            if (index == 0) {
                                //obj.Name += ' ' + splitItem.split('.')[1] + ' of ' + splitItem.split('.')[0];
                                obj.Name += ' ' + splitItem.split('.')[1];
                            } else if (index != splitItems.length - 1) {
                                obj.Name += ' of ' + splitItem.split('.')[1];
                            }
                        }
                    });
                }
                data.GroupCount.push(obj);
            }
        },


        // Reverse Referencing - get data by property
        getDataByPropertyName: function (propertyPath, callback) {
            if (!propertyPath || typeof propertyPath !== 'string' || !propertyPath.length) {
                if (callback && typeof callback == 'function') {
                    callback();
                }
                return;
            }
            var self = this;
            var requestObj = {
                PropertySegments: []
            };

            var segments = propertyPath.split(':');
            var propertySegmentItem = {};
            var classType = segments[segments.length - 1].split('.')[0];
            var propertyName = segments[segments.length - 1].split('.')[1];
            propertySegmentItem.PropertyName = segments[0].split('.')[0];
            propertySegmentItem.PropertyDataType = segments[0].split('.')[0];
            propertySegmentItem.Type = 5;
            requestObj.PropertySegments.push(propertySegmentItem);
            if (segments.length > 1) {
                segments.map(function (segment, index) {
                    if ((index + 1) < segments.length) {
                        var classType = _.find(self.languageSchema.Classes, { Name: segments[index + 1].split('.')[0] });
                        var requiredProps = [];
                        classType.PropertyList.map(function (prop) {
                            if (prop.Type !== COMPLEX_OBJECT_TYPE && prop.Type !== ARRAY_TYPE) {
                                requiredProps.push(prop.Name);
                            } else if (prop.Type === COMPLEX_OBJECT_TYPE && prop.DataType.Name === 'image') {
                                requiredProps.push(prop.Name);
                            }
                        });
                        
                        propertySegmentItem = {};
                        propertySegmentItem.PropertyName = segment.split('.')[1];
                        propertySegmentItem.PropertyDataType = segments[index + 1].split('.')[0];
                        propertySegmentItem.Type = self.isPropertyAnArray(segment.split('.')[1], segment.split('.')[0]) ? 1 : 5;
                        propertySegmentItem.Limit = (propertySegmentItem.Type === ARRAY_TYPE) ? self.maxArraySizeForDataFetching : 0;
                        propertySegmentItem.ObjectKeys = {};
                        requiredProps.map(function (key) {
                            propertySegmentItem.ObjectKeys[key] = true;
                        });
                        requestObj.PropertySegments.push(propertySegmentItem);
                    }
                });
            } else {
                if (!self.schemaData[propertyName] || !Array.isArray(self.schemaData[propertyName])) {
                    return;
                }

                var data = {
                    Data: Object.assign({}, {
                        '_parentClassName': segments[0].split('.')[0],
                        '_propertyName': segments[0].split('.')[1],
                        '_parentClassId': self.schemaData._kid,
                        '_kid': self.schemaData._kid,
                    }),
                };

                data.Data[propertyName] = self.schemaData[propertyName];
                return callback(data, classType, propertyName);
            }

            self.showLoader(true);
            axios({
                method: 'post',
                url: Endpoints.GetDataByProperty,
                data: requestObj,
            }).then(function (response) {
                self.showLoader(false);
                callback(response.data, classType, propertyName);
            }).catch(function (error) {
                self.showLoader(false);
                callback(null, classType, propertyName);
            });
        },

        // checks if the given property for the given class is an array or not
        // from the language 
        isPropertyAnArray: function (propertyName, parentClassName) {
            var self = this;
            var isArray = false;
            var parentClass = _.find(self.languageSchema.Classes, { Name: parentClassName });
            if (parentClass) {
                parentClass = Array.isArray(parentClass) ? parentClass[0] : parentClass;
                if (parentClass.hasOwnProperty('Name')) {
                    var propertyObj = _.find(parentClass.PropertyList, { Name: propertyName });
                    if (propertyObj) {
                        propertyObj = Array.isArray(propertyObj) ? propertyObj[0] : propertyObj;
                        if (propertyObj.hasOwnProperty('Type') && propertyObj.Type === 1) {
                            isArray = true;
                        }
                    }
                }
            }
            return isArray;
        },


        /**
        * Referencing Reverse -  to get related classes by class type
        **/
        getClassesByClassType: function (classType, callback) {
            var self = this;
            var referenceData = self.referenceData;
            if (classType != null) {
                referenceData.isfetchingData = true;
            }
            referenceData.isfetchingData = true;
            axios({
                method: 'POST',
                url: Endpoints.GetClassesByClassType,
                data: { classType: classType },
            }).then(function (response) {
                referenceData.isfetchingData = false;
                callback(response.data);
            }).catch(function (err) {
                referenceData.isfetchingData = false;
                toastr.error("error while getting classes for reverse referencing.", "Error");
            });
        },


        removePropertyRecursivelyFromObject: function (propertyList, object) {
            if (!propertyList || !Array.isArray(propertyList) || !propertyList.length) {
                return object;
            }

            propertyList.map(function (property) {
                recursivelyRemoveProperty(property, object);
            });

            return object;

            function recursivelyRemoveProperty(property, object) {
                for (var key in object) {
                    if (typeof object[key] === 'object') {
                        recursivelyRemoveProperty(property, object[key]);
                    } else if (!_.isEmpty(object[key]) && key === property) {
                        delete object[key];
                    }
                }
            }
        },

        selectFromExisting: function (fieldObj) {
            var self = this;

            if (self.areFieldsEditable) {
                self.unsavedChangesModal(true);
                return;
            }

            if (!self.model._kid) {
                self.saveCurrentObj(function () {
                    self.selectFromExisting(fieldObj);
                });
            }

            var objectSegment = {
                PropertyDataType: fieldObj.propertyDataType,
                PropertyName: fieldObj.propertyName,
                Type: fieldObj.type,
            };
            self.currentClassName = fieldObj.propertyDataType;
            self.currentSchemaDataPath.push(objectSegment);
            self.resetCurrentGroup();
            self.getSchemaForActiveProperty(fieldObj);
            self.openReferenceSideBar();
        },

        openReferenceSideBar: function (isForReverseReference) {
            var self = this;
            var referenceData = self.referenceData;

            Vue.set(referenceData, "requestedClassName", self.currentClassName);
            if (isForReverseReference !== true) {
                self.setSlidingPanelLevel(SlidingPanelLevel.CLASSES);
                Vue.set(referenceData, 'isForReverseReference', false);
            } else if (isForReverseReference === true && referenceData.data.GroupCount.length > 0) {
                self.setSlidingPanelLevel(SlidingPanelLevel.CLASSES);
                Vue.set(referenceData, 'isForReverseReference', true);
            }

            if (isForReverseReference === true && referenceData.data.GroupCount.length === 0) {
                toastr.error("something went wrong");
            }
        },


        referenceSaveAction: function () {
            var self = this;
            var referenceData = self.referenceData;

            if (!self.isForReverseReference) {
                if (referenceData.forwardReference.isMultipleSelect) {
                    self.addSelectedReferenceItemToArray();
                } else {
                    var dataToUpdate = Object.assign({}, referenceData.selectedForwardReferenceItem);
                    dataToUpdate = _.cloneDeep(self.removePropertyRecursivelyFromObject(['_kid', '_id'], dataToUpdate));
                    Vue.set(self, 'model', dataToUpdate);
                    self.resetReferenceData();

                    var requestObj = {
                        BulkUpdates: [],
                        Query: null,
                        UpdateValue: null,
                    };

                    var lastSegment = _.last(self.currentSchemaDataPath);
                    var parentSegment = self.currentSchemaDataPath[self.currentSchemaDataPath.length - 2];

                    var _parentClassId = null;
                    if (parentSegment._meta && parentSegment._meta.parentClassId) {
                        _parentClassId = parentSegment._meta.parentClassId
                    } else if (parentSegment.Filter && parentSegment.Filter._kid) {
                        _parentClassId = parentSegment.Filter._kid;
                    }
                    
                    var updateItem = {
                        Query: {
                            _parentClassId: _parentClassId,
                            _parentClassName: parentSegment.PropertyDataType,
                            _propertyName: lastSegment.PropertyName,
                        },
                        UpdateValue: self.removeExtraPropertiesFromObjectForReference(dataToUpdate),
                    };
                    requestObj.BulkUpdates.push(updateItem);
                    self.resetReferenceData();

                    self.updateDataForSchemaSlim(requestObj, function (response) {
                        self.getSchemaDataForActiveProperty();
                    });
                }
            } else {
                self.referenceData.level = 0;
                var requestObj = {
                    BulkUpdates: [],
                    Query: null,
                    UpdateValue: null
                };
                requestObj.BulkUpdates = self.getReverseReferenceUpdateObject();
                self.resetReferenceData();
                self.updateDataForSchemaSlim(requestObj, function (response) {
                    toastr.success('Save successful');
                });
            }
        },

        getReverseReferenceUpdateObject: function () {
            var self = this;
            var referenceData = self.referenceData;
            var bulkUpdateArr = [];
            _.map(referenceData.reverseReference.selectedItems, function (item) {
                var bulkUpdateItem = {};
                bulkUpdateItem.Query = {
                    _parentClassId: item._kid,
                    _parentClassName: referenceData.requestedClassName,
                    _propertyName: referenceData.reverseReference.propertyToRefer,
                };
                bulkUpdateItem.UpdateValue = self.removePropertyRecursivelyFromObject(['_kid'], Object.assign({}, self.model));
                bulkUpdateArr.push(bulkUpdateItem);
            });

            return bulkUpdateArr;
        },

        resetReferenceData: function () {
            var self = this;
            var referenceData = self.referenceData;
            Vue.set(self, 'referenceData', Object.assign({}, _.cloneDeep(referenceDataObjectTemplate)));
        },


        /**
        * Referencing - set the current model to the selected reference object
        **/
        updateCurrentPropertyWithSelectedReferencedObject: function (obj) {
            var self = this;
            if (obj) {
                var processedObject = self.removeExtraPropertiesFromObjectForReference(obj);
                var currentObjectProperties = self.model;
                var currentModel = Object.assign({}, self.model);
                var originalProperties = Object.keys(self.removeExtraPropertiesFromObjectForReference(currentModel));
                var replaceProperties = Object.keys(processedObject);
                var differenceProperties = _.differenceWith(originalProperties, replaceProperties);

                differenceProperties.map(setEmptyProperties);

                function setEmptyProperties(property) {
                    delete self.model[property];
                }

                var updateProperties = function (propertyName) {
                    Vue.set(self.model, propertyName, processedObject[propertyName]);
                }

                if (processedObject && Object.keys(processedObject)) {
                    Object.keys(processedObject).forEach(updateProperties);
                }

            } else {
                console.error("updateCurrentPRopertyForSelectedReferncedObject obj cant be null or empty", obj);
            }
        },

        /**
        * Referencing forward - select multiple
        **/
        addExistingItemsToArray: function () {
            var self = this;
            self.currentClassName = _.last(self.currentSchemaDataPath).PropertyDataType;
            self.referenceData.forwardReference.isMultipleSelect = true;
            self.openReferenceSideBar();
        },

        /**
        * Referencing - forward reference - add multiple items to the array
        **/
        addSelectedReferenceItemToArray: function () {
            var self = this;
            var selectedItems = self.referenceData.forwardReference.selectedItems.slice();
            if (selectedItems.length < 1) {
                return;
            }

            selectedItems.forEach(function (item) {
                self.removePropertyRecursivelyFromObject(['_kid'], item);
            });

            var requestObj = {
                BulkUpdates: [],
                Query: null,
                UpdateValue: null,
            };

            var lastSegment = _.last(self.currentSchemaDataPath);
            var parentSegment = self.currentSchemaDataPath[self.currentSchemaDataPath.length - 2];

            var _parentClassId = null;
            if (parentSegment._meta && parentSegment._meta.parentClassId) {
                _parentClassId = parentSegment._meta.parentClassId
            } else if (parentSegment.Filter && parentSegment.Filter._kid) {
                _parentClassId = parentSegment.Filter._kid;
            }

            selectedItems.map(function (item) {
                var updateItem = {
                    Query: {
                        _parentClassId: _parentClassId,
                        _parentClassName: parentSegment.PropertyDataType,
                        _propertyName: lastSegment.PropertyName,
                    },
                    UpdateValue: self.removeExtraPropertiesFromObjectForReference(item),
                };

                requestObj.BulkUpdates.push(updateItem);
            });

            self.resetReferenceData();

            self.updateDataForSchemaSlim(requestObj, function (response) {
                if (!response || !Array.isArray(response)) {
                    return;
                }

                var newlyAddedCount = response.length;
                var currentPath = _.cloneDeep(self.currentSchemaDataPath);
                var lastSegment = currentPath[currentPath.length - 1];
                lastSegment.Length = Number(lastSegment.Length) ? (lastSegment.Length + newlyAddedCount) : newlyAddedCount;
                Vue.set(self, 'currentSchemaDataPath', currentPath);
                self.getSchemaDataForActiveProperty();
            });
        },




        /**
        * Referencing 
        **/
        removeExtraPropertiesFromObjectForReference: function (obj) {

            const propertiesToRemove = [
                "_kid",
                "_id",
                "k_referenceid",
                "createdon",
                "updatedon",
                //"isarchived",
                "userid",
                "schemaid",
                "rootaliasurl",
                "_propertyName",
                "_parentClassName",
                "_parentClassId"
            ];

            if (obj) {

                if (!obj.hasOwnProperty('_reflectionId')) {
                    obj['_reflectionId'] = obj["_kid"]
                }

                var removeProperty = function (propertyName) {

                    if (obj.hasOwnProperty(propertyName)) {
                        delete obj[propertyName];
                    }

                };

                _.forEach(propertiesToRemove, removeProperty);

            }
            return obj;

        },

        resetToastrPosition: function () {
            toastr.options = {
                "positionClass": "toast-top-right",
            };
        },


        /**
        * Referencing - Forward and VMN feature
        * to get the data for the className (for referencing)
        **/
        getDataForClassByClassType: function (className, callback) {
            var self = this,
                classType = self.referenceData.requestedClassName || className;
            if (classType != null) {
                self.referenceData.isfetchingData = true;
            }

            if (classType && classType.trim()) {
                $.ajax({
                    type: 'POST',
                    url: Endpoints.GetDataByClassName,
                    data: JSON.stringify({ classType: classType }),
                    contentType: "application/json",
                    success: function (data) {
                        if (data && self.referenceData.requestedClassName) {
                            Vue.set(self.referenceData, 'data', data);
                        } else if (callback) {
                            callback(data);
                        } else {
                            toastr.error("something went wrong.", "Error");
                        }
                    },
                    error: function (err) {
                        toastr.error("error while getting classes.", "Error");
                    },
                    complete: function () {
                        self.referenceData.isfetchingData = false;
                    }
                });

            } else {
                //console.error("getDataForClassByClassType,  classType not valid. classType : ", classType);
            }
        },

        /**
        * Referencing 
        **/
        itemActions: function (item, index) {

            var self = this,
                level = self.referenceData.level;

            if (item) {

                switch (level) {
                    case SlidingPanelLevel.CLASSES:
                        if (self.referenceData.isForReverseReference) {
                            var propertyPathWithSegments = self.referenceData.reverseReference.relatedClassTypes[index];
                            self.getDataByPropertyName(propertyPathWithSegments, function (data, classType, propertyName) {
                                self.referenceData.requestedClassName = classType;
                                self.referenceData.reverseReference.propertyToRefer = propertyName;
                                if (data && data.Data) {
                                    if (Array.isArray(data.Data)) {
                                        self.referenceData.reverseReference.items = data.Data.slice(0);
                                    } else {
                                        self.referenceData.reverseReference.items.push(data.Data);
                                    }
                                }
                                self.setSlidingPanelLevel(SlidingPanelLevel.PROPERTIES);
                            });
                        } else {
                            self.setSelectedclassNameInReferenceData(item);
                        }
                        break;
                    case SlidingPanelLevel.PROPERTIES:
                        if (self.referenceData.isForReverseReference) {

                        } else {
                            self.setSelectedPropertyNameInReferenceData(item);
                        }
                        break;
                    case SlidingPanelLevel.OBJECTS:
                        self.referenceData.selectedForwardReferenceItem = item;
                        break;
                    default:
                        console.error("Stage Not Valid :", level, item);
                        break;
                }
            } else {
                console.error("error item not valid in itemActions", item);
            }

        },

        /**
        * Referencing 
        **/
        setSlidingPanelLevel: function (val) {
            var self = this;

            if (val) {
                self.referenceData.level = val;
            }
        },

        /**
        * Referencing 
        **/
        referenceCancelAction: function () {
            var self = this,
                level = self.referenceData.level;

            if (level == SlidingPanelLevel.CLASSES) {
                self.referenceData.level -= 1;
                self.resetReferenceData();
            } else if (level == SlidingPanelLevel.PROPERTIES) {
                if (self.referenceData.isForReverseReference === true) {
                    self.referenceData.reverseReference.items = [];
                    self.referenceData.reverseReference.selectedItems = [];
                    self.referenceData.reverseReference.selectedPath = null;
                    self.referenceData.reverseReference.propertyToRefer = null;
                }
                self.referenceData.level -= 1;
            } else if (level == SlidingPanelLevel.OBJECTS) {
                if (!self.referenceData.isForReverseReference) {
                    self.referenceData.selectedForwardReferenceItem = null;
                    self.referenceData.forwardReference.selectedItems = [];
                }
                self.referenceData.level -= 1;
            }
        },
    },

    watch: {
        'referenceData.level': function (val, oldVal) {
            if (val === SlidingPanelLevel.HIDDEN) {
                this.resetToastrPosition();
            }

            if (val === SlidingPanelLevel.CLASSES) {
                this.referenceData.selectedClassName = null;
                this.referenceData.selectedPropertyName = null;
            }

            if (oldVal === SlidingPanelLevel.OBJECTS && val === SlidingPanelLevel.PROPERTIES) {
                this.referenceData.selectedPropertyName = null;
            }
        },

        // the oldModel can be used to get the old state of model before user edits the properties
        'model': function (val, oldVal) {
            var self = this;

            if (val && val.hasOwnProperty('_kid') && self.oldModel['_kid'] !== val['_kid']) {
                self.oldModel = Object.assign({}, val);
            }
        }
    }
});


function showHavingIssueModal() {
    vm.showhavingIssuesModal();
}