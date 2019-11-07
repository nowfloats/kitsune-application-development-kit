/*Herein lies all the editor actions*/
import axios from 'axios';
import ace from 'brace';
import { Base64 } from 'js-base64';
import _ from 'lodash';
import React from 'react';
import { toastr } from 'react-redux-toastr';
import { hideLoading } from '../actions/loader';
import PromptMessage, { promptMessageLabel } from "../components/modals/prompt";
import { config } from '../config';
import { editor, projectree } from './actionTypes';
import { footerCollapse, footerUpdate, isSaved } from './footer';
import { showLoading } from "./loader";
import { modalOpen } from "./modal";
import { checkActiveGateway, projectDeleteNode } from './projectTree';
import { resetSchema } from "./schema";
import { isWebFormOpen } from "./webForm";
import { editorState } from '../reducers/editor';

// Helper function to handle Kitsune Errors
export const addKitsuneErrors = (errors, filePath) => {
	let editor = ace.edit(`kitsune-editor-${filePath.replace(/\//g, '-')}`);
	let footerErrors = editor.getSession().getAnnotations();
	const { NOTIFICATION } = config.INTERNAL_SETTINGS.footerTabs;
	let editorErrors = [];
	errors = errors === null ? [] : errors;

	if (errors.length) {
		footerErrors = footerErrors.filter(iterator => {
			return !iterator.kError;
		});

		errors.forEach(iterator => {
			let errorMessage = {
				row: iterator.LineNumber - 1,
				column: iterator.LinePosition - 1,
				text: iterator.Message,
				type: 'kError'
			};
			footerErrors.push(errorMessage);
		});
		footerErrors.forEach(iterator => {
			editorErrors.push({
				row: iterator.row,
				column: iterator.column,
				text: iterator.text,
				kError: iterator.type === 'kError' ? true : false,
				type: iterator.type === 'kError' ? 'error' : iterator.type
			});
		});
		footerErrors.sort((a, b) => {
			return a.row - b.row;
		});
	} else {
		editorErrors = footerErrors;
	}
	return dispatch => {
		dispatch(footerUpdate(footerErrors, NOTIFICATION));
		editor.getSession().setAnnotations(editorErrors);
	}
};

//action for opening a file in ace editor
export const fileOpen = (name, code, path, type, fileConfig) => {
	const newEditor = Object.assign({}, editorState, {
		name,
		code,
		path,
		type,
		fileConfig
	});
	return {
		type: editor.FILE_OPEN,
		payload: { editor: newEditor, isFetching: false }
	};
};

//action for clearing the ace editor
export const editorClear = (index) => dispatch => {
	if (index !== undefined) {
		dispatch(fileChanged(false, index));
		dispatch(fileClose(index));
	} else {
		dispatch(fileChanged(false));

		//close language editor and webform if open.
		dispatch(resetSchema());
		dispatch(isWebFormOpen(false));
		dispatch(clearTabs());
	}
};

export const clearTabs = () => ({
	type: editor.CLEAR_TABS
})

export const fileClose = index => {
	return {
		type: editor.CLOSE_FILE,
		payload: { index }
	};
};

export const tabChanged = index => ({
	type: editor.SWITCH_TAB,
	payload: { index }
})

//action for changing code in ace editor
export const codeChange = (index, code) => {
	return {
		type: editor.CHANGE_CODE,
		payload: { index, code }
	};
};

export const getFileExtension = name => {
	//split the string by `.`
	const extensions = name.split('.');
	//check if file ends with `html.dl` or `htm.dl` as they are the only two cases which
	//doesn't have file extension in the end.
	return (_.endsWith(name.toLowerCase(), 'html.dl') || _.endsWith(name.toLowerCase(), 'htm.dl')) ?
		'html' : extensions[extensions.length - 1];
};

