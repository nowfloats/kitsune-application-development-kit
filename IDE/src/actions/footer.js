import { footer } from './actionTypes';
import { config } from '../config';

//action for initializing the footer log
export function footerInit(saveLog) {
	sessionStorage.setItem('footerErrors', '');
	const newState = {
		notification: [],
		isActive : null,
		saveLog: saveLog.isSaved ? saveLog : { isSaved: false, lastSave: null }
	};

	return {
		type: footer.FOOTER_INIT,
		payload: newState
	}
}

//action to check if payload to be appended already exists
export const footerReplace = (payload, n) => dispatch =>
new Promise((resolve) => {
	dispatch(remove(n));
	dispatch(append(payload));
	resolve();
});

//action for removing lines of the event log
export const remove = n => {
	return {
		type: footer.FOOTER_REMOVE,
		payload: n
	}
};

//action for appending the event log footer
export const append = payload => {
	return {
		type: footer.FOOTER_APPEND,
		payload: payload
	}
};

//action for updating existing footer activity
export const updateUpload = (index, success) => {
	return {
		type: footer.FOOTER_UPDATEUPLOAD,
		payload: { index, success }
	}
};

//action for reseting activity in the store
export const resetLog = () => {
	return {
		type: footer.FOOTER_RESETLOG
	}
};

//action for updating the footer
export function footerUpdate(payload, updateTo) {
	const { NOTIFICATION, EVENT_LOG } = config.INTERNAL_SETTINGS.footerTabs;
	let returnType;

	switch(updateTo) {
	case NOTIFICATION:
		sessionStorage.setItem('footerErrors', JSON.stringify(payload));
		returnType = footer.UPDATE_NOTIFICATION;
		break;
	case EVENT_LOG:
		returnType = footer.UPDATE_LOG;
		break;
	}

	return {
		type: returnType,
		payload: payload
	}
}

//action for toggling the footer
export const footerCollapse = (el, isActive) => ({
	type: footer.FOOTER_COLLAPSE,
	isActive: el !== isActive ? el : null
});

//action for handling save log in footer
export function isSaved(saveLog) {
	return {
		type: footer.SAVE_LOG,
		payload: saveLog
	}
}
