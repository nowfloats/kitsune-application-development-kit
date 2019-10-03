var SlidingHeader = Vue.component('sliding-header', {
    template: '#sliding-panel-header',
    name: SlidingHeader,
    data: function () {
        return {

        }
    },
    props: [
        'className',
        'currentlevel',
        'referenceHeaderText'
    ],
    computed: {
        getReferenceTitle: function () {
            var titleText = '';
            if (this.$parent.isforreversereference === true) {
                titleText = 'Also add the created ' + this.className + ' to';
            } else {
                titleText = 'Select an existing ' + this.className;
                var configuredFor = this.referenceHeaderText.split(' ')[0] || '';
                configuredFor = (configuredFor == this.$root.schemaName) ? 'others' : configuredFor;
                titleText = (configuredFor.length) ? (titleText + ' configured for ' + configuredFor) : titleText;
            }
            return titleText;
        },

        getTotalItemCount: function () {
            var totalCount = 0;
            var referenceData = this.$root.referenceData;
            if (referenceData.data && referenceData.data.Data && referenceData.data.Data.length) {
                if (referenceData.level == 1) {
                    totalCount = this.$root.referenceData.data.Data.length;
                } else if (referenceData.level == 2) {
                    this.$parent.itemlist.map(function (item) {
                       return totalCount += item.matchedProperties;
                    });
                } else {
                    totalCount = this.$parent.getdataforclassandproperty.length;
                }
            } 
            return totalCount;
        }
    }
});

var SlidingFooter = Vue.component('sliding-footer', {
    template: '#sliding-panel-footer',

    name: SlidingFooter,

    props: {
        "cancelaction": {
            type: Function
        },
        "slidingpanellevel": {
            type: Number
        },
        "saveaction": {
            type: Function
        }
    },

    computed: {
        showSaveButton() {
            return this.slidingpanellevel === SlidingPanelLevel.OBJECTS || this.$parent.isforreversereference === true;
        },

        isAnyItemSelected() {
            var isSelected = false;
            var root = this.$root;
            if (this.$parent.isforreversereference === true) {
                isSelected = (root.referenceData.reverseReference.selectedItems.length > 0);
            } else {
                if (root.referenceData.forwardReference.isMultipleSelect) {
                    isSelected = (root.referenceData.forwardReference.selectedItems.length > 0);
                } else {
                    isSelected = (root.referenceData.selectedForwardReferenceItem != null);
                }
                
            }
            return isSelected;
        },

        isListEmpty() {
            var isEmpty = false;
            if (this.$parent.isforreversereference === true) {
                isEmpty = (this.$parent.classlist.length == 0);
            }
            return isEmpty;
        },
    },
});

var SlidingPagination = Vue.component('sliding-pagination', {
    template: '#sliding-panel-list-pagination',
    name: SlidingPagination,
    props: [
        'itemcount',
        'getnextitems',
        'getpreviousitems',
        'ispaginationrequired',
        'currentindex',
        'dividecount'
    ],
    computed: {
        currentListPosition: function () {
            var position = 1;
            if (this.ispaginationrequired() && this.currentindex > 1) {
                position = ((this.currentindex - 1) * this.dividecount);
            }

            if (!this.itemcount) {
                position = 0;
            }
            return position;
        },
        nextListPosition: function () {
            var position = this.itemcount;
            if (this.itemcount > this.dividecount) {
                position = (this.currentindex * this.dividecount);
                position = (position > this.itemcount) ? this.itemcount : position;
            }
            return position;
        }
    }
});

