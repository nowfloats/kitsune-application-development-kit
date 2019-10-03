import {
	createProject,
	apiBaseUrl,
	apiIdeUrl,
	uploadFolder,
	abortCrawl
} from '../../config/config'
import axios from 'axios'
import store from '../store/index'

export default {
	//TODO get userEmail from store

	uploadFolder(folderData, callback) {
		axios.post(`${apiIdeUrl}${uploadFolder}${folderData.ProjectId}/resourceUpload`, {
			UserEmail : store.state.app.UserEmail,
			IsStatic: folderData.IsStatic,
			SourcePath : folderData.SourcePath,
			FileContent : folderData.FileContent
		})
			.then(res => {
				callback(true, res);
			})
			.catch( err => {
				callback(false, err);
			})
	},
	createProject(project, callback) {
		axios.post(`${apiBaseUrl}${createProject}`, {
			ProjectName: project.name,
			Url: project.name,
			UserEmail : store.state.app.UserEmail
		})
			.then(response => {
				callback(true, response.data);
			})
			.catch(err => {
				callback(false, err);
			})
	},

	abortCrawl(payload, callback) {
		axios.post(`${apiBaseUrl}${abortCrawl}`, {
			ProjectId: payload,
			StopCrawl: true
		})
			.then(({ data }) => callback(true, data))
			.catch(err => callback(false, err));
	}
}