//action for fetching kitsune page source and opening it
export const fileSourceFetch = (name, path) => (dispatch, getState) => {
	//Fetch projectID and userID
	const { projectTreeReducer, login } = getState();
	const { ProjectId: projectId } = projectTreeReducer.data;
	const { userID } = login;

	const { NOTIFICATION } = config.INTERNAL_SETTINGS.footerTabs;
	const encodedPath = encodeURIComponent(path)
	//Collapse footer and empty it
	dispatch(footerUpdate([], NOTIFICATION));
	dispatch(footerCollapse(NOTIFICATION, NOTIFICATION));
	//indicate that file is being fetched
	dispatch(fileSourceFetching());
	//close language editor and webform if open.
	dispatch(resetSchema());
	dispatch(isWebFormOpen(false));

	const fileExtension = getFileExtension(name);
	const { extensionMap } = config.INTERNAL_SETTINGS;
	//If our mapper doesn't recognize the extension, then open as a text file.
	const fileType = extensionMap.get(fileExtension) === undefined ? 'text' : extensionMap.get(fileExtension);

	return axios.get(`${config.API.projectAPI}/v2/${projectId}/resource/?user=${userID}&sourcePath=${encodedPath}`)
		.then(async response => {
			let { data: fileConfig } = response;
			const { Base64Data } = fileConfig.File;
			const code = extractCodeValue(fileType, Base64Data ? Base64Data : '', fileConfig.MetaData ? fileConfig.MetaData : {});

			delete fileConfig['HtmlSourceString'];

			dispatch(fileOpen(name, code, path, fileType, fileConfig));

			if (!fileConfig.IsStatic)
				dispatch(fileSourceCompile(path, code));

			localStorage.setItem(`initialCode-${path}`, code);
			sessionStorage.setItem('kErrors', JSON.stringify(response.data.Errors));
			dispatch(hideLoading());
		})
		.catch(error => {
			dispatch(fileSourceError(error));
			dispatch(hideLoading());
		});
};

/**
 * Maps the file name to an icon, taking into account folders
 * Returns the icon class that can be applied as follows:
 *  <i className={mapFileToIcon(name, hasChildren)} style={yourCustomStyle} />
 */
export const mapFileNameToIcon = (fileName, hasChildren = false) => {
	const fileExtension = getFileExtension(fileName);
	const { extensionMap, extensionsIconMap } = config.INTERNAL_SETTINGS;
	const fileType = hasChildren ? 'folder'
		: (extensionMap.get(fileExtension) === undefined ? 'text' : extensionMap.get(fileExtension));
	const iconType = extensionsIconMap.get(fileType) === undefined ? 'fas fa-file' : extensionsIconMap.get(fileType);

	// return the icon class
	return iconType;
}

/**
 * Extact the code value using the filetype, and metadata
 */
const extractCodeValue = (fileType, base64Data, metaData) => {
	const { extensionMap } = config.INTERNAL_SETTINGS;
	const errorCtx = { error: 'unsupported file type', message: 'application zips should be uploaded through Project > import existing application' }

	// Default return the encoded data as is
	let result = base64Data;

	try {
		if (fileType === extensionMap.get('zip')) {
			// Prettify the data, as ace editor will not do this for us
			result = Base64.encode(
				JSON.stringify((metaData.Configuration ?
					JSON.parse(Base64.decode(base64Data)) :
					errorCtx),
					null, '\t')
			);
		}
	}
	catch (e) {
		// Error context can be modified by any closure, and add a throw to get into this block
		toastr.error(errorCtx.error, errorCtx.message);
	}

	return result;
};

