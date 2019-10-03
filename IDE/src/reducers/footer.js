import { footer } from '../actions/actionTypes';
import _ from 'lodash';

const initialState = {
	notification: [],
	log: [],
	isActive: null,
	saveLog: { isSaved: false, lastSave: null }
};

const footerReducer = (state = initialState, action) => {
	switch (action.type) {
	case footer.FOOTER_INIT:
		return {
			...state,
			...action.payload
		};
	case footer.FOOTER_REMOVE:
		return {
			...state,
			log : _.slice(state.log, 0, state.log.length - action.payload)
		};
	case footer.UPDATE_NOTIFICATION:
		return { ...state, notification: action.payload };
	case footer.UPDATE_LOG:
		return { ...state, log : action.payload };
	case footer.FOOTER_APPEND:
		return { ...state, log: _.concat(state.log, action.payload) };
	case footer.FOOTER_UPDATEUPLOAD:
		return {
			...state,
			log: state.log.map(iterator => iterator.index === action.payload.index ? {
				...iterator,
				success: action.payload.success
			} : iterator)
		};
	case footer.FOOTER_RESETLOG:
		return { ...state, log: [] };
	case footer.FOOTER_COLLAPSE:
		return { ...state, isActive : action.isActive };
	case footer.SAVE_LOG:
		return { ...state, saveLog: action.payload };
	}
	return state;
};

export default footerReducer;
