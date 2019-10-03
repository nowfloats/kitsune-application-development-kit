import React from 'react';

const PropertyName = ({ className, name, onChange, isExisting, propNameError }) => (
    <div className={className}>
        <label className='label'>property name*</label>
        <div className='control'>
            <input name='name'
                className='input'
                type='text'
                value={name}
                onChange={onChange}
                disabled={isExisting} />
        </div>
        <p className={propNameError === null ? 'help is-danger hide' : 'help is-danger'}>
            {propNameError}
        </p>
    </div>
);

export default PropertyName;