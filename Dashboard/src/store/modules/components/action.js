import Upload from '../../../api/action'
import * as types from '../../mutation-types'
import Action from '../../../api/action'
import store from '../../../store/index'
import { IDELink, isProdEnv, coreEmailId, defaultErrorMessage } from "../../../../config/config";

const state = {
	isActionInputInFocus: true,
	uploadFiles: [],
	newProjectId: '',
	uploadOnProjectDetails: {
		projectId: '',
		projectName: ''
	},
	uploadProjectId: '',
	filesFailed: [],
	filesUploaded: 0,
	count: 0,
	isFolderDropped: false,
	currentProjectName: ''
};

const getters = {
	isActionInputInFocus : state => state.isActionInputInFocus,
	uploadFiles: state => state.uploadFiles,
	newProjectId: state => state.newProjectId,
	uploadOnProjectDetails: state => state.uploadOnProjectDetails,
	isFolderDropped: state => state.isFolderDropped,
	currentProjectName: state => state.currentProjectName
}

const mutations = {
	toggleContainerActiveClass (state, payload) {
		state.isActionInputInFocus = payload
	},
	setUploadFiles (state, payload) {
		state.uploadFiles = payload
	},
	[types.newProjectId] (state, payload){
		state.newProjectId = payload
	},
	changeFileCount (state) {
		state.count ++;
	},
	resetFilesCount (state) {
		state.count = 0
	},
	[types.setUploadOnProjectDetails] (state, payload) {
		let { uploadOnProjectDetails } = state
		let { projectId, projectName } = payload
		uploadOnProjectDetails.projectId = projectId
		uploadOnProjectDetails.projectName = projectName
	},

	[types.setIsFolderDropped] (state, payload) {
		state.isFolderDropped = payload
	},

	[types.currentProjectName] (state, payload) {
		state.currentProjectName = payload;
	}
};

const actions = {
	callToggleContainerActiveClass ({ commit }, payload) {
		commit('toggleContainerActiveClass', payload)
	},
	setNewProjectId({ commit }, payload) {
		commit(types.newProjectId, payload)
	},
	createProject({ commit, dispatch }, project) {
		Action.createProject(project, (success, res) => {
			if (success){
				commit(types.newProjectId, res);
				dispatch('addUpdatesToPanel', {
					read: false,
					type: 'user-notify',
					title: 'project created successfully',
					content: `the project '${project.name}' has been created successfully. the credentials for
							K-admin have been emailed to ${store.state.app.UserEmail}`,
					url: `${IDELink}`.replace('{projectid}', res),
					actionName: 'edit project',
					callback: null,
					extraInfo: '',
					className: 'alert'
				});
				if(isProdEnv) {
					dispatch('triggerEmail', {
						To: [coreEmailId],
						Subject: `new project created by ${store.state.app.UserEmail} on ${new Date().toLocaleString()}.`,
						EmailBody: `Project Name: ${project.name}<br>Project Id: ${res}`
					})
				}
			}
			else {
				dispatch('toggleLoader');
				dispatch('toggleActionModal');
				const errorMessage = res.message && res.message.toLowerCase() === 'network error'
					? 'please check your internet connection.'
					: res.response
						? res.response.data[0].ErrorMessage.toLowerCase() : defaultErrorMessage;
				dispatch('showToastr', {
					isError: true,
					title : 'unable to create serverless-app project.',
					message : `${errorMessage}`
				})
			}
		})
	},

	uploadCallback({ commit, dispatch }, { success, folderData, length }) {
		let details = {
			projectId: folderData.ProjectId,
			isUploading: '',
			fileName: folderData.SourcePath
		};
		commit('changeFileCount');
		success ? commit('setUploadedFilesCount') : commit('addFailedFiles', folderData.file);
		details.isUploading = state.count !== state.uploadFiles.length;
		dispatch('setUploadDetails', { details, totalFiles: state.uploadFiles.length });
	},

	uploadFolder ({ commit }, folderData) {
		return new Promise((resolve) => {
			Upload.uploadFolder(folderData, (success) => {
				resolve(success);
			})
		});
	},

	uploadFile({ commit }, file) {
		return new Promise((resolve, reject) => {
			Upload.uploadFolder(file, (success, response) => {
				success ? resolve(response) : reject(response);
			})
		})
	},

	setUploadOnProjectDetails({ commit }, payload) {
		commit(types.setUploadOnProjectDetails, payload)
	},

	resetUploadDetails({ commit, dispatch }) {
		commit('resetFilesCount');
		commit('setUploadedFilesCount', 0);
		commit('setUploadFiles', []);
		dispatch('setUploadDetails', { resetFlag: true });
	},

	callSetUploadFiles ({ commit }, payload) {
		commit('setUploadFiles', payload)
	},

	setIsFolderDropped({ commit }, payload) {
		commit(types.setIsFolderDropped, payload)
	},

	setCurrentProjectName({ commit }, payload) {
		commit(types.currentProjectName, payload);
	},

	abortCrawl({ dispatch, commit }, payload) {
		return new Promise((resolve, reject) => {
			const { projectId } = payload;
			Action.abortCrawl(projectId, (success, response) => {
				if(success) {
					dispatch('stopMigration');
					commit(types.setCardsSkeletonLoaderComponentStatus, true);
					dispatch('getAllProjects');
					resolve(response);
				} else {
					reject(response);
				}
			})
		})
	}
};

export default {
	state,
	getters,
	actions,
	mutations
}
