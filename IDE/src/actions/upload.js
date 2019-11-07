/*Herein lies all the upload actions*/
import { upload } from './actionTypes';
import reflect from 'async-es/reflect';
import parallelLimit from 'async-es/parallelLimit';
import axios from 'axios';
import _ from 'lodash';
import { config } from '../config';
import { modalClose } from './modal';
import { updateProjectTreeNode, saveApplicationConfigDetails } from './projectTree';
import { showLoading, hideLoading } from './loader';
import { fileSourceUpdated } from "./editor";
import { append, footerCollapse, resetLog, updateUpload } from "./footer";

export const setPath = path => {
	return {
		type: upload.SET_PATH,
		payload: { path }
	}
};

//action for reseting failed files
export const resetFailed = () => {
	return {
		type: upload.RESET_FAILED
	}
};

export const updateFiles = (files, paths) => {
	return {
		type: upload.UPDATE_FILES,
		payload: { files, paths }
	}
};

export const updateApplicationFiles = (files, paths, appType, deploymentConfig, storageConfig) => {
	return {
		type: upload.UPDATE_APPLICATION_FILES,
		payload: { files, paths, appType, deploymentConfig, storageConfig }
	}
};

export const resetStore = () => {
	return {
		type: upload.RESET_STORE
	}
};

