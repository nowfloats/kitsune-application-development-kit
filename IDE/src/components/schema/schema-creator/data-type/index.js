import React, { Component } from 'react';
import PropTypes from 'prop-types';
import closeIcon from '../../../../images/close.svg';
import { config } from "../../../../config";
import _ from 'lodash';

class DataType extends Component {
	constructor (props) {
		super(props);
		this.toggleCreate = this.toggleCreate.bind(this);
		this.toggleDataTypes = this.toggleDataTypes.bind(this);
		this.dataTypeNameChange = this.dataTypeNameChange.bind(this);
		this.renderList = this.renderList.bind(this);
		this.openDataType = this.openDataType.bind(this);
		this.newDataType = this.newDataType.bind(this);
		this.validateDataType = this.validateDataType.bind(this);
		this.handleCreateMouse = this.handleCreateMouse.bind(this);
		this.handleCreateKeyDown = this.handleCreateKeyDown.bind(this);
		this.state = {
			active: {
				component: true,
				create: false,
				createTrigger: false
			},
			newName: '',
			hasError: false,
			hasWarning: false
		};
	}

	handleCreateMouse({ target }) {
		const { newName } = this.state;
		if(this.createInput !== target && newName === '')
			this.toggleCreate();
	}

	handleCreateKeyDown({ keyCode }) {
		if(keyCode === 27)
			this.toggleCreate();
	}

	toggleCreate() {
		let { active } = this.state;
		active.create = !active.create;
		if(active.create) {
			document.addEventListener('keydown', this.handleCreateKeyDown, false);
			document.addEventListener('mousedown', this.handleCreateMouse, false);
		} else {
			document.removeEventListener('keydown', this.handleCreateKeyDown);
			document.removeEventListener('mousedown', this.handleCreateMouse);
		}
		this.setState({ active, hasError: false }, () => {
			if(active.create) {
				this.createInput.focus();
			}
		});
	}

	toggleDataTypes() {
		const { isOpen } = this.props.definition;
		if(!isOpen) {
			let { active } = this.state;
			active.component = !active.component;
			if(!active.component) {
				active.create = false;
				this.setState({
					active: active,
					newName: '',
					hasError: false
				});
			} else {
				this.setState({ active: active });
			}
		}
	}

	openDataType(datatype) {
		const { openDefinition } = this.props.helpers;
		openDefinition(datatype);
	}

	validLenghDataType(newName) {
		return _.trim(newName).length > 3;
	}
	validateDataType(newName) {
		const { baseClasses } = this.props;
		const isUnique = _.find(baseClasses,(i) => { return i.Name.toUpperCase() === newName.toUpperCase() }) === undefined
		? true : false;
		return isUnique;
	}

	newDataType() {
		const { openNewDefinition } = this.props.helpers;
		const { newName } = this.state;
		const newDataType = {
			Name: newName,
			Schemas: null,
			Description: null,
			ClassType: 0,
			PropertyList: []
		};
		openNewDefinition(newDataType);
	}

	dataTypeNameChange(event) {
		const { active } = this.state;
		const value = _.trim(event.target.value);
		const isValidLength = this.validLenghDataType(value);
		const isValid = this.validateDataType(value);
		active.createTrigger = isValid && isValidLength ? true : false;
		this.setState({
			active: active,
			newName: value,
			hasError: !isValid,
			hasWarning: !isValidLength
		});
	}

	componentWillReceiveProps(nextProps) {
		const{ autoOpen } = nextProps;
		const { isOpen } = nextProps.definition;
		let { active } = this.state;
		if(isOpen && active.component) {
			active = {
				component: false,
				create: false,
				createTrigger: false
			};
			this.setState({ active, newName: '', hasError: false });
		}

		if(autoOpen) {
			document.addEventListener('keydown', this.handleCreateKeyDown, false);
			document.addEventListener('mousedown', this.handleCreateMouse, false);
			active = {
				component: true,
				create: true,
				createTrigger: false
			};
			this.setState({ active },  () => {
				this.createInput.focus();
			});
		}
	}

	renderList() {
		const { baseClasses } = this.props;
		const { dataTypeOrderMap } = config.INTERNAL_SETTINGS;
		const baseClassesMapped = baseClasses.map((iterator) => {
			const lowerName = iterator.Name.toLowerCase();
			const name = lowerName === 'str' ? 'string' : (lowerName === 'kstring' ? 'k-string' : lowerName);
			return {
				...iterator,
				position: dataTypeOrderMap.get(name)
			}
		});
		const sorted = _.sortBy(baseClassesMapped, ['position']);
		return sorted.map((baseClass, index) => {
			const { ClassType: type, Name: name } = baseClass;
			if(type !== 1) {
				const lowerName = name.toLowerCase();
				const conditionalName = lowerName === 'str' ? 'string' : (lowerName === 'kstring' ? 'k-string' : lowerName);
				return (
					<li key={index}
						className={conditionalName}
						onClick={() => this.openDataType(conditionalName)}>
						<span>{conditionalName}</span>
					</li>
				);
			}
		});
	}

	render() {
		const { active, newName, hasError, hasWarning } = this.state;
		const { component, create, createTrigger } = active;
		const { readOnly } = this.props;
		const { isOpen } = this.props.definition;
		return (
			<div className='data-type-container'>
				<button className={component ? 'data-type-show animate' : `data-type-show ${isOpen ? 'disabled' : ''}`}
					aria-label={isOpen ? 'unavailable while editing a data type' : 'show data types'}
					title={isOpen ? 'unavailable while editing a data type' : 'show data types'}
					onClick={this.toggleDataTypes}>
					Show Data Types</button>

				<div className={component ? 'data-type-view' : 'data-type-view animate'}>
					<div className='data-type-close' onClick={this.toggleDataTypes}>
						<img src={closeIcon} />
					</div>
					<div className='data-type-header'>
						<h3>Data types</h3>
						<div className='data-type-search hide'>
							<input type='text' />
							<button>Search</button>
						</div>
					</div>
					<ul className='data-type-list'>
						{this.renderList()}
					</ul>
					{
						!readOnly ?
							<div className='data-type-create'>
								<div className={create ? 'hide' : 'data-type-create-trigger'} onClick={this.toggleCreate}>
									<i className='fas fa-plus' aria-hidden='true' />
									<span className='create-new'>new data type</span>
								</div>
								<form action='javascript:void(0)' className={create ? 'data-type-create-form animated fadeIn' : 'hide'}>
									<div className='field'>
										<div className='control'>
											<input className='input' type='text' placeholder='data type name (min 4 chars)' value={newName}
												onChange={this.dataTypeNameChange} ref={(createInput) => {this.createInput = createInput;}} />
											<button className={createTrigger ? 'button animated fadeIn' : 'hide'}
												onClick={this.newDataType}>start</button>
											<span className={hasError ? 'error-icon tooltip is-tooltip-bottom' : 'hide'}
												aria-label='datatype name already exists' />
											<span className={hasWarning ? 'warning-icon tooltip is-tooltip-bottom' : 'hide'}
												aria-label='datatype name cannot be less than 4' />
										</div>
									</div>
								</form>
							</div> :
							null
					}
				</div>
			</div>
		);
	}
}

DataType.propTypes = {
	definition: PropTypes.object,
	baseClasses: PropTypes.array,
	helpers: PropTypes.object,
	autoOpen: PropTypes.bool,
	readOnly: PropTypes.bool
};

export default DataType;
