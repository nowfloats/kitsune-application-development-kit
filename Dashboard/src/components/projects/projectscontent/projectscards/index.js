import { defaultImageLinkForCards, IDELink } from '../../../../../config/config'
import projectsFooter from './projectsFooter/ProjectsFooter.vue'
// import * as constants from '../../../../../config/config'
import { mapGetters } from 'vuex'

export default {
	name : 'projectscards',

	components: {
		projectsFooter
	},

	props : [
		'projectName',
		'projectId',
		'createdOn',
		'projectStatus' ,
		'screenShotUrl',
		'isProjectsMenu',
		'isContextMenuRequired',
		'kitsuneUrl',
		'isHidden',
		'version',
		'projectStage',
		'customerId',
		'domainVerified',
		'projectType',
		'isChatBot'
	],

	computed : {
		...mapGetters({
			ProjectsStatus : 'getProjectsStatus'
		}),

		ImageLink() {
			return this.screenShotUrl == undefined || this.screenShotUrl == null ? defaultImageLinkForCards :
				this.screenShotUrl;
		},

		FormattedProjectName() {
			let projectName = this.projectName.trim();
			if((projectName.indexOf('http://') >= 0) || (projectName.indexOf('https://')>=0)){
				projectName = projectName.replace('http://','').replace('https://','').replace('/','')
					.replace('www.','').split('.')[0];
			}
			return projectName;
		},

		isProjectCrawled(){
			return (this.projectStatus == this.ProjectsStatus.CRAWLING.key ||
			this.projectStatus == this.ProjectsStatus.CRAWLING.value);
		},

		isProjectBeingPublished(){
			return (this.projectStatus == this.ProjectsStatus.PUBLISHING.key ||
			this.projectStatus == this.ProjectsStatus.PUBLISHING.value);
		},

		isProjectBeingOptimized(){
			return (this.projectStatus == this.ProjectsStatus.BUILDING.key ||
			this.projectStatus == this.ProjectsStatus.BUILDING.value);
		},

		isProjectBeingDownloaded(){
			return (this.projectStatus == this.ProjectsStatus.PREPARINGDOWNLOAD.key ||
			this.projectStatus == this.ProjectsStatus.PREPARINGDOWNLOAD.value)
		},

		isProjectIdle() {
			return (!this.isProjectCrawled && !this.isProjectBeingPublished
				&& !this.isProjectBeingDownloaded);
		}

	},

	methods : {

		optimizeProject(projectId) {
			// TODO take a action to make api call
		},

		publishProject(projectId) {
			// TODO take a action to make api call
		},

		openProjectInIDE() {
			if(this.isProjectsMenu){
				window.open(IDELink.replace('{projectid}',this.projectId));
			}
			if(!this.isProjectsMenu && !this.isChatBot) {
				window.open(`//${this.kitsuneUrl}`, this.projectId);
			}
			if(!this.isProjectsMenu && this.isChatBot) {
				//TODO ana card click handler
			}
		}

	}
}
