/*Herein lies all the projectTree actions*/
import React from 'react';
import { projectree, customer, schemaCreator } from './actionTypes';
import axios from 'axios';
import { modalOpen, modalClose } from './modal';
import { pageStateUpdate } from './pageState';
import { showLoading, hideLoading } from './loader';
import { editorClear } from './editor';
import { resetSchema } from "./schema";
import { buildComplete, building, buildPoll } from './build';
import ProjectError, { projectErrorLabel } from '../components/modals/project-error/index';
import { config, riaComponentName, isProdDeployed } from '../config';
import {
	isSaved,
	footerCollapse,
	footerUpdate,
	append,
	resetLog
} from "./footer";
import { toastr } from 'react-redux-toastr';
import { isWebFormOpen } from "./webForm";
import _ from "lodash";
import riaSample from '../components/modals/component-confirmation/sample-report-template.html';
import partialSample from '../components/modals/component-confirmation/partial-sample.html'
import { filterTree } from "../components/sidebar/project-tree/filter";
import { singleFileUpload } from "./upload";
import { Base64 } from "js-base64";

//action for fetching a project
export const projectFetch = (projectID, isRefresh) => (dispatch, getState) => {
	const { login } = getState();
	const { userID } = login;
	if (!isRefresh) {
		dispatch(showLoading(config.INTERNAL_SETTINGS.loadingText.GENERAL_LOADING.text));
		sessionStorage.setItem('currentThemeId', projectID);
		dispatch(editorClear());
		dispatch(resetSchema());
		dispatch(isWebFormOpen(false));
	}
	const url = `${config.API.project}/${projectID}?userEmail=${userID}`;

	return axios.get(url)
		.then(({ data }) => {
			const { SchemaId, ProjectStatus, Components } = data;
			const { systemSchemas } = getState().schemaCreatorReducer;
			if (systemSchemas.length && !isRefresh) {
				dispatch(resetSystemDefinedLanguages());
			}
			if (Components && Components.length && !isRefresh) {
				Components.forEach(({ SchemaId }) => {
					if (SchemaId) {
						dispatch(generateSystemDefinedLangauge(SchemaId));
					}
				});
			}
			dispatch(projectReceived(data));
			const { allComponents } = getState().projectTreeReducer;
			if (allComponents) {
				dispatch(mapComponents());
			}

			if (SchemaId !== null) {
				addIntellisense(projectID);
			} else {
				sessionStorage.removeItem('kIntellisense');
			}
			dispatch(isSaved({ isSaved: false, lastSave: null }));
			dispatch(pageStateUpdate(config.INTERNAL_SETTINGS.pageStates.FILE_NONE.name));
			dispatch(hideLoading());
			//if projectStatus if in building stage or error stage
			if (ProjectStatus > 1 || ProjectStatus < 0 && !isRefresh) {
				//poll the build
				pollBuild(dispatch, getState);
			}
			dispatch(getCustomerList());
			if (!isRefresh) {
				dispatch(checkActiveGateway());
			}
		})
		.catch(error => {
			dispatch(modalOpen(<ProjectError />, projectErrorLabel, null));
			dispatch(hideLoading());
			throw new Error(error);
		});
};

export const updateProjectData = () => (dispatch, getState) => {
	const { userID } = getState().login;
	const { ProjectId: projectId } = getState().projectTreeReducer.data;
	const url = `${config.API.project}/${projectId}?userEmail=${userID}`;
	return axios.get(url)
		.then(({ data }) => {
			delete data.Assets;
			dispatch(projectDataToStore(data));
		})
		.catch(error => {
			toastr.error('Error updating project');
			throw new Error(error);
		});
};

export const updateProjectName = () => (dispatch, getState) => {
	const { userID } = getState().login;
	const { ProjectId: projectId } = getState().projectTreeReducer.data;
	const url = `${config.API.project}/${projectId}?userEmail=${userID}`;
	return axios.get(url)
		.then(({ data }) => {
			dispatch(projectNameToStore(data.ProjectName));
		})
		.catch(error => {
			toastr.error('Error updating project');
			throw new Error(error);
		});
};

const projectNameToStore = name => ({
	type: projectree.UPDATE_PROJECTNAME,
	name
});

const projectDataToStore = data => ({
	type: projectree.UPDATE_PROJECTDATA,
	data
});

