import * as types from '../../mutation-types';
import migration from '../../../api/migration';
import store from '../../../store/index';
import { coreEmailId, IDELink, isProdEnv } from "../../../../config/config";

const checkForNetworkError = message => message.toLowerCase() === 'network error';

const state = {
	projectId: '',
	projectName: '',
	projectType: '',
	buildVersion : '',
	isCrawlingStarted: false,
	isNewStageReceived: false,
	isMigrationCompleted : false,// TODO check
	stage: 0,
	stageReceivedFromAPI: 0,
	crawler: {
		analysis: {},
		domainsFound: {},
		importingFiles: {},
		replacedLinks: {},
		isCompleted: false
	},
	optimizer: {
		analyzer: {},
		optimize: {},
		replacer: {},
		isCompleted: false
	},
	stopMigration: false,
	timeIntervalForAPICall: 5000,
	isErrorInStartingCrawl : false,
	isErrorInGettingStageDetails : false,
	apiStatusForStages : {
		gettingDomains : false
	},
	isFirstBuild: false,
	buildPollerCounter: 0,
	isNetworkError: false,
	listOfSelectedDomains: []
}

// getters
const getters = {

	getMigrationDetails: state => state,

	getMigrationStage: state => state.stage,

	getAnalysisStageDetails: state => state.crawler.analysis,

	getCrawledProjectName: state => state.projectName,

	getApiStatusForStages: state => state.apiStatusForStages,

	getProjectStatusAndBuildVersion: state =>  {
		return {
			projectType: state.projectType, buildVersion: state.buildVersion, isFirstBuild : state.isFirstBuild
		}
	},

	domainList: state => state.listOfSelectedDomains,

	projectNameAndId: state => ({
		projectId: state.projectId,
		projectName: state.projectName
	})

}

