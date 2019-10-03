import PublishingStage0Header from '../publishheader/stage0/stage0.vue';
import PublishingStage0Body from '../publishbody/stage0/stage0.vue';

export default {
	name: 'PublishStage0',

	props:[
		'projectDetails'
	],

	components:{
		PublishingStage0Header,
		PublishingStage0Body
	}
}
