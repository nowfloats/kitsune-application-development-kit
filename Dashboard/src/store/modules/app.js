import * as types from '../mutation-types';
import { overlayModalNames } from '../../../config/config';
import user from '../../api/user';
import Auth from './Auth';

const state = {
	auth: new Auth(),
	user: {
		name: 'Abhinavpreetu',
		currentBalance: 50.12,
		currency: 'INR'
	},
	UserEmail : '',
	componentIsActive: {
		modal: false,
		overlay: false,
		updates: false,
		profile: false,
		sidebar: false,
		hamburger: false,
		action: false,
		billingform: false,
		addmoney: false,
		processingpayment: false,
		migrationbuildcards: false,
		account: false,
		deleteproject: false,
		skeletonLoader: false,
		publishproject: false,
		optimizeProject: false,
		customerDetails : false,
		verifyDomain: false,
		dnsDetails: false,
		previewProject: false,
		credits: false,
		lowBalance: false,
		abortCrawl: false,
		renameProject: false,
		fullLoader: false,
		deactivateSite: false
	},
	actionContainerCounter: 0,
	containerHistory: [],
	baseApiCalls: {
		UserDetails : false,
		ProjectList : false,
		pollingProjectsInProcess: false
	},
	payment : {
		isDetailsreceived: false,
		isPaymentSuccessfull: false,
		isInstamojoPaymentLinkRequested: false,
		isInstamojoPaymentLinkRequestedSuccessfull: false
	},
	deleteProject : {
		projectId : '',
		projectName : '',
		projectVersion : '',
		isProjectPublished: false
	},
	loader: false,
	requestedForAna: false,
	anaRegistrationDetails: {},
	apiStatus: null,
}

const mutations = {
	[types.toggleStatus] (state, { components, event }) {
		if(event.target.className.includes('overlay')
			|| components.length === overlayModalNames.length) {
			if(event.type == 'keyup' && state.componentIsActive.publishproject) {
				return false;
			} else {
				components.forEach(function(component) {
					state.componentIsActive[component] = state.componentIsActive[component]
						? !state.componentIsActive[component]
						: state.componentIsActive[component];
				})
			}
		}
		else {
			components.forEach(function(component) {
				state.componentIsActive[component] = !state.componentIsActive[component]
			})
		}
	},

	[types.setApiStatus] (state, payload) {
		state.apiStatus = payload;
	},

	[types.requestedForAna] (state, payload) {
		state.requestedForAna = payload;
	},

	[types.toggleOnResize] (state, components) {
		components.forEach(function(component) {
			state.componentIsActive[component] = !state.componentIsActive[component]
		})
	},

	[types.changeContainerCounter] (state, event) {
		let { containerHistory, componentIsActive } = state;
		containerHistory.pop();
		if (containerHistory.length == 0) {
			if(componentIsActive.action) {
				componentIsActive.action = false;
			}
		}
		else {
			state.actionContainerCounter = containerHistory[containerHistory.length - 1]
		}
	},

	[types.setContainerCounter] (state, payload) {
		let counter = Number(payload.split('-')[1]);
		state.containerHistory.push(counter);
		state.actionContainerCounter = state.containerHistory[state.containerHistory.length - 1]
	},

	[types.userDetailsReceived] (state) {
		state.baseApiCalls.UserDetails = true;
	},

	[types.projectListReceived] (state) {
		state.baseApiCalls.ProjectList = true;
	},

	[types.setDefaultUserEmail] (state, userEmail){
		state.UserEmail = userEmail;
	},

	[types.toggleActionModal] (state) {
		state.componentIsActive.action = false
	},

	[types.setMigrationbuildcardsComponentStatus] (state, payload) {
		state.componentIsActive.migrationbuildcards = payload;
	},

	[types.anaRegistrationDetails] (state, payload) {
		state.anaRegistrationDetails = { ...payload };
	},

	[types.setDeleteProjectDetails] (state,payload){
		state.deleteProject.projectId = payload.projectId;
		state.deleteProject.projectName = payload.projectName;
		state.deleteProject.projectVersion = payload.projectVersion;
		state.deleteProject.isProjectPublished = false;
	},

	[types.setCardsSkeletonLoaderComponentStatus] (state,payload){
		state.componentIsActive.skeletonLoader = payload;
	},

	[types.toggleLoader] (state, payload) {
		state.loader = payload;
	},

	[types.setIsProjectPublishedInDeleteProject] (state, payload) {
		state.deleteProject.isProjectPublished = payload
	},

	[types.showPreviewModal] (state, payload) {
		state.componentIsActive.previewProject = payload;
		state.componentIsActive.overlay = payload;
	},

	[types.resetContainerHistory] (state) {
		state.containerHistory = []
	},

	[types.triggerLowBalanceModal] (state) {
		state.componentIsActive = {
			...state.componentIsActive,
			lowBalance: true,
			overlay: true
		}
	},

	[types.toggleFullScreenLoader] (state, payload) {
		state.componentIsActive.fullLoader = payload;
	}

};

