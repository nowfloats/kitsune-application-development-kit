import * as types from '../../mutation-types'
import projects from '../../../api/projects'
import store from '../../../store/index'
import Vue from 'vue';
import Enum from 'enum';
import axios from 'axios';
import { coreEmailId, isProdEnv, cloudOptionsMap } from "../../../../config/config";

const mailIdsForClouds = Object.freeze({
	alicloud: ['chirag.m@getkitsune.com', 'ronak@getkitsune.com']
});

const state = {
	Projects : [],
	projectSearchQuery : '',
	projectStatus : new Enum({ 'IDLE':0, 'PUBLISHING':1, 'BUILDING':2,
		'CRAWLING':3, 'QUEUED':4, 'PUBLISHINGERROR':-1, 'PREPARINGDOWNLOAD': 10,
		'BUILDINGERROR':-2, 'ERROR':-3 }),
	liveProjects : [],
	Errors : [],
	projectType : new Enum ({ CRAWL : 0, DRAGANDDROP : 1, NEWPROJECT : 2, WORDPRESS:3, ERROR : -1 }),
	uploadDetails: {
		projectId: '',
		isUploading: false,
		totalFiles: 0,
		filesUploaded: 0,
		filesFailed: []
	},
	publishProject : {
		stage: 0,
		projectId : '',
		projectName : '',
		customerList: [],
		customerForPublishing: {},
		newCustomerDetails: {
			FullName:  '',
			Email:  '',
			PhoneNumber: 0,
			ProjectId:  '',
			WebsiteTag:  '',
			DeveloperId:  '',
			Domain: ''
		},
		isNewCustomer: false,
		isCreatingCustomer: false,
		isCustomerCreationError : false,
		isPublishing: false,
		isGettingCustomerList : false,
		isAPIError: false,
		isEligibleForPublishing: true,
		aliCloudDetails: {},
		cloudSelected: '',
		cloudDetailsPresent: false,
		gcpCloudDetails: {},
		gcpTokenGeneratorUrl: '',
		gcpCredsFile: null,
		chooseOwnAccount: null,
		domainForCloud: ''
		// doesWebsiteTagExist: false
	},
	uploadArray : [],
	BuildStatus :  new Enum( { 'Queued': 'Queued',
		'Compiled': 'compiling html',
		'QueuedBuild': 'in queue',
		'Analyzer': 'analyzing',
		'Optimizer': 'optimizing',
		'Replacer': 'updating asset versions',
		'Completed': 'completed',
		'Error': 'error' }),
	CrawlStatus : new Enum(  {
		Initialising : 'initiating',
		IdentifyingAllAssetsAndDownloadingWebpage : 'analyzing',
		IdentifyingExternalDomains : 'action needed',
		DownloadingAllStaticAssetsToStorage : 'converting',
		UpdatingWebPagesWithNewStaticAssetUri : 'updating urls',
		Completed : 'completed',
		Error : 'error'
	}),
	IsPollingForProjectsInProcess: false,
	downloadProject : {
		projectId : '',
		isLinkRequested : false,
		downloadLink : '',
		isPolling : false,
		isApiCallError : false
	},
	optimizeProject: {
		projectId : '',
		projectName : '',
		isApiCallCompleted: false,
		isApiCallSuccess:false
	},
	optimizationError: false,
	customerDetails: {
		customerId : '',
		customer : {},
		isFetchingCustomerDetails: false,
		areDetailsFetchedSuccessfully : false
	},
	verifyDomain:{
		customerId : '',
		isFetchingDetails: false,
		isApiCallSuccess : false,
		domainDetails : {}
	},
	dnsDetails:{
		websiteId: '',
		pendingDomains: [],
	},
	previewProject: {
		projectId: '',
		customerList: [],
		isFetchingDetails: false,
		projectName: ''
	},
	renameProject: {},
	deactivateProject: {
		name: '',
		websiteIds: []
	},
	isApiCallInProgress: false,
	lazyLoad: {
		showLoader: false,
		stop: false
	},
	showNotificationLiveSite: [ ...cloudOptionsMap.values() ].toString()
};

