function isClassProcessed(name) {
    let exists = _.find(classesProcessed, (cls) => {
        return cls.Name.trim().toLowerCase() === name.trim().toLowerCase();
    });
    return !!exists;
}

function getProcessedClass(name) {
    let clas = _.find(classesProcessed, (cls) => {
        return cls.Name.trim().toLowerCase() === name.trim().toLowerCase();
    });
    return Object.assign({}, clas.Class);
}

function getPropertyObject() {
    let propertyObj = {
        type: '',
        inputType: '',
        label: '',
        model: '',
        disabled: false
    }
    return Object.assign({}, propertyObj);
}

function getClassObject() {
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
}

function getClassWithClassName(name) {
    let clas = _.find(defaultClasses, (cls) => {
        return cls.Name.trim().toLowerCase() == name.trim().toLowerCase();
    });
    return clas;
}

function getSchemaFromClass(klass, modelName) {

    if (!isClassProcessed(klass.Name)) {

        let klassObj = getClassObject(),
            { Name } = klass;

        klassObj.model = modelName ? modelName : Name.toLowerCase();
        klassObj.schema.groups[0].legend = Name.toLowerCase();

        _.forEach(klass.PropertyList, (property) => {

            let { Type, Name, DataType } = property,
                prop = getPropertyObject(),
                clsObj = getClassObject();

            prop.label = Name;
            prop.model = Name;

            if (Type != 1 && Type != 5) {
                prop.type = 'input';
                prop.inputType = InputTypeMapping[Type];
            } else if (Type == 5) {
                let clas = getClassWithClassName(DataType.Name);
                prop = getSchemaFromClass(clas, Name);
            }
            if (Type != 7) {
                klassObj.schema.groups[0].fields.push(prop);
            }
        })

        klassObj.model = klass.Name;
        classesProcessed.push({ Class: klassObj, Name: klass.Name });
        return klassObj;

    } else {
        let clas = getProcessedClass(klass.Name);
        clas.model = modelName ? modelName : klass.Name;
        return clas;
    }
}

function processAllClasses() {
    _.forEach(defaultClasses, (cls) => {
        getSchemaFromClass(cls)
    })
}