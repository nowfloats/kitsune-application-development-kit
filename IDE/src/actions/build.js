import axios from 'axios';
import { footerCollapse, footerReplace } from './footer';
import { build } from './actionTypes';
import { footerUpdate, append } from './footer';
import { config, isProdDeployed, httpDomainWithoutSSL } from '../config';
import _ from 'lodash';
import { getFileExtension } from "./editor";
import { sendMailToCore } from "./projectTree";

//Message Map for build Messages
const messageMap = new Map();
//Line to Replace Map for build Messages
const linesToReplace = new Map();
messageMap.set(0, 'queued for kit-compiler');
linesToReplace.set(0, 2);
messageMap.set(1, 'kit-compiler: initiated compilation');
linesToReplace.set(1, 3);
messageMap.set(2, 'identifying required serverless components');
linesToReplace.set(2, 2);
messageMap.set(3, 'analyzing static assets');
linesToReplace.set(3, 3);
messageMap.set(4, 'minifying and compressing static assets');
linesToReplace.set(4, 3);
messageMap.set(5, 'updating static asset references to the new optimised asset path');
linesToReplace.set(5, 3);
messageMap.set(6, 'serverless components configured. build complete.');
linesToReplace.set(6, 3);
const projectTypeError = 'project';

// build the project
export const buildProject = () => (dispatch, getState) =>
new Promise((resolve, reject) => {
	const { login, projectTreeReducer } = getState();
	const { userID } = login;
	const { ProjectId: projectId } = projectTreeReducer.data;
	const { EVENT_LOG } = config.INTERNAL_SETTINGS.footerTabs;
	const { build } = config.API;
	//Expand footer and empty it
	dispatch(footerCollapse(EVENT_LOG, null));
	dispatch(footerUpdate([], EVENT_LOG));
	const header = {
		ProjectId: projectId,
		UserEmail: userID
	};
	dispatch(building());
	startBuild({ header, userID, projectId, build, dispatch, resolve, reject })
});

//function to start build
const startBuild = args => {
	const {
		header,
		userID,
		projectId,
		build,
		dispatch,
		resolve,
		reject } = args;

	axios.post(`${build}?user=${userID}&projectid=${projectId}`, header)
		.then(() => {
			//Start polling the build api to check on the progress of the build
			dispatch(buildPoll(10, 1000))
			.then((message) => {
				dispatch(buildComplete(true));
				resolve(message);
			})
			.catch((error) => {
				dispatch(buildComplete(false));
				reject(error);
			})
		})
		.catch(() => {
			const { START_FAILED } = config.INTERNAL_SETTINGS.buildErrors;
			reject(START_FAILED);
		});
};

//action to poll build status
export const buildPoll = (timeout, interval) => (dispatch, getState) => {
	timeout = timeout * 60 * 1000; //convert minute to ms
	const endTime = Number(new Date()) + (timeout || 2000);
	interval = interval || 100;

	const checkCondition = (resolve, reject) => {
		// Get build status
		dispatch(fetchBuild())
		.then((isCompleted) => {
			//if build is complete we're done!
			if(isCompleted) {
				resolve('build complete');
			}
			// If the build isn't complete but the timeout hasn't elapsed, go again
			else if (Number(new Date()) < endTime) {
				setTimeout(checkCondition, interval, resolve, reject);
			}
			// Build isn't complete and too much time, reject!
			else {
				const { POLL_TIMEOUT } = config.INTERNAL_SETTINGS.buildErrors;
				reject(POLL_TIMEOUT);
			}
		})
		.catch((error) => {
			//check if there are build errors
			const { FAILED } = config.INTERNAL_SETTINGS.buildErrors;
			if(error.type === FAILED.type) {
				//pass the reject along the pipeline.
				reject(FAILED);
			}
			// else assume the api went bezerk
			else {
				console.log('build api went into error', error); //eslint-disable-line
				//Api failed. Check if there have been retries before
				const { buildStatusRetries } = getState().buildReducer;
				//If there have been 10 retries, reject the build request.
				if(buildStatusRetries >= 3) {
					const { RETRY_TIMEOUT } = config.INTERNAL_SETTINGS.buildErrors;
					dispatch(setRetry({ buildStatusRetries: 0 }));
					reject(RETRY_TIMEOUT);
				}
				// If there haven't been 10 retries and the timeout hasn't elapsed, go again
				else if (Number(new Date()) < endTime) {
					dispatch(setRetry({ buildStatusRetries: buildStatusRetries+1 }));
					setTimeout(checkCondition, interval, resolve, reject);
				}
				// There haven't been 10 retries but too much time has elapsed, reject!
				else {
					const { POLL_TIMEOUT } = config.INTERNAL_SETTINGS.buildErrors;
					dispatch(setRetry({ buildStatusRetries: 0 }));
					reject(POLL_TIMEOUT);
				}
			}
		});
	};

	return new Promise(checkCondition);
};

