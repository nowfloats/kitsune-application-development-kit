import uploadAlert from '../../../../uploadAlert/UploadAlert.vue'
import projectFooterAction from './projectFooterAction/projectFooterAction.vue'
import projectFooterUpload from './projectFooterUpload/projectFooterUpload.vue'
import projectFooterStatus from './projectFooterStatus/projectFooterStatus.vue'
import { mapGetters } from 'vuex'

export default {
	name: 'projectsFooter',

	components: {
		uploadAlert,
		projectFooterAction,
		projectFooterUpload,
		projectFooterStatus
	},

	props: ['projectId',
		'createdOn',
		'projectName',
		'projectType',
		'isProjectsMenu',
		'isContextMenuRequired',
		'kitsuneUrl',
		'version',
		'projectStatus',
		'projectStage',
		'customerId',
		'domainVerified'
	],

	data() {
		return {
			showAlert: false
		}
	},

	computed :{
		...mapGetters({
			ProjectsStatus : 'getProjectsStatus',
			uploadDetails: 'uploadDetails'
		}),


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
			if(this.isProjectsMenu)
				return (!this.isProjectCrawled && !this.isProjectBeingOptimized && !this.isProjectBeingPublished
					&& !this.isProjectBeingDownloaded);
			else
				return true;
		},

		details() {
			return this.uploadDetails.projectId === this.projectId ? this.uploadDetails : {};
		}
	},

	methods: {
		alertActive: function() {
			this.showAlert = false
		},
		triggerAlert(payload) {
			if(payload === this.projectId) {
				this.showAlert = true;
				let self  = this
				setTimeout(this.alertActive.bind(self), 2000)
			}
		},

		openIDEInParent(){
			this.$emit('OpenIDEEvent');
		}

	},

	mounted() {
		Event.listen('triggerUploadAlert', this.triggerAlert)
	}
}
