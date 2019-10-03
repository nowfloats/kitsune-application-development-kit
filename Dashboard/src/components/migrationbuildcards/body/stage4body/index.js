import ProgressBar from '../../../progressbar/progressbar.vue'
import { mapGetters } from 'vuex'

export default {
	name: 'stage4body',

	props: ['data','analyzer'],

	components:{
		ProgressBar
	},

	computed:{

		...mapGetters({
			project : 'getProjectStatusAndBuildVersion',
			projectTypes: 'getProjectTypes'
		}),

		isReplacerRequired(){
			return (this.project.projectType == this.projectTypes.CRAWL && this.project.isFirstBuild);
		},

		isDataAvailable(){
			return !(this.data == null || this.data == undefined);
		},

		isDataFromAnalyzerAvailable(){
			return !(this.analyzer == null || this.analyzer == undefined);
		},

		getTotalLinks(){
			if(this.isDataAvailable) {
				return this.data.LinksFound ? this.data.LinksFound : 0;
			}
			return 0;
		},

		getLinksReplaced(){
			if(this.isDataAvailable) {
				return this.data.LinksReplaced ? this.data.LinksReplaced : 0;
			}
			return 0;
		},

		getLinksReplacedPercentage() {
			if(this.isDataAvailable) {
				return (this.getLinksReplaced / this.getTotalLinks) * 100;
			}
			return 0;
		},

		getTotalFilesToAnalyze(){
			if(this.isDataFromAnalyzerAvailable) {
				return this.analyzer.TOTAL ? this.analyzer.TOTAL : 0;
			}
			return 0;
		},

		getFilesAnalyzed() {
			if(this.isDataFromAnalyzerAvailable) {
				return (this.analyzer.STYLE ? this.analyzer.STYLE : 0) + (this.analyzer.LINK ? this.analyzer.LINK : 0);
			}
			return 0;
		},

		getFilesAnalyzedPercentage() {
			if(this.isDataFromAnalyzerAvailable) {
				return (this.getFilesAnalyzed / this.getTotalFilesToAnalyze) * 100;
			}
			return 0;
		},

	}
}
