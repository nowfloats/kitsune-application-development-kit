import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { fileChanged } from "../../../actions/editor";
import { config } from "../../../config";
import SchemaBase from './base/index';
import DataType from './data-type/index';
import DataTypeDef from './datatype-definition/index';
import PropertyDetails from './property-details/index';

const validDataTypes = ({ Name }) => {
	const lowerName = Name.toLowerCase();
	const name = lowerName === 'str' ? 'string' : (lowerName === 'kstring' ? 'k-string' : lowerName);
	const { dataTypeOrderMap } = config.INTERNAL_SETTINGS;
	return {
		className: `datatype-options datatype-${name}`,
		value: name,
		label: name,
		position: dataTypeOrderMap.get(name) || 100000
	}
};

const mapMetaTypes = (datatype, isArray) => {
	let result = { type: '', datatype: '' };
	const {
		STRING,
		NUMBER,
		BOOLEAN,
		DATETIME,
		FUNCTION,
		KSTRING
	} = config.INTERNAL_SETTINGS.dataTypes;
	const switchDataType = datatype.toUpperCase().replace('-', '');
	switch (switchDataType) {
		case STRING:
			result.type = 0;
			break;
		case NUMBER:
			result.type = 2;
			break;
		case BOOLEAN:
			result.type = 3;
			break;
		case DATETIME:
			result.type = 4;
			break;
		case FUNCTION:
			result.type = 6;
			break;
		case KSTRING:
			result.type = 7;
			break;
		default:
			result.type = 5;
			break;
	}
	if (isArray) {
		result.type = 1;
	}
	result.datatype = datatype === 'string' ? 'str' : (datatype === 'k-string' ? 'kstring' : datatype);
	return result;
};

class SchemaCreator extends Component {
	constructor(props) {
		super(props);
		this.openDefinition = this.openDefinition.bind(this);
		this.closeDefinition = this.closeDefinition.bind(this);
		this.openNewDefinition = this.openNewDefinition.bind(this);
		this.createNewDefinition = this.createNewDefinition.bind(this);
		this.openMetaDefinition = this.openMetaDefinition.bind(this);
		this.closeMetaDefinition = this.closeMetaDefinition.bind(this);
		this.editMetaDefinition = this.editMetaDefinition.bind(this);
		this.createMetaDefinition = this.createMetaDefinition.bind(this);
		this.compareInitialDefinition = this.compareInitialDefinition.bind(this);
		this.deleteMetaDefinition = this.deleteMetaDefinition.bind(this);
		this.helpers = {
			openDefinition: this.openDefinition,
			closeDefinition: this.closeDefinition,
			openNewDefinition: this.openNewDefinition,
			createNewDefinition: this.createNewDefinition,
			openMetaDefinition: this.openMetaDefinition,
			closeMetaDefinition: this.closeMetaDefinition,
			createMetaDefinition: this.createMetaDefinition,
			editMetaDefinition: this.editMetaDefinition,
			deleteMetaDefinition: this.deleteMetaDefinition,
		};
		this.state = {
			definition: {
				isOpen: false,
				datatype: null,
				editable: true,
				isNew: false
			},
			meta: {
				parent: null,
				isOpen: false,
				style: {
					left: 0,
					top: 0
				},
				isCreate: false,
				properties: null,
				NewDefinition: false,
				customClass: ''
			},
			autoOpenDataType: false
		};
	}

	compareInitialDefinition() {
		const { schemaClasses, fileChanged: isFileChanged, isChanged } = this.props;
		isChanged({
			classes: schemaClasses,
			isChanged: isFileChanged
		});
	}

	/**
	 * The base definition of a property
	 */
	baseProperty({ mapType, props, isFormattable }) {
		return {
			DataType: { Id: null, Name: mapType.datatype },
			Description: props.description,
			EnableSearch: false,
			IsRequired: props.isRequired,
			Name: props.name,
			GroupName: props.groupName,
			PropertyCode: 0,
			Schemas: null,
			Type: mapType.type,
			isNew: true,
			// Derived + advanced properties
			_AdvanceProperties: { ...(props.advancedProps || {}), RichText: !!isFormattable }
		};
	}