//action to fetch build status
export const fetchBuild = () => (dispatch, getState) =>
new Promise((resolve, reject) => {
	const { login, projectTreeReducer } = getState();
	const { userID } = login;
	const { ProjectId: projectId } = projectTreeReducer.data;
	const { build } = config.API;
	axios.get(`${build}?user=${userID}&projectid=${projectId}`)
	.then((response) => {
		const { Stage, Error } = response.data;
		//if build has failed, reject the promise
		//and output the errors for the user to fix
		if(Stage < 0) {
			const { FAILED } = config.INTERNAL_SETTINGS.buildErrors;
			dispatch(outputErrors(response.data))
				.then(() => {
					dispatch(setLastStage(0));
					if(isProdDeployed) {
						const { CreatedOn } = response.data;
						const buildDate = new Date(CreatedOn);
						const currentDate = new Date();
						const hoursElapsed = Math.abs(currentDate - buildDate) / 36e5;
						if(hoursElapsed < 12) {
							const { name } = getState().projectTreeReducer;
							const { PhoneNumber } = getState().login.developerDetails;
							const prettyErrors = JSON.stringify(Error, false, 2);
							const subject = `[URGENT] build failed for ${userID} on ${new Date()}`;
							const body = `Project Name: ${name}<br />Project ID: ${projectId}<br />Developer Phone: ${PhoneNumber}
<br /><pre>Errors: ${prettyErrors}</pre>`;
							sendMailToCore(subject, body);
						}
					}
					reject(FAILED);
				});
		}
		//else output the progress to the user.
		else {
			dispatch(outputConsole(response.data))
				.then(() => {
					dispatch(setLastStage(response.data.Stage));
					resolve(response.data.IsCompleted);
				});
		}
	})
	.catch((error) => {
		reject(error);
	});
});

//output console message on all stages
const outputConsole = data => (dispatch, getState) =>
new Promise((resolve) => {
	const { buildReducer, publishReducer, projectTreeReducer } = getState();
	const { defaultCustomer } = publishReducer;
	const { lastStage } = buildReducer;
	const { isNFSchema } = projectTreeReducer;
	const { ProjectId } = projectTreeReducer.data;
	const { Stage } = data;
	//If current stage is same as last stage
	//replace the current data in footer
	if(Stage === lastStage) {
		dispatch(footerReplace(generateBuildMessage(Stage, data, defaultCustomer, isNFSchema, ProjectId),
			linesToReplace.get(Stage)));
	}
	//else if current stage is last stage + 1
	//Append new stage data
	else if (Stage === lastStage + 1) {
		dispatch(footerReplace(generateBuildMessage(lastStage, data, defaultCustomer, isNFSchema, ProjectId),
			linesToReplace.get(lastStage)))
			.then(() => dispatch(append(generateBuildMessage(Stage, data, defaultCustomer, isNFSchema, ProjectId))));
	}
	//else if current stage is > last stage + 1
	//Append old stage data and new stage data
	else if (Stage > lastStage + 1) {
		dispatch(footerReplace(generateBuildMessage(lastStage, data, defaultCustomer, isNFSchema, ProjectId),
			linesToReplace.get(lastStage)))
			.then(() => {
				for(let i = lastStage+1; i <= Stage; i++) {
					dispatch(append(generateBuildMessage(i, data, defaultCustomer, isNFSchema, ProjectId)));
				}
			});
	}
	resolve();
});

//output kitsune errors on error stages
const outputErrors = data => dispatch =>
new Promise(resolve => {
	const { Error } = data;
	if(Error && Error.length) {
		let consoleData = [
			{ text: `the build process went into error.`, type: 'error' },
			{ text: `please fix the following ${Error.length} error${Error.length === 1 ? '' : 's' }:`, type: 'error' }
		];
		//Group Error by file path
		const groupedErrors = _.chain(Error)
			.map(iterator => ({
				...iterator,
				SourcePath: iterator.SourcePath || projectTypeError
			}))
			.orderBy('SourcePath', 'desc')
			.groupBy('SourcePath')
			.value();
		_.each(groupedErrors, (value, key) => consoleData = [ ...consoleData, generateErrorMessage({ value, key }) ]);
		consoleData = [
			...consoleData,
			{ type: 'error-urgent' }
		];
		dispatch(append(consoleData));
	} else {
		const consoleData = [
			{ text: `the build process went into error.`, type: 'error' },
			{
				text: `something went wrong - if this issue persists, please reach out to us at support@getkitsune.com`,
				type: 'error'
			},
			{ type: 'error-urgent' }
		];
		dispatch(append(consoleData));
	}
	resolve();
});

