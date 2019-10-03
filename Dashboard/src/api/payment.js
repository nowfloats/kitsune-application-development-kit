import * as Constants from '../../config/config'
import axios from 'axios';
import store from '../store/index'
import * as constants from '../../config/config'

export default {

	getPaymentRedirectLinkFromApi (payload,callback) {
		axios.post(`${Constants.apiBaseUrl}${Constants.getPaymentRedirectionLink}`,{
			username: store.state.app.UserEmail,
			amount: payload,
			responseurl: constants.instamojoPaymentRedirectUrl
		})
			.then(response => {
				callback(true, response.data);
			})
			.catch(err => {
				// TODO log the error
				callback(false, err);
			})
	},

	processInternationalPaymentsWithStripe (payload,callback) {
		axios.post(`${Constants.apiBaseUrl}${Constants.processInternationalPaymentRequest}`,{
			username: store.state.app.UserEmail,
			amount: payload.amount*100,
			token: payload.stripe_token,
			currency: payload.currency,
			responseurl: constants.instamojoPaymentRedirectUrl
		})
			.then(response => {
				callback(true, response);
			})
			.catch(err => {
				// TODO log the error
				callback(false, err);
			})
	},

	getPaymentStatusFromApi (payload, callback) {
		axios
			.get(`${constants.apiBaseUrl}
			${constants.instamojopaymentStatus}`.concat(payload))
			.then(res => {
				callback(true, res.data);
			})
			.catch(err => {
				callback(false, err);
			})
	}

}
