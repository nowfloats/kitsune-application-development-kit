import React, { Component } from 'react';
import PropTypes from 'prop-types';
import { config } from "../../../../config";

class BaseChild extends Component {
	constructor (props) {
		super(props);
		this.mapPropType = this.mapPropType.bind(this);
		this.mapStateOfNode = this.mapStateOfNode.bind(this);
		this.nodeHoverIn = this.nodeHoverIn.bind(this);
		this.nodeHoverOut = this.nodeHoverOut.bind(this);
		this.clickHandler = this.clickHandler.bind(this);
		this.doubleClickHandler = this.doubleClickHandler.bind(this);
		this.state = {
			definitionHover: false
		};
	}

	mapPropType(type, datatype) {
		const { mapSchemaType } = config.INTERNAL_SETTINGS;
		let result = { name: mapSchemaType[type], type: mapSchemaType[type] };
		const lowerDataType = datatype.toLowerCase();
		if(result.type === 'object') {
			result.name = lowerDataType;
		} else if( result.type === 'array') {
			result.name = lowerDataType === 'str' ? 'string' : (lowerDataType === 'kstring' ? 'k-string' : lowerDataType);
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
		let result = base[name] === undefined ? 'visible' : base[name];
		return result;
	}

	nodeHoverIn(e) {
		const { definitionHover } = this.state;
		if(
			(e.ctrlKey || e.metaKey)
			&& !definitionHover
		) {
			this.setState({ definitionHover: true });
		} else if(
			!e.ctrlKey
			&& !e.metaKey
			&& definitionHover
		) {
			this.setState({ definitionHover: false });
		}
	}

	nodeHoverOut() {
		const { definitionHover } = this.state;
		if(definitionHover) {
			this.setState({ definitionHover: false });
		}
	}

	clickHandler(stateOfNode, dataType) {
		const { definitionHover } = this.state;
		if(definitionHover) {
			const { openDefinition } = this.props.helpers;
			openDefinition(dataType);
		} else {
			if(stateOfNode === 'disabled') {
				console.log('select disabled node.'); //eslint-disable-line
			} else {
				console.log('select node.'); //eslint-disable-line
			}
		}
	}

	doubleClickHandler(e, stateOfNode) {
		const { readOnly } = this.props;
		if(!readOnly) {
			if(stateOfNode === 'disabled') {
				console.log('edit disabled node.'); //eslint-disable-line
			} else {
				const { parent, property } = this.props;
				const { openMetaDefinition } = this.props.helpers;
				openMetaDefinition({
					parent,
					ref: this.node,
					event: e,
					props: property,
					newDefinition: null
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
			const { definitionHover } = this.state;
			return (
				<li>
					<p className={`node-${mappedType} ${definitionHover ? 'definition-hover' : ''}`}
						ref={(node) => {this.node = node;}}
						onMouseMove={(e) => this.nodeHoverIn(e)}
						onMouseLeave={this.nodeHoverOut}
						onClick={() => this.clickHandler(stateOfNode, dataType)}
						onDoubleClick={e => this.doubleClickHandler(e, stateOfNode)}>
						<span className='class-name'>{mappedName}</span>
						<span className={isArray ? 'brackets': 'hide'}> [ ] </span>:
						<span className='node-name'>{name}</span>
					</p>
				</li>
			);
		}
	}
}

BaseChild.propTypes = {
	parent: PropTypes.string,
	property: PropTypes.object,
	helpers: PropTypes.object,
	readOnly: PropTypes.bool
};

export default BaseChild;
