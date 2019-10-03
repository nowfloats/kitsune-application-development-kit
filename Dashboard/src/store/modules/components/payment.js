import * as types from '../../mutation-types';
import payment from '../../../api/payment';
import store from '../../../store/index';
import { coreEmailId, isProdEnv } from "../../../../config/config";

const state = {

}

// getters
const getters = {

}

// actions
const actions = {

	getPaymentRedirectLink({ commit }, payload) {
		commit(types.updateInstamojoPaymentRequestLink, true);
		payment.getPaymentRedirectLinkFromApi(payload,(success,response)=>{
			commit(types.updateInstamojoPaymentRequestStatus, success);
			if(success) {
				window.location.href=response;
			}
		})
	},

	processInternationalPayments({ commit }, payload) {
		payment.processInternationalPaymentsWithStripe(payload,(success, response)=>{
			// commit(types.updateInstamojoPaymentRequestStatus, success);
			if(success) {
				alert('you kitsune recharge was successful.');
				document.location.reload(true);
			}
		})
	},

	getPaymentStatus({ commit, dispatch }, payload) {
		commit(types.updateProcessingPaymentComponent, true);
		commit(types.updateOverlayComponent, true);

		payment.getPaymentStatusFromApi(payload, (success, response) => {
			commit(types.updatePaymentReceived, true);
			if(success && response === 'success'){
				commit(types.updatePaymentStatus, true);
				const { requestedForAna } = store.state.app;
				if(requestedForAna) {
					dispatch('registerAnaAccount', store.state.app.anaRegistrationDetails)
				}
				if(isProdEnv) {
					dispatch('triggerEmail', {
						To: [coreEmailId],
						Subject: 'Payment Successful',
						EmailBody: `money added by ${store.state.app.UserEmail} on ${new Date().toLocaleString()}`
					})
				}
			}
			else {
				commit(types.updatePaymentStatus, false);
			}
		})
	}

}

// mutations
const mutations = {

	[types.updateProcessingPaymentComponent] (state, payload) {
		store.state.app.componentIsActive.processingpayment = payload;
	},

	[types.updateOverlayComponent] (state, payload) {
		store.state.app.componentIsActive.overlay = payload;
	},

	[types.updatePaymentReceived] (state, payload) {
		store.state.app.payment.isDetailsreceived = payload
	},

	[types.updatePaymentStatus] (state, payload) {
		store.state.app.payment.isPaymentSuccessfull = payload
	},

	[types.updateInstamojoPaymentRequestLink] (state, payload) {
		store.state.app.payment.isInstamojoPaymentLinkRequested = payload
	},

	[types.updateInstamojoPaymentRequestStatus] (state, payload) {
		store.state.app.payment.isInstamojoPaymentLinkRequestedSuccessfull = payload
	}

};


export default {
	state,
	getters,
	actions,
	mutations
}
