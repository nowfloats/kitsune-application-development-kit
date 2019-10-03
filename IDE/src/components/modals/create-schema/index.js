import React, { Component } from 'react';
import PropTypes from 'prop-types';
import { connect } from 'react-redux';
import { schemaCreate } from '../../../actions/schema';
import { modalClose } from '../../../actions/modal';
import CloseIcon from '../../../images/close-thin.svg';
import LanguageIcon from '../../../images/language-white.svg';
import { config } from '../../../config';
import _ from 'lodash';
import InfoIcon from '../../../images/info.svg';

export const createSchemaLable = 'Map Schema to the current Project';

class CreateSchema extends Component {

	constructor (props) {
		super(props)
		this.createSchema = this.createSchema.bind(this);
		this.closeModal = this.closeModal.bind(this);
		this.schemaNameChange = this.schemaNameChange.bind(this);
		this.schemaCreateTrigger = this.schemaCreateTrigger.bind(this);
		this.validateSchemaName = this.validateSchemaName.bind(this);
		this.state= {
			schemaName: '',
			isDisabled: true,
			error: null,
			info: null
		}
	}

	closeModal() {
		const { dispatch } = this.props;
		dispatch(modalClose());
	}

	createSchema() {
		if(this.state.schemaName !== '') {
			const { dispatch } = this.props;
			let name = this.state.schemaName;
			dispatch(schemaCreate(name));
			dispatch(modalClose());
		}
	}

	schemaCreateTrigger(e) {
		if(e.keyCode === 13 && !this.state.error) {
			this.createSchema();
		}
	}

	schemaNameChange({ target }) {
		const { value } = target;
		this.setState({
			schemaName: value
		}, () => { this.validateSchemaName(value) }
		);
	}

	validateSchemaName(schemaValue) {
		const { regex } = config;
		const regExStartWebForm = regex.regexToCheckStartSchemaAndWebForm;
		const regExWebForm = regex.regexToCheckSchemaAndWebForm;
		let errorValue = !regExStartWebForm.test(schemaValue[0]) ? schemaValue[0] : null;
		let errorMessage = /[0-9]/.test(errorValue) ? 'numbers are not allowed at start.' : errorValue + ' not allowed.';
		if(!errorValue && schemaValue.length >= 2) {
			errorValue = _.uniq(schemaValue.replace(regExWebForm,'')).join('');
			errorMessage = errorValue + ' not allowed';
		}
		this.setState({
			error: errorValue ? errorMessage : false,
			isDisabled: errorValue || schemaValue.length === 0
		});
	}


	render () {
		const { isDisabled, error, schemaName, info } = this.state;
		return (
			<div>
				<div className='k-modal-head'>
					<div>
						<img src={LanguageIcon} className='modal-header-icon' />
						<h1 className='modal-header-title'>create a new data model</h1>
					</div>
					<img src={CloseIcon} className='closeIcon' onClick={this.closeModal} />
				</div>
				<div className='k-modal-content k-modal-small'>
					<div className='k-modal-container'>
						<label htmlFor='create-schema-input' className='k-modal-label'>name</label>
						<input id='create-schema-input'
							className='k-modal-input'
							type='text'
							value={this.state.schemaName}
							onChange={this.schemaNameChange}
							onKeyUp={this.schemaCreateTrigger}
							autoFocus />
						<div className={error || info? 'alert-user active' : 'alert-user'}
							data-tool-tip={error ? error : info} >
							<img style={{ marginLeft: "-5px" }}src={InfoIcon} />
						</div>
						<span className={error ? 'warning-icon' : 'hide'} />
						<span className={error === false && schemaName ? 'success-icon' : 'hide'} />
					</div>
				</div>
				<div className='k-modal-footer'>
					<button onClick={this.createSchema} disabled={isDisabled}>start</button>
				</div>
			</div>
		)
	}
}

CreateSchema.propTypes = {
	dispatch: PropTypes.func
}

export default connect()(CreateSchema)