const actions = {
	toggleStatus ({ commit }, payload) {
		commit(types.toggleStatus, payload)
	},

	checkApiStatus ({ commit, dispatch }) {
		return new Promise((resolve, reject) => {
			var timeout = setTimeout(function() {
				dispatch('errorInApiStatusCheck');
				reject();
			}, 5000);

			user.getApiStatus((success, response) => {
				if(success) {
					clearTimeout(timeout);
					commit(types.setApiStatus, response);
					resolve(response);
				} else {
					clearTimeout(timeout);
					dispatch('errorInApiStatusCheck');
					reject();
				}
			})
		})
	},

	errorInApiStatusCheck({ commit }) {
		const payload = {
			success: true,
			isDown: true,
			isMaintenanceBreak: false,
			isApiDown: true,
			detail: {
				start: null,
				end: null,
				title: 'We will be back real soon!',
				description: 'Our engineers have been notified. Sorry for the inconvenience. We will be right back'
			}
		};
		commit(types.setApiStatus, payload);
	},

	toggleRequestedForAna ({ commit }, payload) {
		commit(types.requestedForAna, payload)
	},

	setAnaRegistrationDetails ({ commit }, payload) {
		commit(types.anaRegistrationDetails, payload);
	},

	userDetailsReceived ({ commit }) {
		commit(types.userDetailsReceived);
	},

	projectsListReceived ({ commit }) {
		commit(types.projectListReceived);
	},

	toggleOnResize ({ commit }, components) {
		commit(types.toggleOnResize, components)
	},

	changeContainerCounter ({ commit }, payload) {
		commit(types.changeContainerCounter, payload);
	},

	setContainerCounter ({ commit }, payload) {
		commit(types.setContainerCounter, payload)
	},

	setDefaultUserEmail ({ commit }, userEmail) {
		commit(types.setDefaultUserEmail, userEmail)
	},

	toggleActionModal ({ commit }) {
		commit(types.toggleActionModal)
	},

	toggleLoader ({ commit }, payload) {
		commit(types.toggleLoader, payload)
	},

	setProjectDeleteDetails ({ commit }, payload) {
		commit(types.setDeleteProjectDetails,payload);
	},

	setCardsSkeletonLoader ({ commit },payload){
		commit(types.setCardsSkeletonLoaderComponentStatus, payload);
	},

	showPreviewModal ({ commit }, payload){
		commit(types.showPreviewModal, payload)
	},

	resetContainerHistory({ commit }) {
		commit(types.resetContainerHistory)
	},

	triggerLowBalanceModal({ commit }) {
		commit(types.triggerLowBalanceModal);
	},

	toggleFullScreenLoader({ commit }, payload) {
		commit(types.toggleFullScreenLoader, payload);
	}
};

export default {
	state,
	mutations,
	actions
}
