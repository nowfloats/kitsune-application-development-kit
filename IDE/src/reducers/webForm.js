import { webForm } from '../actions/actionTypes';

const initialState = {
	isOpen: false,
	isFetching: false,
	webForms: [],
	copyOfWebForms: [],
	helper: {},
	isEditable: false,
	canClone: false,
	jsonWebForm: [],
	currentWebForm: {
		Name: "",
		DisplayName: "",
		Description: null,
		Properties: [],
		Type: 'webform'
	}
};

const webFormCreatorReducer = (state = initialState, action) => {
	switch (action.type) {
	case webForm.IS_WEBFORMOPEN:
		return { ...state, isOpen: action.payload.isOpen };
	case webForm.WEBFORMS_FETCHING:
	case webForm.WEBFORMS_FETCHERROR:
		return { ...state, isFetching: action.payload.isFetching };
	case webForm.WEBFORMS_FETCHED:
		return {
			...state,
			isFetching: action.payload.isFetching,
			webForms: action.payload.webForms
		};
	case webForm.SET_CURRENT:
		return { ...state, currentWebForm : action.payload.currentWebForm };
	case webForm.SET_CURRENT_PROPERTIES:
		return {
			...state,
			currentWebForm : {
				...state.currentWebForm,
				Properties : action.payload.Properties
			}
		};
	case webForm.INITIALIZE: {
		let newState = { ...state, currentWebForm: initialState.currentWebForm };
		if(action.payload) {
			let { Name, DisplayName, Description } = state.currentWebForm;
			newState = {
				...newState, currentWebForm: {
					...newState.currentWebForm,
					Name,
					Description,
					DisplayName
				}
			};
		}
		return newState;
	}
	case webForm.SET_NEW_DETAILS:
		return {
			...state,
			currentWebForm: {
				...state.currentWebForm,
				Name: action.payload.name,
				DisplayName: action.payload.displayName,
				Description: action.payload.description
			}
		};
	case webForm.SET_SEARCH_RESULTS:
		return { ...state, copyOfWebForms: action.payload.searchResults };
	case webForm.SET_HELPER:
		return { ...state, helper: action.payload.helper };
	case webForm.SET_IS_EDITABLE:
		return { ...state, isEditable: action.payload.isEditable };
	case webForm.CAN_CLONE:
		return { ...state, canClone: action.payload.canClone };
	case webForm.JSON_WEBFORM:
		return { ...state, jsonWebForm: action.payload.jsonWebForm };
	}
	return state;
};

export default webFormCreatorReducer;
