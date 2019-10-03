const Endpoints = {
    GetFormSchema: 'Schema/GetFormSchema',
    GetLanguageSchema: 'Schema/GetLanguageSchemaById',
    AddDataForSchema: 'Schema/AddDataForSchema',
    UpdateDataForSchema: 'Schema/UpdateDataForSchema',
    GetDataForSchema: 'Schema/GetDataForSchema'
}

Vue.component("VueFormGenerator", VueFormGenerator.component);


const InputTypeMapping = {
    0: 'text',
    1: 'complex',
    2: 'number',
    3: 'checkbox',
    4: 'date',
    5: 'complex',
    6: 'complex'
}

var vm = new Vue({
    el: "#app",

    components: {
        "str-input": StrInput,
        "link-input": LinkInput,
        "datetime-input": DatetimeInput,
        "timing-input": TimingInput,
        "boolean-input": BooleanInput
    },

    data: {
        show_save: false,
        new_site: false,

        path_map: [],
        path: null,

        model: {},
        schemaData: [],

        legends: [],

        languageSchema: [],
        renderedForm: { groups: [] },
        schema: {},
        formOptions: {
            validateAfterLoad: true,
            validateAfterChanged: true
        }
    },

    created: function () {
        this.getLanguageSchema();
        this.getDataForSchema();
    },

    methods: {
        getLanguageSchema: function () {
            var that = this;
            $.ajax({
                type: 'POST',
                url: Endpoints.GetLanguageSchema,
                success: function (data) {
                    if (data != null || data != "") {
                        that.$data.languageSchema = $.parseJSON(data);
                        that.renderSchema();
                        that.openForm(that.$data.legends[0]);
                    }
                },
                error: function () {
                    toastr.error("Error", "No Schema mapped for this site.");
                },
            });
        },

        addOrUpdateDataForSchema: function () {
            if (this.new_site) {
                this.addDataToSchema();
            } else {
                this.updateDataForSchema();
            }
        },

        addDataToSchema: function () {
            var that = this;
            var formData = {
                'WebsiteId': null,
                'Data': {}
            }
            //this.model = VueFormGenerator.schema.createDefaultObject(this.languageSchema);
            formData.Data = this.model;
            $.ajax({
                type: 'POST',
                url: Endpoints.AddDataForSchema,
                data: JSON.stringify(formData),
                success: function (data) {
                    toastr.success("Success", "Data Updated")
                    that.$data.new_site = false;
                },
                error: function () {
                    toastr.error("Error", "Something bad happened!");
                },
                contentType: "application/json"
            });
        },

        updateDataForSchema: function () {
            var that = this;
            var update_value = {}

            if (this.path === "") {
                update_value = this.model;
            } else {
                update_value = _.get(this.model, this.path, null);
            }

            if (update_value != null) {

                var formdata = {
                    'Query': {
                        '_kid': update_value._kid,
                        'k_referenceid': update_value.k_referenceid
                    },
                    'UpdateValue': {}
                }
                formdata.UpdateValue = update_value
                $.ajax({
                    type: 'POST',
                    url: Endpoints.UpdateDataForSchema,
                    data: JSON.stringify(formdata),
                    success: function (data) {
                        toastr.success("Success", "Data Updated")
                    },
                    error: function () {
                        toastr.error("Error", "Something bad happened!");
                    },
                    contentType: "application/json"
                });
            } else {
                toastr.error("Error", "Update cannot be null!");
            }
        },

        prettyJSON: function (json) {
            if (json) {
                json = JSON.stringify(json, undefined, 4);
                json = json.replace(/&/g, '&').replace(/</g, '<').replace(/>/g, '>');
                return json.replace(/("(\\u[a-zA-Z0-9]{4}|\\[^u]|[^\\"])*"(\s*:)?|\b(true|false|null)\b|-?\d+(?:\.\d*)?(?:[eE][+\-]?\d+)?)/g, function (match) {
                    var cls = 'number';
                    if (/^"/.test(match)) {
                        if (/:$/.test(match)) {
                            cls = 'key';
                        } else {
                            cls = 'string';
                        }
                    } else if (/true|false/.test(match)) {
                        cls = 'boolean';
                    } else if (/null/.test(match)) {
                        cls = 'null';
                    }
                    return '<span class="' + cls + '">' + match + '</span>';
                });
            }
        },

        getDataForSchema: function (data) {
            var that = this;
            $.ajax({
                type: 'POST',
                url: Endpoints.GetDataForSchema,
                success: function (data) {
                    if (data != null || data != "") {
                        that.$data.model = data;
                        that.$data.schemaData = data;
                    } else {
                        that.$data.new_site = true;
                    }
                },
                error: function () {
                    that.$data.new_site = true;
                },
            });
        },

        getClassFromName: function (className) {
            var that = this;
            if (className[0] === '[') {
                className = className.split('[')[1].split(']')[0];
            }

            var obj = this.languageSchema.Classes.filter(obj => obj.Name.toUpperCase() == className.toUpperCase());
            return obj[0];
        },

        openForm: function (name) {
            this.schema = this.renderedForm;
            this.show_save = true;
            var fieldSet = this.renderedForm.groups.filter(group => group.legend.toUpperCase() == name.toUpperCase());
            var path = this.path_map.filter(m => m.legend.toUpperCase() == name.toUpperCase());
            if (path != undefined) {
                this.path = path[0].path;
            }
            this.schema = {
                groups: [fieldSet[0]]
            }
        },

        // Render all top level classes
        renderSchema: function () {
            var that = this;
            if (this.languageSchema.length != 0) {

                // Get base class object
                var base_class = this.languageSchema.Classes.filter(ob => ob.Name.toUpperCase() == this.languageSchema.EntityName.toUpperCase());

                // Add base class object to sidebar
                that.$data.legends.push(base_class[0].Name.charAt(0).toUpperCase() + base_class[0].Name.substr(1).toLowerCase());

                // Render base class object
                this.renderObject(base_class[0], base_class[0].Name);

                _.forEach(base_class[0].PropertyList, (prop) => {
                    // If type is `object` or type is` array`
                    if (prop.Type == 1 || prop.Type == 5) {

                        // Add all top level to sidebar
                        that.$data.legends.push(prop.Name.charAt(0).toUpperCase() + prop.Name.substr(1).toLowerCase());
                    }
                });

            }
        },

        renderObject: function (_class, name, modelName) {
            var that = this;
            
            var sidebarItem = {
                legend: name.toUpperCase(),
                fields: []
            }

            this.renderedForm.groups.push(sidebarItem);
            modelName = modelName === undefined ? '' : modelName + '.' + name;

            this.path_map.push({
                legend: name.charAt(0).toUpperCase() + name.substr(1).toLowerCase(),
                path: modelName.startsWith('.') ? modelName.substr(1) : modelName
            });

            _.forEach(_class.PropertyList, function (property) {
                if (property.Type == 5) {
                    var obj = that.getClassFromName(property.DataType.Name);
                    if (obj != undefined && obj.ClassType == 0) {
                        that.renderObject(obj, property.Name, modelName);
                    }
                }
                else if (property.Type == 1) {
                    that.renderObjectArray(property, property.Name, modelName);
                }
                else {
                    that.renderField(property, name, modelName.replace('.', ''));
                }
            });
        },

        renderObjectArray: function (property, name, modelName) {
            var that = this;

            this.renderedForm.groups.push({
                legend: name.toUpperCase(),
                fields: [{
                    type: 'dynamic',
                    model: name,
                    schema: {
                        fields: []
                    }
                }]
            })

            if (modelName == undefined)
                modelName = ''
            else
                modelName = modelName + '.' + name;

            this.path_map.push({
                legend: name.charAt(0).toUpperCase() + name.substr(1).toLowerCase(),
                path: modelName.startsWith('.') ? modelName.substr(1) : modelName
            });

            var obj = this.getClassFromName(property.DataType.Name);
            if (obj != undefined) {
                _.forEach(obj.PropertyList, function (property) {
                    if (property.Type != 1 && property.Type != 5) {
                        that.renderFieldForArray(property, name);
                    }
                });
            }
        },

        renderField: function (property, name, modelName) {
            var fieldSet = this.renderedForm.groups.filter(group => group.legend.toUpperCase() == name.toUpperCase());
            modelName = modelName == '' ? property.Name : modelName + '.' + property.Name;
            // TODO: Render fields from components
            fieldSet[0].fields.push({
                type: InputTypeMapping[property.Type] == 'text' ||
                    InputTypeMapping[property.Type] == 'number' ||
                    InputTypeMapping[property.Type] == 'date' ? 'input' : InputTypeMapping[property.Type],
                inputType: InputTypeMapping[property.Type],
                label: property.Description,
                model: modelName,
                visible: property.Name === '_kid' || property.Name === 'k_referenceid' || property.Name === 'isarchived' ? false : true,
                // required: property.IsRequired,
                placeholder: property.Description,
                format: "YYYY-MM-DD"
            });
        },

        renderFieldForArray: function (property, name) {
            var fieldSet = this.renderedForm.groups.filter(group => group.legend.toUpperCase() == name.toUpperCase());
            modelName = property.Name;
            fieldSet[0].fields[0].schema.fields.push({
                type: InputTypeMapping[property.Type] == 'text' ||
                    InputTypeMapping[property.Type] == 'number' ||
                    InputTypeMapping[property.Type] == 'date' ? 'input' : InputTypeMapping[property.Type],
                inputType: InputTypeMapping[property.Type],
                label: property.Description,
                model: modelName,
                visible: property.Name === '_kid' || property.Name === 'k_referenceid' || property.Name === 'isarchived' ? false : true,
                // required: property.IsRequired,
                placeholder: property.Description,
                format: "YYYY-MM-DD"
            });
        }
    },
});