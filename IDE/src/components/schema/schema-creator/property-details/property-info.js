import React from 'react';

const PropertyInfo = ({ description, onChange }) => (
    <div className='field'>
        <label className='label'>property info</label>
        <div className='control'>
            <input name='description' className='input' type='text' value={description}
                onChange={onChange} />
        </div>
    </div>
);

export default PropertyInfo;