//function to call buildPolling
export const pollBuild = dispatch => {
	const { EVENT_LOG } = config.INTERNAL_SETTINGS.footerTabs;
	dispatch(footerCollapse(EVENT_LOG, null));
	dispatch(footerUpdate([], EVENT_LOG));
	dispatch(building());
	dispatch(buildPoll(10, 2000))
		.then(message => {
			dispatch(buildComplete());
			toastr.success('build successful', `your project has been successfully built.`);
			dispatch(updateProjectData());
			console.log(message); //eslint-disable-line
		})
		.catch(error => {
			dispatch(buildComplete());
			toastr.error('build failed', error.message);
		})
};

//action to get the initial state of the project
export const projectGetInit = () => {
	let init = JSON.parse(localStorage.getItem('project-init'));
	init['isFetching'] = false;

	return {
		type: projectree.PROJECT_GETINIT,
		payload: init
	}
};

// action to preview page
export const previewPage = (node = null, tab) => (dispatch, getState) => {
	const { projectTreeReducer, editorReducer } = getState();
	const { ProjectId: projectId } = projectTreeReducer.data;
	const { activeTabs, visibleIndex } = editorReducer;
	const visibleEditor = activeTabs[visibleIndex];

	node = node === null ? visibleEditor : node;

	if (node) {
		const fileName = encodeURIComponent(node.path ? node.path : node.Path),
			isFetching = node.isFetching;

		if (!isFetching) {
			tab.location.href = `/preview/project=${projectId}/page=${fileName}/data=automotive`;
		}
	}
};


//action to show the fetched project in the projectTree
export const projectReceived = response => {
	const { Assets: projectRoot, ProjectName: projectName, SchemaId: schemaId } = response;
	let payload = response;

	document.title = `${projectName || 'untitled'} | Kitsune IDE`;
	sessionStorage.setItem('projectName', projectName);
	sessionStorage.setItem('schemaId', schemaId);

	//payload.isNFSchema = response.SchemaId === '58d717e667962d6f40f5c198';
	payload.isNFSchema = false;

	if (projectRoot.children === null) {
		projectRoot.children = [];
		delete projectRoot.toggled;
	}
	localStorage.setItem('project-init', JSON.stringify(projectRoot));

	return {
		type: projectree.PROJECT_RECEIVED,
		payload: payload
	}
};

//function to add Intellisense to sessionStorage
export const addIntellisense = projectID =>
	axios.get(`${config.API.intellisense}${projectID}/get-intellisense`)
		.then(response => {
			sessionStorage.setItem('kIntellisense', JSON.stringify(response.data.Intellisense))
		});

//action to initialize projectTree
export const projectInit = () => ({
	type: projectree.PROJECT_INIT,
});

//action to update projectTree
export const projectUpdate = updatedData => ({
	type: projectree.PROJECT_UPDATE,
	payload: updatedData
});

//action for creating a project
export const projectCreate = projectName => (dispatch, getState) => {
	const { login } = getState();
	const { userID } = login;

	dispatch(showLoading(config.INTERNAL_SETTINGS.loadingText.CREATE_PROJECT.text));
	dispatch(editorClear());
	dispatch(resetSchema());
	dispatch(isWebFormOpen(false));

	let header = {
		ProjectId: null,
		ProjectName: projectName,
		UserEmail: userID
	};
	return axios.post(`${config.API.project}`, header)
		.then(response => {
			const { data: projectid } = response;
			dispatch(projectFetch(projectid, false));
			if (isProdDeployed) {
				const subject = `new project created by ${userID} on ${new Date()}`;
				const body = `Project Name: ${projectName}<br />Project ID: ${projectid}`;
				sendMailToCore(subject, body);
			}
			dispatch(hideLoading());
		})
		.catch(() => {
			dispatch(hideLoading());
		});
};

//action for editing a project
export const projectEdit = projectConfig => (dispatch, getState) => {
	const { name: projectName, themeId: projectId } = projectConfig;
	const { login } = getState();
	const { userID } = login;
	dispatch(showLoading());

	let body = {
		UserEmail: userID,
		ProjectId: projectId,
		ProjectName: projectName
	};
	return axios.post(`${config.API.projectEdit}`, body)
		.then(response => {
			const { data: projectid } = response;
			dispatch(updateProjectName());
			toastr.success('successfully edited project', `project name changed to ${projectName}`);
			sessionStorage.setItem('currentThemeId', projectid);
			dispatch(hideLoading());
		})
		.catch(() => {
			dispatch(hideLoading());
		});
};

