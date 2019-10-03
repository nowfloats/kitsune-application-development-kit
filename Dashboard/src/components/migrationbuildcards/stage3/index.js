import MigrationHeader from '../header/stage1header/stage1header.vue';
import Stage3Body from '../body/stage3body/stage3body.vue';
import Stage3Footer from '../footer/stage3footer/stage3footer.vue';

export default {
	name: 'stage3',

	props: ['data'],

	components: {
		MigrationHeader,
		Stage3Body,
		Stage3Footer
	},

	computed:{

		getData() {
			return (this.data == null || this.data == undefined) ? false : true;
		},

		getStylesFound() {
			if(this.getData){
				return this.isEmptyOrNull(this.data.StylesFound) ? 0 : this.data.StylesFound;
			}
			return 0;
		},

		getStylesDownloaded() {
			if(this.getData){
				return this.isEmptyOrNull(this.data.StylesDownloaded) ? 0 : this.data.StylesDownloaded;
			}
			return 0;
		},

		getScriptsFound() {
			if(this.getData){
				return this.isEmptyOrNull(this.data.ScriptsFound) ? 0 : this.data.ScriptsFound;
			}
			return 0;
		},

		getScriptsDownloaded() {
			if(this.getData){
				return this.isEmptyOrNull(this.data.ScriptsDownloaded) ? 0 : this.data.ScriptsDownloaded;
			}
			return 0;
		},

		getAssetsFound() {
			if(this.getData){
				return this.isEmptyOrNull(this.data.AssetsFound) ? 0 : this.data.AssetsFound;
			}
			return 0;
		},

		getAssetsDownloaded() {
			if(this.getData){
				return this.isEmptyOrNull(this.data.AssetsDownloaded) ? 0 : this.data.AssetsDownloaded;
			}
			return 0;
		},

		getStylesPercentage() {
			if(this.getData){
				return (this.data.StylesDownloaded / this.data.StylesFound) * 100;
			}
			return 0;
		},

		getScriptsPercentage() {
			if(this.getData){
				return (this.data.ScriptsDownloaded / this.data.ScriptsFound) * 100;
			}
			return 0;
		},

		getAssetsPercentage() {
			if(this.getData){
				return (this.data.AssetsDownloaded / this.data.AssetsFound) * 100;
			}
			return 0;
		},

		getDownloadDetails() {
			return {
				stylesFound : this.getStylesFound,
				stylesDownloaded : this.getStylesDownloaded,
				scriptsFound : this.getScriptsFound,
				scriptsDownloaded : this.getScriptsDownloaded,
				assetsFound : this.getAssetsFound,
				assetsDownloaded : this.getAssetsDownloaded,
				stylesPercentage : this.getStylesPercentage,
				scriptsPercentage: this.getScriptsPercentage,
				assetsPercentage: this.getAssetsPercentage
			};
		},

		getTotalPercentage() {
			return ((this.getStylesDownloaded+this.getScriptsDownloaded+this.getAssetsDownloaded) /
				(this.getStylesFound+this.getScriptsFound+this.getAssetsFound)) * 100;
		}

	},

	methods: {
		isEmptyOrNull(obj) {
			return obj == null || obj == undefined;
		}
	}
}
