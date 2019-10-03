import React, { Component } from 'react';
import WebFormIcon from '../../../images/webform-white.svg';
import PropTypes from 'prop-types';
import CloseIcon from '../../../images/close-thin.svg'
import { connect } from 'react-redux';
import { modalClose } from '../../../actions/modal';
import { setDetailsOfNewWebForm, isWebFormOpen, setIsEditable } from '../../../actions/webForm';
import { toastr } from 'react-redux-toastr';
import _ from 'lodash';
import { fileOpen } from '../../../actions/editor';

export const createWebformLabel = 'Customer List';

class CreateWebform extends Component {
	constructor (props) {
		super(props);
		this.closeModal = this.closeModal.bind(this);
		this.createWebform = this.createWebform.bind(this);
		this.webformNameChange = this.webformNameChange.bind(this);
		this.checkWebformNameExists = this.checkWebformNameExists.bind(this);
		this.webformDescriptionChange = this.webformDescriptionChange.bind(this);
		this.webFormCreateTrigger = this.webFormCreateTrigger.bind(this);
		this.closeAll = this.closeAll.bind(this)
		this.state = {
			webformName : '',
			description : '',
			webFormDetailsError: 'please enter a name for the webForm',
			showError : false,
		};
	}

	webformNameChange(e) {
		this.setState({ webformName : e.target.value })
	}

	webformDescriptionChange(e) {
		this.setState({ description : e.target.value })
	}

	createWebform() {
		let state = this.state
		if(state.webformName === '') {
			toastr.error('error creating webForm', state.webFormDetailsError)
		}
		else {

			if(!this.checkWebformNameExists()) {
				const webFormNameRegex = /[^A-z\d_\s-]/;
				if (!webFormNameRegex.test(state.webformName)) {
					const { webForm, dispatch } = this.props;
					const { canClone, helper } = webForm;
					dispatch(setIsEditable(true))
					dispatch(setDetailsOfNewWebForm(state.webformName, state.description))
					if(canClone) {
						helper.cloneWebForm();
					}
					dispatch(isWebFormOpen(true))
					dispatch(fileOpen(state.webformName, '', '', '', ''));
					this.closeModal()
				}
				else {
					toastr.error('webform name error', `name should be alphanumeric with '-' and '_' allowed`)
				}
			} else {
				toastr.error('webform name exists', 'please choose another name');
			}

		}
	}

	webFormCreateTrigger(e) {
		if(e.keyCode === 13) {
			this.createWebform();
		}
	}

	checkWebformNameExists(){
		let { webForms } = this.props.webForm;
		let { webformName } = this.state;
		webformName = webformName.trim().toLowerCase();
		let webForm = _.find(webForms,(wf)=>{
			return wf.DisplayName.trim().toLowerCase() === webformName;
		});
		return !!webForm;
	}

	closeModal() {
		const { dispatch } = this.props;
		dispatch(modalClose());
	}

	closeAll() {
		const { dispatch, webForm } = this.props;
		const { canClone } = webForm;
		if(!canClone) {
			dispatch(isWebFormOpen(false));
		}
		this.closeModal();
	}

	render() {
		const { webForm } = this.props;
		const { canClone } = webForm
		let h1 = null;
		let confirmButton = null;
		if(canClone) {
			h1 = `clone and edit this webForm`
			confirmButton = 'continue'
		}
		else {
			h1 = 'create a webForm'
			confirmButton = 'create'
		}
		return (
			<div>
				<div className='k-modal-head'>
					<div>
						<img className='modal-header-icon' src={WebFormIcon} alt='webforms' />
						<h1 className='modal-header-title'>{h1}</h1>
					</div>
					<img src={CloseIcon} className='closeIcon' onClick={this.closeAll} />
				</div>
				<div className='k-modal-content k-modal-form k-modal-small'>
					<div className='k-modal-container'>
						<label htmlFor='createproject-input' className='k-modal-label'>name</label>
						<input id='createproject-input' className='k-modal-input' type='text'
							autoFocus
							value={this.state.webformName}
							onKeyUp={this.webFormCreateTrigger}
							onChange={this.webformNameChange} />
					</div>
					<div className='k-modal-container'>
						<label htmlFor='createproject-input' className='k-modal-label'>description</label>
						<input id='createproject-input' className='k-modal-input' type='text'
							value={this.state.description}
							onKeyUp={this.webFormCreateTrigger}
							onChange={this.webformDescriptionChange} />
					</div>
				</div>
				<div className='k-modal-footer'>
					<button onClick={this.createWebform}>{confirmButton}</button>
				</div>
			</div>
		)
	}
}

CreateWebform.propTypes = {
	dispatch : PropTypes.func,
	webForm: PropTypes.shape({
		webForms: PropTypes.array,
		isEditable: PropTypes.bool,
		canClone: PropTypes.bool,
		currentWebForm: PropTypes.object,
		helper: PropTypes.object
	}),
}

function mapStateToProps(state){
	return {
		webForm : state.webFormReducer
	}
}

export default connect(mapStateToProps)(CreateWebform);
