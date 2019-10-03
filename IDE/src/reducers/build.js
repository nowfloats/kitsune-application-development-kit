import { build } from '../actions/actionTypes';

const initialState = {
	isFetching: false,
	isCompleted: true,
	buildStatusRetries: 0,
	lastStage: 0,
	lastBuildStamp: 0
};

const buildReducer = (state = initialState, action) => {
	switch (action.type) {
	case build.SET_RETRY:
		return { ...state, build: action.payload };
	case build.BUILD_COMPLETE:
	case build.BUILDING:
	case build.SET_STAGE:
		return { ...state, ...action.payload };
	}
	return state;
};

export default buildReducer;
