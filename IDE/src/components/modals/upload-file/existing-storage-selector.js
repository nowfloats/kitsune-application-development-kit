import React, { Component } from 'react'
import PropTypes from 'prop-types';
import _ from 'lodash';
import { connect } from 'react-redux';
import { existingAppDeploymentType } from '../../../actions/actionTypes'
import CloseIcon from '../../../images/close-thin.svg';
import StorageSettingsIcon from '../../../images/settings.svg'
import { modalClose } from "../../../actions/modal";
import { applicationFileUpload, updateApplicationFiles } from "../../../actions/upload";
import { hideLoading, showLoading } from "../../../actions/loader";
import { config } from "../../../config";
import { append } from "../../../actions/footer";
import Select from 'react-select';

class ExistingStorageConnectionSelector extends Component {
	constructor(props) {
		super(props);
		this.renderContainers = this.renderContainers.bind(this);
		this.startUpload = this.startUpload.bind(this);
		this.dbTypeSelected = this.dbTypeSelected.bind(this);
		this.connectionStringChange = this.connectionStringChange.bind(this);
		this.state = {
			selectedDBType: 'mysql',
			connectionString: '',
			enabledIndex: null
		};
	}

	startUpload() {
		let { files, paths } = this.props;
		const { startUpload, purpose } = this.props;
		let result = {
			files: [],
			paths: [],
			appType: purpose,
			storageConfig: {
				dbType: this.state.selectedDBType,
				existingDBEndpoint: this.state.connectionString
			},
			deploymentConfig: {
				type: existingAppDeploymentType.CONTAINERISE,
				existingAppEndpoint: ''
			}
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

	dbTypeSelected(val) {
		this.setState({ selectedDBType: val.value });
	}

	connectionStringChange = event => {
		const { value } = event.target;
		this.setState({ connectionString: value });
	}

	renderContainers() {
		const { files } = this.props;
		const { selectedDBType, connectionString } = this.state;
		let dbOptions = [];
		dbOptions.push({ className:'data-options', value: 'mysql', label: 'MySQL' });
		dbOptions.push({ className:'data-options', value: 'postgresql', label: 'PostgreSQL' });
		dbOptions.push({ className:'data-options', value: 'sqlite', label: 'SQLite' });
		dbOptions.push({ className:'data-options', value: 'sqlserver', label: 'SQL Server' });
		return files.map((iterator, index) => {
			return (
				<div className='k-modal-container' key={index}>
					<label className='k-modal-label'>select the database type:</label>
					<Select name='form-field-name'
						value={dbOptions.find(option => option.value === this.state.selectedDBType)}
						options={dbOptions}
						onChange={this.dbTypeSelected}
						className='k-modal-dropdown'
						classNamePrefix='k-modal-dropdown' />
					<br/>
					<label className='k-modal-label'>set the {selectedDBType} connection string:</label>
					<input className='k-modal-input'
						type='text'
						autoFocus
						value={connectionString}
						onChange={this.connectionStringChange}/>
				</div>
			);
		});
	}

	render() {
		const { files, closeModal, purpose } = this.props;
		if(files.length == 1) {
			return (
				<div className='k-modal-medium path-selector'>
					<div className='k-modal-head'>
						<div>
							<img src={StorageSettingsIcon} className='modal-header-icon' />
							<h1 className='modal-header-title'>step 2 - configure database for the {purpose} application</h1>
						</div>
						<img src={CloseIcon} className='closeIcon' onClick={closeModal} />
					</div>
					<div className='k-modal-content k-modal-no-scroll'>
						{this.renderContainers()}
					</div>
					<div className='k-modal-prompt-footer'>
						<button onClick={this.startUpload}>SKIP (no database attached)</button>
						<button onClick={this.startUpload}>SUBMIT</button>
					</div>
				</div>
			);
		} else return null;
	}
}

ExistingStorageConnectionSelector.propTypes = {
	closeModal: PropTypes.func,
	startUpload: PropTypes.func,
	files: PropTypes.array,
    paths: PropTypes.array,
	purpose: PropTypes.string
};

const mapDispatchToProps = dispatch => {
	const { text: UPLOAD_APPLICATION_ZIP_FILE } = config.INTERNAL_SETTINGS.loadingText.UPLOAD_APPLICATION_ZIP_FILE;
	return ({
		startUpload: ({ files, paths, appType, deploymentConfig, storageConfig }) => {
			dispatch(updateApplicationFiles(files, paths, appType, deploymentConfig, storageConfig));
			dispatch(modalClose());
			dispatch(showLoading(UPLOAD_APPLICATION_ZIP_FILE));
			dispatch(applicationFileUpload())
				.then(({ projectId, count }) => {
					dispatch(append({
						type: 'application_upload_status',
						appType, count
					}));
					dispatch(hideLoading());
				})
		}
	});
};

export default connect(null, mapDispatchToProps)(ExistingStorageConnectionSelector);