// getters
const getters = {
	getAllProjects : state => state.Projects,

	getAllLiveProjects : state => state.liveProjects,

	getTotalProjectsCount : state => state.Projects.length,

	getTotalLiveProjectsCount : state => state.liveProjects.length,

	uploadDetails: state => state.uploadDetails,

	getPublishDetails: state => state.publishProject,

	getOptimizeDetails: state => state.optimizeProject,

	getProjectsStatus : state => state.projectStatus,

	getPreviewProjectDetails: state => state.previewProject,

	getStageName : state => (stage,isBuild) => {
		if(isBuild){
			return state.BuildStatus.getValue(stage);
		}
		else {
			return state.CrawlStatus.getValue(stage);
		}
	},

	getCustomer : state => {
		state.liveProjects.find((pro)=>{
			return pro.CustomerId === state.dnsDetails.customerId;
		})
	},

	getPublishCustomers: state => state.publishProject.customerList,

	getCustomerDetailsForLiveSite : state => state.customerDetails,

	getVerifyDomainDetails : state => state.verifyDomain,

	getDnsDetailsDetails : state => state.dnsDetails,

	getProjectTypes: state => state.projectType,

	getProjectsBySearchQuery: state => {
		return state.Projects.filter(function (item) {
			return item.ProjectName.toLowerCase().indexOf(state.projectSearchQuery) >= 0;
		})
	},

	getCrawlStatus : state => state.CrawlStatus,

	getBuildStatus : state => state.BuildStatus,

	getPendingDomains: state => state.dnsDetails.pendingDomains,

	getOptimizationError: state => state.optimizationError,

	renameProjectDetails: state => state.renameProject,

	isApiCallInProgress: state => state.isApiCallInProgress,

	selectedCloud: state => state.publishProject.cloudSelected,

	aliCloudDetails: state => state.publishProject.aliCloudDetails,

	gcpCloudDetails: state => state.publishProject.gcpCloudDetails,

	areCloudDetailsPresent: state => state.publishProject.cloudDetailsPresent,

	gcpTokenGeneratorUrl: state => state.publishProject.gcpTokenGeneratorUrl,

	gcpCredsFile: state => state.publishProject.gcpCredsFile,

	chooseOwnAccount: state => state.publishProject.chooseOwnAccount,

	lazyLoad: state => state.lazyLoad,

	deactivateProject: state => state.deactivateProject,

	domainForCloud: state => state.publishProject.domainForCloud,

	showNotificationLiveSite: state => state.showNotificationLiveSite
};

