import { webForm } from './actionTypes'
import axios from 'axios'
import { config } from '../config'
import { showLoading, hideLoading } from './loader'
import { toastr } from 'react-redux-toastr'
import { modalClose } from '../actions/modal'
import { fileChanged } from "./editor";

export function isWebFormOpen(bool) {
	return {
		type: webForm.IS_WEBFORMOPEN,
		payload: { isOpen: bool }
	}
}

export function webFormsFetch(isNewWebForm) {
	return (dispatch, getState) => {
		const { GENERAL_LOADING } = config.INTERNAL_SETTINGS.loadingText;
		dispatch(showLoading(GENERAL_LOADING.text))
		dispatch(webFormsFetching(true));
		return axios.get(`${config.API.webFormsList}`)
			.then(function (response) {
				dispatch(webFormsFetched(response.data.WebActions));
				dispatch(filterWebFormlist(response.data.WebActions, ''));
				dispatch(hideLoading())
				if(isNewWebForm){
					setRecentlyCreatedForm(getState,dispatch);
				}
			})
			.catch(function (error) {
				dispatch(webFormFetchError(error))
				dispatch(hideLoading())
			});
	}
}

export function setRecentlyCreatedForm(getState,dispatch) {
	const { currentWebForm, webForms } = getState().webFormReducer;
	const currentForm = webForms.find((webForm)=>{
		return webForm.Name === currentWebForm.Name;
	})
	dispatch(setCurrentWebForm(currentForm));
}

export function webFormsFetching(payload) {
	return {
		type: webForm.WEBFORMS_FETCHING,
		payload: { isFetching: payload }
	}
}

export function webFormsFetched(webForms) {
	return {
		type: webForm.WEBFORMS_FETCHED,
		payload: { isFetching: false, webForms }
	}
}

//action to indicate that webForm was fetched unsuccessfully
export function webFormFetchError(error) {
	toastr.error('error Fetching list of webForms', `${error}`)

	return {
		type: webForm.WEBFORMS_FETCHERROR,
		payload: { isFetching: false }
	}
}

export function setCurrentWebForm(selectedWebForm) {
	return {
		type: webForm.SET_CURRENT,
		payload: { currentWebForm: selectedWebForm }
	}
}

export function setWebactionProperties(webActionProperties) {
	return {
		type: webForm.SET_CURRENT_PROPERTIES,
		payload : { Properties : webActionProperties }
	}
}

export const createWebform = (isNewWebForm, jsonString) => (dispatch, getState) => new Promise((resolve, reject) => {
	const { text: SAVE_WEBFORM } = config.INTERNAL_SETTINGS.loadingText.SAVE_WEBFORM;
	const { ProjectId } = getState().projectTreeReducer.data;
	dispatch(showLoading(SAVE_WEBFORM));
	let { currentWebForm } = getState().webFormReducer;
	currentWebForm["ProjectId"] = ProjectId;
	axios.post(`${config.API.createWebform}`,currentWebForm)
		.then((response) => {
			dispatch(addJsonOfWebForm(response.data, jsonString, isNewWebForm)).then(() => resolve());
		})
		.catch(({ response }) => {
			const { request, data } = response;
			//status can be 400, if webform pre-exists or properties is null
			if(request.status == 400){
				webformAlreadyExists(data.Errors[0].ErrorMessage);
				//else other reasons might have occured
			} else {
				webFormCreatedOrUpdatedError(isNewWebForm, currentWebForm.DisplayName);
			}
			dispatch(modalClose());
			dispatch(hideLoading());
			reject();
		});
});

export const webformAlreadyExists = msg => toastr.error(msg);

export function webFormCreatedOrUpdatedSuccess(dispatch, isNewWebForm, webFormName){
	dispatch(fileChanged(false));
	dispatch(modalClose());
	if(isNewWebForm) {
		toastr.success('webform created', `${webFormName} created successfully`);
	} else {
		toastr.success('webform updated', `${webFormName} updated successfully`);
	}
}

