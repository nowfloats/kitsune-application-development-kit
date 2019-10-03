import React from 'react';

const IsRequired = ({ className, isRequired, isExisting, onChange }) => (
    <div className={className}>
        <label className='checkbox' htmlFor='isRequired'>is required</label>
        <input id='isRequired' name='isRequired' type='checkbox' checked={isRequired}
            onChange={onChange} disabled={isExisting} />
    </div>
);

export default IsRequired;