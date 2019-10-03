import { Base64 } from 'js-base64';
import { editor } from '../actions/actionTypes';

export const editorState = {
	fileChanged: false,
	name: '',
	code: '',
	type: '',
	path: '',
	fileConfig: {
		IsStatic: false,
		File: {
			ContentType: ''
		}
	},
	lastUpdateTimeStamp: 0
};

const initialState = {
	activeTabs: [],
	visibleIndex: 0,
	isFetching: false,
	isOpened: false,
	helper: {},
};

const editorReducer = (state = initialState, action) => {
	// Many of the actions require knowing which editor tab to change,
	// The 'path' serves as an ID key, as each editor would be on a unique path
	let path;
	switch (action.type) {
		case editor.FILE_OPEN:
			return {
				...state,
				activeTabs: [...state.activeTabs, { ...action.payload.editor, tabIndex: state.activeTabs.length }],
				visibleIndex: state.activeTabs.length,
				isFetching: action.payload.isFetching
			}
		case editor.SWITCH_TAB:
			return {
				...state,
				visibleIndex: action.payload.index
			}
		case editor.FILESOURCE_COMPILING:
		case editor.FILESOURCE_COMPILED:
		case editor.FILESOURCE_ERROR:
		case editor.FILESOURCE_FETCHING:
			return {
				...state,
				...action.payload
			}
		case editor.CLOSE_FILE:
			const excludingClosed = [...state.activeTabs.filter((t, index) => action.payload.index !== index)];
			const updatedListCF = excludingClosed.map(tab => action.payload.index <= tab.tabIndex ? { ...tab, tabIndex: tab.tabIndex - 1 } : tab);
			const newIndex = state.visibleIndex - (action.payload.index <= state.visibleIndex ? 1 : 0);

			return {
				...state,
				activeTabs: updatedListCF,
				visibleIndex: newIndex
			};
		case editor.UPDATE_FILETYPE:
			path = action.payload.path;

			const { isStatic } = action.payload;
			const updatedListFT = [...state.activeTabs.map(editor => (editor.path !== path) ? editor : {
				...editor,
				fileConfig: {
					...editor.fileConfig,
					isStatic
				}
			})];

			return {
				...state,
				activeTabs: updatedListFT
			};
		case editor.CHANGE_CODE:
			const updatedListC = [...state.activeTabs.map((editor, index) => (index !== action.payload.index) ? editor : {
				...editor,
				code: Base64.encode(action.payload.code),
				fileChanged: true
			})];

			return {
				...state,
				activeTabs: updatedListC
			};
		case editor.FILE_CHANGED:
			const updatedListFC = [...state.activeTabs.map((editor, index) => (index !== action.payload.index) ? editor : {
				...editor,
				fileChanged: action.payload.fileChanged
			})];
			return {
				...state,
				activeTabs: updatedListFC
			};
		case editor.CLEAR_TABS:
			return {
				...initialState
			}

		///////////////

		case editor.FILESOURCE_UPDATING:
		case editor.FILESOURCE_UPDATEERROR:
		case editor.FILESOURCE_UPDATED:
			return { ...state, ...action.payload };
		case editor.IS_OPEN:
			return { ...state, isOpened: action.payload.isOpened };
		case editor.SET_HELPER:
			return { ...state, helper: action.payload.helper };

		case editor.UPDATE_RENAMEFILE: //eslint-disable-line
			const pathArray = action.payload.path.split('/');
			pathArray[pathArray.length - 1] = action.payload.name;
			const updatedPath = pathArray.join('/');
			return {
				...state,
				name: action.payload.name,
				path: updatedPath
			}
	}
	return state;
};

export default editorReducer;