export function webFormCreatedOrUpdatedError(isNewWebForm, webFormName){
	if(isNewWebForm) {
		toastr.error('webform creation', `${webFormName} creation failed`);
	} else {
		toastr.error('webform updated', `${webFormName} update failed`);
	}
}

export function initializeCurrentWebForm(val) {
	return {
		type: webForm.INITIALIZE,
		payload: val
	}
}

export function setDetailsOfNewWebForm(displayName, description) {
	let name = displayName.toLowerCase().trim().split(' ').join('');
	return {
		type: webForm.SET_NEW_DETAILS,
		payload: { name: name, description: description, displayName: displayName }
	}
}

export function filterWebFormlist(webFormsList, name) {
	let searchResults = webFormsList.filter((webForms) => {
		return webForms.DisplayName.toLowerCase().indexOf(name) >= 0;
	});

	return {
		type: webForm.SET_SEARCH_RESULTS,
		payload: { searchResults: searchResults }
	}
}

export function setHelper(helper) {
	return {
		type: webForm.SET_HELPER,
		payload: { helper: helper }
	}
}

export function setIsEditablePromise(value) {
	return dispatch => {
		dispatch(setIsEditable(value));
		return Promise.resolve();
	}
}

export function setIsEditable(value) {
	return {
		type: webForm.SET_IS_EDITABLE,
		payload: { isEditable: value }
	}
}

export function setCanClone(value) {
	return {
		type: webForm.CAN_CLONE,
		payload: { canClone: value }
	}
}

export const addJsonOfWebForm = (webActionId, jsonString, isNewWebForm) => (dispatch, getState) =>
new Promise((resolve, reject) => {
	let addJsonApi = config.API.addJsonOfWebForm;
	let { currentWebForm } = getState().webFormReducer;
	let payload = {
		UserId: config.API.userIdForDevKitsune,
		JsonString: jsonString
	};
	let auth = axios.defaults.headers.common.Authorization;
	delete axios.defaults.headers.common.Authorization;
	addJsonApi = addJsonApi.replace('{webActionId}', webActionId)
	axios.post(`${addJsonApi}`,payload)
		.then((response) => {
			if(response.status === 200 ) {
				webFormCreatedOrUpdatedSuccess(dispatch, isNewWebForm, currentWebForm.DisplayName);
				dispatch(webFormsFetch(true));
				dispatch(getJsonOfWebForm(webActionId)).then(()=> resolve());
			}
		})
		.catch((error) => {
			webFormCreatedOrUpdatedError(isNewWebForm, currentWebForm.DisplayName);
			reject();
		});
	axios.defaults.headers.common = { ...axios.defaults.headers.common, Authorization: auth };
});

export const getJsonOfWebForm = (webActionId, webFormProperties) => dispatch => new Promise((resolve, reject) => {
	let getJsonApi = config.API.getJsonOfWebForm;
	getJsonApi = getJsonApi.replace(`{webActionId}`, webActionId)
	axios.get(getJsonApi)
		.then((res) => {
			if(res.status == 200) {
				if(res.data.Data.length == 0) {
					let data = JSON.stringify(webFormProperties);
					data = data.split(`"`).join(`'`);
					dispatch(addJsonOfWebForm(webActionId, data, false)).then(()=> resolve());
				}
				else {
					let jsonString = res.data.Data[0].json.split(`'`).join(`"`);
					jsonString = JSON.parse(jsonString);
					dispatch(setJsonOfWebForm(jsonString))
					dispatch(hideLoading());
					resolve();
				}
			}
		})
		.catch((err) => {
			dispatch(hideLoading())
			toastr.error('error getting webForm data')
			reject();
		})
});

export function setJsonOfWebForm (value) {
	return {
		type: webForm.JSON_WEBFORM,
		payload: { jsonWebForm: value }
	}
}
