import infiniteLoader from '../../../../../infinite-loader-bar/infiniteloadingbar.vue';
import { mapGetters, mapActions } from 'vuex'

export default {
	name: 'projectFooterStatus',

	props: ['projectStatus',
		'projectType',
		'projectProcessStage',
		'projectStage',
		'projectId',
		'projectName'
	],

	computed : {
		...mapGetters({
			ProjectsStatus: 'getProjectsStatus',
			getStageName : 'getStageName',
			crawlStatus : 'getCrawlStatus',
			buildStatus : 'getBuildStatus'
		}),

		getProjectStatus(){
			return this.ProjectsStatus.getKey(this.projectStatus);
		},

		getProcessStage(){
			if(this.getProjectStatus === this.ProjectsStatus.BUILDING.key)
				return this.getStageName(this.projectStage,true);
			else if (this.getProjectStatus === this.ProjectsStatus.CRAWLING.key){
				return this.getStageName(this.projectStage,false);
			}
		},

		isIdentifyingDomainName(){
			if (this.getProjectStatus === this.ProjectsStatus.CRAWLING.key &&
				this.projectStage == this.crawlStatus.IdentifyingExternalDomains.key){
				return true;
			}
			else{
				return false;
			}
		},

		isRequired(){
			return (this.projectStatus === this.ProjectsStatus.BUILDING.key ||
				this.projectStatus === this.ProjectsStatus.CRAWLING.key);
		},

		isQueued() {
			return (this.projectStatus === this.ProjectsStatus.BUILDING.key &&
			this.projectStage == this.buildStatus.Queued.key);
		}

	},

	methods:{

		...mapActions({
			getStageForMigration : 'getStageForMigration',
			resetMigrationStatus: 'resetMigrationStatus'
		}),

		maximizeMigration(){
			this.resetMigrationStatus();
			this.getStageForMigration(
				{ projectId : this.projectId,
					projectName : this.projectName,
					projectType : this.projectType });
		}

	},

	components: {
		infiniteLoader
	}

}
