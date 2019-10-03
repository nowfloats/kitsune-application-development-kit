import React from 'react';
import { connect } from 'react-redux';
import { isArray, isNullOrUndefined } from 'util';
import BooleanProperty from './boolean-property';
import SelectProperty from './select-property';
import TextFieldProperty from './text-property';
// import DatePicker from 'react-datetime';

class AdvancedProperties extends React.Component {
    constructor(props) {
        super(props);

        this.state = {
            supported: []
        };

        this.propertyChanged = this.propertyChanged.bind(this);
        this.propertyToggle = this.propertyToggle.bind(this);
    }

    propertyToggle(name, { value: checked }) {
        const advancedProps = { ...(this.props.data || {}) };
        const propName = name.startsWith('toggle-') ? name.substring('toggle-'.length) : name;

        // Set the value on the prop
        advancedProps[propName] = checked ? (advancedProps[propName] || true) : null;

        if (this.props.onChange) this.props.onChange(advancedProps, 'set-value');
    }

    propertyChanged(evt, ctx) {
        const advancedProps = { ...(this.props.data || {}) };
        const { supported } = this.state;

        // If the context is set, the first argument is the property name
        const name = typeof evt === 'string' ? evt : evt.target.name;
        // Allow the component to provide a self formatted value
        let value = !!ctx ? ctx.value : evt.target.value;

        const propSupport = supported.find(prop => prop[0] === name);
        // Pick only the type definition
        const typeDef = propSupport[1]

        // Update our internal state
        if (this.isPropertyOfType(typeDef, 'array') && typeDef.creatable) {
            advancedProps[name] = JSON.stringify(ctx.options || []);
        } else if (typeDef.range) {
            advancedProps[name] = JSON.stringify(value);
        } else {
            advancedProps[name] = value;
        }

        // Inform the container of a change
        if (this.props.onChange) this.props.onChange(advancedProps, 'set-value');
    }

    componentDidMount() {
        this.mapState(this.props);
    }

    componentWillReceiveProps(props) {
        this.mapState(props);
    }

    mapState(props) {
        const { dataType, supportedTypes, isArray } = props;

        // Either the config has a set of types, or supports all if empty
        const supported = _.filter(
            Object.entries(supportedTypes),
            ([, typeDef]) => typeDef.schemaTypes.length <= 0 || _.indexOf(typeDef.schemaTypes, _.toLower(isArray ? 'array' : dataType)) >= 0
        );

        this.setState({ supported });
    }

    /**
     * Picks up the relevant properties, and maps them to an internal state,
     * making it easier to reuse, by containing functionality within this component
     */
    computeInternalState() {
        const { data } = this.props;
        const { supported } = this.state;

        // Get a list of supported advanced properties for this data type
        const advancedPropsData = { ...(data || {}) };
        const selectPropOptions = {};
        const enabledProps = [];

        // Populate the internal state, using persisted or default values
        for (let [supportedProp, typeDef] of supported) {
            const propDataValue = advancedPropsData[supportedProp];
            const hasValue = !isNullOrUndefined(propDataValue);

            // Keep track of enabled props
            if (hasValue) {
                // If we have a value of type boolean, set the value, else set the wrapper to be enabled
                enabledProps.push(`${this.isPropertyOfType(typeDef, 'boolean') ? '' : 'toggle-'}${supportedProp}`);

                if (this.isPropertyOfType(typeDef, 'array') && typeDef.creatable) {
                    // Creatable arrays save the options in the value
                    selectPropOptions[supportedProp] = JSON.parse(propDataValue);
                }
            } else {
                // No saved value, pick defaults
                advancedPropsData[supportedProp] = typeDef.value;
                // Populate options in case of arrays
            }

            if (this.isPropertyOfType(typeDef, 'array') && !typeDef.creatable) selectPropOptions[supportedProp] = typeDef.options || [];
        }

        // Set the state
        return {
            advancedProps: advancedPropsData,
            selectPropOptions,
            enabledProps
        };
    }