// actions
const actions = {

	startCrawling ({ commit,dispatch }, payload) {
		// this hides the action modal and show the migration modals
		commit(types.resetMigrationProcess);
		commit(types.toggleActionModal);
		commit(types.updateOverlayComponent, true);
		commit(types.setMigrationbuildcardsComponentStatus, true);
		commit(types.setProjectNameForMigration, payload.Url);

		migration.startCrawling(payload, (success, response) => {
			if(success && !response.IsError) {
				dispatch('getAllProjects');
				dispatch('addUpdatesToPanel', {
					read: false,
					type: 'user-notify',
					title: 'project created successfully',
					content: `the project '${store.state.action.currentProjectName}' has been created successfully
					. the credentials for K-admin have been emailed to ${store.state.app.UserEmail}`,
					url: `${IDELink}`.replace('{projectid}', response.ProjectId),
					actionName: 'edit project',
					callback: null,
					extraInfo: '',
					className: 'alert'
				});
				if(isProdEnv) {
					dispatch('triggerEmail', {
						To: [coreEmailId],
						Subject: `new project created by ${store.state.app.UserEmail} on ${new Date().toLocaleString()}.`,
						EmailBody: `Project Name: ${state.projectName}<br>Project Id: ${state.projectId}`
					})
				}
				// if api call is success update the stage and migration status
				commit(types.setProjectIdForMigration, response.ProjectId);
				commit(types.setIsCrawlingStartedForMigration, true);
				commit(types.setProjectTypeInMigrationProcess, store.state.projects.projectType.CRAWL);
				// commit(types.setBuildVersionInMigrationProcess,1);
				commit(types.setIsFirstBuildInMigrationProcess,true);
			} else {
				// TODO Handle error
				response.message && checkForNetworkError(response.message)
					? commit(types.setIsNetworkError, true)
					: commit(types.setIsErrorInStartingCrawlInMigrationProcess, true);
			}
		})
	},

	retryCrawling ({ commit, dispatch }) {
		dispatch('startCrawling',{ Url : state.projectName });
		commit(types.setIsErrorInStartingCrawlInMigrationProcess, false);
		commit(types.setIsNetworkError, false);
	},

	getAnalysisDetails ( { commit, dispatch } ) {
		migration.getAnalyzeDetails(state.projectId, (success, response) => {
			if(success) {
				commit(types.setAnalysisInMigration, response);
				dispatch('poll', { stage: response.Stage, optimizer: false });
			} else {
				response.message && checkForNetworkError(response.message)
					? commit(types.setIsNetworkError, true)
					: commit(types.setIsErrorInGettingStageDetailsInMigrationProcess, true);
				// TODO error handling
			}
		})
	},

	getListDomainsFound ({ commit }) {
		commit(types.setIsGettingDomainsInMigrationProcess, true);
		migration.getListDomainsFound(state.projectId, (success, response) => {
			commit(types.setIsGettingDomainsInMigrationProcess,false);
			if(success && !response.IsError) {
				commit(types.setDomainsFoundInMigration, response.DomainList);
				commit(types.setStageForMigration, 2);
				commit(types.setIsNewStageReceivedForMigration, false);
			} else {
				response.message && checkForNetworkError(response.message)
					? commit(types.setIsNetworkError, true)
					: commit(types.setIsErrorInGettingStageDetailsInMigrationProcess, true);
				// TODO error handling
			}
		})
	},

	submitSelectedDomains ({ dispatch, commit }) {
		const payload = {
			projectId: state.projectId,
			selectedDomains: state.listOfSelectedDomains
		};
		migration.saveSelectedDomains(payload, (success, response) => {
			if(success && response) {
				dispatch('poll', { stage: 3, optimizer: false });
			} else {
				response.message && checkForNetworkError(response.message)
					? commit(types.setIsNetworkError, true)
					: commit(types.setIsErrorInGettingStageDetailsInMigrationProcess, true);
				// TODO error logging
			}
		})
	},

	getDownloadDetails ({ commit, dispatch }) {
		migration.getDownloadDetails(state.projectId, (success, response) => {
			if(success) {
				commit(types.setDownloadDetailsInMigration, response);
				dispatch('poll', { stage: response.Stage, optimizer: false });
			} else {
				response.message && checkForNetworkError(response.message)
					? commit(types.setIsNetworkError, true)
					: commit(types.setIsErrorInGettingStageDetailsInMigrationProcess, true);
				// TODO error
			}
		})
	},

	getLinksReplacedDetails ({ commit, dispatch }) {
		migration.getFilesReplacedDeatils(state.projectId, (success, response) => {
			if(success){
				commit(types.setReplacedLinksInMigration, response);
				// to remove when it start working
				if(response.Status === 5) {
					dispatch('setIsCompletedCrawling', true);
					migration.updateCrawlComplete(state.projectId, (success, response) => {
						if(success && response) {
							dispatch('poll', { stage: 5 , optimizer: false });
						}
					})
				} else {
					dispatch('poll', { stage: response.Status , optimizer: false });
				}

			} else {
				//
				response.message && checkForNetworkError(response.message)
					? commit(types.setIsNetworkError, true)
					: commit(types.setIsErrorInGettingStageDetailsInMigrationProcess, true);
			}
		})
	},

	setIsCompletedCrawling ({ commit }, payload) {
		commit(types.setCrawlerIsCompletedInMigration, payload);
	},

	getBuildAnalyseDetails ({ commit, dispatch }) {
		function callItAgain() {
			dispatch('getBuildAnalyseDetails');
		}
		migration.getBuidStats(state.projectId, (success, response) => {
			if(success) {
				const { Analyzer, Stage } = response;
				commit(types.setOptimizerAnalyzerInMigration, Analyzer);
				dispatch('poll', { stage: Stage, optimizer: true });
				commit(types.incrementBuildCounter, true);
			} else {
				//sometimes api returns 204 as content has not been created
				//so we are polling the api and looking for 204 until 15 secs
				//if it returns 200 in between, its considered success, else error
				if(response === 204 && state.buildPollerCounter < 5) {
					commit(types.incrementBuildCounter);
					setTimeout(() => callItAgain(), 3000);
				} else {
					commit(types.incrementBuildCounter, true);
					response.message && checkForNetworkError(response.message)
						? commit(types.setIsNetworkError, true)
						: commit(types.setIsErrorInGettingStageDetailsInMigrationProcess, true);
				}
			}
		})
	},

	getBuildOptimizeDetails ({ commit, dispatch }) {
		migration.getBuidStats(state.projectId, (success, response) => {
			if(success) {
				commit(types.setOptimizeInMigration,response. Optimizer);
				dispatch('poll',{ stage: response.Stage, optimizer: true });
			} else {
				response.message && checkForNetworkError(response.message)
					? commit(types.setIsNetworkError, true)
					: commit(types.setIsErrorInGettingStageDetailsInMigrationProcess, true);
			}
		})
	},

	getBuildReplacerDetails ({ commit, dispatch }) {
		migration.getBuidStats(state.projectId, (success, response) => {
			if(success){
				commit(types.setReplacerInMigration, response.Replacer);
				dispatch('poll', { stage: response.Stage, optimizer: true });
			} else {
				response.message && checkForNetworkError(response.message)
					? commit(types.setIsNetworkError, true)
					: commit(types.setIsErrorInGettingStageDetailsInMigrationProcess, true);
			}
		})
	},

	setIsMigrationCompleted ({ commit }, payload){
		commit(types.setMigrationCompleted, payload);
	},

	setIsNewStageReceived ({ commit }, payload) {
		commit(types.setIsNewStageReceivedForMigration, payload)
	},

	setStageReceived ({ commit }, payload) {
		commit(types.setStageReceivedFromAPI, payload)
	},

	poll ({ commit, dispatch }, payload) {
		if(!state.stopMigration) {
			if (payload.optimizer) {
				payload.stage = (payload.stage <= 3) ? 5 : (payload.stage + 5 - 3);
			}
			commit(types.setStageReceivedFromAPI, payload.stage);
			if(state.stage !== state.stageReceivedFromAPI) {
				let expectedStage = state.stage + 1;
				let receivedStage = state.stageReceivedFromAPI;
				if(expectedStage !== receivedStage) {
					commit(types.setStageReceivedFromAPI, expectedStage);
				}
			}

			if (state.isNewStageReceived) {
				commit(types.setStageForMigration, state.stageReceivedFromAPI);
				commit(types.setIsNewStageReceivedForMigration, false);
			}

			// if(payload.stage === 3 && !payload.optimize){
			// 	commit(types.setStageForMigration, state.stageReceivedFromAPI);
			// }
			switch (state.stageReceivedFromAPI) {
			case -1:
				commit(types.setIsErrorInGettingStageDetailsInMigrationProcess, true);
				commit(types.setIsErrorInStartingCrawlInMigrationProcess, true);
				break;
			case 0:
			case 1:
				setTimeout(() => {
					dispatch('getAnalysisDetails');
				}, state.timeIntervalForAPICall);
				break;
			case 2:
				setTimeout(() => {
					dispatch('getListDomainsFound');
				}, state.timeIntervalForAPICall);
				break;
			case 3:
				setTimeout(() => {
					dispatch('getDownloadDetails');
				}, state.timeIntervalForAPICall);
				break;
			case 4:
				setTimeout(() => {
					dispatch('getLinksReplacedDetails');
				}, state.timeIntervalForAPICall);
				break;
			case 5:
				setTimeout(() => {
					dispatch('getBuildAnalyseDetails');
				}, state.timeIntervalForAPICall);
				break;
			case 6:
				setTimeout(() => {
					dispatch('getBuildOptimizeDetails');
				}, state.timeIntervalForAPICall);
				break;
			case 7:
				setTimeout(() => {
					dispatch('getBuildReplacerDetails');
				}, state.timeIntervalForAPICall);
				break;
			case 8:
				commit(types.setStageForMigration, state.stageReceivedFromAPI);
				dispatch('stopMigration');
				dispatch('setIsMigrationCompleted', true);
				break;
			default:
				console.log(`stage error in poll : ${payload}`);
			}

			if (state.stage !== state.stageReceivedFromAPI) {
				dispatch('completeStage', state.stage);
				commit(types.setIsNewStageReceivedForMigration, true);
			}
		}
	},

	minimizeMigrationCards ({ commit, dispatch }) {
		dispatch('stopMigration');
		commit(types.updateOverlayComponent, false);
		commit(types.setMigrationbuildcardsComponentStatus, false);
	},

	resetMigrationStatus({ commit }) {
		commit(types.resetMigrationProcess);
	},

	stopMigration({ commit }) {
		commit(types.setStopMigration, true);
	},

	completeStage ({ commit }, payload) {
		switch (payload) {
		case 3:
			commit(types.completeImportingFilesInMigrationProcess);
			break;
		case 5:
			commit(types.completeInsertingPlaceholdersInMigrationProcess);
			break;
		case 6:
			commit(types.completeOptimizingFilesInMigrationProcess);
			break;
		case 7:
			commit(types.completeGeneratingHTMLInMigrationProcess);
			break;
		default:
			// console.log('not');
			break;
		}
	},

	getStageForMigration({ commit, dispatch }, payload) {
		let projectType = store.state.projects.projectType;
		commit(types.resetMigrationProcess);
		commit(types.setProjectIdForMigration, payload.projectId);
		commit(types.setProjectNameForMigration, payload.projectName);
		commit(types.updateOverlayComponent, true);
		commit(types.setMigrationbuildcardsComponentStatus, true);
		commit(types.setProjectTypeInMigrationProcess, payload.projectType);
		if(payload.projectType == projectType.CRAWL) {
			dispatch('getStageForResumingMigrationFromCrawler');
		} else {
			dispatch('getStageForResumingFromBuildStats');
		}

	},

	getStageForResumingMigrationFromCrawler({ commit, dispatch }, payload) {
		migration.getAnalyzeDetails(state.projectId, (success, response) => {
			if(success){
				let stage = response.Stage;
				if(stage === 5){
					dispatch('getStageForResumingFromBuildStats');
				} else{
					commit(types.setIsFirstBuildInMigrationProcess, true);
					dispatch('resumeMigration', { stage: stage, optimizer: false });
				}
			} else {
				commit(types.setIsErrorInGettingStageDetailsInMigrationProcess, true);
			}
		})
	},

	getStageForResumingFromBuildStats({ commit, dispatch }, payload) {
		migration.getBuidStats(state.projectId, (success, response) => {
			if(success) {
				let stage = response.Stage;
				commit(types.setIsFirstBuildInMigrationProcess, response.FirstBuild);
				dispatch('resumeMigration', { stage: stage, optimizer: true })
			} else {
				commit(types.setIsErrorInGettingStageDetailsInMigrationProcess, true);
			}
		})
	},

	getReplacerDetailsForResumingMigrationProcess({ commit, dispatch }, payload) {
		let callRequired = false;
		if((payload.stage <= 5 && payload.stage >= 3)  && !payload.optimize) {
			callRequired = true;
		}

		if(payload.stage <= 3 && payload.optimize) {
			callRequired = true;
		}

		if(callRequired) {
			migration.getFilesReplacedDeatils(state.projectId, (success, response) => {
				if(success) {
					commit(types.setReplacedLinksInMigration, response);
				} else {
					// todo handle error
				}
			})
		}

	},

	resumeMigration({ commit, dispatch }, payload) {
		let stage = payload.stage;

		if (payload.optimizer) {
			stage = (payload.stage <= 3) ? 5 : (payload.stage + 5 - 3);
		}

		commit(types.setStageReceivedFromAPI, stage);
		commit(types.setStageForMigration, stage);
		dispatch('getReplacerDetailsForResumingMigrationProcess', payload);
		dispatch('poll', payload);
		setTimeout(() => {
			commit(types.setIsCrawlingStartedForMigration, true);
		}, state.timeIntervalForAPICall + 500);

	},

	setSelectedDomains({ commit }, payload) {
		commit(types.setSelectedDomains, payload);
	}

};