//action to use to check if file has been changed and handle the action being clicked.
export const checkFileChanged = (callback, index) => (dispatch, getState) => {
	const { activeTabs } = getState().editorReducer;

	let fileChanged = false;

	if (index !== undefined && index >= 0) {
		fileChanged = activeTabs[index].fileChanged;
	} else {
		for (let tab of activeTabs) {
			if (tab.fileChanged) {
				fileChanged = true;
				break;
			}
		}
	}

	const { isOpen: isSchemaOpen } = getState().schemaCreatorReducer;
	const { isOpen: isWebformOpen } = getState().webFormReducer;
	const { name: SAVE_PROJECT } = config.INTERNAL_SETTINGS.promptMessages.SAVE_PROJECT;
	const { name: SAVE_LANGUAGE } = config.INTERNAL_SETTINGS.promptMessages.SAVE_LANGUAGE;
	const { name: SAVE_WEBFORM } = config.INTERNAL_SETTINGS.promptMessages.SAVE_WEBFORM;

	let promptItem = null;
	if (isWebformOpen) {
		promptItem = SAVE_WEBFORM;
	} else if (isSchemaOpen) {
		promptItem = SAVE_LANGUAGE;
	} else if (activeTabs.length >= 0) {
		promptItem = SAVE_PROJECT;
	}
	fileChanged ? dispatch(modalOpen(<PromptMessage promptItem={promptItem} />, promptMessageLabel, callback)) :
		callback();
};

//action to indicate fetching of kitsune page source
export const fileSourceFetching = () => {
	return {
		type: editor.FILESOURCE_FETCHING,
		payload: {
			isFetching: true
		}
	};
};

//action to error in kitsune page source
export const fileSourceError = error => {
	return {
		type: editor.FILESOURCE_ERROR,
		payload: { isFetching: false }
	};
};

//helper to check if update is valid or not
const checkUpdateValidation = (dispatch, currentState) => {
	const { name: fileName, code: fileCode, path } = currentState;
	const { SAVE_FILE } = config.INTERNAL_SETTINGS.loadingText;

	let initCode = localStorage.getItem(`initialCode-${path}`);
	//Will execute if code has changed
	if (initCode !== fileCode) {
		dispatch(showLoading(SAVE_FILE.text));
		return true;
	} else { //Will execute if code is same
		toastr.info('already up-to-date', `${fileName} is already up to date.`);
	}

	return false;
};

//helper to check if kitsune tags exists
const checkKitsuneTags = (IsStatic, name, code) => {
	const isHTML = _.endsWith(name, 'html') || _.endsWith(name, 'htm')
		|| _.endsWith(name, 'html.dl') || _.endsWith(name, 'htm.dl');
	if (isHTML) {
		const kPropsRe = /\[\[.+\]\]/;
		const kTagsRe = /\<(\w+).+k-\w+.+\>/;
		const kTags = code.search(kTagsRe);
		const kProps = code.search(kPropsRe);

		if (!IsStatic && kTags === -1 && kProps === -1) {
			return true;
		} else if (IsStatic && (kTags !== -1 || kProps !== -1)) {
			return false;
		} else {
			return IsStatic;
		}
	} else return IsStatic;
};