    isPropertyOfType(typeDef, checkType) {
        const { type } = typeDef;
        return (type === 'k-default' ? checkType === this.props.dataType : checkType === type)
    }

    shouldIncludeProperty(property, includes, excludes) {
        const isRange = !!property.range;

        let { type } = property;
        if (type === 'k-default') type = this.props.dataType;

        const include = (
            // Either no includes were provided
            !includes || includes.length <= 0 ||
            // Or the property belongs to the list
            _.indexOf(includes, type) >= 0 ||
            // Or it's a range, and we are including ranges
            (isRange && _.indexOf(includes, 'range') >= 0)
        );

        const exclude = (
            // Excludes are provided,
            !!excludes &&
            (
                // And the property belongs to the list
                _.indexOf(excludes, type) >= 0 ||
                // Or it's a range, and we are exlcuding ranges
                (isRange && _.indexOf(excludes, 'range') >= 0)
            )
        );

        // Included, and not exlcuded
        return include && !exclude;
    }

    wrapPropertyInBoolean(state, name, label, child) {
        return (
            <BooleanProperty name={`toggle-${name}`} label={label} value={state.enabledProps.indexOf(`toggle-${name}`) >= 0} onChange={this.propertyToggle}>
                {child}
            </BooleanProperty>
        )
    }

    renderAdvancedProperty(computedState, name, typeDef, handleChange) {
        const { advancedProps, selectPropOptions } = computedState;

        let { type, creatable, range, label } = typeDef;
        let value = advancedProps[name];

        if (type === 'k-default') {
            type = this.props.isArray ? 'array' : this.props.dataType;
        }
        switch (type) {
            case 'string':
                if (value === true) value = '';
                return this.wrapPropertyInBoolean(computedState, name, label, (<TextFieldProperty type='text' name={name} value={value} onChange={handleChange} />));
            case 'number':
                if (value === true) value = '';
                return this.wrapPropertyInBoolean(computedState, name, label, (<TextFieldProperty type='number' name={name} range={range} value={range ? JSON.parse(value || '{}') : value} onChange={handleChange} />));
            case 'array':
                if (value === true) value = null;
                return this.wrapPropertyInBoolean(computedState, name, label, (<SelectProperty name={name} creatable={!!creatable} options={selectPropOptions[name] || []} value={creatable ? null : value} onChange={handleChange} />));
            case 'datetime':
                return null;// this.wrapPropertyInBoolean(computedState, name, label, (<DatePicker value={!!value ? new Date(value) : ''} onChange={(val) => handleChange(name, { value: val.toUTCString() })} />));
            case 'boolean':
                return (<BooleanProperty name={name} label={label} value={value} onChange={handleChange} />);
        }

        return null;
    }

    renderSupportedTypes(computedState, { include = [], exclude = [], className = '' }) {
        const { supported: properties } = this.state;

        // Force lists to an array
        if (!isArray(include)) include = [include];
        if (!isArray(exclude)) exclude = [exclude];

        // Map all the props, exlcuding the array above
        return properties.map(([name, typeDef], i) => (this.shouldIncludeProperty(typeDef, include, exclude) &&
            <div key={`advanced-props-${this.props.dataType}-${i}`} className={`${i === 0 ? 'margin-above' : ''} ${className}`}>
                {this.renderAdvancedProperty(computedState, name, typeDef, this.propertyChanged)}
            </div>
        ));
    }

    render() {
        const computedState = this.computeInternalState();
        const { dataType } = this.props;

        return !!dataType ? (
            <details className='advanced-section'>
                <summary>Advanced Settings</summary>
                {
                    this.renderSupportedTypes(computedState, { className: 'field' })
                }
            </details>
        ) : null;
    }
}

const mapStateToProps = state => {
    return {
        supportedTypes: state.schemaCreatorReducer.supportedMetaProperties
    }
}

export default connect(mapStateToProps)(AdvancedProperties);