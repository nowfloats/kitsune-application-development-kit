import * as types from '../../mutation-types'
import transaction from '../../../api/billing'

const state = {
	transactionHistory: [],
	isFetchingUsageData: false,
	usage: [],
	storage: [],
	isFetchingInvoiceDetails: false,
	isFetchingTransactionData: false,
	invoiceDetails: {
		status: 'default',
		S3Link: ''
	}
}

const getters = {
	getTransactionHistory: state => state.transactionHistory,
	isFetchingUsageData: state => state.isFetchingUsageData,
	getUsage: state => state.usage,
	getStorage: state => state.storage,
	getInvoice: state => state.invoiceDetails,
	isFetchingInvoiceDetails: state => state.isFetchingInvoiceDetails,
	isFetchingTransactionData: state => state.isFetchingTransactionData
};

const mutations = {
	[types.setTransactionDetails] (state, payload) {
		state.transactionHistory = payload
	},

	[types.isFetchingUsageData] (state, payload) {
		state.isFetchingUsageData = payload
	},

	[types.usageDetails] (state, payload) {
		state.usage = payload
	},

	[types.storageDetails] (state, payload) {
		state.storage = payload
	},

	[types.invoiceDetails] (state, payload) {
		state.invoiceDetails = payload
	},

	[types.isFetchingInvoiceDetails] (state, payload) {
		state.isFetchingInvoiceDetails = payload
	},

	[types.isFetchingTransactionData] (state, payload) {
		state.isFetchingTransactionData = payload;
	}
};

const actions = {
	setTransactionHistory({ commit }) {
		commit(types.isFetchingTransactionData, true);
		transaction.userTransactionDetails((success, res) => {
			if(success) {
				commit(types.setTransactionDetails, res.WalletStats);
			}
			else {
				//TODO error handler
			}
			commit(types.isFetchingTransactionData, false);
		})
	},

	setUsageDetails({ commit }, payload) {
		commit(types.isFetchingUsageData, true);
		transaction.userUsageDetails(payload.fromDate, payload.endDate, (success, res) =>{
			if(success) {
				commit(types.usageDetails, res);
				commit(types.isFetchingUsageData, false);
			}
			else {
				//TODO error handler
				commit(types.isFetchingUsageData, false);
			}
		})
	},

	setStorageDetails({ commit }, payload) {
		commit(types.isFetchingUsageData, true);
		transaction.userStorageDetails(payload, (success, res) =>{
			if(success) {
				commit(types.storageDetails, res);
				commit(types.isFetchingUsageData, false);
			}
			else {
				//TODO error handler
				commit(types.isFetchingUsageData, false);
			}
		})
	},

	setInvoiceDetails({ commit }, payload) {
		commit(types.isFetchingInvoiceDetails, true);
		transaction.userInvoiceDetails(payload, (success, res) => {
			if(success) {
				commit(types.invoiceDetails, res);
				commit(types.isFetchingInvoiceDetails, false);
			}
			else {
				commit(types.isFetchingInvoiceDetails, false);
			}
		})
	}
};

export default {
	state,
	getters,
	mutations,
	actions
}
