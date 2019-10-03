import infiniteLoaderBar from '../../../infinite-loader-bar/infiniteloadingbar.vue';

export default {
	name : 'stage1footer',

	props: ['data'],

	computed: {

		isDataAvailable(){
			return !(this.data == null || this.data == undefined || Object.keys(this.data).length === 0);
		},

		linksFound(){
			if(this.isDataAvailable){
				return this.data.LinksFound;
			}
			return 0;
		},

		stylesFound(){
			if(this.isDataAvailable){
				return this.data.StylesFound;
			}
			return 0;
		},

		scriptsFound(){
			if(this.isDataAvailable){
				return this.data.ScriptsFound;
			}
			return 0;
		},

		assetsFound(){
			if(this.isDataAvailable){
				return this.data.AssetsFound;
			}
			return 0;
		}

	},

	components:{
		infiniteLoaderBar
	}

}
