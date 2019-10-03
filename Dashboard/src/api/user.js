import * as Constants from '../../config/config'
import axios from 'axios';
import store from '../store/index'

export default {
	//TODO LOG THE ERROR
	getUserDetailsFromApi (callback) {
		axios.get(`${Constants.apiBaseUrl}${Constants.getUserDetails}${store.state.app.UserEmail}`)
			.then(response => {
				callback(true, response.data);
			})
			.catch(err => {
				callback(false, err)
			})
	},

	updateUserDetailsApi (payload,callback) {
		axios.post(`${Constants.apiBaseUrl}${Constants.updateUserDetails}`, payload)
			.then(response => {
				callback(true, response.data);
			})
			.catch(err => {
				callback(false, err)
			})
	},

	getUserIdFromDatabase (payload,callback) {
		axios.get(`${Constants.apiBaseUrl}${Constants.getUserId
			.replace('{userEmail}',store.state.app.UserEmail)}`)
			.then(response => {
				callback(true, response.data);
			})
			.catch(err => {
				callback(false, err)
			})
	},

	updateDataBaseForUserFirstTimeLoginIn (payload,callback) {
		axios.post(`${Constants.apiBaseUrl}${Constants.createDeveloperAccount}`, payload)
			.then(response => {
				callback(true, response.data);
			})
			.catch(error => {
				callback(false, error);
			})
	},

	getApiStatus(callback) {
		axios.get(`${Constants.checkApiStatus}`)
			.then(({ data }) => callback(true, data))
			.catch(err => callback(false, err));
	},

	getLowBalanceDetails(callback) {
		axios.get(`${Constants.apiBaseUrl}${Constants.getLowBalanceDetails}${store.state.app.UserEmail}`)
			.then(response => callback(true, response.data))
			.catch(error => callback(false, error));
	}
}
