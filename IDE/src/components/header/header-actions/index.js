import React, { Component } from 'react';
import PropTypes from 'prop-types';
import { connect } from 'react-redux';
import PromptMessage, { promptMessageLabel } from "../../modals/prompt";
import { modalOpen } from "../../../actions/modal";
import { config } from "../../../config";

class HeaderActions extends Component {
	constructor (props) {
		super(props);
	}

	render() {
		const { projectStatus, profileImage, isBuildComplete, clearSession, buildProject } = this.props;
		const publishStatus = projectStatus === 1 ? true : false;
		const buildClass = (projectStatus <= 0) && !publishStatus && isBuildComplete ? '': 'disabled';
		return(
			<ul className='header-actions'>
				<li className='build' title='build and preview the project'>
					<button onClick={buildProject} className={buildClass}>build</button>
				</li>
				<li className='disable disable-icon hide' title='this feature is currently under construction'>
					<button className='settings' />
				</li>
				<li className='profile'>
					<img src={profileImage} alt='user image' />
					<ul>
						<li>
							<a href='https://dashboard.kitsune.tools/' target='_blank'>dashboard</a>
						</li>
						<li className='disable disable-submenu' title='this feature is currently under construction'>
							<a href='#'>account info</a>
						</li>
						<li onClick={clearSession}>
							<a href='#'>logout</a>
						</li>
					</ul>
				</li>
			</ul>
		);
	}
}

HeaderActions.propTypes = {
	projectStatus: PropTypes.number,
	profileImage : PropTypes.string.isRequired,
	isBuildComplete: PropTypes.bool,
	clearSession: PropTypes.func,
	buildProject: PropTypes.func
};

const mapDispatchToProps = (dispatch) => {
	const { name: logOutPrompt } = config.INTERNAL_SETTINGS.promptMessages.LOG_OUT;
	return ({
		clearSession: () => dispatch(modalOpen(<PromptMessage promptItem={logOutPrompt} />, promptMessageLabel, null))
	})
};

const mapStateToProps = (state) => {
	return {
		buildProject: state.editorReducer.helper.buildProject,
		projectStatus: state.projectTreeReducer.data.ProjectStatus !== undefined ?
			state.projectTreeReducer.data.ProjectStatus : 99,
		profileImage: state.login.profileImage,
		isBuildComplete: state.buildReducer.isCompleted
	};
};

export default connect(mapStateToProps, mapDispatchToProps)(HeaderActions);
