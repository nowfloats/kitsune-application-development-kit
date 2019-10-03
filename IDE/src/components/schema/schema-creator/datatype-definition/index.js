import React, { Component } from 'react';
import PropTypes from 'prop-types';
import DatatypeGroup from "./group";
import { config } from '../../../../config';
import _ from "lodash";
import { SortPropertyByGroupName } from "../../../../helpers/schema-creator";

class DataTypeDef extends Component {
	constructor (props) {
		super(props);
		this.renderGroups= this.renderGroups.bind(this);
		this.mapDatatype = this.mapDatatype.bind(this);
		this.addNodeHandler = this.addNodeHandler.bind(this);
		this.cancelHandler = this.cancelHandler.bind(this);
		this.createDefinition = this.createDefinition.bind(this);
		this.handleKeyTrigger = this.handleKeyTrigger.bind(this);
		this.mapStateOfNode = this.mapStateOfNode.bind(this);
		localStorage.setItem('initialDefinition', '');
		this.state = {
			update: false,
		};
	}

	mapStateOfNode(name) {
		const { base } = config.INTERNAL_SETTINGS.schemaConfig;
		return base[name] ? base[name] : 'visible';
	}

	addNodeHandler(e, parent) {
		const { openMetaDefinition } = this.props.helpers;
		const { isNew, datatype } = this.props.definition;
		openMetaDefinition({
			parent,
			ref: this.addNode,
			event: e,
			props: null,
			newDefinition: isNew ? datatype : null
		});
	}

	cancelHandler() {
		const { definition, helpers } = this.props;
		const { closeDefinition } = helpers;
		const { datatype } = definition;
		let initialDef = JSON.parse(localStorage.getItem('initialDefinition'));
		datatype.PropertyList = initialDef.PropertyList;
		closeDefinition();
	}

	handleKeyTrigger(e) {
		const { isNew } = this.props.definition;
		if(e.keyCode === 13) {
			if(isNew) {
				this.createButton.click();
			} else {
				this.okButton.click();
			}
		} else if(e.keyCode === 27) {
			this.cancelButton.click();
		}
	}

	componentWillReceiveProps(nextProps) {
		const { datatype, isOpen } = nextProps.definition;
		const { prevIsOpen } = this.props.definition;
		const { isOpen: metaIsOpen } = nextProps.meta;
		const { isOpen: prevMetaIsOpen } = this.props.meta;
		if(!prevIsOpen && isOpen) {
			document.addEventListener('keydown', this.handleKeyTrigger, false);
		} else if(prevIsOpen && !isOpen) {
			document.removeEventListener('keydown', this.handleKeyTrigger, false);
		}
		if(isOpen && !prevMetaIsOpen && metaIsOpen) {
			document.removeEventListener('keydown', this.handleKeyTrigger, false);
		}
		let initialDef = localStorage.getItem('initialDefinition');
		if(datatype !== null && initialDef === '') {
			localStorage.setItem('initialDefinition', JSON.stringify(datatype));
		} else if (datatype === null) {
			localStorage.setItem('initialDefinition', '');
		}
	}

	createDefinition() {
		const { definition, helpers } = this.props;
		const { createNewDefinition } = helpers;
		const { datatype } = definition;
		createNewDefinition(datatype);
	}

