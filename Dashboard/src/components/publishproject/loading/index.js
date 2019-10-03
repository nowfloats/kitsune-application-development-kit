import PublishHeader from '../publishheader/stage0/stage0.vue'
import PublishLoaderBody from '../publishbody/publishloader/PublishLoader.vue';

export default {
	name: 'LoadingPublish',

	props: [
		'projectDetails'
	],

	components:{
		PublishHeader,
		PublishLoaderBody
	}

}