// mutations
const mutations = {

	[types.setProjectIdForMigration] (state, payload) {
		state.projectId = payload;
	},

	[types.setIsCrawlingStartedForMigration] (state, payload) {
		state.isCrawlingStarted = payload;
	},

	[types.setStageForMigration] (state, payload) {
		state.stage = payload; // TODO for build stages
	},

	[types.setAnalysisInMigration] (state, payload) {
		state.crawler.analysis = payload;
	},

	[types.setDomainsFoundInMigration] (state, payload) {
		state.crawler.domainsFound = payload;
	},

	[types.setDownloadDetailsInMigration] (state, payload) {
		state.crawler.importingFiles = payload;
	},

	[types.setReplacedLinksInMigration] (state, payload) {
		state.crawler.replacedLinks = payload;
	},

	[types.setOptimizerAnalyzerInMigration] (state, payload) {
		state.optimizer.analyzer = payload;
	},

	[types.setCrawlerIsCompletedInMigration] (state, payload) {
		state.crawler.isCompleted = payload;
	},

	[types.setOptimizeInMigration] (state, payload) {
		state.optimizer.optimize = payload;
	},

	[types.setReplacerInMigration] (state, payload) {
		state.optimizer.replacer = payload;
	},

	[types.setMigrationCompleted] (state, payload) {
		state.isMigrationCompleted = payload;
	},

	[types.setIsNewStageReceivedForMigration] (state, payload) {
		state.isNewStageReceived = payload;
	},

	[types.setStageReceivedFromAPI] (state, payload) {
		state.stageReceivedFromAPI = payload;
	},

	[types.setProjectNameForMigration] (state, payload) {
		state.projectName = payload;
	},

	[types.setStopMigration] (state, payload) {
		state.stopMigration = payload;
	},

	[types.resetMigrationProcess] (state) {
		state.stage = 0;
		state.stageReceivedFromAPI = 0;
		state.projectId = '';
		state.projectName = '';
		state.isCrawlingStarted = false;
		state.isNewStageReceived = false;
		state.isMigrationCompleted = false;
		state.crawler.isCompleted = false;
		state.crawler.analysis = {};
		state.crawler.domainsFound = [];
		state.crawler.replacedLinks = {};
		state.crawler.importingFiles = {};
		state.optimizer.isCompleted = false;
		state.optimizer.optimize = {};
		state.optimizer.replacer = {};
		state.optimizer.analyzer = {};
		state.stopMigration = false;
		state.isErrorInGettingStageDetails = false;
		state.isErrorInStartingCrawl = false;
		state.isFirstBuild = false;
	},

	[types.completeImportingFilesInMigrationProcess] (state) {
		const { importingFiles } = state.crawler;
		importingFiles.ScriptsDownloaded = importingFiles.ScriptsFound;
		importingFiles.StylesDownloaded = importingFiles.StylesFound;
		importingFiles.AssetsDownloaded = importingFiles.AssetsFound;

	},

	[types.completeOptimizingFilesInMigrationProcess] (state) {
		const { optimize } = state.optimizer;
		optimize.FILE = optimize.TOTAL_FILE;
		optimize.LINK = optimize.TOTAL_LINK;
		optimize.SCRIPT = optimize.TOTAL_SCRIPT;
		optimize.STYLE = optimize.TOTAL_STYLE;
	},

	[types.completeInsertingPlaceholdersInMigrationProcess] (state) {
		const { replacedLinks } = state.crawler;
		const { analyzer } = state.optimizer;
		analyzer.STYLE = analyzer.TOTAL_STYLE;
		analyzer.LINK = analyzer.TOTAL_LINK;
		replacedLinks.LinksReplaced = replacedLinks.LinksFound;
	},

	[types.completeGeneratingHTMLInMigrationProcess] (state) {
		const { replacer } = state.optimizer;
		replacer.STYLE = replacer.TOTAL_STYLE;
		replacer.LINK = replacer.TOTAL_LINK;
	},

	[types.setIsErrorInStartingCrawlInMigrationProcess] (state, payload) {
		state.isErrorInStartingCrawl = payload;
	},

	[types.setIsNetworkError] (state, payload) {
		state.isNetworkError = payload;
	},

	[types.setIsErrorInGettingStageDetailsInMigrationProcess] (state, payload) {
		state.isErrorInGettingStageDetails = payload;
	},

	[types.setIsGettingDomainsInMigrationProcess] (state, payload){
		state.apiStatusForStages.gettingDomains = payload;
	},

	[types.setProjectTypeInMigrationProcess] (state, payload){
		state.projectType = payload;
	},

	[types.setBuildVersionInMigrationProcess] (state, payload){
		state.buildVersion = payload;
	},

	[types.setIsFirstBuildInMigrationProcess] (state, payload){
		state.isFirstBuild = payload;
	},

	[types.incrementBuildCounter] ({ buildPollerCounter }, payload ) {
		payload ? buildPollerCounter = 0 : buildPollerCounter++;
	},

	[types.setSelectedDomains] (state, payload) {
		state.listOfSelectedDomains = payload;
	}

}

export default {
	state,
	getters,
	actions,
	mutations
}
