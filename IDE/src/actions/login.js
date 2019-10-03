import { login } from './actionTypes';
import { cookieDomain, config } from '../config'
import { showLoading, hideLoading } from "./loader";
import axios from 'axios';
import { toastr } from 'react-redux-toastr';
import { webFormsFetch } from "./webForm";
import { fetchprojectComponents } from "./projectTree";
import { fetchDefaultDataTypes, schemasFetch } from "./schema";
import { readCookie } from "../reducers/login";


export const setRedirectUrl = redirectUrl => {
	return {
		type: login.SET_REDIRECT,
		payload: { redirectUrl }
	};
};

//action to fetch developer details
export const fetchDeveloperDetails = () => (dispatch, getState) =>
new Promise((resolve, reject) => {
	const { userID } = getState().login;
	const { text: USER_DETAILS } = config.INTERNAL_SETTINGS.loadingText.USER_DETAILS;
	const { userDetails } = config.API;
	dispatch(showLoading(USER_DETAILS));

	axios.get(`${userDetails}${userID}`)
		.then(response => {
			dispatch(fetchedDeveloperDetails(response.data));
			dispatch(hideLoading());
			resolve();
		})
		.catch(() => {
			toastr.error('Unable to fetch user details');
			dispatch(hideLoading());
			reject();
		})
});

//action to handle fetched developer details
export const fetchedDeveloperDetails = details => {
	return {
		type: login.DEVELOPER_DETAILS,
		payload: details
	}
};

export const createUser = () => (dispatch, getState)  => new Promise(resolve => {
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
	updateDataBaseForUserFirstTimeLoginIn(userInfo, (success, response) => {
		if(success && response){
			axios.defaults.headers.common['Authorization'] = response;
			resolve();
		}
	})

})

const updateDataBaseForUserFirstTimeLoginIn = (payload, callback) => {

	axios.post(`${config.API.createUser}`, payload).then(response => {
		callback(true, response.data);
	})
	.catch(error => {
		callback(false, error);
	})
}

export const getUserToken = () => (dispatch, getState) => new Promise(resolve => {
	const { userID } = getState().login;
	const { userToken } = config.API;
	return axios.get(`${userToken}${userID}`)
		.then(({ data }) => {
			const { Id: userAuth } = data;
			if (!userAuth) {
				dispatch(createUser());
			} else {
				axios.defaults.headers.common['Authorization'] = userAuth;
				resolve();
			}
		});
});

export const initializeUser = () => dispatch => {
	dispatch(fetchprojectComponents());
	dispatch(fetchDeveloperDetails());
	dispatch(schemasFetch());
	dispatch(webFormsFetch());
	dispatch(fetchDefaultDataTypes());
};

export const logIn = (userInfo, userImage) => {

	let userEmail = userInfo.email;
	let userName = userInfo.name;
	let cookieDomainConfig = cookieDomain;
	if(window.location.hostname === 'localhost')
		cookieDomainConfig = 'localhost';

	//helper function to create a cookie
	const createCookie = function(cookieName, cookieValue, days) {
		var dt, expires;
		dt = new Date();
		dt.setTime(dt.getTime()+(days*24*60*60*1000));
		expires = '; expires='+dt.toGMTString();
		document.cookie = cookieName+'='+cookieValue + expires+'; path=/ ; domain='+cookieDomainConfig;
	};

	//TODO: encrypt the data
	createCookie('userId', userEmail, 1);
	createCookie('userImage', userImage, 1);
	createCookie('userName', userName, 1);

	const payload = {
		loggedIn: true,
		profileImage: userImage,
		userID: userEmail
	};

	return {
		type: login.LOGIN,
		payload: payload
	};
};

export const logOut = () => {
	return {
		type: login.LOGOUT,
		payload: false
	};
};