const publishMessage = projectDomain => dispatch => {
	let consoleData = [
		{
			text: `======================================`,
			type: "info"
		},
		{
			text: `Project Published`,
			type: "info"
		},
		{
			text: 'Visit Site',
			type: 'link',
			href: `http://${projectDomain}`
		}
	];
	const { EVENT_LOG } = config.INTERNAL_SETTINGS.footerTabs;
	dispatch(footerCollapse(EVENT_LOG, null));
	dispatch(append(consoleData));
};

const publishAllMessage = projectDomain => dispatch => {
	let consoleData = [
		{
			text: `======================================`,
			type: "info"
		},
		{
			text: `project published to all`,
			type: "info"
		},
		{
			text: 'visit site',
			type: 'link',
			href: `http://${projectDomain}`
		}
	];
	const { EVENT_LOG } = config.INTERNAL_SETTINGS.footerTabs;
	dispatch(footerCollapse(EVENT_LOG, null));
	dispatch(append(consoleData));
};

export const pollPublish = (domainName, userID, isAll) => (dispatch, getState) => {
	//TODO: Use poller function here
	const { ProjectId: projectId } = getState().projectTreeReducer.data;
	const project = window.setInterval(() => {
		const url = `${config.API.project}/${projectId}?userEmail=${userID}`;
		return axios.get(url)
			.then(({ data }) => {
				if (data.ProjectStatus != 1) {
					window.clearInterval(project);
					dispatch(hideLoading());
					toastr.success('published', 'project has been published');
					if (isProdDeployed) {
						const { name } = getState().projectTreeReducer;
						const subject = `publish success for ${userID} on ${new Date()}`;
						const body = `Project Name: ${name}<br />Project ID: ${projectId}<br />
												Published url: ${domainName.toLowerCase()}`;
						sendMailToCore(subject, body);
					}
					dispatch(projectReceived(data));
					isAll ? dispatch(publishAllMessage(domainName)) : dispatch(publishMessage(domainName));
				}
			})
			.catch(() => window.clearInterval(project));
	}, 3000);
};

//helper function to check developer net balance
export const checkDeveloperNetBalance = wallet => (wallet !== null && wallet !== undefined) ?
	(parseFloat(wallet.Balance) - parseFloat(wallet.UnbilledUsage)) >= 100 : false;

//action for publishing a project
export const projectPublish = ({ websiteID, websiteDomain, copyDataFromDemoWebsite }) => (dispatch, getState) => {
	const { userID } = getState().login;
	const { text: PUBLISH_PROJECT } = config.INTERNAL_SETTINGS.loadingText.PUBLISH_PROJECT;
	const { ProjectId: projectId } = getState().projectTreeReducer.data;
	const { publish } = config.API;
	dispatch(showLoading(PUBLISH_PROJECT));
	dispatch(projectPublishing());
	return axios.post(`${publish}?customerId=${websiteID}
		&userEmail=${userID}&copyDataFromDemoWebsite=${copyDataFromDemoWebsite}&ProjectId=${projectId}`)
		.then(() => {
			dispatch(pollPublish(websiteDomain, userID, false));
			dispatch(modalClose());
		})
		.catch(error => {
			toastr.error('publishing failed', `${error}`);
			dispatch(hideLoading());
			dispatch(modalClose());
		});
};

//action to publish to all
export const publishAll = () => (dispatch, getState) => {
	const { login, projectTreeReducer, publishReducer } = getState();
	const { userID } = login;
	const { ProjectId: projectId } = projectTreeReducer.data;
	const { defaultCustomerDomain } = publishReducer;
	const { text: PUBLISH_ALL } = config.INTERNAL_SETTINGS.loadingText.PUBLISH_ALL;
	const { publish } = config.API;
	dispatch(showLoading(PUBLISH_ALL));
	return axios.post(`${publish}?projectId=${projectId}&userEmail=${userID}&publishToAll=true`)
		.then(() => {
			dispatch(pollPublish(defaultCustomerDomain, userID, true));
			dispatch(modalClose());
		})
		.catch(error => {
			toastr.error('publishing failed', error);
			dispatch(hideLoading());
			dispatch(modalClose());
		});
};

