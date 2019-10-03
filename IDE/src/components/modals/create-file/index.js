import React, { Component } from 'react';
import _ from 'lodash';
import PropTypes from 'prop-types';
import { connect } from 'react-redux';
import { resetStore, singleFileUpload } from '../../../actions/upload';
import { modalClose } from '../../../actions/modal';
import CloseIcon from '../../../images/close-thin.svg';
import ProjectIcon from '../../../images/project-white.svg'
import { toastr } from "react-redux-toastr";

export const createFileLabel = 'Create File';

class CreateFile extends Component {

	constructor(props) {
		super(props);
		this.createFile = this.createFile.bind(this);
		this.closeModal = this.closeModal.bind(this);
		this.handleChange = this.handleChange.bind(this);
		this.fileCreate = this.fileCreate.bind(this);
		this.state= {
			fileName: '',
			filePath: ''
		};
	}

	componentDidMount = () => {
		this.setState({ filePath: this.props.path });
	}

	componentWillUnmount() {
		this.props.dispatch(resetStore());
	}

	closeModal = () => {
		const { dispatch } = this.props;
		dispatch(modalClose());
	}

	createFile = () => {
		const { fileName, filePath } = this.state;
		if(fileName !== '') {
			const { dispatch } = this.props;
			const path = _.trimEnd(filePath, '/');
			dispatch(singleFileUpload(fileName, 'IA==', path))
				.then(() => {
					toastr.success('file has been created.');
				})
				.catch(() => {
					toastr.error('failed to create file.');
				});
			dispatch(modalClose());
		}
	}

	fileCreate = event => {
		if(event.keyCode === 13) {
			this.createFile();
		}
	};

	handleChange = event => {
		const { name, value } = event.target;
		this.setState({ [name]: value });
	}

	render = () => {
		const { fileName, filePath } = this.state;
		return (
			<div>
				<div className='k-modal-head'>
					<div>
						<img src={ProjectIcon} className='modal-header-icon' />
						<h1 className='modal-header-title'>create new file</h1>
					</div>
					<img src={CloseIcon} className='closeIcon' onClick={this.closeModal} />
				</div>
				<div className='k-modal-content k-modal-small k-modal-form'>
					<div className='k-modal-container'>
						<label htmlFor='createproject-input' className='k-modal-label'>name (with extension)</label>
						<input name='fileName'
							id='createproject-input'
							className='k-modal-input'
							type='text'
							value={fileName}
							onChange={this.handleChange}
							onKeyUp={this.fileCreate}
							autoFocus />
					</div>
					<div className='k-modal-container'>
						<label className='k-modal-label'>path</label>
						<span className='k-modal-label file-path'>/</span>
						<input name='filePath'
							className='k-modal-input'
							type='text'
							value={filePath}
							onChange={this.handleChange}
							onKeyUp={this.fileCreate} />
					</div>
				</div>
				<div className='k-modal-footer'>
					<button onClick={this.createFile}>create</button>
				</div>
			</div>
		);
	}
}

CreateFile.propTypes = {
	dispatch: PropTypes.func,
	path: PropTypes.string
};

const mapStateToProps = state => {
	return {
		path: state.uploadReducer.paths[0]
	};
};

export default connect(mapStateToProps)(CreateFile);
