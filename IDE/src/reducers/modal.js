import { modals } from '../actions/actionTypes';

const initialState = {
	html: {},
	label: 'init label',
	show: false,
	themesfetching: false,
	callback: null
};

const modalReducer = (state = initialState, action) => {
	switch (action.type) {
	case modals.MODAL_OPEN:
	case modals.MODAL_CLOSE:
		return action.payload;
	case modals.MODAL_THEMESFETCHING:
		return { ...state, themesfetching: action.payload.isFetching };
	default:
		return state
	}
};

export default modalReducer;
