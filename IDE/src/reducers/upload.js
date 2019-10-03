import { upload } from '../actions/actionTypes';

const initialState = {
	files: [],
	paths: [''],
	appType: '',
	isFetching: false,
	failed: {
		files: [],
		paths: []
	}
};

const uploadReducer = (state = initialState, action) => {
	switch (action.type) {
	case upload.UPLOAD:
		return action.payload;
	case upload.UPDATE_FILES:
		return {
			...state,
			files: action.payload.files,
			paths: action.payload.paths
		};
	case upload.UPDATE_APPLICATION_FILES:
		return {
			...state,
			files: action.payload.files,
			paths: action.payload.paths,
			appType: action.payload.appType,
			deploymentConfig: action.payload.deploymentConfig,
			storageConfig: action.payload.storageConfig
		};
	case upload.SET_PATH:
		return { ...state, paths: [ action.payload.path ] };
	case upload.SET_FAILED:
		return {
			...state,
			failed: {
				files: [
					...state.failed.files ,
					action.payload.file
				],
				paths: [
					...state.failed.paths,
					action.payload.path
				]
			}
		};
	case upload.RESET_FAILED:
		return {
			...state,
			failed: { ...initialState.failed }
		};
	case upload.UPLOADING:
	case upload.UPLOADED:
	case upload.UPLOAD_ERROR:
		return { ...state, isFetching: action.payload.isFetching };
	case upload.RESET_STORE:
		return { ...initialState };
	}
	return state;
};

export default uploadReducer;
