import { projectree } from '../actions/actionTypes';
import _ from 'lodash';

const initialState = {
	name: "",
	toggled: true,
	children: [],
	IsKitsune: true,
	Path: null,
	data: {},
	isNFSchema: false, // removing use of ISNFSchema
	components: [],
	allComponents: null,
	projectComponents: null,
	gateways: {
		instamojo: false,
		paytm: false,
		payu: false,
		stripe: false,
		paddle: false
	}
};

const projectTreeReducer = (state = initialState, action) => {
	switch (action.type) {
	case projectree.PROJECT_RECEIVED: {
		const Assets = action.payload.Assets;
		Assets['data'] = action.payload;
		Assets.isNFSchema = action.payload.isNFSchema;
		delete Assets.data.Assets;
		return {
			...state,
			...Assets,
			projectComponents: action.payload.Components === null ? [] : action.payload.Components
		};
	}

	case projectree.PROJECT_INIT:
		document.title = `Kitsune IDE`;
		return {
			...initialState,
			allComponents: state.allComponents
		};

	case projectree.PROJECT_UPDATE:
	case projectree.PROJECT_GETINIT:
		return {
			...state,
			...action.payload
		};

	case projectree.UPDATE_MAPPEDSCHEMA:
		return {
			...state,
			data: {
				...state.data,
				SchemaId: action.payload
			}
		};

	case projectree.APPLICATION_ZIP_UPLOAD_COMPLETE:
		return {
			...state,
			existingAppDetails: action.payload
		};

	case projectree.PROJECT_PUBLISHING:
		return {
			...state,
			publishStarted: true
		};

	case projectree.COMPONENTS_FETCHED:
		return {
			...state,
			allComponents: action.components
		};

	case projectree.UPDATE_COMPONENT:
		return {
			...state,
			components: state.components.map(iterator => iterator.componentID === action.component.componentID ? ({
				...iterator,
				isMapped: action.component.boolean
			}) : iterator)
		};

	case projectree.COMPONENTS_MAP:
		return {
			...state,
			components: state.allComponents.map(iterator => {
				let isComponentMapped = false;
				state.projectComponents.forEach(({ ProjectId }) => {
					if(iterator.componentID === ProjectId)
						isComponentMapped = true;
				});
				return isComponentMapped ? {
					...iterator,
					isMapped: true
				} : iterator;
			})
		};

		case projectree.PROJECT_DELETENODE: //eslint-disable-line
		const newState = {
			...state
		};
		const pathArray = action.path.split('/');
		let child = newState;
		for(var i = 1; i < pathArray.length - 1; i++) {
			child = _.find(child.children, { 'name' : pathArray[i] })
		}
		child.children = _.reject(child.children, { 'name' : pathArray[pathArray.length - 1] });
		let initProject = { ...newState };
		delete initProject.allComponents;
		delete initProject.components;
		delete initProject.projectComponents;
		localStorage.setItem('project-init', JSON.stringify(initProject));
		return newState;

	case projectree.UPDATE_PROJECTDATA:
		return {
			...state,
			data: {
				...state.data,
				...action.data
			}
		};

	case projectree.UPDATE_PROJECTNAME: //eslint-disable-line
		document.title = `${action.name || 'untitled'} | kitsune IDE`;
		sessionStorage.setItem('projectName', action.name);
		const newStateProject = {
			...state,
			name: action.name,
			data: {
				...state.data,
				ProjectName: action.name
			}
		};
		let initNameProject = { ...newStateProject };
		delete initNameProject.allComponents;
		delete initNameProject.components;
		delete initNameProject.projectComponents;
		localStorage.setItem('project-init', JSON.stringify(initNameProject));
		return newStateProject;

	case projectree.UPDATE_PROJECTREENODE: //eslint-disable-line
		let newStateTreeNode = {
			...state
		};
		let iterableChild = newStateTreeNode.children;
		if(action.payload.iterator !== "") {
			let existingIteratorArray = action.payload.iterator.split('/');
			existingIteratorArray.forEach(iterator => {
				iterableChild = iterableChild.filter(({ name }) => name === iterator);
				iterableChild = iterableChild[0].children;
			});
		}
		if(action.payload.newObject.children === null) {
			iterableChild.push(action.payload.newObject);
		} else {
			iterableChild.unshift(action.payload.newObject);
		}
		return newStateTreeNode;

	case projectree.UPDATE_RENAMENODE: //eslint-disable-line
		let newStateRenameNode = {
			...state
		};
		let renameIterableChild = newStateRenameNode.children;
		let renamePathArray = action.payload.path.split('/');
		renamePathArray.forEach((iterator, index) => {
			renameIterableChild = renameIterableChild.filter(({ name }) => name === iterator);
			if(index !== renamePathArray.length - 1) {
				renameIterableChild = renameIterableChild[0].children;
			}
		});
		renameIterableChild[0].name = action.payload.name;
		var nodePathArray = renameIterableChild[0].Path.split('/');
		nodePathArray[nodePathArray.length - 1] = action.payload.name;
		renameIterableChild[0].Path = nodePathArray.join('/');
		return newStateRenameNode;

	case projectree.SET_ACTIVEGATEWAY:
		return {
			...state,
			gateways: {
				...action.payload
			}
		}
	}
	return state;
};
export default projectTreeReducer;
