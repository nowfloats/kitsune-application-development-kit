import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import DeleteIcon from '../../../../images/delete.svg';
import PropertyDataType from './data-type';
import PropertyGroup from './property-group';
import PropertyName from './property-name';
import PropertyInfo from './property-info';
import IsRequired from './is-required';
import AdvancedProperties from './advanced';
import { modalOpen, modalClose } from '../../../../actions/modal';
import PromptMessage, { promptMessageLabel } from "../../../modals/prompt/index";
import { config } from '../../../../config';
import { connect } from 'react-redux';

class PropertyDetails extends Component {
	constructor(props) {
		super(props);
		this.handleInputChange = this.handleInputChange.bind(this);
		this.handleDataType = this.handleDataType.bind(this);
		this.handleArrayCheckbox = this.handleArrayCheckbox.bind(this);
		this.handleFormatCheckbox = this.handleFormatCheckbox.bind(this);
		this.createNew = this.createNew.bind(this);
		this.validateAll = this.validateAll.bind(this);
		this.isPropUnique = this.isPropUnique.bind(this);
		this.handleKeyTrigger = this.handleKeyTrigger.bind(this);
		this.renderFooter = this.renderFooter.bind(this);
		this.editCurrent = this.editCurrent.bind(this);
		this.deleteCurrent = this.deleteCurrent.bind(this);
		this.handleAdvancedProp = this.handleAdvancedProp.bind(this);

		this.state = {
			propertyDetails: {
				oldName: '',
				name: '',
				groupName: '',
				description: '',
				isRequired: false,
				selectedDataType: '',
				advancedProps: null
			},
			isArray: false,
			propNameError: null,
			isFormattable: false,
			isString: false,
			isExisting: false,
			isDisabled: true,     // is button disable 
			isUnique: true
		};
	}

	validateAll() {
		const { selectedDataType, name } = this.state.propertyDetails;
		const { propNameError } = this.state;
		this.setState({
			isDisabled: (propNameError !== null || selectedDataType === '' || name === '')
		});
	}

	addAnimate() {
		document.getElementById('property-details').classList.add("shakes");
		setTimeout(() => {
			document.getElementById('property-details').classList.remove("shakes");
		}, 1000);
	}

	handleDataType({ value }) {
		let { propertyDetails, isFormattable } = this.state;
		const { closeMetaDefinition } = this.props.helpers;
		propertyDetails.selectedDataType = value;
		const isString = (value.toUpperCase() === 'STRING' || value.toUpperCase() === 'K-STRING');
		const isFormattableNew = isString ? isFormattable : false;
		this.setState({ propertyDetails, isString, isFormattable: isFormattableNew }, this.validateAll);

		if (value === 'create-data-type') {
			closeMetaDefinition(true);
		}
	}

	handleAdvancedProp(props, action) {
		if (action === 'set-value') {
			const { propertyDetails } = this.state;
			propertyDetails.advancedProps = { ...props };
			this.setState({ propertyDetails }, this.validateAll);
		}
	}

	isPropUnique(propName) {
		const { parent, NewDefinition } = this.props.meta;
		const { baseClasses } = this.props;
		const currentClass = NewDefinition === null ? _.filter(baseClasses, ({ Name }) => Name === parent)[0] :
			NewDefinition;
		const isUnique = _.find(currentClass.PropertyList, { 'Name': _.trim(propName) }) === undefined;
		return isUnique;
	}

	handleInputChange({ target }) {
		let { propertyDetails } = this.state;
		let value = target.type === 'checkbox' ? target.checked : target.value;
		const name = target.name;
		if (name === 'name') {
			value = _.replace(value, ' ', '');
			const isUnique = this.isPropUnique(value);
			const isNameValid = /^(?=(?!_)[\w\d\_]{3})[\w\d\_]*$/.test(value);
			let nameError = null;
			if (!isUnique) {
				nameError = 'this property name already exists';
			} else if (!isNameValid) {
				if (value.length < 3) {
					nameError = 'the name of property should be atleast 3 characters';
				} else if (value.startsWith('_')) {
					nameError = 'the name of property should not start with _';
				} else if (!/^[\w\d\_]+$/.test(value)) {
					nameError = 'the name can only have alphanumerics and _';
				}
			}
			propertyDetails[name] = value;
			this.setState({ propertyDetails, isUnique: isUnique, propNameError: nameError }, this.validateAll);
		} else {
			propertyDetails[name] = value;
			this.setState({ propertyDetails }, this.validateAll);
		}
	}

