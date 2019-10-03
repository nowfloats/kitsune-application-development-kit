var image_class =  {
    Class: {
        type: "object",
        model: "image",
        schema: {
            groups: [
                {
                    legend: "image",
                    fields: [
                        {
                            className: 'STR',
                            type: "input",
                            inputType: "text",
                            label: "description",
                            model: "description",
                            visible: true,
                            placeholder: "enter image description",
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
                                            vm.areFieldsEditable = true;
                                        } else {
                                            Vue.set(btn, 'classes', 'btn btn-default fa fa-pencil');
                                            vm.addOrUpdateData();
                                        }
                                        Vue.set(field, 'disabled', !field.disabled);
                                    }
                                }
                            ]
                        },
                        {
                            className : 'STR',
                            type: "label",
                            label: "url",
                            model: "url",
                            visible: true,
                            placeholder: "Image url",
                            disabled: true,
                            buttons: [
                                {
                                    classes: "btn btn-default",
                                    label: "upload",
                                    title: "edit",
                                    onclick: function (model, field) {
                                        cancelUpload();
                                        let btn = document.getElementById('uploadFileInit');
                                        btn.click();
                                    }
                                }
                            ]
                        }
                    ]
                }
            ]
        }
    },
    Name: "image"
}

var link_class =  
{
    "Class": {
        type: "object",
        model: "image",
        schema: {
            groups: [
                {
                    legend: "image",
                    fields: [
                        {
                            className: 'STR',
                            type: "input",
                            inputType: "text",
                            label: "description",
                            model: "description",
                            visible: true,
                            placeholder: "enter link description",
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
                                            vm.areFieldsEditable = true;
                                        } else {
                                            Vue.set(btn, 'classes', 'btn btn-default fa fa-pencil');
                                            vm.addOrUpdateData();
                                        }
                                        Vue.set(field, 'disabled', !field.disabled);
                                    }
                                }
                            ]
                        },
                        {
                            className: 'STR',
                            type: "input",
                            inputType: "text",
                            label: "url",
                            model: "url",
                            visible: true,
                            placeholder: "link",
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
                                            vm.areFieldsEditable = true;
                                        } else {
                                            Vue.set(btn, 'classes', 'btn btn-default fa fa-pencil');
                                            vm.addOrUpdateData();
                                        }
                                        Vue.set(field, 'disabled', !field.disabled);
                                    }
                                }
                            ]
                        }
                    ]
                }
            ]
        }
    },
    Name: "link"
}

var base_class = {
    Class: {
        type: "object",
        model: "image",
        schema: {
            groups: [
                {
                    legend: "base",
                    fields: [
                        
                    ]
                }
            ]
        }
    },
    Name: "base"
}

var root_class = {
    Class: {
        type: "object",
        model: "image",
        schema: {
            groups: [
                {
                    fields: [
                        {
                            type: "arrobj",
                            model: "data",
                            className: "base",
                            schema: {
                                groups: [{
                                    legend: "data",
                                    fields: []
                                }]
                            }
                        }
                    ]
                }
            ]
        }
    },
    Name: "root"
}

function _compareString(a, b) {
    if (_.isString(a) && _.isString(b)) {
        a = a.trim().toLowerCase();
        b = b.trim().toLowerCase();
        return a == b;
    }
    console.error('parameters not valid : ', a, b);
    return false;
}