// actions
const actions = {

	downloadURI ({ commit },payload){
		let link = document.getElementById("kdownload");
		link.href = "";
		link.href = payload;
		link.click();
		let project = state.Projects.filter(function(project){
			return project.ProjectId == state.downloadProject.projectId
		})[0];
		let processProject = {
			project,
			status: 0
		};
		commit(types.setProjectStatus, processProject)
	},

	getAllProjects ({ commit,dispatch }, payload) {
		commit(types.setLazyLoadDetails, { stop: false, showLoader: true });
		projects.getAllProjectsfromApi(payload, (sucess, { Projects }) => {
			if(sucess) {
				const lazyLoad = payload ? payload.lazyLoad : false;
				commit(types.getListOfAllProjects, { payload: Projects, lazyLoad: lazyLoad });
				commit(types.projectListReceived);
				commit(types.setCardsSkeletonLoaderComponentStatus, false);
				dispatch('startPollingForProjectsInProcess');
				const { uploadFiles } = store.getters;
				if(uploadFiles.length === 0) {
					store.dispatch('toggleLoader', false);
					store.dispatch('toggleActionModal');
				}
				if (lazyLoad) {
					if(Projects.length === 0 || Projects.length < payload.limit) {
						commit(types.setLazyLoadDetails, { stop: true, showLoader: false });
					} else {
						commit(types.setLazyLoadDetails, { stop: false, showLoader: false });
					}
				} else {
					commit(types.setLazyLoadDetails, { stop: false, showLoader: false });
				}
			} else {
				store.dispatch('toggleLoader', false);
				store.dispatch('toggleActionModal');
			}

		})
	},

	sortProjectsByCreatedOn({ commit }, payload) {
		commit(types.sortAllProjectsByCreatedOn, payload)
	},

	optimizeProject({ commit,dispatch }) {
		commit(types.setIsApiCallCompletedInOptimizeProject,false);
		projects.optimizeProject(state.optimizeProject.projectId ,(success, response)=>{
			commit(types.setIsApiCallCompletedInOptimizeProject,true);
			if(success && response){
				commit(types.setIsApiCallSuccessfullInOptimizeProject,true);
				commit(types.setCardsSkeletonLoaderComponentStatus,true);
				dispatch('startPollingForProjectsInProcess');
				dispatch('checkBuildStats');
			}else{
				commit(types.setIsApiCallSuccessfullInOptimizeProject,false);
			}
		})
	},

	checkBuildStats({ commit, dispatch }) {
		projects.checkBuildStats(state.optimizeProject.projectId, (success, response) => {
			if(success) {
				const { Stage, IsCompleted, Error } = response;
				if(IsCompleted) {
					return;
				}
				if(Stage >= 0 && Stage < 9) {
					setTimeout(() => {
						dispatch('checkBuildStats');
					}, 1000)
				}
				if(Stage < 0) {
					if(isProdEnv) {
						dispatch('triggerEmail', {
							To: [coreEmailId],
							Subject: `[URGENT] build failed for ${store.state.app.UserEmail} on ${new Date().toLocaleString()}`,
							EmailBody: `Project Name: ${state.optimizeProject.projectName}
							<br />Project Id: ${state.optimizeProject.projectId}
							<br />Developer Phone: ${store.state.user.user.PhoneNumber}
							<br /><pre>${JSON.stringify(Error, null, 2)}</pre>`
						});
					}
					commit(types.setOptimizeErrorStatus, true);
				}
			} else {
				//TODO handler
				console.log(response);
			}
		})
	},

	checkProjectCanBeDeletedOtNot({ commit,dispatch }) {
		projects.checkProjectIsPublishedOrNot(store.state.app.deleteProject.projectId,(success,published)=>{
			if (success) {
				if (!published) {
					dispatch('deleteProject');
				}
				else {
					commit(types.setIsProjectPublishedInDeleteProject,true);
				}
			}
			else {
				dispatch('showToastr', {
					isError : true,
					title: 'Error',
					message: 'error while checking project status.'
				})
			}
		})
	},

	showToastr({ dispatch }, payload) {
		const { toasterTypes } = store.state.toastr;
		let toastrMessage = {
			type : '',
			title: '',
			message : ''
		};
		if (payload.isError) {
			toastrMessage.type = toasterTypes.ERROR;
		} else {
			toastrMessage.type = toasterTypes.SUCCESS;
		}
		toastrMessage.title = payload.title ? payload.title : "";
		toastrMessage.message = payload.message ? payload.message : "";
		dispatch('addToaster', toastrMessage);
	},

	deleteProject({ commit,dispatch }) {
		projects.deleteProject(store.state.app.deleteProject.projectId,(success, res)=>{
			let toasterTypes = store.state.toastr.toasterTypes;
			if(success && res){
				commit(types.setDeleteProjectComponentStatus, false);
				commit(types.updateOverlayComponent, false);
				dispatch('getAllProjects');
				commit(types.setCardsSkeletonLoaderComponentStatus,true);
				dispatch('addToaster',{
					type : toasterTypes.SUCCESS,
					title : 'project successfully deleted',
					message: store.state.app.deleteProject.projectName
				});
			}
			else {
				dispatch('addToaster',{
					type : toasterTypes.ERROR,
					title : 'project deletetion failed',
					message: store.state.app.deleteProject.projectName
				});
			}
		})
	},

	setUploadDetails ({ commit }, payload) {
		commit(types.setUploadDetails, payload)
	},

	checkEligibleForPublishingAndGetCustomerList({ commit,dispatch }){
		let userBalance = store.getters.getUserNetBalance;
		if(userBalance >= 100){
			commit(types.setEligibleForPublish,true);
			dispatch('getCustomerList');
		}else{
			commit(types.setEligibleForPublish,false);
		}
	},

	getCustomerList ({ commit, dispatch }, payload) {
		if (!payload || !payload.limit) {
			commit(types.setIsGettingListOfCustomersForPublishProject, true);
		}
		payload = {
			...payload,
			projectId: state.publishProject.projectId
		};
		projects.customerListForProject(payload, (success, response) => {
			if (!payload || !payload.limit) {
				commit(types.setIsGettingListOfCustomersForPublishProject, false);
			}
			if (success) {
				commit(types.setCustomerListInProjectDetails, response.Websites);
				if(response.Websites.length === 0 || response.Websites.length < payload.limit) {
					commit(types.setLazyLoadDetails, { stop: true, showLoader: false });
				} else {
					commit(types.setLazyLoadDetails, { stop: false, showLoader: false });
				}
			} else {
				dispatch('showToastr', {
					isError: true,
					title : 'ERROR',
					message : 'error while fetching customer list.'
				})
			}
		})
	},

	setPublishProjectDetails ( { commit }, payload ) {
		commit(types.setPublishProjectDetails, payload);
	},

	setPreviewProjectDetails( { commit }, payload ) {
		commit(types.setProjectIdInProjectPreview, payload)
	},

	publish({ dispatch }){
		const { isNewCustomer } = state.publishProject;
		if(isNewCustomer){
			dispatch('createNewCustomer');
		}
		else {
			dispatch('publishNewProject');
		}
	},

	createNewCustomer ({ commit, dispatch }) {
		commit(types.setIsCreatingNewCustomerInPublishProject, true);
		const { newCustomerDetails, cloudSelected } = state.publishProject;
		const payload = {
			...newCustomerDetails,
			CloudProviderType: cloudSelected
		};
		projects.createCustomer(payload, (success, response)=>{
			commit(types.setIsCreatingNewCustomerInPublishProject, false);
			if(success){
				commit(types.setIsCustomerCreationErrorForPublishProject, false);
				commit(types.setCustomerForPublishProject, { WebsiteId : response });
				dispatch('publishNewProject');
			}
			else {
				commit(types.setIsCreatingNewCustomerInPublishProject, true);
				commit(types.setIsAPIErrorInPublishProject, true);
				commit(types.setIsCustomerCreationErrorForPublishProject, true);
				dispatch('showToastr', {
					isError : true,
					title:  'error',
					message: 'error while creating customer.'
				})
			}
		});

	},

	publishNewProject ( { commit,dispatch }) {
		commit(types.setIsPublishingForPublishProject, true);
		commit(types.setStageForPublishProject, 0);
		commit(types.setCardsSkeletonLoaderComponentStatus, true);
		projects.publishProject(state.publishProject.customerForPublishing.WebsiteId,(success, response)=>{
			if(success && !response.IsError){
				setTimeout(() => {
					dispatch('getAllProjects')
				}, 2000);
				setTimeout(() => {
					dispatch('getAllLiveProjects')
				}, 4000)

				if(isProdEnv) {
					const publishUrl = state.publishProject.isNewCustomer
						? `${state.publishProject.newCustomerDetails.WebsiteTag}.getkitsune.com`
						: `${state.publishProject.customerForPublishing.WebsiteDomain}`;
					dispatch('triggerEmail', {
						To: [coreEmailId],
						Subject: `publish success for ${store.state.app.UserEmail} on ${new Date().toLocaleString()}`,
						EmailBody: `Project Name: ${state.publishProject.projectName}
						<br>Project Id: ${state.publishProject.projectId}
						<br>Publish Url: ${publishUrl}`
					})
				}
			}else{
				commit(types.setIsAPIErrorInPublishProject, true);
				commit(types.setCardsSkeletonLoaderComponentStatus, false);
				const { Message : message } = response;
				dispatch('showToastr',{
					isError : true,
					title : 'error in pubishing',
					message : message
				});
			}
		});
	},

	getPendingDomains({ commit }) {
		return new Promise( (resolve, reject) => {
			projects.getPendingDomains(state.dnsDetails.websiteId, (success, response) => {
				if(success) {
					resolve();
					commit(types.setPendingDomains, response.RequestedDomains)
				}else {
					reject();
					//TODO error handler
				}
			})
		})
	},

	updateDomain({ commit }, payload) {
		return new Promise((resolve, reject) => {
			projects.updateDomain(payload, (success, response) => {
				if(success) {
					resolve();
				}else {
					reject();
					//TODO error handler
				}
			})
		})
	},

	setStageForPublishing({ commit }, payload) {
		commit(types.setStageForPublishProject,payload);
	},

	resetStoreForPublishingProject ({ commit }) {
		commit(types.resetStoreForPublishProject);
	},

	setCustomerForPublishingProject ({ commit },payload) {
		commit(types.setCustomerForPublishProject,payload);
	},

	setDomainNameForNewCustomerInPublishProject ({ commit },payload) {
		commit(types.setDomainNameForNewCustomerInPublishProject,payload);
	},

	setIsNewCustomerInPublishProject ({ commit },payload) {
		commit(types.setIsNewCustomerInPublishProject,payload);
	},

	setCustomerDetailsForNewCustomerInPublishProject ({ commit }, payload) {
		commit(types.setCustomerDetailsForNewCustomerInPublishProject,payload);
	},

	setIsCreatingNewCustomerInPublishProject ({ commit },payload) {
		commit(types.setIsCreatingNewCustomerInPublishProject,payload);
	},

	getAllLiveProjects({ commit, dispatch }, payload) {
		commit(types.setLazyLoadDetails, { stop: false, showLoader: true });
		projects.getAllLiveProjects(payload, (success, response) => {
			if(success) {
				const { PageSize, TotalCount, LiveWebsites } = response;
				const lazyLoad = payload && payload.lazyLoad;
				if(lazyLoad) {
					if(TotalCount < PageSize || LiveWebsites.length === 0) {
						commit(types.setLazyLoadDetails, { stop: true, showLoader: false });
					} else {
						commit(types.setLazyLoadDetails, { stop: false, showLoader: false });
					}
				} else {
					commit(types.setLazyLoadDetails, { stop: false, showLoader: false })
				}
				commit(types.getListOfAllLiveProjects, { payload: LiveWebsites, lazyLoad });
			} else {
				commit(types.setLazyLoadDetails, { stop: false, showLoader: false });
				dispatch('showToastr', {
					isError: true,
					title : 'error',
					message: 'error while fetching live sites.'
				})
			}
		})
	},

	requestProjectDownloadLink ({ commit,dispatch },payload){
		let project = state.Projects.filter(function(project){
			return project.ProjectId == state.downloadProject.projectId
		})[0];
		let processProject = {
			project,
			status: 10
		}
		commit(types.setProjectStatus, processProject)
		commit(types.setIsLinkRequestedInDownloadProject,true);
		projects.getProjectDownlink({ projectId : state.downloadProject.projectId },(success,response)=>{
			commit(types.setIsLinkRequestedInDownloadProject,false);
			if(success && !response.IsError){
				if(response.DownloadUrl != null || response.DownloadUrl != undefined){
					commit(types.setDownloadLinkInDownloadProject,response.DownloadUrl);
					dispatch('downloadURI',state.downloadProject.downloadLink);
				}
				else{
					commit(types.setIsPollingInDownloadProject,true);
					dispatch('getRequestedProjectDownloadLinkStatus');
				}
				//TODO HANDLE ERROR
			} else {
				let processProject = {
					project,
					status: 0
				};
				commit(types.setProjectStatus, processProject)
			}
		})
	},

	setProjectIdInDownloadProject ({ commit },payload){
		commit(types.setProjectIdInDownloadProject,payload);
	},

	getRequestedProjectDownloadLinkStatus ( { commit, dispatch },payload ){
		projects.getProjectDownlinkStatus({ projectId : state.downloadProject.projectId },(success,response)=>{
			if(success){
				if(response.LinkUrl != null){
					commit(types.setDownloadLinkInDownloadProject,response.LinkUrl);
					commit(types.setIsPollingInDownloadProject,false);
					dispatch('downloadURI',state.downloadProject.downloadLink);
				}else{

					setTimeout(() => {
						dispatch('getRequestedProjectDownloadLinkStatus');
					}, 3000)

				}
			}
		})
	},

	startPollingForProjectsInProcess({ commit,dispatch }){
		if(!state.IsPollingForProjectsInProcess){
			commit(types.setIsPollingForProjectsInProcess,true);
			dispatch('getAllProjectsUnderProcessing');
		}
	},

	getAllProjectsUnderProcessing({ commit,dispatch }) {
		projects.getProjectsInProcess((success,response)=>{
			commit(types.setPollingCompletedForBaseApiCalls,true);
			if(success){
				dispatch('updateAllProjectsForCurrentStage',response);
			}
			else{
				//TODO error handler
			}
		})
	},

	updateAllProjectsForCurrentStage({ commit,dispatch }, payload){
		const projects = payload.Projects;
		let projectDownloadStatus = state.projectStatus.PREPARINGDOWNLOAD;
		commit(types.setCardsSkeletonLoaderComponentStatus, false);

		if(projects.length != 0){

			for(let i=0; i<state.Projects.length;i++){

				let projectInProcess = state.Projects[i];
				let project = projects.find((pro)=>{
					return pro.ProjectId == projectInProcess.ProjectId;
				})
				if(project != undefined){
					commit(types.setProjectStatus,{ project : projectInProcess, status : project.ProjectStatus });
					switch(project.ProjectStatus){
					case state.projectStatus.BUILDING.key:
						commit(types.setProjectStage,{ project: projectInProcess, stage: project.BuildStage })
						break;
					case state.projectStatus.CRAWLING.key:
						commit(types.setProjectStage,{ project: projectInProcess, stage: project.CrawlStage })
						break;
					default:
						commit(types.setProjectStage, { project: projectInProcess, stage: null } )
						break;
					}
				}
				else{
					if(projectInProcess.ProjectStatus != projectDownloadStatus.key &&
						projectInProcess.ProjectStatus != projectDownloadStatus.value){
						commit(types.setProjectStatus,{ project : projectInProcess, status : state.projectStatus.IDLE.value });
					}
				}

			}

			setTimeout(() => {
				dispatch('getAllProjectsUnderProcessing')
			}, 3000)

		}
		else {
			commit(types.setIsPollingForProjectsInProcess,false);
			dispatch('resetStagesOfAllProjects');
		}

	},

	resetStagesOfAllProjects({ commit }){
		let projects = state.Projects;
		let projectDownloadStatus = state.projectStatus.PREPARINGDOWNLOAD;
		for(let i=0; i<projects.length;i++){
			let currentProject = projects[i];
			if(currentProject.ProjectStatus != projectDownloadStatus.key &&
				currentProject.ProjectStatus != projectDownloadStatus.value)
				commit(types.setProjectStatus, { project: currentProject, status: state.projectStatus.IDLE.value });
		}
	},

	setProjectIdAndNameForOptimizeProjectAndStartBuild({ commit,dispatch }, payload) {
		commit(types.setProjectIdAndNameInOptimizeProject,payload);
		dispatch('optimizeProject');
	},

	getCustomerDetails({ commit }){
		commit(types.setIsFetchingCustomerDetailsInCustomerDetails,true);
		projects.getCustomerDetails({ websiteId : state.customerDetails.customerId }, (success,response)=>{
			commit(types.setIsFetchingCustomerDetailsInCustomerDetails,false);
			if(success){
				commit(types.setAreDetailsFetchedSuccessfullyInCustomerDetails,true);
				commit(types.setCustomerInCustomerDetails,response);
			}else{
				commit(types.setAreDetailsFetchedSuccessfullyInCustomerDetails,false);
			}
		})
	},

	setCustomerIdForCustomerdetails({ commit },payload){
		commit(types.resetCustomerDetails);
		commit(types.setCustomerIdInCustomerDetails,payload);
	},

	getMapDomainMappingAndMap({ commit }){
		commit(types.setIsFetchingDetailsInVerifyDomain,true);
		projects.checkAndMapDomain({ customerId : state.dnsDetails.websiteId },(success,response)=>{
			commit(types.setIsFetchingDetailsInVerifyDomain,false);
			if(success){
				commit(types.setdomainDetailsInVerifyDomain,response);
				commit(types.setIsApiCallSuccessInVerifyDomain,true);
			}
			else{
				commit(types.setIsApiCallSuccessInVerifyDomain,false);
			}
		})
	},

	setCustomerIdForVerifyDomain({ commit },payload){
		commit(types.resetVerifyDomain);
		commit(types.setcustomerIdInVerifyDomain,payload);
	},

	setWebsiteIdInDnsDetails({ commit },payload){
		commit(types.setWebsiteIdInDnsDetails,'');
		commit(types.setWebsiteIdInDnsDetails,payload);
	},

	setProjectSearchQueryInProjects({ commit },payload){
		commit(types.setProjectSearchQueryInProjects,payload);
	},

	getCustomerListForProjectPreview ({ commit, dispatch }){
		commit(types.setIsFetchingDetailsInProjectPreview,true);
		projects.customerListForProject(state.previewProject.projectId,(success,response)=>{
			commit(types.setIsFetchingDetailsInProjectPreview,false);
			if(success){
				const { Customers } = response;
				commit(types.setCustomerListInProjectPreview, Customers);
				dispatch('showPreviewModal', true);
			}else {
				// TODO error handler
			}
		})
	},

	checkIfWebsiteTagExists({ commit }, payload) {
		return new Promise((resolve, reject) => {
			projects.checkIfWebsiteTagExists(payload, (success, response) => {
				if(success) {
					resolve(response);
				}else {
					reject(response);
					// TODO error handler
				}
			})
		})
	},

	setOptimizeErrorStatus({ commit }, payload) {
		commit(types.setOptimizeErrorStatus, payload);
	},

	setRenameProject({ commit }, payload) {
		commit(types.setProjectForRenaming, payload);
	},

	renameProject({ dispatch, commit }, payload) {
		commit(types.apiCallProgressStatus, true);
		return new Promise((resolve, reject) => {
			projects.renameProject(payload, (success, response) => {
				if(success) {
					const { ProjectName } = payload;
					dispatch('showToastr', {
						isError: false,
						title : 'project renamed',
						message : `'${ProjectName}' successfully renamed.`
					});
					commit(types.setCardsSkeletonLoaderComponentStatus, true);
					dispatch('getAllProjects');
					resolve();
				} else {
					dispatch('showToastr', {
						isError: true,
						title : 'ERROR',
						message : 'error while renaming project'
					});
					reject(response);
				}
				commit(types.apiCallProgressStatus, false);
			})
		});
	},

	setSelectedCloud({ commit }, payload) {
		commit(types.setSelectedCloud, payload);
	},

	setAliCloudDetails({ commit }, payload) {
		commit(types.setAliCloudDetails, payload);
	},

	setGCPDetails({ commit }, payload) {
		commit(types.setGCPDetails, payload);
	},

	getKAdminUrl({ commit }, payload) {
		return new Promise((resolve, reject) => {
			projects.getKAdminUrl(payload, (success, response) => {
				success ? resolve(response) : reject(response);
			})
		})
	},

	publishToCloud({ dispatch }, payload) {
		const { isNewCustomer } = state.publishProject;
		if(isNewCustomer) {
			projects.publishToCloud(payload, (success, response) => {
				if(success) {
					dispatch('publish');
					const { cloudSelected, aliCloudDetails, newCustomerDetails } = store.state.projects.publishProject;
					if(Object.keys(aliCloudDetails).length && cloudSelected == cloudOptionsMap.get(1)) {
						if(isProdEnv) {
							let data = { ...aliCloudDetails, ...newCustomerDetails };
							dispatch('triggerEmail', {
								To: mailIdsForClouds[cloudSelected],
								Subject: `cloud details entered by ${store.state.app.UserEmail}`,
								EmailBody: `<pre>
								${JSON.stringify(data, null, 2)}
							</pre>`
							})
						}
					}
				} else {
					dispatch('showToastr', {
						isError: true,
						title : 'error while publishing',
						message : `could not publish to ${payload.provider}`
					});
				}
			})
		} else {
			dispatch('publish');
		}
	},

	getCloudProviderDetails({ commit }, payload) {
		projects.getCloudProviderDetails(payload, (success, response) => {
			if(success && response.data) {
				commit(types.setSelectedCloud, cloudOptionsMap.get(response.data.provider));
				commit(types.setCloudDetailsPresent, true);
			} else {
				commit(types.setCloudDetailsPresent, false);
			}
		})
	},

	resetCloudDetails({ commit }) {
		commit(types.resetCloudDetails);
	},

	getGCPTokenGeneratorUrl({ commit }) {
		projects.getUrlForGCPTokenGeneration((success, { auth }) => {
			if(success) {
				commit(types.gcpTokenGeneratorUrl, auth);
			}
		})
	},

	setGCPCredsFile({ commit }, payload) {
		commit(types.setGCPCredsFile, payload);
	},

	setChooseOwnAccount({ commit }, payload) {
		commit(types.setChooseOwnAccount, payload);
	},

	setDeactivateSiteDetails({ commit }, payload) {
		commit(types.setDeactivateSiteDetails, payload);
	},

	deactivateSite({ commit, dispatch }, event) {
		commit(types.apiCallProgressStatus, true);
		const { websiteIds, name } = state.deactivateProject;
		projects.deactivateSite(websiteIds, (success, response) => {
			commit(types.apiCallProgressStatus, false);
			dispatch('toggleStatus', { components: ['overlay', 'deactivateSite'], event });
			if(success) {
				dispatch('getAllLiveProjects');
				dispatch('showToastr', {
					isError : false,
					title: `${name}`,
					message: 'successfully deactivated the site.'
				})
			} else {
				dispatch('showToastr', {
					isError : true,
					title: `${name}`,
					message: 'unable to deactivate the site.'
				})
			}
		})
	},

	setDomainForCloud({ commit }, payload) {
		commit(types.setDomainForCloud, payload);
	},

	setShowNotificationLiveSite({ commit }, payload) {
		commit(types.setShowNotificationLiveSite, payload);
	}
};

