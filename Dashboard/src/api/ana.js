import * as Constants from '../../config/config';
import axios from 'axios';

export default {
	registerAnaAccount (payload, callback) {
		axios.post(`${Constants.anaBaseUrl}${Constants.anaRegistration}`, payload,
			{ headers: {
				"client-id": "kitsune",
				"api-key": "[[ANA_API_KEY]]"
			}
			})
			.then(({ data }) => callback(true, data))
			.catch(({ response }) => callback(false, response))
	},

	getAnaBots (callback) {
		//TODO: Make better architecture to maintain API endpoints.
		//TODO: replace ['Authorization'] with .Authorization
		axios.get(`${Constants.anaBaseUrl}${Constants.getAnaBots}`.replace('{businessId}',
			axios.defaults.headers.common['Authorization']), { headers: undefined })
			.then(response => callback(true, response))
			.catch(err => callback(false, err))
	},

	getAccountDetails(callback) {
		axios.get(`${Constants.anaBaseUrl}${Constants.getAccountDetails}${axios.defaults.headers.common['Authorization']}`,
			{ headers: undefined })
			.then(response => callback(true, response))
			.catch(err => callback(false, err))
	},

	triggerEmail({ EmailBody, To, Subject, Type = 1 }, callback) {
		const params = {
			To,
			Subject,
			EmailBody,
			Type
		};

		axios.post(`${Constants.apiBaseUrl}${Constants.emailer}`, params, { headers: undefined })
			.then(({ data }) => callback(true, data))
			.catch(({ response }) => callback(false, response))
	}
}
