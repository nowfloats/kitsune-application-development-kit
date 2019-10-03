import React, { Component } from 'react'
import PropTypes from 'prop-types';
import { connect } from 'react-redux'
import { projectCreate } from '../../../actions/projectTree'
import { modalClose } from '../../../actions/modal'
import CloseIcon from '../../../images/close-thin.svg'
import ProjectIcon from '../../../images/project-white.svg'

export const createProjectLabel = 'Create Project';

class CreateProject extends Component {

	constructor (props) {
		super(props)
		this.createProject = this.createProject.bind(this)
		this.closeModal = this.closeModal.bind(this)
		this.themeNameChange = this.themeNameChange.bind(this)
		this.themeCreate = this.themeCreate.bind(this)
		this.state= {
			projectName: ''
		}
	}

	closeModal() {
		const { dispatch } = this.props;
		dispatch(modalClose());
	}

	createProject() {
		if(this.state.projectName !== '') {
			const { dispatch } = this.props;
			let name = this.state.projectName;
			dispatch(projectCreate(name));
			dispatch(modalClose());
		}
	}

	themeCreate(e) {
		if(e.keyCode === 13) {
			this.createProject();
		}
	}

	themeNameChange(event) {
		this.setState({ projectName: event.target.value });
	}

	render () {
		return (
			<div>
				<div className='k-modal-head'>
					<div>
						<img src={ProjectIcon} className='modal-header-icon' />
						<h1 className='modal-header-title'>create a new project</h1>
					</div>
					<img src={CloseIcon} className='closeIcon' onClick={this.closeModal} />
				</div>
				<div className='k-modal-content k-modal-small'>
					<div className='k-modal-container'>
						<label htmlFor='createproject-input' className='k-modal-label'>name</label>
						<input id='createproject-input' className='k-modal-input' type='text' value={this.state.projectName}
							onChange={this.themeNameChange} onKeyUp={this.themeCreate} autoFocus />
					</div>
				</div>
				<div className='k-modal-footer'>
					<button onClick={this.createProject}>create</button>
				</div>
			</div>
		)
	}
}

CreateProject.propTypes = {
	dispatch: PropTypes.func
}

export default connect()(CreateProject)
