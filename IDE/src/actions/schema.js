import { schemaCreator } from './actionTypes'
import axios from 'axios'
import { config } from '../config'
import { showLoading, hideLoading } from './loader'
import { fileChanged, fileOpen } from "./editor";
import { toastr } from 'react-redux-toastr'
import { addIntellisense, updateMappedSchema } from "./projectTree";
import { editorClear } from "./editor";

//action to open schema
export const openSchema = () => ({
	type: schemaCreator.OPEN_SCHEMA
});

//action to reset schema state
export const resetSchema = () => ({
	type: schemaCreator.RESET
});

//action to create a new schema for the user
export const schemaCreate = name => dispatch => {
	let sampleSchema = config.DATA.newSchema;
	sampleSchema.UserId = axios.defaults.headers.common['Authorization'];
	let schemaName = name.split(' ').join('');
	sampleSchema.Entity.EntityName = sampleSchema.Entity.Classes[0].Name = schemaName;
	const { text: CREATE_LANGUAGE } = config.INTERNAL_SETTINGS.loadingText.CREATE_LANGUAGE;
	dispatch(showLoading(CREATE_LANGUAGE));
	dispatch(schemaCreating());
	return axios.post(`${config.API.newOrUpdateSchema}`, sampleSchema)
		.then(response => {
			dispatch(mapSchema(response.data));
			dispatch(schemaFetchDetails(response.data));
			dispatch(schemaCreated());
		})
		.catch(error => {
			dispatch(hideLoading())
			dispatch(schemaCreateError(error.response.data.Message))
		});
};

// action to fetch default datatypes of language editor
export const fetchDefaultDataTypes = () => (dispatch, getState) => new Promise((resolve, reject) => {
	const { text: FETCHING_DATATYPES } = config.INTERNAL_SETTINGS.loadingText.FETCHING_DATATYPES;
	dispatch(showLoading(FETCHING_DATATYPES));
	const { schemaDetails } = config.API;
	const { userID } = getState().login;
	return axios.get(`${schemaDetails}/get-defaultclass?userid=${userID}`)
		.then(response => {
			dispatch(fetchedDefaultDataTypes(response.data));
			dispatch(hideLoading());
			resolve();
		})
		.catch(() => {
			toastr.error('Unable to fetch default datatypes');
			dispatch(hideLoading());
			reject();
		});

});

export const fetchedDefaultDataTypes = defaultDataTypes => ({
	type: schemaCreator.SCHEMA_SETDEFAULT,
	payload: defaultDataTypes
});

//action to indicate that schemas are being created
const schemaCreating = () => ({
	type: schemaCreator.SCHEMA_CREATING,
	payload: { isFetching: true }
});

// action to indicate that schema has been fetched successfully
const schemaCreated = () => ({
	type: schemaCreator.SCHEMA_CREATED,
	payload: { isFetching: false }
});

//action to indicate that schema was fetched unsuccessfully
const schemaCreateError = error => {
	toastr.error('error creating new language', error)

	return {
		type: schemaCreator.SCHEMA_CREATEERROR,
		payload: { isFetching: false }
	}
};

//action to save a schema for the user
export const saveSchema = schemaDetails => dispatch => new Promise((resolve, reject) => {
	let schemaData = {};
	schemaData.Entity = schemaDetails;
	schemaData.UserId = axios.defaults.headers.common['Authorization'];
	schemaData.LanguageId = sessionStorage.getItem('schemaId');
	const { text: SAVE_LANGUAGE } = config.INTERNAL_SETTINGS.loadingText.SAVE_LANGUAGE;
	dispatch(showLoading(SAVE_LANGUAGE));
	return axios.post(`${config.API.newOrUpdateSchema}`, schemaData)
		.then(({ data }) => {
			toastr.success('saved language', `language was successfully saved.`);
			let themeId = sessionStorage.getItem('currentThemeId');
			addIntellisense(themeId);
			dispatch(updateSchemaDetails(data));
			dispatch(fileChanged(false));
			dispatch(hideLoading());
			resolve();
		})
		.catch(() => {
			dispatch(hideLoading())
			toastr.error('error', 'we encountered an error.');
			reject();
		});
});

export const mapSchema = (schemaId) => (dispatch, getState) => {
	const { ProjectId: projectId } = getState().projectTreeReducer.data;
	let userToken = axios.defaults.headers.common['Authorization'];

	const body = {
		SchemaId: schemaId,
		ProjectId: projectId,
		UserId: userToken
	};
	dispatch(showLoading())
	return axios.post(`${config.API.mapSchema}`, body)
		.then(() => {
			dispatch(hideLoading())
			dispatch(schemasFetch())
			dispatch(updateMappedSchema(schemaId));
			sessionStorage.setItem('schemaId', schemaId);
			addIntellisense(projectId);
		})
		.catch(() => {
			dispatch(hideLoading())
			toastr.error('error', 'we encountered an error.')
		});
};