//action for updating kitsune page source(All files)
export const fileSourceUpdate = (isSingleFileUpdate) => (dispatch, getState) => {
	const { editorReducer, login, projectTreeReducer } = getState();
	const { activeTabs } = editorReducer;
	const allSaves = [];
	if (!isSingleFileUpdate) {
		for (let i = 0; i < activeTabs.length; i++) {
			let tab = activeTabs[i];
			const isUpdateValid = checkUpdateValidation(dispatch, tab);
			if (tab.fileChanged && isUpdateValid) {
				const { userID } = login;
				const { ProjectId: projectId } = projectTreeReducer.data;
				const { name, path, code, fileConfig } = tab;
				const { IsStatic } = fileConfig;
				const decryptedCode = Base64.decode(code);
				const isStatic = checkKitsuneTags(IsStatic, name, decryptedCode);

				dispatch(fileSourceUpdating());

				// if(!isStatic)
				// 	dispatch(fileSourceCompile(path, decryptedCode));

				let header = {
					'UserEmail': userID,
					'FileContent': code,
					'SourcePath': path,
					'IsStatic': isStatic
				};

				allSaves.push(
					axios.post(`${config.API.project}/${projectId}/resource`, header)
						.then(function (response) {
							if (path === '/kitsune-settings.json') {
								dispatch(checkActiveGateway());
							}
							if (isStatic !== IsStatic)
								dispatch(fileTypeUpdate(isStatic));
							dispatch(addKitsuneErrors(response.data.ErrorMessages, path))
							dispatch(fileSourceUpdated());
							dispatch(fileChanged(false, i));
							dispatch(isSaved({ isSaved: true, lastSave: new Date() }));
							toastr.success('file has been saved', `${name} has been saved successfully.`);
							localStorage.setItem(`initialCode-${path}`, decryptedCode);
						})
						.catch(function (error) {
							dispatch(fileSourceUpdateError(error));
						})
				);
			}
		}
		return Promise.all(allSaves).finally(() => dispatch(hideLoading()));
	}
	const { visibleIndex } = editorReducer;
	const tab = activeTabs[visibleIndex];
	const isUpdateValid = checkUpdateValidation(dispatch, tab);
	if (tab.fileChanged && isUpdateValid) {
		const { userID } = login;
		const { ProjectId: projectId } = projectTreeReducer.data;
		const { name, path, code, fileConfig } = tab;
		const { IsStatic } = fileConfig;
		const decryptedCode = Base64.decode(code);
		const isStatic = checkKitsuneTags(IsStatic, name, decryptedCode);

		dispatch(fileSourceUpdating());

		// if(!isStatic)
		// 	dispatch(fileSourceCompile(path, decryptedCode));

		let header = {
			'UserEmail': userID,
			'FileContent': code,
			'SourcePath': path,
			'IsStatic': isStatic
		};

		allSaves.push(
			axios.post(`${config.API.project}/${projectId}/resource`, header)
				.then(function (response) {
					if (path === '/kitsune-settings.json') {
						dispatch(checkActiveGateway());
					}
					if (isStatic !== IsStatic)
						dispatch(fileTypeUpdate(isStatic));
					dispatch(addKitsuneErrors(response.data.ErrorMessages, path))
					dispatch(fileSourceUpdated());
					dispatch(fileChanged(false, visibleIndex));
					dispatch(isSaved({ isSaved: true, lastSave: new Date() }));
					toastr.success('file has been saved', `${name} has been saved successfully.`);
					localStorage.setItem(`initialCode-${path}`, decryptedCode);
				})
				.catch(function (error) {
					dispatch(fileSourceUpdateError(error));
				})
		);
	}
	return Promise.all(allSaves).finally(() => dispatch(hideLoading()));
};

//action to indicate updating of kitsune page source
export const fileSourceUpdating = () => {
	return {
		type: editor.FILESOURCE_UPDATING,
		payload: { isFetching: true }
	};
};

//action to indicate kistune page is updated
export const fileSourceUpdated = () => {
	return {
		type: editor.FILESOURCE_UPDATED,
		payload: { isFetching: false, lastUpdateTimeStamp: Date.now() }
	};
};

//action for updating fileType
export const fileTypeUpdate = isStatic => {
	return {
		type: editor.UPDATE_FILETYPE,
		payload: isStatic
	};
};

//action for compiling kitsune page source
export const fileSourceCompile = (path, updateCode) => (dispatch, getState) => {
	const { login, projectTreeReducer } = getState();
	const { userID } = login;
	const { ProjectId: projectId } = projectTreeReducer.data;

	dispatch(fileSourceCompiling());

	const uriPath = encodeURIComponent(path);
	let header = {
		Email: userID,
		FileContent: updateCode
	};

	return axios.post(`${config.API.project}/${projectId}/Compile?resourcename=${uriPath}&user=${userID}`, header)
		.then(function (response) {
			if (response.request.status === 200) {
				dispatch(addKitsuneErrors(response.data.ErrorMessages, path));
				sessionStorage.setItem('kErrors', JSON.stringify(response.data.ErrorMessages));
				dispatch(fileSourceCompiled());
			} else {
				dispatch(fileSourceError(response.data.ErrorMessages));
			}
			dispatch(hideLoading());
		})
		.catch(function (error) {
			dispatch(fileSourceError(error));
			dispatch(hideLoading());
		});
};

