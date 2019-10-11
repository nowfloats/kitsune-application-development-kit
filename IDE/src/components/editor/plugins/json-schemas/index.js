import ApplicationSchema from "./application-schema";

const SCHEMA_MAP_FILE_TYPE = {
    // Any predefined schemas mapped to file-type can be added here
    'application': ApplicationSchema
}

const SCHEMA_MAP_FILE_NAME = {
    // Any predefined schemas mapped to a file-name can be added here
}

/**
 * Recursively iterates over any valid JSON object
 * and creates a schema for the same
 * 
 * @param {any} root The current root node
 */
const generateSchemaRecursive = (root) => {
    const result = { type: '', properties: {}, items: [] };

    if (typeof root === 'object') {
        const isArray = Array.isArray(root);
        // Mod the result based on type
        if (isArray) {
            result.type = 'array';
            delete result.properties;
        } else {
            result.type = 'object';
            delete result.items;
        }

        // Iterate over object/array entries
        for (let [key, value] of Object.entries(root)) {
            const iterator = isArray ? result.items : result.properties;

            // Recursively handle the value
            iterator[key] = generateSchemaRecursive(value);

            // If we're not in an array, set the title
            if (!isArray) {
                iterator[key].title = key;
            }
        }
    }
    // Must be a primitive otherwise
    else {
        result.type = typeof (root || 'string');
        delete result.properties;
        delete result.items;
    }

    return result;
}

const getJSONSchema = (fileName, fileType, fileData) => {
    if (fileType in SCHEMA_MAP_FILE_TYPE) return SCHEMA_MAP_FILE_TYPE[fileType];
    else if (fileName in SCHEMA_MAP_FILE_NAME) return SCHEMA_MAP_FILE_NAME[fileName];
    else {
        // Use fileData to build a schema, with an empty top level display name
        const schema = generateSchemaRecursive(fileData);
        return schema;
    }
}

export default getJSONSchema;