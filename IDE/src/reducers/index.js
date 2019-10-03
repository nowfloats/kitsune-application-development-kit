import { combineReducers } from 'redux';
import locationReducer from './location';
import editorReducer from './editor';
import footerReducer from './footer';
import buildReducer from './build';
import publishReducer from './publish';
import projectTreeReducer from './projectTree';
import modalReducer from './modal';
import uploadReducer from './upload';
import loaderReducer from './loader';
import pageStateReducer from './pageState';
import schemaCreatorReducer from './schemaCreator';
import loginReducer from './login';
import webFormReducer from  './webForm';
import serverMaintenanceReducer from  './serverMaintenance';
import { reducer as toastrReducer } from 'react-redux-toastr';

export const makeRootReducer = asyncReducers =>
	combineReducers({
		location: locationReducer,
		...asyncReducers,
		editorReducer,
		footerReducer,
		buildReducer,
		publishReducer,
		projectTreeReducer,
		modalReducer,
		uploadReducer,
		loaderReducer,
		pageStateReducer,
		schemaCreatorReducer,
		webFormReducer,
		serverMaintenanceReducer,
		toastr: toastrReducer,
		login: loginReducer
	});

export const injectReducer = (store, { key, reducer }) => {
	if (Object.hasOwnProperty.call(store.asyncReducers, key)) return;

	store.asyncReducers[key] = reducer;
	store.replaceReducer(makeRootReducer(store.asyncReducers));
};

export default makeRootReducer;