//action to indicate project is being published
export const projectPublishing = () => ({
	type: projectree.PROJECT_PUBLISHING,
	payload: { isFetching: true }
});

export const getCustomerList = () => (dispatch, getState) => {
	const { ProjectId: projectId } = getState().projectTreeReducer.data;
	return axios.get(`${config.API.customers}${projectId}&limit=1000`)
		.then(({ data }) => {
			dispatch(customerListReceived(data));
		})
		.catch(() => {
			dispatch(hideLoading());
		});
};

export const saveApplicationConfigDetails = data => ({
	type: projectree.APPLICATION_ZIP_UPLOAD_COMPLETE,
	payload: data //{appType, deploymentConfig, storageConfig}
});

export const updateMappedSchema = schemaID => ({
	type: projectree.UPDATE_MAPPEDSCHEMA,
	payload: schemaID
});

export const customerListReceived = data => ({
	type: customer.CUSTOMER_RECEIVED,
	payload: { isFetching: false, data }
});

//middleware to fetch project components
export const fetchprojectComponents = () => (dispatch, getState) =>
	axios.get(config.API.listOfRiaApps)
		.then(({ data }) => {
			const { Projects } = data;
			const components = Projects.map(({ ProjectId, ProjectName, SchemaId }) => ({
				componentID: ProjectId,
				componentName: ProjectName,
				languageId: SchemaId,
				isMapped: false
			}));
			dispatch(setprojectComponents(components));
			const { projectComponents } = getState().projectTreeReducer;
			if (projectComponents)
				dispatch(mapComponents());
		})
		.catch(error => toastr.error('error Fetching list of components', error));

//action creator to set project components
const setprojectComponents = components => ({
	type: projectree.COMPONENTS_FETCHED,
	components
});

export const checkActiveGateway = () => (dispatch, getState) => {
	fetchKitsuneSettings(getState)
		.then(({ payments }) => {
			const payload = {
				instamojo: false,
				paytm: false,
				payu: false,
				aamarpay: false,
				stripe: false,
				paddle: false
			};
			if (payments && payments.preview && payments.live) {
				payments.preview.forEach(({ gateway }) => {
					const isLive = payments.live.filter(({ gateway: liveGateway }) => gateway === liveGateway).length;
					if (isLive) {
						payload[gateway] = true;
					}
				});
			}
			dispatch(setActiveGateways(payload));
		})
		.catch(error => {
			toastr.error('unable to fetch settings');
			throw new Error(error);
		})
};

const setActiveGateways = payload => ({
	type: projectree.SET_ACTIVEGATEWAY,
	payload
});

const mapComponents = () => ({ type: projectree.COMPONENTS_MAP });

const preEnableReports = (dispatch, componentName) => new Promise(resolve => {
	const searchRia = filterTree(JSON.parse(localStorage.getItem('project-init')), 'periodic_performance_report.html');
	const checkTemplates = checkComponentSettingsFile(searchRia, componentName);
	dispatch(checkSettingsContent())
		.then(({ exists, settings }) => {
			if (!exists || !checkTemplates) {
				dispatch(append({
					type: 'progress',
					text: `moving the requisite files to your project folder`,
				}));
			}
			if (!exists) {
				const { riaJSON } = config.INTERNAL_SETTINGS;
				settings.reports = riaJSON;
				const kitsuneSettingsEncoded = Base64.encode(JSON.stringify(settings, false, 2));
				dispatch(updateSettingsContent(kitsuneSettingsEncoded, '/kitsune-settings.json'));
			}
			//check if ria template file already exist
			if (!checkTemplates) {
				//if not create them
				const riaSampleEncoded = Base64.encode(riaSample);
				dispatch(singleFileUpload('periodic_performance_report.html', riaSampleEncoded,
					`/__components/${componentName}/templates`));
			}
			resolve();
		});
});

const updateSettingsContent = (content, path) => (dispatch, getState) => {
	const { userID } = getState().login;
	const { ProjectId: projectId } = getState().projectTreeReducer.data;
	let header = {
		UserEmail: userID,
		FileContent: content,
		SourcePath: path,
		IsStatic: false
	};

	return axios.post(`${config.API.project}/${projectId}/resource`, header)
		.then(() => dispatch(checkActiveGateway()))
		.catch(error => {
			toastr.error('error updating your settings');
			throw new Error(error);
		});
};

