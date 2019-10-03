import React from 'react';
import Select from 'react-select';
import selectStyles from '../../../../styles/components/_select_dropdown';

const colorMap = {
    'string': '114,189,77',
    'number': '255,119,0',
    'boolean': '54,163,227',
    'datetime': '172,92,209',
    'k-string': '154,119,104',
    'array': '207,192,24',
    'object': '108,156,177',
    'default': '81,105,133'
};

const PropertyDataType = (props) => {
    const { className, selectedDataType, options, onChange, isExisting, isString, isArray, isFormattable, handleFormat, handleArray } = props;
    return (
        <div className={className}>
            <label className='label'>data type*</label>
            <div>
                <Select name='data-type-select'
                    isSearchable={false}
                    value={options.find(option => option.value === selectedDataType)}
                    options={options}
                    onChange={onChange}
                    styles={selectStyles({ ...props, colorMap })}
                    isDisabled={isExisting} />
            </div>
            {(isString || selectedDataType !== '') ?
                <div className='field inline-field custom-field'>
                    {isString ?
                        <div>
                            <label className='checkbox' htmlFor='isString'>is formattable</label>
                            <input id='isString' name='isString' type='checkbox' checked={isFormattable}
                                onChange={handleFormat} />
                        </div> : null
                    }
                    {selectedDataType !== '' ?
                        <div>
                            <label className='checkbox' htmlFor='isArray'>is array</label>
                            <input id='isArray' name='isArray' type='checkbox' checked={isArray}
                                onChange={handleArray} disabled={isExisting} />
                        </div> : null
                    }
                </div> : null
            }
        </div>
    );
}

export default PropertyDataType;