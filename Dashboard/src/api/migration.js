import * as constants from '../../config/config'
import axios from 'axios'
import store from '../store/index'

export default {

	startCrawling(payload, callback) {
		if(payload!=null || payload!=undefined){
			axios.post(`${constants.apiBaseUrl}${constants.startCrawl}`, {
				IsDeepKrawl: false,
				Url: payload.Url,
				UserEmail: store.state.app.UserEmail
			})
				.then(response => {
					if (response.status === 200){
						callback(true, response.data);
					} else {
						callback(false, response.data);
					}
				})
				.catch(err => {
					callback(false, err);
				})
		}
		else {
			// TODO error
		}

	},

	getAnalyzeDetails(payload, callback) {
		if(payload!=null || payload!=undefined) {
			axios.get(`${constants.apiBaseUrl}${constants.getAnalysisDetails
				.replace('{projectId}', payload)
				.replace('{userEmail}', store.state.app.UserEmail)
				}`)
				.then(response => {
					if (response.status === 200) {
						callback(true, response.data);
					} else {
						callback(false, response.data);
					}
				})
				.catch(err => {
					callback(false, err);
				})
		}else{
			// TODO log error
		}
	},

	getListDomainsFound(payload, callback) {
		if(payload!=null || payload!=undefined) {
			axios.get(`${constants.apiBaseUrl}${constants.getDomainsFoundOnSite
				.replace('{projectId}', payload)
				.replace('{userEmail}', store.state.app.UserEmail)
				}`)
				.then(response => {
					if (response.status === 200) {
						callback(true, response.data);
					} else {
						callback(false, response.data);
					}
				})
				.catch(err => {
					callback(false, err);
				})
		}else{
			// TODO error
		}
	},

	saveSelectedDomains (payload,callback) {
		if(payload!=null || payload!=undefined) {
			axios.post(`${constants.apiBaseUrl}${constants.saveSelectedDomains}`, {
				ProjectId: payload.projectId,
				Domains: payload.selectedDomains,
				UserEmail: store.state.app.UserEmail
			})
				.then(response => {
					if (response.status === 200) {
						callback(true, response.data);
					} else {
						callback(false, response.data);
					}
				})
				.catch(err => {
					callback(false, err);
				})
		}else{
			// TODO error handling
		}
	},

	getDownloadDetails (payload,callback) {
		if(payload!=null || payload!=undefined) {
			axios.get(`${constants.apiBaseUrl}${constants.getDownloadDetails
				.replace('{projectId}', payload)
				.replace('{userEmail}', store.state.app.UserEmail)
				}`)
				.then(response => {
					if (response.status === 200) {
						callback(true, response.data);
					} else {
						callback(false, response.data);
					}
				})
				.catch(err => {
					callback(false, err);
				})
		}else{
			// TODO error
		}
	},

	getFilesReplacedDeatils (payload,callback) {
		if(payload!=null || payload!=undefined) {
			axios.get(`${constants.apiBaseUrl}${constants.getReplacedLinks
				.replace('{projectId}', payload)
				}`)
				.then(response => {
					if (response.status === 200) {
						callback(true, response.data);
					} else {
						callback(false, response.data);
					}
				})
				.catch(err => {
					callback(false, err);
				})
		}else{
			// TODO error
		}
	},

	getBuidStats (payload,callback) {
		if(payload!=null || payload!=undefined) {
			axios.get(`${constants.apiBaseUrl}${constants.getBuildStats
				.replace('{projectId}', payload)
				.replace('{userEmail}', store.state.app.UserEmail)
				}`)
				.then(({ status, data }) => {
					if (status === 200) {
						callback(true, data);
					} else {
						callback(false, status);
					}
				})
				.catch(err => {
					callback(false, err);
				})
		}else{
			// TODO error
		}
	},

	updateCrawlComplete (payload,callback) {
		axios.post(`${constants.apiBaseUrl}${constants.updateCrawlComplete
			.replace('{projectId}', payload)
			}`)
			.then(response => {
				if (response.status === 200) {
					callback(true, response.data);
				} else {
					callback(false, response.data);
				}
			})
			.catch(err => {
				callback(false, err);
			})
	},


}