	createMetaDefinition(parent, props, isArray, isFormattable, newDef) {
		const { schemaClasses } = this.props;
		const currentClass = newDef === null ? _.filter(schemaClasses, ({ Name }) => Name === parent)[0] : newDef;
		const mapType = mapMetaTypes(props.selectedDataType, isArray);
		// Base schema for props
		currentClass.PropertyList.push(this.baseProperty({ mapType, props, isFormattable }));
		this.closeMetaDefinition(false);
	}

	editMetaDefinition({
		parent,
		props,
		isArray,
		isFormattable,
		newDef
	}) {
		const { schemaClasses } = this.props;
		const currentClass = newDef === null ? _.filter(schemaClasses, ({ Name }) => Name === parent)[0] : newDef;
		const mapType = mapMetaTypes(props.selectedDataType, isArray);
		const currentProp = _.find(currentClass.PropertyList, { 'Name': props.oldName });
		currentProp.Name = props.name;
		currentProp.GroupName = props.groupName;
		currentProp.Description = props.description;
		currentProp.DataType.Name = mapType.datatype;
		currentProp.IsRequired = currentProp.isRequired;
		currentProp.Type = mapType.type;
		currentProp._AdvanceProperties = { ...(props.advancedProps || {}), RichText: !!isFormattable };
		this.closeMetaDefinition(false);
	}

	deleteMetaDefinition({
		parent,
		props,
		newDef
	}) {
		const { schemaClasses } = this.props;
		const currentClass = newDef === null ? _.filter(schemaClasses, ({ Name }) => Name === parent)[0] : newDef;
		currentClass.PropertyList = _.reject(currentClass.PropertyList, { 'Name': props.oldName });
		this.closeMetaDefinition(false);
	}

	openMetaDefinition({
		parent,
		ref,
		event,
		props,
		newDefinition
	}) {
		let { left, width } = ref.getBoundingClientRect();
		let viewPortHeight = window.innerHeight;
		// Calculate half of viewport and make a range in middle with 10% of viewport height
		let middleViewPort = viewPortHeight / 2;
		let middleOffset = Math.ceil(viewPortHeight * .15);
		let middleStart = middleViewPort - middleOffset;
		let middleEnd = middleViewPort + middleOffset;
		let elBottom = viewPortHeight - event.pageY;
		let newLeft = left + width + 15;
		// let metaMaxHeight = 520;
		let meta = {
			parent: parent,
			isOpen: true,
			style: {
				left: newLeft,
			},
			isCreate: props === null,
			properties: props === null ? null : props,
			NewDefinition: newDefinition,
		};

		//check where the element clicked is on the page
		if (event.pageY < middleStart) {
			let newTop = event.pageY - 100;
			//element is near the top
			meta.style.top = newTop;
			meta.customClass = 'near-top';
		} else if (event.pageY > middleEnd) {
			//element is near the bottom
			let newBottom = elBottom - 30;
			meta.style.bottom = newBottom;
			meta.customClass = 'near-bottom';
		} else {
			//element is right in middle
			let newTop = event.pageY - 203;
			//element is near the top
			meta.style.top = newTop;
			meta.customClass = 'near-middle';
		}
		this.setState({ meta, autoOpenDataType: false });
	}

	closeMetaDefinition(openDataType = false) {
		let meta = {
			parent: null,
			isOpen: false,
			style: {
				left: 0,
				top: 0
			}
		};
		this.setState({ meta, autoOpenDataType: openDataType });
		this.compareInitialDefinition();
	}

	openDefinition(datatype) {
		const lowerDataType = datatype.toLowerCase();
		datatype = lowerDataType === 'string' ? 'str' : (lowerDataType === 'k-string' ? 'kstring' : lowerDataType);
		const { schemaClasses } = this.props;
		let filteredDefinition = schemaClasses.filter(({ Name }) => Name.toLowerCase() === datatype);
		const [definitionToOpen] = filteredDefinition;
		const definition = {
			isOpen: true,
			datatype: definitionToOpen,
			editable: (definitionToOpen.ClassType === 2 || definitionToOpen.ClassType === 3) ? false : true,
			isNew: false
		};
		this.setState({ definition, autoOpenDataType: false });
	}

