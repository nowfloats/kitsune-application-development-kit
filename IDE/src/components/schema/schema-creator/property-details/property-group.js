import React from 'react';

const PropertyGroup = ({ className, onChange, groupName }) => (
    <div className={className}>
        <label className='label'>property group</label>
        <div className='control'>
            <input name='groupName'
                className='input'
                type='text'
                value={groupName}
                onChange={onChange} />
        </div>
    </div>
);

export default PropertyGroup;