var ListComponent = Vue.component('list-component', {
    template: '#list-component',
    name: ListComponent,
    data: function () {
        return {
            selectedItems: [],
            selectedItem: null,
            pagination: {
                currentIndex: 1,
                divideCount: 10
            },
        }
    },
    props: [
        'list',
        'checkboxrequired',
        'itemaction',
        'isrenderingobjectdata',
        'propertiestodisplay'
    ],
    components: {
        "SlidingPagination": SlidingPagination
    },
    methods: {
        onClickListItem: function () {
            this.getRelatedItems(function (relatedItems) {
                if (relatedItems && relatedItems.error) {
                    console.log('error fetching related Items');
                    return;
                };
            });
        },

        isPropertyValid: function (item, propertyName) {
            if (item && propertyName) {
                return item.hasOwnProperty(propertyName);
            } else {
                console.error("arguments not valid : ", item, propertyName);
            }
            return false;
        },

        getNextItems: function () {
            if (this.list.length - (this.pagination.currentIndex * this.pagination.divideCount) > 0) {
                this.pagination.currentIndex++;
            }
        },

        getPreviousItems: function () {
            if (this.pagination.currentIndex > 1) {
                this.pagination.currentIndex--;
            }
        },

        isPaginationRequired: function () {
            return (this.list.length > this.pagination.divideCount);
        },

        onItemSelectedForReference: function (item, index) {
            var self = this;
            var root = self.$root;
            if (root.referenceData.isForReverseReference === true) {
                var selectedItems = root.referenceData.reverseReference.selectedItems;
                var foundIndex = selectedItems.indexOf(item);
                if (foundIndex != -1) {
                    selectedItems.splice(foundIndex, 1);
                } else {
                    selectedItems.push(item);
                }
            } else {
                if (self.$parent.$parent.ismultiselect) {
                    var selectedItems = root.referenceData.forwardReference.selectedItems;
                    var foundIndex = selectedItems.indexOf(item);
                    if (foundIndex != -1) {
                        selectedItems.splice(foundIndex, 1);
                    } else {
                        selectedItems.push(item);
                    }   
                } else {
                    this.selectedItem = item;
                    this.itemaction(item);
                }
            }
        },

        isChecked: function (item) {
            if (_.isEmpty(item)) {
                return;
            }
            var root = this.$root;
            if (root.referenceData.isForReverseReference) {
                return !_.isEmpty(_.find(root.referenceData.reverseReference.selectedItems, { _kid: item._kid }));
            } else {
                if (root.referenceData.forwardReference.isMultipleSelect) {
                    return !_.isEmpty(_.find(root.referenceData.forwardReference.selectedItems, { _kid: item._kid }));
                } else {
                    return (this.selectedItems.indexOf(item) != -1);
                }
            }
        },

        imageDimensions() {
            let self = this,
                img = self.$refs.img,
                imageProperties = self.imageProperties,
                imgDimensions = self.$refs.imgDimensions;

            if (imageProperties && !imageProperties.isDefault && imgDimensions && img && img.naturalWidth) {
                imgDimensions.innerText = `${img.naturalWidth}px X ${img.naturalHeight}px`;
            }
        },

        selectFirstItem: function () {
            var self = this;
            if (self.$parent.$parent.level === 1) {
                self.itemaction(self.$parent.$parent.classlist[0]);
            } else if (self.$parent.$parent.level === 2) {
                self.itemaction(self.$parent.$parent.itemlist[0]);
            }
        },

        isSystemProperty: function (property) {
            return this.$parent.$parent.systemproperties.includes(property);
        },

        isRenderingObect: function (obj) {
            return (typeof obj === 'object');
        },

        onClassItemClick: function (item, index) {
            var root = this.$root;
            if (root.referenceData.isForReverseReference === true) {
                var itemPath = root.referenceData.reverseReference.relatedClassTypes[index];
                this.itemaction(itemPath, index);
            } else {
                this.itemaction(item);
            }
        },

        selectDeselectAll: function () {
            var root = this.$root;
            if (this.areAllItemsSelected) {
                if (root.referenceData.isForReverseReference) {
                    root.referenceData.reverseReference.selectedItems = [];
                } else {
                    root.referenceData.forwardReference.selectedItems = [];
                }
            } else {
                if (root.referenceData.isForReverseReference) {
                    root.referenceData.reverseReference.selectedItems = root.referenceData.reverseReference.items.slice();
                } else {
                    root.referenceData.forwardReference.selectedItems = root.getDataForClassAndProperty.slice();
                }
            }
            
        },

        getFormattedPropertyValue: function (value) {
            if (value && Array.isArray(value)) {
                return '[...]';
            } else if (value && typeof value === 'object') {
                return '(...)';
            } else {
                return value;
            }
        },

        getFormattedPropertyName: function (name) {
            return _.startCase(name);
        },

        isAnyPropertyPresentToDisplay: function (item) {
            var isAvailable = false;
            if (Array.isArray(item) || !Array.isArray(this.propertiestodisplay) || _.isEmpty(this.propertiestodisplay)) {
                return isAvailable;
            }
            for (key in item) {
                if (this.propertiestodisplay.includes(key)) {
                    isAvailable = true;
                }
            }
            return isAvailable;
        },
    },
    computed: {
        showCheckboxes: function () {
            return this.$parent.$parent.isforreversereference === true;
        },

        isRenderingImageClass: function () {
            var root = this.$root;
            return (root.currentClassName === 'image');
        },

        listRepository: function () {
            var startIndex = (this.pagination.currentIndex > 1) ? ((this.pagination.currentIndex - 1) * this.pagination.divideCount) : 0;
            var endIndex = this.isPaginationRequired() ? (this.pagination.currentIndex * this.pagination.divideCount) : this.list.length;
            return this.list.slice(startIndex, endIndex);
        },

        areAllItemsSelected: function () {
            var root = this.$root;
            var areAllSelected = false;
            if (root.referenceData.isForReverseReference) {
                areAllSelected = (root.referenceData.reverseReference.items.length &&
                    root.referenceData.reverseReference.selectedItems.length === root.referenceData.reverseReference.items.length);
            } else {
                if (root.referenceData.forwardReference.isMultipleSelect) {
                    areAllSelected = (root.getDataForClassAndProperty.length === root.referenceData.forwardReference.selectedItems.length);
                }
            }
            return areAllSelected;
        },

        showMultiSelect: function () {
            var root = this.$root;
            var show = false;
            if (root.referenceData.isForReverseReference) {
                show = (root.referenceData.level == 2 && root.referenceData.reverseReference.items.length > 1);
            } else {
                show = (root.referenceData.level == 3);
            }
            return show;
        },
    },

    watch: {
        '$parent.$parent.level': function (val, oldVal) {
            if (val !== oldVal && (val > oldVal) && (val === 2) && this.$parent.$parent.itemlist.length === 1) {
                this.selectFirstItem();
            }
        },

        'list': function (val, oldVal) {
            if (val && val.length === 1 && this.$parent.$parent.level == 1) {
                this.selectFirstItem();
            }
        }
    }
});