//helper to generate error messages
const generateErrorMessage = ({ value, key }) => {
	//check if error is project level
	if(key === projectTypeError) {
		//error is project level
		return {
			type: projectTypeError,
			errors: value.map(({ Message }) => ({ icon: 'info-circle', text: Message }))
		};
	} else {
		//error is file level
		return {
			icon: getFileFont(key),
			text: ` has ${value.length} error${value.length === 1 ? '' : 's'}.`,
			type: 'file',
			path: key
		};
	}
};

//get fileExtension font
const getFileFont = fileName => {
	const fileExtension = getFileExtension(fileName);
	const { extensionMap, extensionsIconMap } = config.INTERNAL_SETTINGS;
	//If our mapper doesn't recognize the extension, then open as a text file.
	const fileType = extensionMap.get(fileExtension) === undefined ? 'text' : extensionMap.get(fileExtension);

	return extensionsIconMap.get(fileType) === undefined ? 'fas fa-file' : extensionsIconMap.get(fileType);
};

//generate build messages for all stages
const generateBuildMessage = (stage, data, defaultCustomer, isNFSchema, projectID) => {
	let result = [
		{ text: `stage ${stage}:`, type: 'info' },
		{ text: messageMap.get(stage), type: 'info' }
	];
	//if stage needs progress
	if(stage === 1 || stage === 3 || stage === 4 || stage === 5) {
		result = result.concat({ text: getProgressBarMessage(stage, data), type: 'progress' });
	}
	//else if stage needs project link(completed)
	else if(stage === 6) {
		result = result.concat(
			{
				text: 'preview project',
				type: 'link',
				href: `${httpDomainWithoutSSL}/preview/project=${projectID}/customer=${defaultCustomer}`
			},
			{
				type: 'view-arch',
				text: 'view cloud-native architecture of your application',
				class: 'view-architecture',
			}
		);
	}
	return result
};

// UI type
// Progress: [█████████████████████████████████████████████-----] 90.0% Complete
function progressBar(count, total) {
	if(count > total) {
		return `progress: `;
	}
	if(!count && !total) {
		count = 0;
		total = 100;
	}

	const completedCount = parseInt((count/total)*100);
	const nonComplete = 100 - completedCount;
	return `progress: [  ${'#'.repeat(completedCount*0.25)}
						${'-'.repeat(nonComplete)}  ] ${completedCount}% complete`;
}

function calFiles(files) {
	const { LINK, SCRIPT, STYLE, FILE } = files;
	const link = LINK ? parseInt(LINK) : 0;
	const script = SCRIPT ? parseInt(SCRIPT) : 0;
	const style = STYLE ? parseInt(STYLE) : 0;
	const file = FILE ? parseInt(FILE) : 0;
	return link + script + style + file;
}

function getProgressBarMessage(stage, data) {
	//TODO: Make this a pure function
	let changing, processed, modified;
	const { Compiler, Analyzer, Optimizer, Replacer } = data;

	if(stage === 1 && Compiler) {
		changing = calFiles(Compiler);
		modified = Compiler.TOTAL;
		processed = `${progressBar(changing, modified)} | ${changing}/${modified} compiled`;
		return processed;
	}
	else if(stage === 3 && Analyzer) {
		changing = calFiles(Analyzer);
		modified = Analyzer.TOTAL;
		processed = `${progressBar(changing, modified)} | ${changing}/${modified} analyzed`;
		return processed;
	}
	else if(stage === 4 && Optimizer) {
		changing = calFiles(Optimizer);
		modified = Optimizer.TOTAL;
		processed = `${progressBar(changing, modified)} | ${changing}/${modified} optimized`;
		return processed;
	}
	else if(stage === 5 && Replacer) {
		changing = calFiles(Replacer);
		modified = Replacer.TOTAL;
		processed = `${progressBar(changing, modified)} | ${changing}/${modified} updated`;
		return processed;
	}
}

//action to set retry variables
export const setRetry = payload => {
	return {
		type: build.SET_RETRY,
		payload: payload
	}
};

//action to indicate project is building
export const building = () => {
	return {
		type: build.BUILDING,
		payload: { isFetching: true, isCompleted: false }
	};
};

//action to indicate project has completed building
export const buildComplete = (isSuccessful) => {
	const newState = isSuccessful ? { isFetching: false, isCompleted: true, lastStage: 0, lastBuildStamp: Date.now() } :
		{ isFetching: false, isCompleted: true, lastStage: 0 };
	return {
		type: build.BUILD_COMPLETE,
		payload: newState
	};
};

//action to set last build stage
export const setLastStage = stage => {
	return {
		type: build.SET_STAGE,
		payload: { lastStage: stage }
	}
};
