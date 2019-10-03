//TODO: Add accessibility to react-select

import React, { Component } from 'react'
import PropTypes from 'prop-types';
import { connect } from 'react-redux'
import { projectFetch } from '../../../actions/projectTree'
import { modalClose } from '../../../actions/modal'
import CloseIcon from '../../../images/close-thin.svg'
import Select from 'react-select';
import ProjectIcon from '../../../images/project-white.svg'

export const openProjectLabel = 'Open Project';

class OpenProject extends Component {

	constructor (props) {
		super(props)
		this.openTheme = this.openTheme.bind(this)
		this.projectChange = this.projectChange.bind(this)
		this.closeModal = this.closeModal.bind(this);
		this.selectProject = this.selectProject.bind(this);
		this.state= {
			selectedProject: ''
		}
	}

	closeModal() {
		const { dispatch } = this.props;
		dispatch(modalClose());
	}

	openTheme() {
		const { dispatch } = this.props;
		let themeid = this.state.selectedProject;
		dispatch(projectFetch(themeid, false));
		dispatch(modalClose());
	}

	selectProject(e) {
		if(this.state.selectedProject !== '') {
			if(e.keyCode === 13) {
				this.openTheme();
			}
		}
	}

	projectChange(val) {
		this.setState({ selectedProject: val.value });
	}

	render () {
		let themes = this.props.data
		let options = []
		themes.forEach(function(iteration) {
			if(iteration.ProjectStatus !== 3) {
				options.push({ className:'data-options', value: iteration.ProjectId, label: iteration.ProjectName })
			}
		})

		return (
			<div>
				<div className='k-modal-head'>
					<div>
						<img src={ProjectIcon} className='modal-header-icon' />
						<h1 className='modal-header-title'>select project</h1>
					</div>
					<img src={CloseIcon} className='closeIcon' onClick={this.closeModal} />
				</div>
				<div className='k-modal-content k-modal-small'>
					<div className='k-modal-container'>
						<label className='k-modal-label'>name</label>
						<Select name='form-field-name'
							value={options.find(option => option.value === this.state.selectedProject)}
							options={options}
							onChange={this.projectChange}
							className='k-modal-dropdown'
							classNamePrefix='k-modal-dropdown'
							onInputKeyDown={this.selectProject} />
					</div>
				</div>
				<div className='k-modal-footer'>
					<button onClick={this.openTheme}>open</button>
				</div>
			</div>
		)
	}
}

OpenProject.propTypes = {
	dispatch: PropTypes.func,
	data: PropTypes.array
}

const mapStateToProps = (state) => {
	return {
		projectTree: state.projectTreeReducer
	}
}

export default connect(mapStateToProps)(OpenProject)