// mutations
const mutations = {

	[types.getListOfAllProjects] (state, { payload, lazyLoad }) {
		state.Projects = lazyLoad ? [...state.Projects, ...payload] : payload;
	},

	[types.getListOfAllLiveProjects] (state, { payload, lazyLoad }) {
		state.liveProjects = lazyLoad ? [...state.liveProjects, ...payload] : payload;
	},

	[types.sortAllProjectsByCreatedOn] (state, payload) {
		if(payload)
			state.Projects.sort( (a,b)=> {return new Date(a.CreatedOn).getTime() - new Date(b.CreatedOn).getTime();}  );
		else
			state.Projects.sort( (a,b)=> {return new Date(b.CreatedOn).getTime() - new Date(a.CreatedOn).getTime();}  );
	},

	[types.setUploadDetails] (state, { details, totalFiles, resetFlag }) {
		state.uploadDetails = resetFlag ? ({
			projectId: '',
			isUploading: false,
			totalFiles: 0,
			filesUploaded: 0,
			filesFailed: []
		}) : { ...state.uploadDetails, ...details, totalFiles };
	},

	[types.setDeleteProjectComponentStatus] (state,payload) {
		store.state.app.componentIsActive.deleteproject = payload;
	},

	[types.setPublishProjectDetails] (state,payload) {
		state.publishProject.projectId = payload.projectId;
		state.publishProject.projectName = payload.projectName;
	},

	[types.setProjectIdInProjectPreview] (state, payload) {
		state.previewProject.projectId = payload.projectId;
		state.previewProject.projectName = payload.projectName;
	},

	[types.setIsGettingListOfCustomersForPublishProject] (state,payload) {
		state.publishProject.isGettingCustomerList = payload;
	},

	[types.setIsFetchingDetailsInProjectPreview] (state, payload) {
		state.previewProject.isFetchingDetails = payload;
	},

	[types.setCustomerListInProjectDetails] (state,payload) {
		state.publishProject.customerList = [
			...state.publishProject.customerList,
			...payload
		];
	},

	[types.setCustomerListInProjectPreview] (state, payload) {
		state.previewProject.customerList = payload;
	},

	[types.setIsPublishingForPublishProject] (state,payload) {
		state.publishProject.isPublishing = payload;
	},

	[types.setCustomerForPublishProject] (state, payload){
		state.publishProject.customerForPublishing = payload;
	},

	[types.setStageForPublishProject] (state,payload){
		state.publishProject.stage = payload;
	},

	[types.setDomainNameForNewCustomerInPublishProject] (state,payload){
		const { subDomain, domain } = payload;
		state.publishProject.newCustomerDetails =
			{ ...state.publishProject.newCustomerDetails, WebsiteTag: subDomain, Domain: domain };
	},

	[types.setPendingDomains] (state, payload) {
		state.dnsDetails.pendingDomains = payload;
	},

	[types.setIsNewCustomerInPublishProject] (state,payload){
		state.publishProject.isNewCustomer = payload;
	},

	[types.setCustomerDetailsForNewCustomerInPublishProject] (state, payload){
		let developerId = axios.defaults.headers.common['Authorization'];
		state.publishProject.newCustomerDetails.FullName = payload.customerName;
		state.publishProject.newCustomerDetails.Email = payload.customerEmail;
		state.publishProject.newCustomerDetails.ProjectId = state.publishProject.projectId;
		state.publishProject.newCustomerDetails.PhoneNumber = payload.phoneNumber;
		state.publishProject.newCustomerDetails.DeveloperId = developerId;
	},

	[types.resetStoreForPublishProject] (state){
		const { publishProject } = state;
		publishProject.stage = 0;
		publishProject.isPublishing = false;
		publishProject.customerForPublishing = '';
		publishProject.customerList = [];
		publishProject.isGettingCustomerList = false;
		publishProject.projectName = '';
		publishProject.projectId = '';
		publishProject.isCreatingCustomer = false;
		publishProject.isNewCustomer = false;
		publishProject.newCustomerDetails.DeveloperId = '';
		publishProject.newCustomerDetails.Email = '';
		publishProject.newCustomerDetails.PhoneNumber = '';
		publishProject.newCustomerDetails.FullName = '';
		publishProject.newCustomerDetails.ProjectId = '';
		publishProject.newCustomerDetails.WebsiteTag = '';
		publishProject.isAPIError = false;
		publishProject.isEligibleForPublishing = false;
		publishProject.isCustomerCreationError = false;
	},

	[types.setIsCreatingNewCustomerInPublishProject] (state,payload) {
		state.publishProject.isCreatingCustomer = payload;
	},

	[types.setIsAPIErrorInPublishProject] (state,payload){
		state.publishProject.isAPIError = payload;
	},

	[types.setProjectStatus] (state,payload){
		Vue.set(payload.project, 'ProjectStatus' , payload.status);
	},

	[types.setProjectStage] (state,payload){
		Vue.set(payload.project, 'ProjectStage' , payload.stage);
	},

	[types.setIsPollingForProjectsInProcess] (state,payload) {
		state.IsPollingForProjectsInProcess = payload;
	},

	[types.setPollingCompletedForBaseApiCalls] (payload) {
		store.state.app.baseApiCalls.pollingProjectsInProcess = payload;
	},

	[types.setIsLinkRequestedInDownloadProject] (state,payload){
		state.downloadProject.isLinkRequested = payload;
	},

	[types.setProjectIdInDownloadProject] (state,payload){
		state.downloadProject.projectId = payload;
	},

	[types.setIsPollingInDownloadProject] (state,payload){
		state.downloadProject.isPolling = payload;
	},

	[types.setDownloadLinkInDownloadProject] (state,payload){
		state.downloadProject.downloadLink = payload;
	},

	[types.setIsApiCallErrorInDownloadProject] (state,payload){
		state.downloadProject.isApiCallError = payload;
	},

	[types.setProjectIdAndNameInOptimizeProject] (state,payload) {
		state.optimizeProject.projectId = payload.projectId;
		state.optimizeProject.projectName = payload.projectName;
	},

	[types.setIsApiCallCompletedInOptimizeProject] (state,payload) {
		state.optimizeProject.isApiCallCompleted = payload;
	},

	[types.setIsApiCallSuccessfullInOptimizeProject] (state,payload) {
		state.optimizeProject.isApiCallSuccess = payload;
	},

	[types.setOptimizeProjectInComponentStatus] (state,payload){
		store.state.app.componentIsActive.optimizeProject = payload;
	},

	[types.setCustomerInCustomerDetails] (state, payload) {
		state.customerDetails.customer = payload;
	},

	[types.setIsFetchingCustomerDetailsInCustomerDetails] (state,payload){
		state.customerDetails.isFetchingCustomerDetails = payload;
	},

	[types.setAreDetailsFetchedSuccessfullyInCustomerDetails] (state,payload){
		state.customerDetails.areDetailsFetchedSuccessfully = payload;
	},

	[types.setCustomerIdInCustomerDetails] (state,payload){
		state.customerDetails.customerId = payload;
	},

	[types.resetCustomerDetails] (state) {
		state.customerDetails.customerId = '';
		state.customerDetails.areDetailsFetchedSuccessfully = false;
		state.customerDetails.isFetchingCustomerDetails = false;
		state.customerDetails.customer = {};
	},

	[types.setcustomerIdInVerifyDomain] (state,payload){
		state.verifyDomain.customerId = payload;
	},

	[types.setdomainDetailsInVerifyDomain] (state,payload){
		state.verifyDomain.domainDetails = payload;
	},

	[types.setIsApiCallSuccessInVerifyDomain] (state,payload){
		state.verifyDomain.isApiCallSuccess = payload;
	},

	[types.setIsFetchingDetailsInVerifyDomain] (state,payload){
		state.verifyDomain.isFetchingDetails = payload;
	},

	[types.resetVerifyDomain] (state){
		state.verifyDomain.isFetchingDetails = false;
		state.verifyDomain.isApiCallSuccess = false;
		state.verifyDomain.customerId = '';
		state.verifyDomain.domainDetails = {};
	},

	[types.setWebsiteIdInDnsDetails] (state,payload){
		state.dnsDetails.websiteId = payload;
	},

	[types.setEligibleForPublish] (state,payload){
		state.publishProject.isEligibleForPublishing = payload;
	},

	[types.setProjectSearchQueryInProjects] (state,payload){
		state.projectSearchQuery = payload;
	},

	[types.setIsCustomerCreationErrorForPublishProject] (state, payload) {
		state.publishProject.isCustomerCreationError = payload;
	},

	[types.setOptimizeErrorStatus] (state, payload) {
		state.optimizationError = payload;
	},

	[types.setProjectForRenaming] (state, payload) {
		state.renameProject = payload;
	},

	[types.apiCallProgressStatus] (state, payload) {
		state.isApiCallInProgress = payload;
	},

	[types.addFailedFiles] (state, payload) {
		state.uploadDetails.filesFailed.push(payload);
	},

	[types.setUploadedFilesCount] (state, payload) {
		state.uploadDetails.filesUploaded = payload ? payload : ++state.uploadDetails.filesUploaded;
	},

	[types.setSelectedCloud] (state, payload) {
		state.publishProject.cloudSelected = payload;
	},

	[types.setAliCloudDetails] (state, payload) {
		state.publishProject.aliCloudDetails = { ...payload }
	},

	[types.setGCPDetails] (state, payload) {
		state.publishProject.gcpCloudDetails = { ...payload };
	},

	[types.setCloudDetailsPresent] (state, payload) {
		state.publishProject.cloudDetailsPresent = payload;
	},

	[types.resetCloudDetails] (state) {
		state.publishProject.aliCloudDetails = {};
		state.publishProject.cloudDetailsPresent = false;
		state.publishProject.cloudSelected = '';
		state.publishProject.gcpCloudDetails = {};
		state.publishProject.gcpTokenGeneratorUrl = '';
		state.publishProject.gcpCredsFile = null;
		state.publishProject.chooseOwnAccount = null;
	},

	[types.gcpTokenGeneratorUrl] (state, payload) {
		state.publishProject.gcpTokenGeneratorUrl = payload
	},

	[types.setGCPCredsFile] (state, payload) {
		state.publishProject.gcpCredsFile = payload;
	},

	[types.setChooseOwnAccount] (state, payload) {
		state.publishProject.chooseOwnAccount = payload;
	},

	[types.setLazyLoadDetails] (state, payload) {
		state.lazyLoad = { ...payload };
	},

	[types.setDeactivateSiteDetails] (state, payload) {
		state.deactivateProject = payload;
	},

	[types.setDomainForCloud] (state, payload) {
		state.publishProject.domainForCloud = payload;
	},

	[types.setShowNotificationLiveSite] (state, payload) {
		state.showNotificationLiveSite = payload;
	}
};

export default {
	state,
	getters,
	actions,
	mutations
}
