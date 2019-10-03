import React, { Component } from 'react';
import PropTypes from 'prop-types';
import { config } from "../../../../config";

class DatatypeChild extends Component {
	constructor (props) {
		super(props);
		this.mapPropType = this.mapPropType.bind(this);
		this.mapStateOfNode = this.mapStateOfNode.bind(this);
		this.clickHandler = this.clickHandler.bind(this);
		this.doubleClickHandler = this.doubleClickHandler.bind(this);
	}

	mapPropType(type, datatype) {
		const { mapSchemaType } = config.INTERNAL_SETTINGS;
		let result = { name: mapSchemaType[type], type: mapSchemaType[type] };
		if(result.type === 'object') {
			result.name = datatype.toLowerCase();
		} else if( result.type === 'array') {
			result.name = datatype.toLowerCase();
			if(
				result.name === 'string'
				|| result.name === 'number'
				|| result.name === 'boolean'
				|| result.name === 'datetime'
			) {
				result.type = `array node-${result.name}`;
			} else {
				result.type = 'array node-object';
			}
		}
		return result;
	}

	mapStateOfNode(name) {
		const { base } = config.INTERNAL_SETTINGS.schemaConfig;
		const { editable } = this.props;
		let result = base[name] === undefined ? 'visible' : base[name];
		if(result !== 'hidden' && !editable) {
			result = 'disabled';
		}
		return result;
	}

	clickHandler(stateOfNode) {
		if(stateOfNode === 'disabled') {
			console.log('select disabled node.'); //eslint-disable-line
		} else {
			console.log('select node.'); //eslint-disable-line
		}
	}

	doubleClickHandler(e, stateOfNode) {
		const { readOnly } = this.props;
		if(!readOnly) {
			if(stateOfNode === 'disabled') {
				console.log('edit disabled node.'); //eslint-disable-line
			} else {
				const { parent, property, isNew, datatype } = this.props;
				const { openMetaDefinition } = this.props.helpers;
				openMetaDefinition({
					parent,
					ref: this.node,
					event: e,
					props: property,
					newDefinition: isNew ? datatype : null
				});
			}
		}
	}

	render() {
		const { property } = this.props;
		const { Name: name, Type: type, DataType } = property;
		const { Name: dataType } = DataType;
		const { name: mappedName, type: mappedType } = this.mapPropType(type, dataType);
		const isArray = mappedType.split(' ')[0] === 'array';
		const stateOfNode = this.mapStateOfNode(name);

		if(stateOfNode === 'hidden') {
			return null;
		} else {
			return (
				<li>
					<p className={`node-${mappedType}`}
						ref={(node) => {this.node = node;}}
						onClick={() => this.clickHandler(stateOfNode)}
						onDoubleClick={(e) => this.doubleClickHandler(e, stateOfNode)}>
						<span className='class-name'>{mappedName}</span>
						<span className={isArray ? 'brackets': 'hide'}> [ ] </span>:
						<span className='node-name'>{name}</span>
					</p>
				</li>
			);
		}
	}
}

DatatypeChild.propTypes = {
	property: PropTypes.object,
	editable: PropTypes.bool,
	parent: PropTypes.string,
	helpers: PropTypes.object,
	isNew: PropTypes.bool,
	readOnly: PropTypes.bool,
	datatype: PropTypes.object
};

export default DatatypeChild;