	handleKeyTrigger({ keyCode }) {
		const { isDisabled } = this.state;

		if (keyCode === 13) {
			isDisabled ? null : this.createButton.click();
		} else if (keyCode === 27) {
			// when user press Esc key
			isDisabled ? this.cancelButton.click() : this.addAnimate();
		} else if (keyCode === 46) {
			this.deleteButton.click();
		}
	}

	handleArrayCheckbox({ target }) {
		this.setState({ isArray: target.checked }, this.validateAll);
	}

	handleFormatCheckbox({ target }) {
		this.setState({ isFormattable: target.checked }, this.validateAll);
	}

	createNew() {
		const { helpers, meta } = this.props;
		const { propertyDetails, isArray, isFormattable } = this.state;
		const { createMetaDefinition } = helpers;
		const { parent, NewDefinition } = meta;
		createMetaDefinition(parent, propertyDetails, isArray, isFormattable, NewDefinition);
	}

	editCurrent() {
		const { helpers, meta } = this.props;
		const { propertyDetails, isArray, isFormattable } = this.state;
		const { editMetaDefinition } = helpers;
		const { parent, NewDefinition } = meta;
		editMetaDefinition({
			parent,
			props: propertyDetails,
			isArray,
			isFormattable,
			newDef: NewDefinition
		});
	}

	deleteCurrent() {
		const { showModal, closeModal, hasWebsiteData, helpers, meta } = this.props;
		const { propertyDetails } = this.state;
		const { deleteMetaDefinition } = helpers;
		const { parent, NewDefinition } = meta;

		if (hasWebsiteData) {
			showModal(
				<PromptMessage promptItem={config.INTERNAL_SETTINGS.promptMessages.DELETE_PROPERTY.name} />,
				promptMessageLabel, () => {
					deleteMetaDefinition({
						parent,
						props: propertyDetails,
						newDef: NewDefinition
					});
					closeModal();
				})
		} else {
			deleteMetaDefinition({
				parent,
				props: propertyDetails,
				newDef: NewDefinition
			});
		}
	}

	componentWillReceiveProps({ meta }) {
		if (this.props.meta.isOpen && !meta.isOpen) {
			document.removeEventListener('keydown', this.handleKeyTrigger, false);
			const propertyDetails = {
				oldName: '',
				name: '',
				groupName: '',
				description: '',
				isRequired: false,
				selectedDataType: '',
			};
			this.setState({
				propertyDetails,
				isArray: false,
				isString: false,
				isFormattable: false,
				isExisting: false,
				isDisabled: true,
				isUnique: true,
				advancedProps: null
			});
		} else if (!this.props.meta.isOpen && meta.isOpen) {
			if (meta.properties !== null) {
				let isArray = meta.properties.DataType.Name.toUpperCase() === 'ARRAY' || meta.properties.Type === 1;
				const propertyDetails = {
					oldName: meta.properties.Name,
					name: meta.properties.Name,
					groupName: meta.properties.GroupName === null ? '' : meta.properties.GroupName,
					description: meta.properties.Description,
					isRequired: meta.properties.IsRequired,
					selectedDataType: meta.properties.DataType.Name.toLowerCase() === 'str' ?
						'string' : meta.properties.DataType.Name.toLowerCase(),
					advancedProps: meta.properties._AdvanceProperties || {}
				};
				const isString = (propertyDetails.selectedDataType.toUpperCase() === 'STRING' ||
					propertyDetails.selectedDataType.toUpperCase() === 'K-STRING');
				const isFormattable = (meta.properties._AdvanceProperties && meta.properties._AdvanceProperties.RichText);
				this.setState({
					propertyDetails,
					isArray: isArray,
					isString,
					isFormattable,
					isExisting: meta.properties.isNew === undefined,
					isDisabled: true,
					isUnique: true
				});
			}
			document.addEventListener('keydown', this.handleKeyTrigger, false);
		}
	}

