import { login } from '../actions/actionTypes';
import Auth from '../components/login/Auth';

//helper function to read a cookie value
export const readCookie = name => {
	var nameEQ = name + "=";
	var ca = document.cookie.split(';');
	for(var i=0;i < ca.length;i++) {
		var c = ca[i];
		while (c.charAt(0)==' ') c = c.substring(1,c.length);
		if (c.indexOf(nameEQ) == 0) return c.substring(nameEQ.length,c.length);
	}
	return null;
};

//retrieve data from the cookie
let userIdCookie = readCookie('userId');
let userPicCookie = readCookie('userImage');
let userNameCookie = readCookie('userName');

const initialState = {
	loggedIn: userIdCookie !== null,
	redirectUrl: '/',
	profileImage: userPicCookie === null ? '': userPicCookie,
	userID: userIdCookie === null ? '' : userIdCookie,
	userName: userNameCookie,
	developerDetails: null,
	auth: new Auth()
};

const loginReducer = (state = initialState, action) => {
	switch (action.type) {
	case login.LOGIN:
		return { ...state, ...action.payload };
	case login.LOGOUT:
		return { ...state, loggedIn: action.payload };
	case login.SET_REDIRECT:
		return { ...state, redirectUrl: action.payload.redirectUrl };
	case login.DEVELOPER_DETAILS:
		return { ...state, developerDetails: action.payload };
	}
	return state;
};

export default loginReducer;