//action for fetching all schemas of the user
export const schemasFetch = () => dispatch => {
	let userToken = axios.defaults.headers.common['Authorization'];
	let projectId = sessionStorage.getItem('currentThemeId');
	const { FETCH_LANGUAGE } = config.INTERNAL_SETTINGS.loadingText;
	dispatch(showLoading(FETCH_LANGUAGE.text));
	dispatch(schemasFetching());
	return axios.get(`${config.API.schemaList}?userid=${userToken}&projectid=${projectId}`)
		.then(response => {
			dispatch(schemasFetched(response.data.Schemas))
			dispatch(hideLoading())
		})
		.catch(error => {
			dispatch(schemaFetchError(error))
			dispatch(hideLoading())
		});
};

/**
 * Fetch, and parse the supported meta properties (visible under create property > advanced)
 */
export const supportedPropertiesFetch = () => dispatch => {
	// TODO: Actually fetch from the endpoint once Chirag provides the value
	return Promise.resolve(true).then(() => {
		dispatch(propertiesFetched(config.INTERNAL_SETTINGS.advancedSchemaProperties))
	});
};

// action to setpu the cupported advanced property meta
const propertiesFetched = types => ({
	type: schemaCreator.SET_SUPPORTED_META,
	supportedTypes: types
});

//action to indicate that schemas are being fetched
const schemasFetching = () => ({
	type: schemaCreator.SCHEMAS_FETCHING,
	payload: { isFetching: true }
});

// action to indicate that schema has been fetched successfully
const schemasFetched = schemas => ({
	type: schemaCreator.SCHEMAS_FETCHED,
	payload: { isFetching: false, schemas }
});

//action to indicate that schema was fetched unsuccessfully
const schemaFetchError = error => {
	toastr.error('error fetching list of schemas', `${error}`)

	return {
		type: schemaCreator.SCHEMAS_FETCHERROR,
		payload: { isFetching: false }
	}
};

//action for fetching schema details
export const schemaFetchDetails = (schemaID, readOnly, skipOpen = false) => dispatch => {
	const { OPEN_LANGUAGE } = config.INTERNAL_SETTINGS.loadingText;
	dispatch(showLoading(OPEN_LANGUAGE.text))
	dispatch(schemaFetchingDetails())
	return axios.get(`${config.API.schemaDetails}/${schemaID}`)
		.then(response => {
			let schemaDetails = response.data;
			schemaDetails.Classes = schemaDetails.Classes.filter(i => i.ClassType !== 3);
			//make the schema read-only for NF-Business-Schema
			if (schemaID === '58d717e667962d6f40f5c198')
				dispatch(schemaFetchedDetails(schemaDetails, true));
			else
				dispatch(schemaFetchedDetails(schemaDetails, readOnly));
			dispatch(supportedPropertiesFetch())
			dispatch(getWebsiteData());
			if (!skipOpen) {
				// dispatch(editorClear());
				dispatch(openSchema());
				dispatch(fileOpen(schemaDetails.EntityName, '', '', '', ''));
			}
			dispatch(hideLoading());
		})
		.catch(error => {
			dispatch(schemaFetchDetailsError(error));
			dispatch(hideLoading());
		});
};

//action for updating open schema details
export const updateSchemaDetails = schemaID => dispatch => {
	dispatch(schemaFetchingDetails());
	return axios.get(`${config.API.schemaDetails}/${schemaID}`)
		.then(({ data }) => {
			const languageWithoutDefaults = {
				...data,
				Classes: data.Classes.filter(({ ClassType }) => ClassType !== 3)
			};
			dispatch(schemaFetchedDetails(languageWithoutDefaults));
			dispatch(getWebsiteData());
		})
		.catch(error => {
			dispatch(schemaFetchDetailsError(error));
			dispatch(hideLoading());
		});
};
//action to indicate that schema details are being fetched
const schemaFetchingDetails = () => ({
	type: schemaCreator.SCHEMADETAILS_FETCHING,
	payload: { isFetching: true }
});

// action to indicate that schema details has been fetched successfully
export const schemaFetchedDetails = (schemaDetails, readOnly) => ({
	type: schemaCreator.SCHEMADETAILS_FETCHED,
	payload: { isFetching: false, schemaDetails, readOnly }
});

//action to indicate that schema details was fetched unsuccessfully
const schemaFetchDetailsError = error => {
	toastr.error('error fetching list of schemas', `${error}`)

	return {
		type: schemaCreator.SCHEMADETAILS_FETCHERROR,
		payload: { isFetching: false }
	}
}

export const getWebsiteData = () => (dispatch, getState) => {
	const { websites } = getState().publishReducer;
	const { schemaDetails } = getState().schemaCreatorReducer;
	if (schemaDetails && schemaDetails.EntityName && websites && websites.length > 0) {
		return axios.get(`${config.API.websiteData.replace('{schema}', schemaDetails.EntityName)}${websites[0].value}`)
			.then(({ data }) => {
				if (data) {
					dispatch(websiteDataReceived(data));
				}
			})
	}
}

const websiteDataReceived = data => ({
	type: schemaCreator.HAS_WEBSITE_DATA,
	payload: data.Data && data.Data.length > 0
});

export const setHelper = helper => ({
	type: schemaCreator.SET_HELPER,
	payload: { helper }
});
