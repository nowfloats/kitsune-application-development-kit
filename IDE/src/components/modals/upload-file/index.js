import React, { Component } from 'react';
import _ from 'lodash';
import PropTypes from 'prop-types';
import FileDropper from './file-dropper';
import ApplicationUploadTypeModal from '../application-upload-type'
import PathSelector from './path-selector';
import ExistingStorageConnectionSelector from './existing-storage-selector';
import { connect } from 'react-redux';
import { modalClose } from '../../../actions/modal';
import { resetStore } from '../../../actions/upload';
import { isNullOrUndefined } from 'util';

export const uploadFileLabel = 'Upload File';
export const uploadApplicationLabel = 'Upload Application';

const handleFileObject = files => {
	let result = [];
	files.forEach((iterator) => {
		const isFolder = iterator.fullPath.split('/')[2] !== undefined;
		const folderName = isFolder ? iterator.fullPath.split('/')[1] : null;
		iterator.folder = {
			isFolder: isFolder,
			name: folderName
		};
		if(isFolder) {
			const searchObj = _.find(result, { 'name': folderName });
			if(searchObj) {
				searchObj.folder.folderArray.push(iterator);
			} else {
				const newFolder = {
					name: folderName,
					folder: {
						isFolder: isFolder,
						name: folderName,
						folderArray: [iterator]
					}
				};
				result.push(newFolder);
			}
		} else {
			result.push(iterator);
		}

	});
	return result;
};

class UploadFile extends Component {
	constructor(props) {
		super(props);
		this.onFileDrop = this.onFileDrop.bind(this);
		this.state = {
			files: [],
			paths: ['']
		};
	}

	onFileDrop(files) {
		files = handleFileObject(files);
		if(files.length) {
			this.setState({
				files: files,
				paths: _.fill(Array(files.length), this.state.paths[0])
			})
		}
	}

	componentDidMount() {
		const { path } = this.props;
		if(path !== '') {
			this.setState({
				paths: [ path ]
			})
		}
	}

	componentWillUnmount() {
		this.props.resetUpload();
	}

	render() {
		const { closeModal, appType } = this.props;
		const { files, paths } = this.state;
		if(isNullOrUndefined(appType)){
			return (
				<div>
					<PathSelector closeModal={closeModal} files={files} paths={paths} />
					<FileDropper closeModal={closeModal} files={files} onFileDrop={this.onFileDrop} />
				</div>
			);
		} else {
			return (
				<div>
					<ApplicationUploadTypeModal closeModal={closeModal} appType={appType}/>
				</div>
			);
		}
	}
}

UploadFile.propTypes = {
	closeModal: PropTypes.func,
	resetUpload: PropTypes.func,
	path: PropTypes.string,
	appType: PropTypes.string
};

const mapDispatchToProps = dispatch => {
	return({
		closeModal: e => {
			dispatch(modalClose());
			e.stopPropagation();
		},
		resetUpload: () => dispatch(resetStore())
	});
};

const mapStateToProps = state => {
	return {
		path: state.uploadReducer.paths[0]
	}
};

export default connect(mapStateToProps, mapDispatchToProps)(UploadFile);
