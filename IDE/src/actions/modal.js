/*Herein lies all the editor actions*/
import React from 'react';
import { modals } from './actionTypes';
import axios from 'axios';
import { config } from '../config';
import ProjectError, { projectErrorLabel } from '../components/modals/project-error/index';
import { showLoading, hideLoading } from '../actions/loader';
import OpenProject from '../components/modals/open-project/index';
import { toastr } from 'react-redux-toastr';
//action for initializing react-modal

//action for opening a modal
export const modalOpen = (html, label, callback) => ({
	type: modals.MODAL_OPEN,
	payload: { html: html, label: label, show: true, callback }
});

//action for closing a react-modal
export const modalClose = () => ({
	type: modals.MODAL_CLOSE,
	payload: { html: {}, label: 'init label', show: false }
});

//action for fetching theme for the logged in user and opening a modal
export const modalThemesFetch = label => (dispatch, getState) => {
	const { login } = getState();
	const { userID } = login;
	const { projectList } = config.API;

	dispatch(showLoading(config.INTERNAL_SETTINGS.loadingText.FETCH_PROJECTS.text));
	dispatch(modalThemesFetching());

	return axios.get(`${projectList}?user=${userID}`)
		.then(response => {
			dispatch(modalOpen(<OpenProject data={response.data} />, label, null));
			dispatch(hideLoading());
		})
		.catch(() => {
			dispatch(modalOpen(<ProjectError />, projectErrorLabel, null));
			dispatch(hideLoading());
		});
};

//action to indicate themes being fetched
export const modalThemesFetching = () => ({
	type: modals.MODAL_THEMESFETCHING,
	payload: { isFetching: true }
});

const uploadImages = selectedFiles => new Promise((resolve, reject) => {
	const { userIdForDevKitsune, bugImages } = config.API;
	let filesUploaded = [];

	if (selectedFiles.length > 0) {
		selectedFiles.forEach(({ fileObject, name }) => {
			const data = new FormData();
			data.append('file', fileObject);

			axios({
				method: 'POST',
				headers: {
					'Authorization': userIdForDevKitsune,
					'content-type': 'multipart/form-data'
				},
				url: `${bugImages}?assetFileName=${name}`,
				data
			})
				.then(({ data }) => {
					filesUploaded = [...filesUploaded, data];
					if (filesUploaded.length === selectedFiles.length) {
						resolve(filesUploaded);
					}
				})
				.catch(error => reject(error));
		});
	} else {
		// no images attached
		resolve([])
	}
});

// action to report bug
export const reportBug = payload => (dispatch, getState) => {
	const { userIdForDevKitsune, reportBug } = config.API;
	const { description, selectedFiles } = payload;

	uploadImages(selectedFiles)
		.then(images => {
			const { login, projectTreeReducer } = getState();
			const { ProjectId } = projectTreeReducer.data;
			const data = {
				ActionData: {
					description,
					images,
					projectId: ProjectId ? ProjectId : null
				},
				WebsiteId: login.userID
			};

			axios({
				method: 'POST',
				headers: {
					'Authorization': userIdForDevKitsune,
					'content-type': 'application/json'
				},
				url: reportBug,
				data
			})
				.then(() => {
					dispatch(modalClose());
					toastr.success('Thank you!', 'our developers will look into this.');
				})
				.catch(() => toastr.error('Oops!', 'something went wrong.'));
		})
		.catch(() => toastr.error('Oops!', 'something went wrong.'));
};