	renderFooter() {
		const { isDisabled } = this.state;
		const { isCreate } = this.props.meta;
		const { closeMetaDefinition } = this.props.helpers;
		return (
			<div className={`property-details-footer${isCreate ? '' : ' is-editing'}`}>
				{
					isCreate ? null :
						<div>
							<a className='button delete-button' ref={del => this.deleteButton = del}
								onClick={this.deleteCurrent} title='delete'>
								<img src={DeleteIcon} />
							</a>
						</div>
				}
				<div>
					<a className='button' ref={(cancel) => { this.cancelButton = cancel; }}
						onClick={() => closeMetaDefinition()}>cancel</a>
					<a className='button is-primary'
						ref={create => this.createButton = create}
						onClick={isDisabled ? null : (isCreate ? this.createNew : this.editCurrent)}
						disabled={isDisabled}>{isCreate ? 'create' : 'save'}</a>
				</div>
			</div>
		);
	}

	render() {
		const { meta, options, parentWidth, parentHeight, definition, isFooterActive } = this.props;
		const { isOpen: definitionIsOpen } = definition;
		const { propertyDetails, propNameError, isExisting: isExisting, isArray, isString, isFormattable } = this.state;
		const { name, groupName, description, isRequired, selectedDataType, advancedProps } = propertyDetails;
		const { isOpen, style, isCreate, customClass, properties } = meta;
		const { datatype, array } = options;
		const overlayClass = isFooterActive ? 'property-details-overlay footer-active' : 'property-details-overlay';
		// tweaking the data types to render on the property details
		let dataTypeReverse = datatype.reverse();
		let arrayReverse = array.reverse();
		const createDataType = { className: "data-options", value: "create-data-type", label: "+ Create data type" };
		const title = (isCreate ? 'Create new' : 'Edit');
		if (dataTypeReverse.filter(item => (item.value === "create-data-type")).length < 1 && !definitionIsOpen)
			dataTypeReverse.push(createDataType);
		if (isOpen) {
			return (
				<div className={overlayClass} style={{ ...parentWidth, ...parentHeight }}>
					<form id='property-details' className={`property-details ${customClass}`} style={style}>
						<h3 className='property-details-header'>{title} property</h3>
						<div className='property-details-body'>
							<PropertyDataType className='field' {...{ selectedDataType, options: isArray ? arrayReverse : dataTypeReverse, isExisting, isString, isArray }} onChange={this.handleDataType} handleArray={this.handleArrayCheckbox} handleFormat={this.handleFormatCheckbox} />
							<PropertyName className='field' {...{ name, isExisting, propNameError }} onChange={this.handleInputChange} />
							<PropertyGroup className='field' {...{ groupName }} onChange={this.handleInputChange} />
							<PropertyInfo className='field' {...{ description }} onChange={this.handleInputChange} />
							<IsRequired className='field custom-field hide' {...{ isRequired, isExisting }} onChange={this.handleInputChange} />
							<AdvancedProperties data={advancedProps} dataType={selectedDataType} isArray={isArray} onChange={this.handleAdvancedProp} />
						</div>
						{this.renderFooter()}
					</form>
				</div>
			);
		} else {
			return null;
		}
	}
}

PropertyDetails.propTypes = {
	definition: PropTypes.object,
	baseClasses: PropTypes.array,
	helpers: PropTypes.object,
	meta: PropTypes.object,
	options: PropTypes.object,
	parentHeight: PropTypes.object,
	parentWidth: PropTypes.object,
	isFooterActive: PropTypes.string
};

const mapDispatchToProps = dispatch => ({
	showModal: (item, label, callback) => dispatch(modalOpen(item, label, callback)),
	closeModal: () => dispatch(modalClose())
});

export default connect(null, mapDispatchToProps)(PropertyDetails);