//action to indicate compiling of kitsune page source
export const fileSourceCompiling = () => {
	return {
		type: editor.FILESOURCE_COMPILING,
		payload: { isFetching: true }
	};
};

//action to indicate kitsune page is compiled
export const fileSourceCompiled = () => {
	return {
		type: editor.FILESOURCE_COMPILED,
		payload: { isFetching: false }
	};
};

//action for error in kitsune page source update
export const fileSourceUpdateError = error => {
	toastr.error('error Saving File', `${error}`);
	return {
		type: editor.FILESOURCE_UPDATEERROR,
		payload: { isFetching: false }
	};
};

//action for indicating that file has been changed
export const fileChanged = (bool, index) => {
	if (bool)
		window.addEventListener('beforeunload', config.INTERNAL_SETTINGS.keepOnPage);
	else
		window.removeEventListener('beforeunload', config.INTERNAL_SETTINGS.keepOnPage);

	return {
		type: editor.FILE_CHANGED,
		payload: { index: (index === undefined) ? 0 : index, fileChanged: bool }
	}
};

//action for indicating that a file is open
export const isOpen = isOpened => {
	return {
		type: editor.IS_OPEN,
		payload: { isOpened: isOpened }
	}
};

//action for deleting file
export const fileDelete = path => (dispatch, getState) => {
	const { login, projectTreeReducer, editorReducer } = getState();
	const { userID } = login;
	const { ProjectId: projectId } = projectTreeReducer.data;
	const activeTabPaths = editorReducer.activeTabs.map(tab => tab.path);

	dispatch(fileDeleting());

	return axios.delete(`${config.API.project}/${projectId}/resource?user=${userID}&resourcename=${path}`)
		.then((response) => {
			if (response.request.status === 200) {
				dispatch(fileDeleted());

				const activeTabIndex = activeTabPaths.indexOf(path);
				if (activeTabIndex >= 0)
					dispatch(editorClear(activeTabIndex));

				dispatch(projectDeleteNode(decodeURIComponent(path[0])));
			} else {
				dispatch(fileSourceUpdateError('Error deleting file'));
			}
			dispatch(hideLoading());
		})
		.catch((error) => {
			dispatch(fileSourceUpdateError(error));
			dispatch(hideLoading());
		});
};

//action for deleting folder
export const folderDelete = path => (dispatch, getState) => {
	const { login, projectTreeReducer, editorReducer } = getState();
	const { userID } = login;
	const { ProjectId: projectId } = projectTreeReducer.data;
	const { activeTabs } = editorReducer;

	return axios.delete(`${config.API.project}/${projectId}/resource?user=${userID}&resourcepath=${path}/*`)
		.then(() => {
			for (let i = 0; i < activeTabs.length; i++) {
				if (tab.path.startsWith(`${path}/`)) {
					dispatch(editorClear(i));
				}
			}
			toastr.success('folder deleted');
			dispatch(projectDeleteNode(path));
			dispatch(hideLoading());
		})
		.catch(() => {
			toastr.success('error deleting folder');
			dispatch(hideLoading());
		});
};

//action to indicate file is being deleted
export const fileDeleting = () => {
	return {
		type: editor.FILE_DELETING,
		payload: { isFetching: true }
	}
};

//action to indicate file has been deleted
export const fileDeleted = () => {
	toastr.success('file has been deleted');

	return {
		type: editor.FILE_DELETED,
		payload: { isFetching: false }
	}
};

//action to make page default
export const makePageDefault = (path) => (dispatch, getState) => {
	const { login, projectTreeReducer } = getState();
	const { userID } = login;
	const { ProjectId: projectId } = projectTreeReducer.data;

	dispatch(makingPageDefault());

	return axios.post(
		`${config.API.projectEdit}/${projectId}/MakeResourceDefault?userEmail=${userID}&sourcePath=${path}`)
		.then((response) => {
			if (response.data) {
				dispatch(madePageDefault());
			} else {
				dispatch(makePageDefaultError());
			}
		})
		.catch((error) => {
			dispatch(makePageDefaultError());
		})
};

