import React from 'react';

export default class TextFieldProperty extends React.Component {
    constructor(props) {
        super(props);

        this.rangeChange = this.rangeChange.bind(this);
    }

    rangeChange(evt) {
        const range = { ...this.props.value };

        range[evt.target.name] = parseFloat(evt.target.value);
        this.props.onChange(this.props.name, { value: range });
    }

    render() {
        const { name, type, value, range, onChange, label } = this.props;

        return (
            <div key={`adv-prop-label-${name}`} className={`control ${range ? 'range-container' : ''}`}>
                <label key={`adv-prop-label-${name}`} className='label'>{label}</label>
                {range && <span className='range-text input-range has-text-centered'>min: </span>}
                <input name={range ? 'min' : name}
                    className={`input ${range ? 'range-min input-range' : ''}`}
                    type={range ? 'number' : type}
                    value={range ? (value.min || '') : value}
                    placeholder={range ? 'n/a' : ''}
                    onChange={range ? this.rangeChange : onChange} />
                {range && <span className='range-text input-range has-text-centered'> max: </span>}
                {range &&
                    <input name='max'
                        className='input range-max input-range'
                        type='number'
                        placeholder='n/a'
                        value={value.max || ''}
                        onChange={this.rangeChange} />
                }
            </div>
        );
    }
}