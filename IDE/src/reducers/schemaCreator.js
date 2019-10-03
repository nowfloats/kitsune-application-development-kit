import { schemaCreator } from '../actions/actionTypes';
import _ from 'lodash';

const initialState = {
	isOpen: false,
	isFetching: false,
	schemas: [],
	schemaDetails: {
		EntityName: '',
		EntityDescription: '',
		DefaultClasses: null,
		Classes: null
	},
	hasAssociatedWebsiteData: false,
	systemSchemas: [],
	readOnly: true,
	helper: {},
	supportedMetaProperties: {}
};

const schemaCreatorReducer = (state = initialState, action) => {
	switch (action.type) {
		case schemaCreator.OPEN_SCHEMA:
			return { ...state, isOpen: true };
		case schemaCreator.SCHEMAS_FETCHING:
		case schemaCreator.SCHEMADETAILS_FETCHING:
		case schemaCreator.SCHEMADETAILS_FETCHERROR:
		case schemaCreator.SCHEMA_CREATED:
		case schemaCreator.SCHEMA_CREATING:
		case schemaCreator.SCHEMA_CREATEERROR:
		case schemaCreator.SCHEMAS_FETCHERROR:
			return { ...state, isFetching: action.payload.isFetching };
		case schemaCreator.SCHEMAS_FETCHED:
			return {
				...state,
				isFetching: action.payload.isFetching,
				schemas: action.payload.schemas
			};
		case schemaCreator.SCHEMA_SETDEFAULT:
			return {
				...state,
				schemaDetails: {
					...state.schemaDetails,
					DefaultClasses: action.payload
				}
			};
		case schemaCreator.RESET:
			localStorage.removeItem('initSchemaDetails');
			return {
				...initialState,
				schemas: state.schemas,
				helper: state.helper,
				schemaDetails: {
					...initialState.schemaDetails,
					DefaultClasses: state.schemaDetails.DefaultClasses
				},
				systemSchemas: state.systemSchemas
			};
		case schemaCreator.SCHEMADETAILS_FETCHED:
			localStorage.setItem('initSchemaDetails', JSON.stringify([
				...state.schemaDetails.DefaultClasses,
				...action.payload.schemaDetails.Classes
			]));
			return {
				...state,
				isFetching: action.payload.isFetching,
				schemaDetails: {
					EntityName: action.payload.schemaDetails.EntityName,
					EntityDescription: action.payload.schemaDetails.EntityDescription,
					DefaultClasses: state.schemaDetails.DefaultClasses,
					Classes: [
						...state.schemaDetails.DefaultClasses,
						...action.payload.schemaDetails.Classes
					]
				},
				readOnly: action.payload.readOnly
			};
		case schemaCreator.SET_HELPER:
			return { ...state, helper: action.payload.helper };
		case schemaCreator.ADD_SYSDEFLANG:
			return {
				...state,
				systemSchemas: [
					...state.systemSchemas,
					action.sysLang
				]
			};
		case schemaCreator.DEL_SYSDEFLANG:
			return {
				...state,
				systemSchemas: _.reject(state.systemSchemas, ({ SchemaId }) => SchemaId === action.langId)
			};
		case schemaCreator.RESET_SYSDEF:
			return {
				...state,
				systemSchemas: initialState.systemSchemas
			}
		case schemaCreator.SET_SUPPORTED_META:
			return {
				...state,
				supportedMetaProperties: action.supportedTypes
			}
		case schemaCreator.HAS_WEBSITE_DATA:
			return {
				...state,
				hasAssociatedWebsiteData: action.payload
			};
	}
	return state;
};

export default schemaCreatorReducer;
