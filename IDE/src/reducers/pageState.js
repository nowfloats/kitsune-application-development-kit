import { pages } from '../actions/actionTypes';

const initialState = {
	pageState: 'PROJECT_NONE'
};

const pageStateReducer = (state = initialState, action) => {
	switch (action.type) {
	case pages.PAGESTATE_UPDATE:
		return action.payload;
	}
	return state;
};

export default pageStateReducer;
