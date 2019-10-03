const ApplicationSchema = {
    title: '',
    type: 'object',
    properties: {
        Status: {
            type: "string",
            title: "status",
            default: ""
        },
        Configuration: {
            type: "object",
            title: "configuration",
            properties: {
                type: {
                    type: "string",
                    title: "type"
                },
                existingAppEndpoint: {
                    type: "string",
                    title: "app endpoint"
                },
                dbType: {
                    type: "string",
                    title: "database type"
                },
                existingDBEndpoint: {
                    type: "string",
                    title: "database endpoint"
                }
            }
        },
        Keywords: {
            type: "string",
            title: "keywords"
        },
        MetaInfo: {
            type: "string",
            title: "meta information"
        },
        Title: {
            type: "string",
            title: "title"
        },
        Description: {
            type: "string",
            title: "description"
        }
    }
}

export default ApplicationSchema;