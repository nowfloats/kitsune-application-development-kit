import MigrationHeader from '../header/stage1header/stage1header.vue';
import MigrationFooter from '../footer/stage3footer/stage3footer.vue';
import Stage5Body from '../body/stage5body/stage5body.vue';

export default {
	name : 'stage5',

	props: ['data'],

	components: {
		MigrationHeader,
		MigrationFooter,
		Stage5Body
	},

	computed: {

		getOptimize() {
			return (this.data == null || this.data == undefined) ? false : true;
		},

		getTotalAssets() {
			if(this.getOptimize){
				return this.data.TOTAL_FILE ? this.data.TOTAL_FILE : 0;
			}
			return 0;
		},

		getTotalScripts() {
			if(this.getOptimize){
				return this.data.TOTAL_SCRIPT ? this.data.TOTAL_SCRIPT : 0;
			}
			return 0;
		},

		getTotalStyles() {
			if(this.getOptimize){
				return this.data.TOTAL_STYLE ? this.data.TOTAL_STYLE : 0;
			}
			return 0;
		},

		getTotalLinks() {
			if(this.getOptimize){
				return this.data.TOTAL_LINK ? this.data.TOTAL_LINK : 0;
			}
			return 0;
		},

		getTotalFilesToOptimize(){
			if(this.getOptimize){
				return this.data.TOTAL ? this.data.TOTAL : 0;
			}
			return 0;
		},

		getAssetsOptimized() {
			if(this.getOptimize){
				return this.data.FILE ? this.data.FILE : 0;
			}
			return 0
		},

		getStylesOptimized() {
			if(this.getOptimize){
				return this.data.STYLE ? this.data.STYLE : 0;
			}
			return 0;
		},

		getScriptsOptimized() {
			if(this.getOptimize){
				return this.data.SCRIPT ? this.data.SCRIPT : 0;
			}
			return 0;
		},

		getLinksOptimized() {
			if(this.getOptimize){
				return this.data.LINK ? this.data.LINK : 0;
			}
			return 0;
		},

		getLinksProcessedPercentage() {
			return (this.getLinksOptimized / this.getTotalLinks) * 100;
		},

		getStylesProcessedPercentage() {
			return (this.getStylesOptimized / this.getTotalStyles) * 100;
		},

		getScriptsProcessedPercentage() {
			return (this.getScriptsOptimized / this.getTotalScripts) * 100;
		},

		getAssetsProcessedPercentage() {
			return (this.getAssetsOptimized / this.getTotalAssets) * 100;
		},

		getOptimizeDetails() {
			return {
				totalFilesToOptimize : this.getTotalFilesToOptimize,
				assetsOptimized : this.getAssetsOptimized,
				stylesOptimized: this.getStylesOptimized,
				scriptsOptimized: this.getScriptsOptimized,
				linksOptimized: this.getLinksOptimized,
				linksProcessedPercentage: this.getLinksProcessedPercentage,
				stylesProcessedPercentage: this.getStylesProcessedPercentage,
				scriptsProcessedPercentage: this.getScriptsProcessedPercentage,
				assetsProcessedPercentage: this.getAssetsProcessedPercentage,
				totalAssets : this.getTotalAssets,
				totalLinks : this.getTotalLinks,
				totalStyles : this.getTotalStyles,
				totalScripts : this.getTotalScripts
			}
		},

		getTotalPercentage() {
			return ( (this.getAssetsOptimized+this.getStylesOptimized+this.getScriptsOptimized+this.getLinksOptimized)
				/ (this.getTotalFilesToOptimize)) * 100;
		}

	}
}
