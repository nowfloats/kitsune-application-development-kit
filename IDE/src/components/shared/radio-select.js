import PropTypes from 'prop-types';
import React from 'react';
import { isArray } from 'util';

/**
 * A list of radio buttons styled as per the shared component guides
 */
const RadioSelect = ({ children, selectedValue = null, onChange, className, labelClass, radioClass, contentClass }) => {
    if (!isArray(children)) children = [children];

    return (
        <div className={className}>
            {
                children.map((child, i) => (child &&
                    React.cloneElement(child, {
                        key: `kt-radio-select-${i}`,
                        checked: (selectedValue === child.props.value),
                        onChange: evt => {
                            if(selectedValue !== evt.target.value) {
                                onChange(evt.target.value);
                            }
                        },
                        labelClass, radioClass, contentClass
                    })
                ))
            }
        </div>
    );
};

/**
 * A single radio element in the radio options, allows styling each option, and supplying values
 */
RadioSelect.Value = ({ value, name, checked, children, onChange, disabled, labelClass, radioClass, contentClass }) => (
    <label className={`kt-radio-label ${disabled ? 'disabled' : ''} ${labelClass || ''}`}>
        <input type="radio" disabled={disabled} name={name} value={value}
            checked={checked}
            onChange={onChange} />
        <span className={`kt-radio-content ${disabled ? 'disabled' : ''} ${contentClass || ''}`}>
            {children}
        </span>
        <span className={`kt-radio-checkmark ${disabled ? 'disabled' : ''} ${radioClass || ''}`}></span>
    </label>
);

RadioSelect.propTypes = {
    selectedValue: PropTypes.string,
    selectedIndex: PropTypes.number
}

RadioSelect.defaultProps = {
    selectedIndex: 0
}

export default RadioSelect;