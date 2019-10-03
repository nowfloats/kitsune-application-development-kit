import React, { Component } from 'react'
import PropTypes from 'prop-types';
import { connect } from 'react-redux'
import { modalClose } from '../../../actions/modal'
import CloseIcon from '../../../images/close-thin.svg'
import { buildProject } from "../../../actions/build";
import { toastr } from "react-redux-toastr";
import { updateProjectData } from "../../../actions/projectTree";
import { httpDomainWithoutSSL } from '../../../config';
export const buildRequiredLabel = 'Prompt Messsage';

class BuildRequired extends Component {
	constructor(props) {
		super(props);
		this.closeModal = this.closeModal.bind(this);
		this.buildNow = this.buildNow.bind(this);
		this.previewNow = this.previewNow.bind(this);
	}

	closeModal() {
		const { dispatch } = this.props;
		dispatch(modalClose());
	}

	buildNow() {
		const { dispatch } = this.props;
		this.closeModal();
		dispatch(buildProject())
			.then(() => {
				toastr.success('build successful', `your project has been successfully built.`);
				dispatch(updateProjectData());
			})
			.catch((error) => {
				toastr.error('build Failed', error.message);
			});
	}

	previewNow() {
		const { projectID, defaultCustomer } = this.props;

		window.open(`${httpDomainWithoutSSL}/preview/project=${projectID}/customer=${defaultCustomer}`, '_blank');
		this.closeModal();
	}

	render() {
		return(
			<div>
				<div className='k-modal-head'>
					<h1>build required</h1>
					<img src={CloseIcon} className='closeIcon' onClick={this.closeModal} />
				</div>
				<div className='k-modal-content k-modal-small'>
					<p>your modified changes are not yet built.
						if you need to preview your latest changes, please build first.</p>
				</div>
				<div className='k-modal-prompt-footer'>
					<button onClick={this.previewNow}>preview anyway</button>
					<button onClick={this.buildNow}>build now</button>
				</div>
			</div>
		);
	}
}

BuildRequired.propTypes = {
	dispatch: PropTypes.func,
	projectID: PropTypes.string,
	defaultCustomer: PropTypes.string
};

const mapStateToProps = (state) => {
	return {
		defaultCustomer: state.publishReducer.defaultCustomer,
		projectID: state.projectTreeReducer.data ? state.projectTreeReducer.data.ProjectId : null,
	}
};

export default connect(mapStateToProps)(BuildRequired);
