import Ana from '../../../api/ana';
import store from "../../index";
import * as types from '../../mutation-types';
import anaEmailSubject from '../../../components/emailBody.html'
import helpers from '../../../components/mixin';
import { coreEmailId } from "../../../../config/config";

const state = {
	anaFlowList: {},
	showArrowBots: false,
	isAccountPresent: false
};

const getters = {
	getListOfBots: state => state.anaFlowList,
	showArrowBots: state => state.showArrowBots,
	isAnaAccountPresent: state => state.isAccountPresent
};

const mutations = {
	[types.setFlowList] (state, payload) {
		state.anaFlowList = { ...payload };
	},
	[types.toggleShowArrorBots] (state, payload) {
		state.showArrowBots = payload;
	},
	[types.isAnaAccountAlreadyPresent] (state, payload) {
		state.isAccountPresent = payload;
	}
};

const actions = {
	registerAnaAccount({ commit, dispatch }, payload) {
		dispatch('toggleRequestedForAna', false);
		const { name, email, password } = payload.user;
		Ana.registerAnaAccount(payload, (success, response) => {
			const { SUCCESS, ERROR } = store.state.toastr.toasterTypes;
			dispatch('toggleLoader', false);
			dispatch('toggleActionModal');
			if(success){
				commit(types.isAnaAccountAlreadyPresent, true);
				const emailBody = helpers.methods.generateAnaEmailBody({ UserName: email, Password: password, Name: name });
				dispatch('triggerEmail', { To: [email],
					EmailBody: emailBody, Subject: anaEmailSubject });
				//TODO: add titles and messages as constants everywhere for toastr
				dispatch('addToaster',{
					type : SUCCESS,
					title : 'success',
					message: 'registered for ANA cloud'
				});
			} else {
				if(response.status === 409) {
					dispatch('addToaster',{
						type : ERROR,
						title : 'duplicate',
						message: 'you have already registered for ANA cloud'
					});
				} else {
					dispatch('addToaster',{
						type : ERROR,
						title : 'failed',
						message: 'unable to register'
					});
				}
			}
		})
	},

	getAnaBots( { commit, dispatch } ) {
		Ana.getAnaBots((success, response) => {
			const { ERROR } = store.state.toastr.toasterTypes;
			if(success) {
				commit(types.setFlowList, response);
			} else {
				dispatch('addToaster',{
					type : ERROR,
					title : 'failed',
					message: 'failed to get list of bots'
				});
			}
		})
	},

	getAnaAccountDetails({ commit }) {
		Ana.getAccountDetails((success, { status }) => {
			if(success) {
				commit(types.isAnaAccountAlreadyPresent, true);
			} else {
				if(status === 422) {
					commit(types.isAnaAccountAlreadyPresent, false);
				}
			}
		})
	},

	toggleShowArrowBots( { commit }, payload){
		commit(types.toggleShowArrorBots, payload);
	},

	toggleIsAccountAlreadyPresent({ commit }, payload) {
		commit(types.isAnaAccountAlreadyPresent, payload);
	},

	triggerEmail({ commit, dispatch }, payload) {
		Ana.triggerEmail(payload, (success, response) => {
			const { SUCCESS } = store.state.toastr.toasterTypes;
			if(success && response !== "" && payload.Email.indexOf(coreEmailId) < 0) {
				dispatch('addToaster',{
					type : SUCCESS,
					title : 'email sent',
					message: 'check your inbox for account details'
				});
			} else {
				//TODO error handler
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
