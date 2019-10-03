import React from 'react';
import Form from 'react-jsonschema-form';

class JSONSchemEditor extends React.Component {
    render() {
        const { schema, className, formData, onChange } = this.props;
        
        return (schema &&
            <Form
                className={`json-editor ${className || ''}`}
                schema={schema}
                formData={formData}
                onChange={onChange}
            />
        )
    }
}

export default JSONSchemEditor;