import React from 'react';
import Select, { Creatable } from 'react-select';
import selectStyles from '../../../../../styles/components/_select_dropdown';

class SelectProperty extends React.Component {
    constructor(props) {
        super(props);

        this.handleChange = this.handleChange.bind(this);
    }

    handleChange(selected, { action }) {
        const value = selected ? selected.value : null;
        const options = [...this.props.options];

        // Handle both creation, and deletion
        if (action === 'create-option') {
            options.splice(0, 0, selected);

            this.props.onChange(this.props.name, { value: value, options });
        } else if (action === 'clear') {
            options.splice(options.indexOf(this.props.value), 1);

            this.props.onChange(this.props.name, { value: value, options });
        } else if (action === 'select-option') {
            this.props.onChange(this.props.name, { value: value, options });
        }
    }

    render() {
        const { className, creatable, label, value, options } = this.props;
        const Component = creatable ? Creatable : Select;

        return (
            <div>
                <label className='label'>{label}</label>
                <Component
                    isClearable={creatable}
                    isSearchable={creatable}
                    styles={selectStyles(this.props)}
                    options={options}
                    className={className}
                    value={options.find(option => option.value === value)}
                    onChange={this.handleChange}
                    onInputChange={this.handleChange} />
            </div>
        );
    }
}

export default SelectProperty;