import ApplicationSchema from "./application-schema";

const SCHEMA_MAP_FILE_TYPE = {
    'application': ApplicationSchema
}

const SCHEMA_MAP_FILE_NAME = {
    // 'kitsune-settings.json': ApplicationSchema
}

const generateSchemaRecursive = (root) => {
    const result = {};

    if (typeof root === 'object') {
        for (let [key, value] of Object.entries(root)) {
            result[key] = {
                title: key,
                type: value === null ? 'string' : Array.isArray(value) ? 'array' : (typeof value)
            };

            if (result[key].type === 'object') {
                result[key].properties = generateSchemaRecursive(value);
            }

            if (result[key].type === 'array') {
                result[key].items = [];
                for (let item of value) {
                    result[key].items.push(generateSchemaRecursive(item));
                }
            }
        }

        return result;
    } else {
        return { type: Array.isArray(root) ? 'array' : typeof (root || 'string') };
    }
}

const getJSONSchema = (fileName, fileType, fileData) => {
    if (fileType in SCHEMA_MAP_FILE_TYPE) return SCHEMA_MAP_FILE_TYPE[fileType];
    else if (fileName in SCHEMA_MAP_FILE_NAME) return SCHEMA_MAP_FILE_NAME[fileName];
    else {
        // Use fileData to build a schema
        const schema = { title: '', type: 'object' };
        schema.properties = generateSchemaRecursive(fileData);

        return schema;
    }
}

export default getJSONSchema;