	closeDefinition() {
		const definition = {
			isOpen: false,
			datatype: null,
			editable: false,
			isNew: false
		};
		this.setState({ definition, autoOpenDataType: false });
		this.compareInitialDefinition();
	}

	openNewDefinition(datatype) {
		const definition = {
			isOpen: true,
			datatype: datatype,
			editable: true,
			isNew: true
		};
		this.setState({ definition, autoOpenDataType: false });
	}

	createNewDefinition(definition) {
		const { schemaClasses } = this.props;
		schemaClasses.push(definition);
		this.closeDefinition();
	}

	render() {
		const {
			parentWidth,
			schemaClasses,
			isFooterActive,
			parentHeight,
			hasWebsiteData,
			readOnly
		} = this.props;
		const {
			definition,
			meta,
			autoOpenDataType
		} = this.state;
		if (!_.endsWith(parentHeight.height, ' - 30px)')) { // 30px is the height of tab
			parentHeight.height = _.replace(parentHeight.height, ')', ' - 30px)');
		}
		let options = {
			datatype: _.chain(schemaClasses)
				.filter(({ ClassType, Name }) => (ClassType !== 1 && Name.toUpperCase() !== 'ARRAY'))
				.orderBy(['ClassType'], ['asc'])
				.map(validDataTypes)
				.sortBy(['position'])
				.reverse()
				.value(),
			array: null
		};
		options.array = _.map(options.datatype, iterator => ({
			...iterator,
			label: iterator.label.concat('[]')
		}));
		return (
			<div className='schema-creator' ref={container => { this.container = container }}>
				<DataType baseClasses={schemaClasses}
					definition={definition}
					helpers={this.helpers}
					autoOpen={autoOpenDataType}
					readOnly={readOnly} />
				<SchemaBase baseClasses={schemaClasses}
					helpers={this.helpers}
					readOnly={readOnly} />
				<DataTypeDef parentWidth={parentWidth}
					baseClasses={schemaClasses}
					definition={definition}
					meta={meta}
					helpers={this.helpers}
					isFooterActive={isFooterActive}
					parentHeight={parentHeight}
					readOnly={readOnly} />
				<PropertyDetails definition={definition}
					parentWidth={parentWidth}
					baseClasses={schemaClasses}
					meta={meta}
					helpers={this.helpers}
					options={options}
					isFooterActive={isFooterActive}
					hasWebsiteData={hasWebsiteData}
					parentHeight={parentHeight} />
			</div>
		);
	}
}

SchemaCreator.propTypes = {
	schemaClasses: PropTypes.array,
	fileChanged: PropTypes.bool,
	isChanged: PropTypes.func,
	parentWidth: PropTypes.object,
	parentHeight: PropTypes.object,
	readOnly: PropTypes.bool,
	isFooterActive: PropTypes.string
};

const mapDispatchToProps = dispatch => ({
	isChanged: ({
		classes,
		isChanged
	}) => {
		let initSchemaDetails = JSON.parse(localStorage.getItem('initSchemaDetails'));
		let isSame = _.isEqual(classes, initSchemaDetails);
		if (!isSame && !isChanged) {
			dispatch(fileChanged(true));
		} else if (isSame && isChanged) {
			dispatch(fileChanged(false));
		}
	}
});

const mapStateToProps = state => {
	const { activeTabs, visibleIndex } = state.editorReducer;
	const editor = activeTabs[visibleIndex];
	return {
		hasWebsiteData: state.schemaCreatorReducer.hasAssociatedWebsiteData,
		schemaClasses: state.schemaCreatorReducer.schemaDetails.Classes,
		fileChanged: editor ? editor.fileChanged : false,
		isFooterActive: state.footerReducer.isActive
	}
};

export default connect(mapStateToProps, mapDispatchToProps)(SchemaCreator);
