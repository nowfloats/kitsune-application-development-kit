import * as Constants from '../../config/config'
import axios from 'axios';
import store from '../store/index';
import { cloudOptionsMap } from '../../config/config';

export default {
	//TODO remove the email id
	//TODO Error handling and logging
	getAllProjectsfromApi ({ limit = 100, skip = 0 } = {}, callback) {
		axios.get(`${Constants.apiBaseUrl}${Constants.getAllProjects}`
		.replace('{userEmail}', store.state.app.UserEmail)
		.replace('{limit}', limit)
		.replace('{skip}', skip))
			.then(response => {
				callback(true, response.data);
			})
			.catch(err => {
				// TODO log the error
				callback(false, err);
			})
	},

	// TODO retreive useremail from store
	optimizeProject(projectId, callback) {
		axios.post(`${Constants.apiBaseUrl}${Constants.optimizeProject}${store.state.app.UserEmail}`, {
			ProjectId: projectId
		})
			.then(res => {
				callback(true, res.data);
			})
			.catch(err => {
				callback(false, err);
			})

	},

	checkBuildStats(projectId, callback) {
		axios.get(`${Constants.apiBaseUrl}${Constants.optimizeProject}${store.state.app.UserEmail}
		&projectId=${projectId}`)
			.then(({ data }) => {
				callback(true, data);
			})
			.catch(err => {
				callback(false, err);
			})
	},


	checkProjectIsPublishedOrNot (projectId,callback){
		axios.get(`${Constants.apiBaseUrl}${Constants.projectPublishedOrNot}${projectId}`)
			.then(res => {
				if (!res.data) {
					callback(true,false);
				}
				else {
					callback(true,true);
				}
			})
			.catch(err=>{
				callback(false,err);
			})
	},


	/**
	 * First api call is to check whether the project is published or not
	 * if Published : true dont delete project
	 * else : delete project*/
	deleteProject(projectId, callback) {
		axios.post(`${Constants.apiBaseUrl}${Constants.archiveProject}`, {
			CrawlId: projectId,
			UserEmail: store.state.app.UserEmail
		})
			.then(res => {
				callback(true, res);
			})
			.catch(err => {
				callback(false, err);
			})
	},

	customerListForProject({ projectId, limit = 100, skip = 0 } = {}, callback) {
		axios.get(`${Constants.apiBaseUrl}
		${Constants.customerListForProject
			.replace('{projectId}', projectId)
			.replace('{limit}', limit)
			.replace('{skip}', skip)}`)
			.then(response => {
				callback(true, response.data);
			})
			.catch(err => {
				callback(false, err);
			})
	},

	publishProject(customerId, callback){
		axios.post(`${Constants.apiBaseUrl}
		${Constants.publishProject.replace('{customerId}', customerId)
			.replace('{userEmail}', store.state.app.UserEmail)
			}`)
			.then(response => {
				callback(true, response.data);
			})
			.catch(err => {
				callback(false, err)
			})
	},

	getAllLiveProjects ({ limit = 100, skip = 0 } = {}, callback) {
		axios.get(`${Constants.apiBaseUrl}${Constants.getAllLivesProject}`
			.replace('{limit}', limit)
			.replace('{skip}', skip))
			.then(response => callback(true, response.data))
			.catch(err => callback(false, err))
	},

	checkIfWebsiteTagExists ({ subDomain, projectId }, callback) {
		axios.get(`${Constants.apiBaseUrl}${Constants.checkIfWebsiteTagExists}${subDomain}?projectId=${projectId}`)
			.then(response => {
				callback(true, response)
			})
			.catch(err => {
				callback(false, err)
			})
	},

	createCustomer (payload, callback) {
		axios.post(`${Constants.apiBaseUrl}${Constants.createNewCustomer}`, payload)
			.then((response) => {
				callback(true, response.data);
			})
			.catch((error) => {
				callback(false, error);
			})
	},

	updateDomain(payload, callback) {
		axios.post(`${Constants.apiBaseUrl}${Constants.updateDomain
			.replace('{customerId}', payload.websiteId)
			.replace('{domain}', payload.domain)}`)
			.then( response => callback(true, response.data))
			.catch( error => callback(false, error));
	},

	getProjectsInProcess(callback) {
		axios.get(`${Constants.apiBaseUrl}${Constants.projectsInProcess
			.replace('{userEmail}', store.state.app.UserEmail)}`)
			.then(response=>{
				callback(true,response.data);
			})
			.catch(error=>{
				callback(false,error);
			})
	},

	getPendingDomains(payload, callback) {
		axios.get(`${Constants.apiBaseUrl}${Constants.getPendingDomains}${payload}`)
			.then( response => callback(true, response.data))
			.catch( error => callback(false, error))
	},

	getProjectDownlink (payload,callback) {
		axios.post(`${Constants.apiBaseUrl}${Constants.getDownloadProjectLink 
			.replace('{projectId}',payload.projectId)
			.replace('{userEmail}',store.state.app.UserEmail)}`)
			.then(response=>{
				callback(true,response.data);
			})
			.catch(error=>{
				callback(false,error)
			})
	},

	getProjectDownlinkStatus (payload,callback) {
		axios.get(`${Constants.apiBaseUrl}${Constants.getDownloadProjectLinkStatus
			.replace('{projectId}',payload.projectId)}`)
			.then(response=>{
				callback(true,response.data);
			})
			.catch(error=>{
				callback(false,error);
			})
	},

	getCustomerDetails(payload,callback){
		axios.get(`${Constants.apiBaseUrl}${Constants.getCustomerDetails
			.replace("{websiteId}", payload.websiteId)}`)
			.then((response)=>{
				callback(true,response.data);
			})
			.catch((error)=>{
				callback(false,error);
			})
	},

	checkAndMapDomain(payload,callback){
		axios.post(`${Constants.apiBaseUrl}${Constants.checkAndMapDomain
			.replace('{customerId}',payload.customerId)}`)
			.then((response)=>{
				callback(true,response.data);
			})
			.catch((error)=>{
				callback(false,error);
			})
	},

	renameProject(payload, callback) {
		const { UserEmail } = store.state.app;
		axios.post(`${Constants.apiBaseUrl}${Constants.createProject}`, {
			...payload,
			UserEmail: UserEmail
		})
			.then(({ data }) => callback(true, data))
			.catch(err => callback(false, err))
	},

	getKAdminUrl(payload, callback){
		axios.get(`${Constants.apiBaseUrl}${Constants.kAdminUrl}${payload}`)
			.then(response => {
				callback(true, response);
			})
			.catch(error => {
				callback(false, error);
			})
	},

	publishToCloud(payload, callback) {
		const { cloudSelected } = store.state.projects.publishProject;
		let data = {};
		if(cloudSelected == cloudOptionsMap.get(1)) {
			data = { ...store.state.projects.publishProject.aliCloudDetails };
		} else if(cloudSelected == cloudOptionsMap.get(2)) {
			data = {
				...store.state.projects.publishProject.gcpCloudDetails,
				accountId: '',
				key: '',
				region: ''
			};
		}
		axios.post(`${Constants.apiBaseUrl}${Constants.publishToCloud}${payload}`, data)
			.then(response => callback(true, response))
			.catch(err => callback(false, err))
	},

	getCloudProviderDetails(payload, callback) {
		axios.get(`${Constants.apiBaseUrl}${Constants.getCloudProviderDetails}${payload}`)
			.then(response => {
				callback(true, response);
			})
			.catch(error => {
				callback(false, error);
			})
	},

	getUrlForGCPTokenGeneration(callback) {
		axios.get(`${Constants.gcpTokenGenerator}`)
			.then(({ data }) => callback(true, data))
			.catch(err => callback(false, err));
	},

	deactivateSite(payload, callback) {
		axios.post(`${Constants.apiBaseUrl}${Constants.deactivateSite}`, {
			websiteIds: payload
		})
			.then(({ data }) => callback(true, data))
			.catch(err => callback(false, err));
	}
}
