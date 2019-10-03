import React, { Component } from 'react'
import PropTypes from 'prop-types';
import { connect } from 'react-redux'
import { modalClose } from '../../../actions/modal';
import { fileRename } from "../../../actions/editor";
import CloseIcon from '../../../images/close-thin.svg';
import ProjectIcon from '../../../images/project-white.svg';

export const renameFileLabel = 'Rename File';

class RenameFile extends Component {
	constructor (props) {
		super(props);
		this.closeModal = this.closeModal.bind(this);
		this.fileNameChange = this.fileNameChange.bind(this);
		this.renameFile = this.renameFile.bind(this);
		this.fileNameKeyTrigger = this.fileNameKeyTrigger.bind(this);
		this.state = {
			name: props.currentNode.name
		}
	}

	fileNameChange(event) {
		this.setState({ name: event.target.value });
	}

	fileNameKeyTrigger(e) {
		if(e.keyCode === 13) {
			this.renameFile();
		}
	}

	renameFile() {
		this.props.dispatch(fileRename(this.props.currentNode, this.state.name));
		this.closeModal();
	}

	closeModal() {
		const { dispatch } = this.props;
		dispatch(modalClose());
	}

	render () {
		return (
			<div>
				<div className='k-modal-head'>
					<div>
						<img src={ProjectIcon} className='modal-header-icon' />
						<h1 className='modal-header-title'>rename file</h1>
					</div>
					<img src={CloseIcon} className='closeIcon' onClick={this.closeModal} />
				</div>
				<div className='k-modal-content k-modal-small k-modal-form'>
					<div className='k-modal-container'>
						<label htmlFor='editfile-name' className='k-modal-label'>name </label>
						<input id='editfile-name' className='k-modal-input' type='text' value={this.state.name}
							onChange={this.fileNameChange} onKeyUp={this.fileNameKeyTrigger} autoFocus />
					</div>
				</div>
				<div className='k-modal-footer'>
					<button onClick={this.renameFile}>save</button>
				</div>
			</div>
		)
	}
}

RenameFile.propTypes = {
	dispatch: PropTypes.func,
	currentNode: PropTypes.object
};

export default connect()(RenameFile);
