import React, { Component } from 'react'
import PropTypes from 'prop-types';
import { connect } from 'react-redux'
import { modalClose } from '../../../actions/modal'
import CloseIcon from '../../../images/close-thin.svg'
import WebFormIcon from '../../../images/webform-white.svg';
import { showLoading } from "../../../actions/loader";
import { config } from "../../../config";

export const saveWebFormLabel = 'Save WebForm';

class SaveWebForm extends Component {

	constructor (props) {
		super(props);
		this.saveWebForm = this.saveWebForm.bind(this);
		this.closeModal = this.closeModal.bind(this);
		this.checkIfAgreed = this.checkIfAgreed.bind(this);
		this.state = {
			isAgreed: false
		}
	}

	closeModal() {
		const { dispatch } = this.props;
		dispatch(modalClose());
	}

	saveWebForm() {
		const { text: SAVE_WEBFORM } = config.INTERNAL_SETTINGS.loadingText.SAVE_WEBFORM
		const { dispatch, helper, callback } = this.props;
		dispatch(showLoading(SAVE_WEBFORM))
		helper.saveEnabled(callback);
	}

	checkIfAgreed( ) {
		let checkbox = document.querySelector('#agree');
		this.setState({ isAgreed : checkbox.checked })
	}

	render () {
		return (
			<div>
				<div className='k-modal-head'>
					<div>
						<img className='modal-header-icon' src={WebFormIcon} alt='webforms' />
						<h1 className='modal-header-title'>saving webform</h1>
					</div>
					<img src={CloseIcon} className='closeIcon' onClick={this.closeModal} />
				</div>
				<div className='k-modal-content k-modal-small save-webForm-content'>
					<div className='k-modal-container save-webForm-container'>
						<p className='first-para'>this step will render the code required to run your webform.
							saving the file will give you the final output, that you will not
							be able to edit. proceed only if you are ready to deploy.
						</p>
						<p>
							if you would like to use this webform for a new project,
							you can always clone and then edit it.
						</p>
					</div>
				</div>
				<div className='save-webForm-modal-footer'>
					<div>
						<input type='checkbox' onClick={this.checkIfAgreed}
							name='agree' className='regular-checkbox' value='agree' id='agree' />
						<label htmlFor='agree' />
						<span>i agree</span>
					</div>
					<button className={this.state.isAgreed ? '' : 'disable-confirm'}
						disabled={!this.state.isAgreed}
						onClick={this.saveWebForm}>save webForm</button>
				</div>
			</div>
		)
	}
}

SaveWebForm.propTypes = {
	dispatch: PropTypes.func,
	helper: PropTypes.object,
	callback: PropTypes.func
}

const mapStateToProps = state => {
	return {
		helper : state.webFormReducer.helper,
		callback: state.modalReducer.callback
	};
};

export default connect(mapStateToProps)(SaveWebForm)