const checkSettingsContent = () => (dispatch, getState) =>
	new Promise((resolve, reject) => {
		fetchKitsuneSettings(getState)
			.then(settings => {
				resolve({
					exists: settings.reports !== undefined,
					settings
				})
			})
			.catch(error => {
				reject(error);
				throw new Error(error);
			})
	});

const fetchKitsuneSettings = getState => new Promise((resolve, reject) => {
	const { userID } = getState().login;
	const { projectAPI } = config.API;
	const { ProjectId: projectId } = getState().projectTreeReducer.data;
	const encodedPath = encodeURIComponent(`/kitsune-settings.json`);
	return axios.get(`${projectAPI}/v2/${projectId}/resource/?user=${userID}&sourcePath=${encodedPath}`)
		.then(({ data }) => resolve(JSON.parse(Base64.decode(data.File.Base64Data))))
		.catch(error => {
			toastr.error('unable to fetch your config');
			reject(error);
		});
});

const preEnableKPay = (dispatch, getState, gatewayName) => new Promise((resolve, reject) => {
	fetchKitsuneSettings(getState)
		.then(settings => {
			dispatch(append({
				type: 'progress',
				text: `moving the requisite files to your project folder`,
			}));
			if (settings.payments) {
				if (settings.payments.preview) {
					const isPreview = settings.payments.preview.filter(({ gateway }) => gateway === gatewayName).length;
					if (!isPreview) {
						settings.payments.preview = [
							...settings.payments.preview,
							config.INTERNAL_SETTINGS.paymentSettings[gatewayName].preview
						]
					}
				} else {
					settings.payments.preview = [config.INTERNAL_SETTINGS.paymentSettings[gatewayName].preview];
				}
				if (settings.payments.live) {
					const isLive = settings.payments.live.filter(({ gateway }) => gateway === gatewayName).length;
					if (!isLive) {
						settings.payments.live = [
							...settings.payments.live,
							config.INTERNAL_SETTINGS.paymentSettings[gatewayName].live
						]
					}
				} else {
					settings.payments.live = [config.INTERNAL_SETTINGS.paymentSettings[gatewayName].live];
				}
			} else {
				settings.payments = {
					preview: [config.INTERNAL_SETTINGS.paymentSettings[gatewayName].preview],
					live: [config.INTERNAL_SETTINGS.paymentSettings[gatewayName].live]
				};
			}
			const kitsuneSettingsEncoded = Base64.encode(JSON.stringify(settings, false, 2));
			dispatch(updateSettingsContent(kitsuneSettingsEncoded, '/kitsune-settings.json'));
			dispatch(append({
				type: 'progress',
				text: `finalizing the setup`,
			}));
			dispatch(append({
				type: 'progress-with-bold',
				textPreBold: ``,
				boldText: gatewayName,
				textPostBold: ` has been successfully setup for your project`
			}));
			resolve();
		})
		.catch(error => {
			dispatch(append({
				type: 'error',
				text: `something went wrong - if this issue persists, please reach out to us at support@getkitsune.com`
			}));
			reject(error);
		});
});

const preEnableCallTracker = (dispatch, getState) => new Promise((resolve, reject) => {
	fetchKitsuneSettings(getState)
		.then(settings => {
			dispatch(append({
				type: 'progress',
				text: `moving the requisite files to your project folder`,
			}));
			if (!settings.call_tracker) {
				settings.call_tracker = {
					"domain": ["*"]
				}
			}
			const kitsuneSettingsEncoded = Base64.encode(JSON.stringify(settings, false, 2));
			dispatch(updateSettingsContent(kitsuneSettingsEncoded, '/kitsune-settings.json'));
			resolve();
		})
		.catch(error => {
			dispatch(append({
				type: 'error',
				text: `something went wrong - if this issue persists, please reach out to us at support@getkitsune.com`
			}));
			reject(error);
		})
});

