import { mapGetters } from 'vuex'


export default {
	name : 'stage1header',

	data() {
		return {
			stageNames : [

				{
					completed : 'analyzed websites',
					pending : 'analyzing websites'
				},

				{
					completed : 'recognized links',
					pending : 'recognizing links'
				},

				{
					completed : 'imported files',
					pending : 'importing files'
				},

				{
					completed : 'inserted placeholders',
					pending : 'inserting placeholders'
				},

				{
					completed : 'optimized files',
					pending : 'optimizing files'
				},

				{
					completed : 'generated html',
					pending : 'generating html'
				},

			]
		}
	},

	computed: {
		...mapGetters({
			stage : 'getMigrationStage',
			projectName : 'getCrawledProjectName',
			project : 'getProjectStatusAndBuildVersion',
			projectTypes: 'getProjectTypes'
		}),

		isSixStagesRequired(){
			return (this.project.projectType == this.projectTypes.CRAWL && this.project.isFirstBuild);
		},



	},

	methods: {

		getStatus(index){
			return index < this.stage;
		},

		getStageName(index){
			return this.stageNames[index-1];
		},

		isCurrentStage(index){
			return index === this.stage;
		},

		getStageNumber(index){

			if(!this.isSixStagesRequired && index >= 4){
				return index-3;
			}
			else{
				return index;
			}


		}

	}

}
