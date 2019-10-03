import { loader } from '../actions/actionTypes';

const initialState = {
	show: false
};

const loaderReducer = (state = initialState, action) => {
	switch (action.type) {
	case loader.LOADER_SHOW:
	case loader.LOADER_HIDE:
		return action.payload;
	}
	return state;
};

export default loaderReducer;