const disableGateway = gatewayName => (dispatch, getState) => {
	fetchKitsuneSettings(getState)
		.then(settings => {
			settings.payments.preview = _.reject(settings.payments.preview, (({ gateway }) => gateway === gatewayName));
			settings.payments.live = _.reject(settings.payments.live, (({ gateway }) => gateway === gatewayName));
			const kitsuneSettingsEncoded = Base64.encode(JSON.stringify(settings, false, 2));
			dispatch(updateSettingsContent(kitsuneSettingsEncoded, '/kitsune-settings.json'));
			dispatch(append({
				type: 'progress-with-bold',
				textPreBold: '',
				boldText: gatewayName,
				textPostBold: ` has been disabled successfully`
			}));
		})
		.catch(error => {
			dispatch(append({
				type: 'error',
				text: `something went wrong - if this issue persists, please reach out to us at support@getkitsune.com`
			}));
			throw new Error(error);
		});
};

const disableCallTracker = () => (dispatch, getState) => {
	fetchKitsuneSettings(getState)
		.then(settings => {
			if (settings.call_tracker) {
				delete settings.call_tracker;
			}
			const kitsuneSettingsEncoded = Base64.encode(JSON.stringify(settings, false, 2));
			dispatch(updateSettingsContent(kitsuneSettingsEncoded, '/kitsune-settings.json'));
		})
		.catch(error => {
			dispatch(append({
				type: 'error',
				text: `something went wrong - if this issue persists, please reach out to us at support@getkitsune.com`
			}));
			throw new Error(error);
		});
};

const preEnablePartials = dispatch => new Promise(resolve => {
	const searchPartialTree = filterTree(JSON.parse(localStorage.getItem('project-init')), 'partial');
	const partialFolder = _.find(searchPartialTree.children, ({ name }) => name === 'partial');
	if (!partialFolder) {
		dispatch(append({
			type: 'progress',
			text: `moving the requisite files to your project folder`,
		}));
		const partialSampleEncoded = Base64.encode(partialSample);
		dispatch(singleFileUpload('_header.html', partialSampleEncoded, `/partial/`));
	}
	resolve();
});

export const toggleProjectComponent = (componentID, boolean, componentName, gatewayName) => (dispatch, getState) => {
	const { EVENT_LOG } = config.INTERNAL_SETTINGS.footerTabs;
	dispatch(resetLog());
	dispatch(footerCollapse(EVENT_LOG, null));
	dispatch(append({
		type: 'heading',
		message: 'component progress:'
	}));
	if (boolean) {
		switch (componentName) {
			case 'reports':
				preEnableReports(dispatch, riaComponentName)
					.then(() => dispatch(toggleComponent(componentID, boolean, componentName)));
				break;
			case 'payment gateway':
				preEnableKPay(dispatch, getState, gatewayName)
					.catch(error => {
						throw new Error(error);
					});
				break;
			case 'partial views':
				preEnablePartials(dispatch)
					.then(() => dispatch(toggleComponent(componentID, boolean, componentName)));
				break;
			case 'call tracker':
				preEnableCallTracker(dispatch, getState)
					.then(() => dispatch(toggleComponent(componentID, boolean, componentName)))
				break;
		}
		dispatch(append({
			type: 'progress-with-bold',
			textPreBold: `enabling `,
			boldText: componentName === 'payment gateway' ? gatewayName : componentName,
			textPostBold: ``
		}));
	} else {
		if (componentName === 'payment gateway') {
			dispatch(disableGateway(gatewayName));
		} else if (componentName === 'call tracker') {
			dispatch(disableCallTracker());
			dispatch(toggleComponent(componentID, boolean, componentName));
		} else {
			dispatch(toggleComponent(componentID, boolean, componentName));
		}
	}
};

const toggleComponent = (componentID, boolean, componentName) => (dispatch, getState) => {
	const { toggleEnableRiaApp } = config.API;
	const { ProjectId: projectId } = getState().projectTreeReducer.data;
	return axios.get(`${toggleEnableRiaApp}`
		.replace('{projectId}', projectId)
		.replace('{enable}', boolean)
		.replace('{componentId}', componentID))
		.then(() => {
			dispatch(updateComponentMapping({ componentID, boolean }));
			dispatch(updateSystemDefinedLanguages(componentID, boolean, componentName));
		})
		.catch(error => {
			dispatch(append({
				type: 'error',
				text: `something went wrong - if this issue persists, please reach out to us at support@getkitsune.com`
			}));
			throw new Error(error);
		});
};