	renderGroups() {
		const { helpers, definition, readOnly } = this.props;
		const { datatype, editable, isNew } = definition;
		if(datatype !== null) {
			const { Name, PropertyList } = datatype;
			if(PropertyList.length) {
				const groupedPropertyList = _.chain(PropertyList)
					.reject(({ Name }) => {
						const stateOfNode = this.mapStateOfNode(Name);
						return stateOfNode === 'hidden';
					})
					.groupBy('GroupName')
					.reduce((result, value, key) => {
						const target = key && key !== 'null' ? key : 'unnamed';
						result[target] ? result[target].push(...value) :
							result[target] = value;
						return result;
					}, {})
					.toPairs()
					.value();
				const unnamedIndex = _.findIndex(groupedPropertyList, ([ name ]) => name === 'unnamed');
				let sortedGroups;
				if(unnamedIndex !==  -1) {
					const unnamedGroup = groupedPropertyList[unnamedIndex];
					const groupsWithoutUnnamed = unnamedIndex === 0 ? _.slice(groupedPropertyList, 1) :
						(unnamedIndex === groupedPropertyList.length -1 ?
								_.slice(groupedPropertyList, 0, groupedPropertyList.length - 1) :
						[
							..._.slice(groupedPropertyList, 0, unnamedIndex),
							..._.slice(groupedPropertyList, unnamedIndex+1)
						]
						);
					sortedGroups = [
						...groupsWithoutUnnamed.sort(SortPropertyByGroupName),
						unnamedGroup
					];
				} else {
					sortedGroups = groupedPropertyList.sort(SortPropertyByGroupName)
				}
				return (
					<li>
						<p>{Name === 'str' ? 'string' : (Name === 'kstring' ? 'k-string' : Name)}</p>
						<ul>
							{
								_.map(sortedGroups, ([ groupName, groupProps ], index) =>
									<DatatypeGroup parent={Name}
										key={index}
										editable={editable}
										isNew={isNew}
										groupProps={groupProps}
										groupName={groupName}
										helpers={helpers}
										readOnly={readOnly} />)
							}
							{
								editable && !readOnly ?
									<li className='add-node'>
										<p ref={addNode => this.addNode = addNode} onClick={e => this.addNodeHandler(e, Name)}>+</p>
									</li> :
									null
							}
						</ul>
					</li>
				);
			} else {
				return (
					<li>
						<p>{Name === 'str' ? 'string' : (Name === 'kstring' ? 'k-string' : Name)}</p>
						{
							editable && !readOnly ?
								<ul>
									<li className='add-node'>
										<p ref={addNode => this.addNode = addNode} onClick={e => this.addNodeHandler(e, Name)}>+</p>
									</li>
								</ul> :
								null
						}
					</li>
				);
			}
		} else {
			return null;
		}
	}

	mapDatatype() {
		const { datatype } = this.props.definition;
		if(datatype === null) {
			return '';
		} else {
			const { mapDataType } = config.INTERNAL_SETTINGS;
			return mapDataType[datatype.ClassType];
		}
	}

	render() {
		const { definition, helpers, parentWidth, isFooterActive, parentHeight, readOnly } = this.props;
		const { isOpen, editable, isNew } = definition;
		const { closeDefinition } = helpers;
		let containerClass = isOpen ? ( isFooterActive ? 'datatype-definition-container footer-active' :
			'datatype-definition-container') : 'datatype-definition-container hide';
		return (
			<div className={containerClass} style={{ ...parentWidth, ...parentHeight }}>
				<div className='definition-overlay' />
				<div className='datatype-definition'>
					<ul>
						<p>{this.mapDatatype()} datatype</p>
						{this.renderGroups()}
					</ul>
				</div>
				<div className='definition-footer'>
					<button className={isNew? 'hide': ''} ref={(node) => this.okButton = node}
						onClick={closeDefinition}>ok</button>
					<button className={isNew? '': 'hide'} ref={(node) => this.createButton = node}
						onClick={this.createDefinition}>create</button>
					<button className={editable && !readOnly ? '' : 'hide'} ref={(node) => this.cancelButton = node}
						onClick={this.cancelHandler}>cancel</button>
					<p className={editable && !readOnly? 'hide' : 'info-msg'}>
						this datatype is system defined and cannot be edited.
					</p>
				</div>
			</div>
		);
	}
}

DataTypeDef.propTypes = {
	helpers: PropTypes.object,
	definition: PropTypes.object,
	parentWidth: PropTypes.object,
	meta: PropTypes.object,
	parentHeight: PropTypes.object,
	readOnly: PropTypes.bool,
	isFooterActive: PropTypes.string
};

export default DataTypeDef;
