import React, { Component } from 'react'
import PropTypes from 'prop-types';
import _ from 'lodash';
import { connect } from 'react-redux';
import CloseIcon from '../../../images/close-thin.svg';
import ProjectIcon from '../../../images/project-white.svg'
import deleteFile from '../../../images/delete.svg';
import { modalClose } from "../../../actions/modal";
import { multipleUpload, updateFiles } from "../../../actions/upload";
import { hideLoading, showLoading } from "../../../actions/loader";
import { config } from "../../../config";
import { append } from "../../../actions/footer";

const RemoveButton = props => {
	const { files, deleteFile, removeFile } = props;
	if(files.length > 1) {
		return <img className='remove-file' src={deleteFile} onClick={removeFile} />;
	}
	return null;
};

const ApplyAllCheckbox = props => {
	const { files, index, checked, applyChecked, isDisabled } = props;
	if(files.length > 1) {
		return (
			<div className='apply-all'>
				<input id={`checkBox-${index}`}
					type='checkbox'
					checked={checked}
					onChange={applyChecked}
					disabled={isDisabled} />
				<label htmlFor={`checkBox-${index}`} />
				<span>apply to all</span>
			</div>
		);
	}
	return null;
};


class PathSelector extends Component {
	constructor(props) {
		super(props);
		this.renderContainers = this.renderContainers.bind(this);
		this.onPathChange = this.onPathChange.bind(this);
		this.onApplyChecked = this.onApplyChecked.bind(this);
		this.startUpload = this.startUpload.bind(this);
		this.removeFile = this.removeFile.bind(this);
		this.state = {
			files: [],
			paths: [],
			applyToAll: [],
			enabledIndex: null
		};
	}

	componentWillReceiveProps({ files, paths }) {
		this.setState({ files, paths, applyToAll: _.fill(Array(files.length), false) });
	}

	onPathChange(e, index) {
		let { paths, enabledIndex } = this.state;
		if(enabledIndex !==  null) {
			paths = _.fill(Array(paths.length), e.target.value);
		} else {
			paths[index] = e.target.value;
		}
		this.setState({ paths });
	}

	onApplyChecked(e, index) {
		let { applyToAll, enabledIndex, paths } = this.state;
		applyToAll[index] = e.target.checked;
		enabledIndex = e.target.checked ? index : null;

		this.setState({ applyToAll, enabledIndex, paths: _.fill(Array(paths.length), paths[index]) });
	}

	removeFile(name) {
		let { files } = this.state;
		files = _.filter(files, i =>  i.name !== name);
		this.setState({ files });
	}

	startUpload() {
		let { files, paths } = this.state;
		const { startUpload } = this.props;
		let result = {
			files: [],
			paths: []
		};
		files.forEach((iterator, index) => {
			if(iterator.folder.isFolder) {
				result.files.push(...iterator.folder.folderArray);
				result.paths.push(..._.fill(Array(iterator.folder.folderArray.length), paths[index]));
			} else {
				result.files.push(iterator);
				result.paths.push(paths[index]);
			}
		});
		startUpload(result);
	}

	renderContainers() {
		const { files, paths, applyToAll, enabledIndex } = this.state;
		return files.map((iterator, index) => {
			const isDisabled = index !== enabledIndex && enabledIndex !== null;
			return (
				<div className='k-modal-container' key={index}>
					<label className='k-modal-label'>{iterator.name}</label>
					<span className='k-modal-label file-path'>/ </span>
					<input className='k-modal-input'
						type='text'
						autoFocus
						value={paths[index]}
						onChange={e => this.onPathChange(e, index)}
						disabled={isDisabled} />
					<RemoveButton files={files} deleteFile={deleteFile} removeFile={()=> this.removeFile(iterator.name)} />
					<ApplyAllCheckbox files={files}
						index={index}
						checked={applyToAll[index]}
						applyChecked={e => this.onApplyChecked(e, index)}
						isDisabled={isDisabled} />
				</div>
			);
		});
	}

	render() {
		const { files, closeModal } = this.props;
		if(files.length) {
			return (
				<div className='k-modal-medium path-selector'>
					<div className='k-modal-head'>
						<div>
							<img src={ProjectIcon} className='modal-header-icon' />
							<h1 className='modal-header-title'>Assign path to file{files.length > 1 ? 's' : ''}</h1>
						</div>
						<img src={CloseIcon} className='closeIcon' onClick={closeModal} />
					</div>
					<div className='k-modal-content'>
						{this.renderContainers()}
					</div>
					<div className='k-modal-footer'>
						<button onClick={this.startUpload}>START UPLOAD</button>
					</div>
				</div>
			);
		} else return null;
	}
}

PathSelector.propTypes = {
	closeModal: PropTypes.func,
	startUpload: PropTypes.func,
	files: PropTypes.array,
	paths: PropTypes.array
};

RemoveButton.propTypes = {
	files: PropTypes.array,
	deleteFile: PropTypes.string,
	removeFile: PropTypes.func
};

ApplyAllCheckbox.propTypes = {
	files: PropTypes.array,
	index: PropTypes.number,
	checked: PropTypes.bool,
	applyChecked: PropTypes.func,
	isDisabled: PropTypes.bool
};

const mapDispatchToProps = dispatch => {
	const { text: UPLOAD_FILE } = config.INTERNAL_SETTINGS.loadingText.UPLOAD_FILE;
	return ({
		startUpload: ({ files, paths }) => {
			dispatch(updateFiles(files, paths));
			dispatch(modalClose());
			dispatch(showLoading(UPLOAD_FILE));
			dispatch(multipleUpload())
				.then(({ projectId, count }) => {
					dispatch(append({
						type: 'status',
						count
					}));
					dispatch(hideLoading());
				})
		}
	});
};

export default connect(null, mapDispatchToProps)(PathSelector);
