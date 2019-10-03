import React from 'react';

class BooleanProperty extends React.Component {
    constructor(props) {
        super(props);

        this.handleChange = this.handleChange.bind(this);
    }

    handleChange(evt) {
        this.props.onChange(evt.target.name, {value: evt.target.checked});
    }

    render() {
        const { name, label, value, children } = this.props;

        return (
            <div className="boolean-container">
                <input name={name}
                    className='regular-checkbox'
                    type='checkbox'
                    checked={value === 'true' || value === true}
                    onChange={this.handleChange} />
                <label className='boolean-container-label'>{label}</label>
                <div className="boolean-children">
                    {children}
                </div>
            </div>
        )
    }
}

export default BooleanProperty;