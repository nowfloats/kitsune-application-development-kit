import React, { Component } from 'react';
import { connect } from 'react-redux';
import PropTypes from 'prop-types';
import Dropzone from 'kt-dropzone';
import { modalOpen } from '../../../actions/modal';
import CloseIcon from '../../../images/close.png';
import Code from '../../../images/code-dropper.svg';
import Images from '../../../images/image-dropper.svg';
import Archives from '../../../images/archive-dropper.svg';
import ExistingStorageConnectionSelector from './existing-storage-selector';
import { isNullOrUndefined } from 'util';

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

class FileDropper extends Component {
	constructor(props) {
		super(props);
		this.uploadApplicationZip = this.uploadApplicationZip.bind(this);
	}

	uploadApplicationZip(files){
		const { dispatch, appType, closeModal } = this.props;
		let fileObject = handleFileObject(files);
		let paths = [''];
		if(fileObject.length) {
			paths = _.fill(Array(fileObject.length), paths[0]);
		}
		dispatch(
			modalOpen(<ExistingStorageConnectionSelector purpose={appType} closeModal={closeModal} files={fileObject} paths={paths} />, 
			'upload app', null)
			);
	}

	render() {
		const { closeModal, onFileDrop, files, appType } = this.props;
		if(!files.length) {
			if(isNullOrUndefined(appType)){
				return (
					<Dropzone className='k-modal-large file-dropper' onDrop={onFileDrop}>
						<img className='close-icon' src={CloseIcon} onClick={closeModal} />
						<div className='dropper-icons'>
							<img className='archives' src={Archives} alt='archive files icon' />
							<img className='code' src={Code} alt='code files icon' />
							<img className='images' src={Images} alt='image files icon' />
						</div>
						<h2>drop here to upload to your project</h2>
						<div className='dropper-details'>
							<p>you can drop as many files as you want.</p>
							<p>post dropping you need to allocate a path in the project tree.</p>
							<p>for folders, <span className='tooltip is-tooltip-bottom' aria-label={`we don't recommend folder
							selection because webkit browsers like chrome don't always preserve the folder structure`}>
								we recommend </span> drag and drop.</p>
							<p>you can also select &amp; upload multiple files by clicking here.</p>
						</div>
					</Dropzone>
				);
			} else {
				return (
					<Dropzone className='k-modal-large file-dropper' onDrop={this.uploadApplicationZip}>
						<img className='close-icon' src={CloseIcon} onClick={closeModal} />
						<div className='dropper-icons'>
							<img className='archives' src={Archives} alt='archive files icon' />
							<img className='code' src={Code} alt='code files icon' />
							<img className='images' src={Images} alt='image files icon' />
						</div>
						<h2 className='no-opacity'>step 1 - upload the zip file of your {appType} application</h2>
						<div className='dropper-details'>
							<p>ensure that the zip file contains all the necessary files required to run your application.</p>
							<p>the files will be used to create the container image to run your application.</p>
						</div>
					</Dropzone>
				);
			}
		} else return null;
	}
}

FileDropper.propTypes = {
	closeModal: PropTypes.func,
	onFileDrop: PropTypes.func,
	files: PropTypes.array,
	appType: PropTypes.string,
	dispatch: PropTypes.func
};

export default connect()(FileDropper);