var SlidingBody = Vue.component('sliding-body', {
    template: '#sliding-panel-body',
    name: SlidingBody,
    data: function () {
        return {
            nestingLevel: 0
        }
    },
    props: [
        'checkboxrequired',
        'list',
        'itemaction',
        'isrenderingobjectdata',
        'propertiestodisplay'
    ],
    components: {
        "ListComponent": ListComponent
    }
});

var SlidingPanel = Vue.component('sliding-panel', {
    template: '#sliding-panel',

    name: SlidingPanel,

    components: {
        "SlidingHeader": SlidingHeader,
        "SlidingBody": SlidingBody,
        "SlidingFooter": SlidingFooter
    },


    props: {
        "classlist": {
            type: Array
        },
        "itemlist": {
            type: Array
        },
        "getdataforclassbyclasstype": {
            type: Function
        },
        "itemaction": {
            type: Function
        },
        "level": {
            type: Number,
            default: function () {
                return 0;
            }
        },
        "setslidingpanellevel": {
            type: Function
        },
        "checkboxrequired": {
            type: Boolean,
            default: function () {
                return false;
            }
        },
        "cancelaction": {
            type: Function
        },
        "saveaction": {
            type: Function
        },
        "getdataforclassandproperty": {
            type: Array
        },
        "getpropertiestodisplay": {
            type: Array
        },
        "classname": {
            type: String
        },
        "getreferenceheadertext": {
            type: String
        },
        "isforreversereference": {
            type: Boolean
        },
        "systemproperties": {
            type: Array
        },
        "isfetchingdata": {
            type: Boolean
        },
        "ismultiselect": {
            type: Boolean,
        }
    },

    watch: {
        classname: function (newVal, oldVal) {
            if (!this.isforreversereference) {
                this.getdataforclassbyclasstype();
            }
        },

        'isfetchingdata': function (val, oldVal) {
            var root = this.$root;
            if (val) {
                root.showLoader(true);
            } else {
                root.showLoader(false);
            }
        }
    }

});


var ContextPopoverMenu = Vue.component('context-popover-menu', {
    template: '#context-popover-menu',
    name: ContextPopoverMenu,
    data: function () {
        return {
            isMenuOpen : false
        };
    },
    props: [
        "menuitems",
        "position",
        "isopen"
    ],
    methods: {
        toggleMenuOpen: function () {
            this.isMenuOpen = !this.isMenuOpen;
        },

        onMenuItemClick: function (item) {
            this.isMenuOpen = false;
            item.action();
        }
    },
    computed: {
        isContextMenuOpen: function () {
            return(this.isopen == true || this.isMenuOpen == true);
        }
    }
});