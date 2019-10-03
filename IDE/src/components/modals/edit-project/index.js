import React, { Component } from 'react'
import PropTypes from 'prop-types';
import { connect } from 'react-redux'
import { projectEdit } from '../../../actions/projectTree'
import { modalClose } from '../../../actions/modal'
import CloseIcon from '../../../images/close-thin.svg'
import ProjectIcon from '../../../images/project-white.svg'

export const editProjectLabel = 'Edit Project';

class EditProject extends Component {

	constructor (props) {
		super(props)
		this.editProject = this.editProject.bind(this)
		this.closeModal = this.closeModal.bind(this)
		this.themeNameChange = this.themeNameChange.bind(this)
		this.themeEdit = this.themeEdit.bind(this)
		this.state= {
			name: sessionStorage.getItem('projectName'),
		}
	}

	closeModal() {
		const { dispatch } = this.props;
		dispatch(modalClose());
	}

	editProject() {
		if(this.state.name !== '' || this.state.category !== '') {
			const { dispatch } = this.props;
			let projectConfig = {
				name: this.state.name,
				themeId: sessionStorage.getItem('currentThemeId')
			}

			dispatch(projectEdit(projectConfig));
			dispatch(modalClose());
		}
	}

	themeEdit(e) {
		if(e.keyCode === 13) {
			this.editProject();
		}
	}

	themeNameChange(event) {
		this.setState({ name: event.target.value });
	}

	render () {
		return (
			<div>
				<div className='k-modal-head'>
					<div>
						<img src={ProjectIcon} className='modal-header-icon' />
						<h1 className='modal-header-title'>rename project</h1>
					</div>
					<img src={CloseIcon} className='closeIcon' onClick={this.closeModal} />
				</div>
				<div className='k-modal-content k-modal-small k-modal-form'>
					<div className='k-modal-container'>
						<label htmlFor='editproject-name' className='k-modal-label'>name </label>
						<input id='editproject-name' className='k-modal-input' type='text' value={this.state.name}
							onChange={this.themeNameChange} onKeyUp={this.themeEdit} autoFocus />
					</div>
				</div>
				<div className='k-modal-footer'>
					<button onClick={this.editProject}>save</button>
				</div>
			</div>
		)
	}
}

EditProject.propTypes = {
	dispatch: PropTypes.func
}

export default connect()(EditProject)