//helper to create uploaded iterator
export const generateUploadIterator = path => (dispatch, getState) => {
	const singleSlashPath = _.chain(path)
		.replace(/^\//, '')
		.replace(/[\/]+/g, '/')
		.value();
	const { children } = getState().projectTreeReducer;
	const pathArray = singleSlashPath.split('/');
	let currentChild = children;
	let pathIterator = 0;
	pathArray.every(iterator => {
		const doesIteratorExist = currentChild.filter(({ name }) => name === iterator);
		if (doesIteratorExist.length) {
			currentChild = doesIteratorExist[0].children;
			pathIterator = pathIterator + 1;
			return true;
		} else {
			return false;
		}
	});
	const resultIterator = pathArray.slice(0, pathIterator).join('/');
	dispatch(generateUploadObject(singleSlashPath, resultIterator));
};

//helper to create upload object and update the store.
export const generateUploadObject = (path, existingIterator) => dispatch => {
	let pathToGenerate = _.replace(path, existingIterator, '');
	pathToGenerate = pathToGenerate[0] === '/' ?
		pathToGenerate.substring(1, pathToGenerate.length) : pathToGenerate;
	const pathToGenerateArray = pathToGenerate.split('/');
	let resultObject = {};
	let itObj = resultObject;
	let iterablePath = existingIterator === '' ? existingIterator : `/${existingIterator}`;
	pathToGenerateArray.forEach((iterator, index) => {
		iterablePath = `${iterablePath}/${iterator}`;
		const iterableObject = {
			IsKitsune: true,
			Path: iterablePath,
			children: index === pathToGenerateArray.length - 1 ? null : [],
			name: iterator,
			toggled: false
		};
		if (index === 0) {
			resultObject = iterableObject;
			itObj = iterableObject;
		} else {
			let pushable = itObj;
			for (let i = 0; i < index; i++) {
				if (i === index - 1) {
					pushable = pushable.children;
				} else {
					pushable = pushable.children[0];
				}
			}
			pushable.push(iterableObject);
		}
	});
	dispatch(updateProjectTreeNode(resultObject, existingIterator));
};

// Upload file XHR for pages and assets
export const singleFileUpload = (fileName, fileCode, filePath) => (dispatch, getState) =>
	new Promise((resolve, reject) => {
		const { ProjectId: projectId } = getState().projectTreeReducer.data;
		const { login } = getState();
		const { userID } = login;
		const { text: UPLOAD_FILE } = config.INTERNAL_SETTINGS.loadingText.UPLOAD_FILE;

		dispatch(showLoading(UPLOAD_FILE));
		dispatch(modalClose());
		dispatch(singleFileUploading());
		fileCode = fileCode.substr(fileCode.indexOf(",") + 1);
		const url = `${config.API.project}/${projectId}/resourceUpload`,
			sourcePath = `${filePath ? `${filePath}/` : ''}${fileName}`,
			header = {
				UserEmail: userID,
				FileContent: fileCode,
				SourcePath: sourcePath,
				IsStatic: true
			};
		return axios.post(url, header)
			.then(() => {
				dispatch(generateUploadIterator(sourcePath));
				resolve('file has been uploaded.');
				dispatch(singleFileUploaded());
				dispatch(fileSourceUpdated());
				dispatch(hideLoading());
			})
			.catch(() => {
				reject('failed to upload file.');
				dispatch(singleFileUploadError());
				dispatch(hideLoading())
			});
	});

export const setFailed = (file, path) => {
	return {
		type: upload.SET_FAILED,
		payload: { file, path }
	}
};

export const multipleUpload = () => (dispatch, getState) =>
	new Promise(resolve => {
		const { projectTreeReducer, login, uploadReducer } = getState();
		const { ProjectId: projectId } = projectTreeReducer.data;
		const { userID } = login;
		const { files, paths } = uploadReducer;
		const { EVENT_LOG } = config.INTERNAL_SETTINGS.footerTabs;
		const url = `${config.API.project}/${projectId}/resourceUpload`;
		dispatch(showLoading('reading files...'));
		createUploadTasks(userID, projectId, files, paths, resolve, url, dispatch)
			.then(tasks => {
				dispatch(hideLoading());
				dispatch(resetLog());
				dispatch(footerCollapse(EVENT_LOG, null));
				dispatch(append({
					type: 'heading',
					message: 'upload progress:'
				}));
				parallelLimit(tasks, 3);
			})
	});

export const applicationFileUpload = () => (dispatch, getState) =>
	new Promise(resolve => {
		const { projectTreeReducer, login, uploadReducer } = getState();
		const { ProjectId: projectId } = projectTreeReducer.data;
		const { userID } = login;
		const { files, paths, appType, deploymentConfig, storageConfig } = uploadReducer;
		const { EVENT_LOG } = config.INTERNAL_SETTINGS.footerTabs;
		const url = `${config.API.project}/${projectId}/ApplicationUpload`;
		dispatch(showLoading('reading application files...'));
		createApplicationUploadTasks(userID, projectId, files, paths, resolve, url, appType, deploymentConfig, storageConfig, dispatch)
			.then(tasks => {
				dispatch(hideLoading());
				dispatch(resetLog());
				dispatch(footerCollapse(EVENT_LOG, null));
				dispatch(append({
					type: 'heading',
					message: 'application upload progress:'
				}));
				parallelLimit(tasks, 3);
			})
	});

const createUploadTasks = (userID, projectId, files, paths, resolvePrevious, url, dispatch) =>
	new Promise(resolve => {
		let tasks = [];
		files.forEach((iterator, index) => {
			const reader = new FileReader();
			reader.readAsDataURL(iterator.fileObject);
			reader.onloadend = event => {
				const fileCode = event.target.result.substr(event.target.result.indexOf(",") + 1);
				const sourcePath = iterator.folder.isFolder ?
					`${paths[index]}/${iterator.fullPath.split('/').slice(1).join('/')}` :
					`${paths[index]}/${iterator.name}`;
				const header = {
					UserEmail: userID,
					FileContent: fileCode,
					SourcePath: sourcePath,
					IsStatic: true
				};
				tasks = tasks.concat((function (iterator, index, files, url, header, dispatch) {
					return reflect((callback) => {
						dispatch(append({
							type: 'message',
							message: `uploading ${iterator.name}......`,
							index: index,
							success: null
						}));
						return axios.post(url, header)
							.then(response => {
								dispatch(generateUploadIterator(sourcePath));
								dispatch(updateUpload(index, true));
								callback();
								if (index === files.length - 1) {
									resolvePrevious({ projectId, count: files.length });
								}
							})
							.catch((error) => {
								dispatch(updateUpload(index, false));
								dispatch(setFailed(iterator, paths[index]));
								callback();
								if (index === files.length - 1) {
									resolvePrevious({ projectId, count: files.length });
								}
								throw new Error(error);
							})
					});
				}(iterator, index, files, url, header, dispatch)));
				if (index === files.length - 1) {
					resolve(tasks);
				}
			}
		});
	});

const createApplicationUploadTasks = (userID, projectId, files, paths, resolvePrevious, url, appType, deploymentConfig, storageConfig, dispatch) =>
	new Promise(resolve => {
		let tasks = [];
		files.forEach((iterator, index) => {
			const sourcePath = iterator.folder.isFolder ?
				`${paths[index]}/${iterator.fullPath.split('/').slice(1).join('/')}` :
				`${paths[index]}/${iterator.name}`;
			const header = {
				UserEmail: userID,
				'Content-Type': 'application/json'
			};
			const applicationData = new FormData();
			applicationData.append("SourcePath", sourcePath);
			applicationData.append("ResourceType", "APPLICATION");
			applicationData.append("PageType", appType);
			applicationData.append("UserEmail", userID);
			applicationData.append("Configuration", JSON.stringify({ ...deploymentConfig, ...storageConfig }));
			applicationData.append("file", iterator.fileObject);
			console.log(applicationData);
			tasks = tasks.concat((function (iterator, index, files, url, header, dispatch) {
				return reflect((callback) => {
					dispatch(append({
						type: 'message',
						message: `uploading ${iterator.name}......`,
						index: index,
						success: null
					}));
					return axios({
						method: 'post',
						url: url,
						data: applicationData,
						config: header
					})
						.then(function (response) {
							dispatch(generateUploadIterator(sourcePath));
							dispatch(updateUpload(index, true));
							callback();
							if (index === files.length - 1) {
								dispatch(saveApplicationConfigDetails({
									appType: appType,
									deploymentConfig: deploymentConfig,
									storageConfig: storageConfig
								}));
								resolvePrevious({ projectId, count: files.length });
							}
						})
						.catch(function (response) {
							dispatch(updateUpload(index, false));
							dispatch(setFailed(iterator, paths[index]));
							callback();
							if (index === files.length - 1) {
								resolvePrevious({ projectId, count: files.length });
							}
							throw new Error(error);
						})
				});
			}(iterator, index, files, url, header, dispatch)));
			if (index === files.length - 1) {
				resolve(tasks);
			}
		});
	});

// Upload file XHR for pages and assets
export function singleFileUploading() {
	return {
		type: upload.UPLOADING,
		payload: { isFetching: true }
	}
}

// Upload file XHR for pages and assets
export function singleFileUploaded() {
	return {
		type: upload.UPLOADED,
		payload: { isFetching: false }
	}
}

export function singleFileUploadError() {
	return {
		type: upload.UPLOAD_ERROR,
		payload: { isFetching: false }
	}
}
