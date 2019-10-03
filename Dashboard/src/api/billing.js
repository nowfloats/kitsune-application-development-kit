import { debitDetails, storageDetails, paymentDetails, apiBaseUrl, invoiceDetails } from '../../config/config'
import axios from 'axios'
import store from '../store/index'

export default {
	userTransactionDetails(callback) {
		axios.get(`${apiBaseUrl}${paymentDetails}${store.state.app.UserEmail}`)
			.then(response => {
				callback(true, response.data);
			})
			.catch(err => {
				callback(false, err);
			})
	},

	userUsageDetails(fromDate, endDate, callback) {
		const usageUrl = `${apiBaseUrl}${debitDetails}${store.state.app.UserEmail}
		&fromDate=${fromDate}&toDate=${endDate}`
		axios.get(usageUrl, { headers: { Authorization: store.state.app.UserEmail } })
			.then(response => {
				callback(true, response.data);
			})
			.catch(err => {
				callback(false, err);
			})
	},

	userStorageDetails({ fromDate, endDate }, callback) {
		const storageUrl = `${apiBaseUrl}${storageDetails}${store.state.app.UserEmail}
		&fromDate=${fromDate}&toDate=${endDate}`;
		axios.get(storageUrl)
			.then(response => {
				callback(true, response.data);
			})
			.catch(err => {
				callback(false, err);
			})
	},

	userInvoiceDetails(payload, callback) {
		if(payload !== null || payload !== undefined) {
			axios.get(`${apiBaseUrl}${invoiceDetails
				.replace('{userEmail}', store.state.app.UserEmail)
				.replace('{month}', payload.month)
				.replace('{year}', payload.year)
				}`)
				.then(response => {
					callback(true, response.data);
				})
				.catch(err => {
					callback(false, err);
				})
		}
	}
}