const generateSystemDefinedLangauge = languageID => dispatch => {
	return axios.get(`${config.API.schemaDetails}/${languageID}`)
		.then(({ data }) => {
			const { EntityDescription, EntityName } = data;
			dispatch(addSystemDefinedLanguage({
				SchemaId: languageID,
				EntityDescription,
				EntityName
			}));
		})
		.catch(error => {
			dispatch(append({
				type: 'error',
				text: `something went wrong - if this issue persists, please reach out to us at support@getkitsune.com`
			}));
			throw new Error(error);
		});
};

const updateSystemDefinedLanguages = (componentId, boolean, componentName) => (dispatch, getState) => {
	const { allComponents } = getState().projectTreeReducer;
	const { languageId } = _.filter(allComponents, ({ componentID }) => componentID === componentId)[0];
	if (boolean) {
		if (languageId) {
			axios.get(`${config.API.schemaDetails}/${languageId}`)
				.then(({ data }) => {
					const { EntityDescription, EntityName } = data;
					dispatch(append({
						type: 'progress',
						text: `associating ${EntityName} language to your project`,
					}));
					dispatch(addSystemDefinedLanguage({
						SchemaId: languageId,
						EntityDescription,
						EntityName
					}));
					dispatch(append({
						type: 'progress',
						text: `finalizing the setup`,
					}));
					dispatch(append({
						type: 'progress-with-bold',
						textPreBold: ``,
						boldText: componentName,
						textPostBold: ` component has been successfully setup for your project`
					}));
				})
				.catch(error => {
					dispatch(append({
						type: 'error',
						text: `something went wrong - if this issue persists, please reach out to us at support@getkitsune.com`
					}));
					throw new Error(error);
				});
		} else {
			dispatch(append({
				type: 'progress',
				text: `finalizing the setup`,
			}));
			dispatch(append({
				type: 'progress-with-bold',
				textPreBold: ``,
				boldText: componentName,
				textPostBold: ` component has been successfully setup for your project`
			}));
		}
	} else {
		if (languageId) {
			dispatch(delSystemDefinedLanguage(languageId));
		}
		dispatch(append({
			type: 'progress-with-bold',
			textPreBold: `the `,
			boldText: componentName,
			textPostBold: ` component has been disabled successfully`
		}));
	}
};

const resetSystemDefinedLanguages = () => ({ type: schemaCreator.RESET_SYSDEF });

const addSystemDefinedLanguage = sysLang => ({
	type: schemaCreator.ADD_SYSDEFLANG,
	sysLang
});

const delSystemDefinedLanguage = langId => ({
	type: schemaCreator.DEL_SYSDEFLANG,
	langId
});

//helper to check if components settings file already exists
const checkComponentSettingsFile = (filteredTree, componentName) => {
	//find components folder
	const componentsFolder = _.find(filteredTree.children, ({ name }) => name === '__components');
	//check if components folder doesn't exist
	if (!componentsFolder) {
		return false;
	}

	//find component folder inside components folder
	const componentFolder = _.find(componentsFolder.children, ({ name }) => name === componentName);
	//check if component folder doesn't exists
	if (!componentFolder) {
		return false;
	}
	//find settings file inside component folder
	const settingsFiles = _.find(componentFolder.children, ({ name }) => name === 'periodic_performance_report.html');
	//check if settings file doesn't exists
	if (!settingsFiles) {
		return false;
	}

	//if we've come this far in the function, it means all the files were found
	//hence the setting file already exists in the correct location
	return true;
};

const updateComponentMapping = component => ({
	type: projectree.UPDATE_COMPONENT,
	component
});

export const projectDeleteNode = path => ({
	type: projectree.PROJECT_DELETENODE,
	path
});

// action creator to update projectree node
export const updateProjectTreeNode = (newObject, iterator) => ({
	type: projectree.UPDATE_PROJECTREENODE,
	payload: {
		newObject,
		iterator
	}
});

export const sendMailToCore = (subject, body) => axios.post(config.API.sendMail, {
	To: ['core@getkitsune.com'],
	Subject: subject,
	EmailBody: body,
	Type: 1
})
	.then(() => {
		console.log('successfully sent mail to core team'); //eslint-disable-line
	})
	.catch(error => {
		console.log('unable to send mail to core team');//eslint-disable-line
		throw new Error(error);
	});