//action to indicate page is being made default
export const makingPageDefault = () => {
	return {
		type: editor.MAKING_PAGEDEFAULT,
		payload: { isFetching: true }
	};
};

//action to indicate page has been made default\
export const madePageDefault = () => {
	toastr.success('successfully made page default.');
	return {
		type: editor.MADE_PAGEDEFAULT,
		payload: { isFetching: false }
	};
};

//action to indicate error while making page default
export const makePageDefaultError = () => {
	toastr.error('there seems to an error making this page default.');
	return {
		type: editor.MAKE_PAGEDEFAULTERROR,
		payload: { isFetching: false }
	};
};

export const fileRename = ({ name: nodeName, Path }, name) => (dispatch, getState) => {
	const { text: RENAMING } = config.INTERNAL_SETTINGS.loadingText.RENAMING_FILE;
	dispatch(showLoading(RENAMING));
	const { activeTabs } = getState().editorReducer;

	const isFileOpen = activeTabs.some(tab => tab.path === Path);
	dispatch(fileRenameFetch(nodeName, Path))
		.then(content => {
			const pathArray = Path.split('/');
			pathArray.pop();
			const path = pathArray.join('/');
			dispatch(fileRenameUpload(name, content, path))
				.then(() => dispatch(fileRenameDelete([encodeURIComponent(Path)]))
					.then(() => dispatch(updateRenameNode(name, Path, isFileOpen)))
					.catch(error => {
						toastr.error('error renaming file');
						throw new Error(error);
					})
					.finally(() => dispatch(hideLoading()))
				)
				.catch(error => {
					dispatch(hideLoading());
					toastr.error('error renaming file');
					throw new Error(error);
				});
		})
		.catch(error => {
			dispatch(hideLoading());
			toastr.error('error renaming file');
			throw new Error(error);
		});
};

export const fileRenameFetch = (name, path) => (dispatch, getState) =>
	new Promise((resolve, reject) => {
		//Fetch projectID and userID
		const { projectTreeReducer, login } = getState();
		const { ProjectId: projectId } = projectTreeReducer.data;
		const { userID } = login;
		const encodedPath = encodeURIComponent(path);

		return axios.get(`${config.API.projectAPI}/v2/${projectId}/resource/?user=${userID}&sourcePath=${encodedPath}`)
			.then(({ data }) => resolve(data.File.Base64Data))
			.catch(error => reject(error));
	});

const fileRenameUpload = (fileName, fileCode, filePath) => (dispatch, getState) =>
	new Promise((resolve, reject) => {
		const { ProjectId: projectId } = getState().projectTreeReducer.data;
		const { login } = getState();
		const { userID } = login;

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
			.then(() => resolve())
			.catch(error => reject(error));
	});

const fileRenameDelete = path => (dispatch, getState) =>
	new Promise((resolve, reject) => {
		const { login, projectTreeReducer } = getState();
		const { userID } = login;
		const { ProjectId: projectId } = projectTreeReducer.data;

		return axios.delete(`${config.API.project}/${projectId}/resource?user=${userID}&resourcename=${path}`)
			.then(() => resolve())
			.catch(error => reject(error));
	});

const updateRenameNode = (name, path, isFileOpen) => dispatch => {
	const singleSlashPath = _.chain(path)
		.replace(/^\//, '')
		.replace(/[\/]+/g, '/')
		.value();
	dispatch(renameNodeToProject(name, singleSlashPath));
	if (isFileOpen) {
		dispatch(renameNodeToEditor(name, path))
	}
};

const renameNodeToProject = (name, path) => ({
	type: projectree.UPDATE_RENAMENODE,
	payload: {
		name,
		path
	}
});

const renameNodeToEditor = (name, path) => ({
	type: editor.UPDATE_RENAMEFILE,
	payload: {
		name,
		path
	}
});

export const setHelper = helper => {
	return {
		type: editor.SET_HELPER,
		payload: { helper: helper }
	};
};
