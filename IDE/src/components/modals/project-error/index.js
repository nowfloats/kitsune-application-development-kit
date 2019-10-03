import React, { Component } from 'react'
import PropTypes from 'prop-types';
import { connect } from 'react-redux'
import { modalClose } from '../../../actions/modal'
import { projectFetch } from '../../../actions/projectTree'
import { showLoading, hideLoading } from '../../../actions/loader'
import CloseIcon from '../../../images/close-thin.svg'
import ErrorIcon from '../../../images/Error-Icon.png'
import { config } from '../../../config'

export const projectErrorLabel = 'Project Error';

class ProjectError extends Component {
	constructor(props){
		super(props);
		this.closeModal = this.closeModal.bind(this);
		this.reloadPage = this.reloadPage.bind(this);
	}
	//for closing the error modal
	closeModal() {
		const { dispatch } = this.props;
		dispatch(modalClose());
	}
	//reloading the page
	reloadPage() {
		const { dispatch } = this.props;
		dispatch(showLoading(config.INTERNAL_SETTINGS.loadingText.GENERAL_LOADING.text))
		dispatch(modalClose())
		dispatch(projectFetch(sessionStorage.getItem('currentThemeId'), false))
		dispatch(hideLoading());
	}
	render() {
		return (
			<div>
				<div className='k-modal-error-head k-modal-small'>
					<img src={CloseIcon} onClick={this.closeModal} />
				</div>
				<div className='k-modal-error-content '>
					<img src={ErrorIcon} alt='Error Icon' />
					<h1>oops!</h1>
					<p>our highly trained fox couldn't find the project you are looking for</p>
				</div>
				<div className='k-modal-error-footer'>
					<button onClick={this.reloadPage}>reload</button>
				</div>
			</div>
		);
	}
}

ProjectError.propTypes = {
	dispatch: PropTypes.func
}

export default connect()(ProjectError)

