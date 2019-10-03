import * as types from '../../mutation-types'
import user from '../../../api/user'
import store from '../../../store/index'
import Helpers  from '../../../components/mixin';
import axios from 'axios';


const state = {
	user : {},
	userProfileUpdate : {
		isUserDetailsUpdateRequested : false,
		isUserDetailsUpdatedSuccessfully : false
	},
	isUserInfoUpdated: false,
	accountComponent: 'default',
	userInfoForLoggingIn : {
		userId : '',
		isGettingUserId: false,
		isUpdatingDatabase : false,
		isCompleted : false,
		userData : {}
	},
	lowBalanceDetails : {},
	showSaveButton: false
};

// getters
const getters = {
	getUserDetails : state => state.user,
	getUserProfileUpdate : state => state.userProfileUpdate,
	getUserInfoUpdated: state => state.isUserInfoUpdated,
	getaccountComponent: state => state.accountComponent,
	getLowBalanceDetails: state => state.lowBalanceDetails,

	getUserNetBalance: state => {
		let userWallet = state.user.Wallet;
		if(userWallet !=null || userWallet != undefined){
			let userBalance = parseFloat(userWallet.Balance);
			let unbilledUsgae = parseFloat(userWallet.UnbilledUsage);
			return userBalance - unbilledUsgae;
		}
		return 0;
	}

}

// actions
const actions = {
	//TODO LOG THE ERROR
	getUserDetails ({ commit }) {
		user.getUserDetailsFromApi((success, details)=>{
			if(success) {
				commit(types.getUserDetails, details);
				commit(types.userDetailsReceived);
				commit(types.isUserInfoUpdated, false)
				commit(types.accountComponent, 'default')
			} else
			{
				// TODO
			}
		})
	},

	updateUserDetails ({ commit,dispatch },payload) {
		// TODO make api call to update user profile if success <-
		commit(types.userDetailsUpdateRequested, true);
		user.updateUserDetailsApi(payload,(success,response)=>{
			if(success && response){
				dispatch('getUserDetails');
				commit(types.userDetailsUpdateRequestSuccess, true);
				commit(types.updateBillingFormComponent, false);
				commit(types.userDetailsUpdateRequested, false);
				commit(types.updateAddMoneyComponent, true);
			}
			else{
				// TODO error popups required
			}
		})
	},

	editAccountComponent({ commit }, payload) {
		commit(types.accountComponent, payload)
	},

	editUserDetails ({ commit, dispatch }, payload) {
		commit(types.isUserInfoUpdated, true);
		let toasterTypes = store.state.toastr.toasterTypes;
		user.updateUserDetailsApi(payload,(success,response)=>{
			if(success && response){
				dispatch('getUserDetails')
				dispatch('addToaster',{
					title: '',
					type: toasterTypes.SUCCESS,
					message : 'profile successfully updated'
				});
			} else{
				dispatch('addToaster',{
					title: '',
					type: toasterTypes.ERROR,
					message : 'profile updated failed'
				})
			}
		})
	},

	setDefaultAuthorizationHeaderInAxios({ commit, dispatch }, payload) {
		axios.defaults.headers.common['Authorization'] = payload;
	},


	getUserId ({ commit,dispatch }, payload){
		user.getUserIdFromDatabase(store.state.app.UserEmail,(success,response)=>{
			if(success){
				if(response.Id != null){
					dispatch('setDefaultAuthorizationHeaderInAxios', response.Id);
					dispatch('getBaseUserDetailsForLoggingIn');
				}
				else {
					dispatch('createDeveloperAccount');
				}
			}
		})
	},

	setUserDataForUpdatingDatabaseForFirstTimeLogIn ({ commit },payload){
		commit(types.setUserDataForUpdatingDatabase,payload);
	},

	createDeveloperAccount({ commit,dispatch }){
		//retrieve data from the cookie
		const { readCookie } = Helpers.methods;
		let userIdCookie = readCookie('userId');
		let userImageCookie = readCookie('userImage');
		let userNameCookie = readCookie('userName');
		let userTokenCookie = readCookie('userToken');


		let userInfo = {
			UserEmail: userIdCookie,
			UserName: userIdCookie,
			ProfilePic: userImageCookie,
			DisplayName: userNameCookie,
			Logins: [
				{
					LoginProvider: 'Google',
					ProviderKey: userTokenCookie
				}
			],
			SecurityStamp: ''
		};
		user.updateDataBaseForUserFirstTimeLoginIn(userInfo,(success,response)=>{
			if(success && response != null || response != undefined){
				dispatch('setDefaultAuthorizationHeaderInAxios', response);
				dispatch('getBaseUserDetailsForLoggingIn');
			}else{
				// TODO apt failure response
			}
		})
	},

	getBaseUserDetailsForLoggingIn({ dispatch }){
		dispatch('getUserDetails');
		dispatch('getAllProjects');
		dispatch('getAllLiveProjects');
		dispatch('getAnaBots');
		dispatch('getAnaAccountDetails');
		dispatch('getLowBalanceDetails');
	},

	getLowBalanceDetails: ({ commit, dispatch }) => {
		user.getLowBalanceDetails((success, response) => {
			if(success) {
				commit(types.setLowBalanceDetails, response);
				const { balanceWentZeroDate } = response;
				if(balanceWentZeroDate) {
					dispatch('triggerLowBalanceModal');
				}
			} else {
				//TODO handler
			}
		})
	}

};

// mutations
const mutations = {

	[types.getUserDetails] (state, payload) {
		state.user = payload;
	},

	[types.updateAddMoneyComponent] (state, payload) {
		store.state.app.componentIsActive.addmoney = payload;
	},

	[types.updateBillingFormComponent] (state, payload) {
		store.state.app.componentIsActive.billingform = payload;
	},

	[types.userDetailsUpdateRequested] (state, payload) {
		state.userProfileUpdate.isUserDetailsUpdateRequested = payload;
	},

	[types.userDetailsUpdateRequestSuccess] (state, payload) {
		state.userProfileUpdate.isUserDetailsUpdatedSuccessfully = payload;
	},

	[types.isUserInfoUpdated] (state, payload) {
		state.isUserInfoUpdated = payload
	},

	[types.accountComponent] (state, payload) {
		state.accountComponent = payload
	},

	[types.setUserIdInUserInformationForLogging] (state,payload){
		state.userInfoForLoggingIn.userId = payload;
	},

	[types.setIsGettingUserIdInUserInformationForLogging] (state,payload){
		state.userInfoForLoggingIn.isGettingUserId = payload;
	},

	[types.setIsUpdatingUserDetialsInformationForLogging] (state,payload){
		state.userInfoForLoggingIn.isUpdatingDatabase = payload;
	},

	[types.setUserValidationCompletedInUserInformationForLogging] (state,payload){
		state.userInfoForLoggingIn.isCompleted = payload;
	},

	[types.setUserDataForUpdatingDatabase] (state,payload){
		state.userInfoForLoggingIn.userData = payload;
	},

	[types.setLowBalanceDetails] (state, payload) {
		state.lowBalanceDetails = { ...payload };
	}
};


export default {
	state,
	getters,
	actions,
	mutations
}
