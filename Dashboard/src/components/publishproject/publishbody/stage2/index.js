import { mapActions, mapGetters } from 'vuex'
import helpers from '../../../mixin'
import publishFooter from '../../footer/pfooter.vue';
import { cloudOptionsMap } from '../../../../../config/config';

export default {
	name: 'PublishStage2Body',

	props: [
		'projectName',
		'customerDomain',
		'projectId'
	],

	mixins: [helpers],

	computed: {
		...mapGetters({
			selectedCloud: 'selectedCloud'
		}),
	},

	components: {
		publishFooter
	},

	methods:{
		...mapActions([
			'publish',
			'toggleStatus',
			'resetStoreForPublishingProject',
			'publishToCloud'
		]),


		publishProject() {
			switch(this.selectedCloud) {
			case cloudOptionsMap.get(0): this.publish();
				break;
			case cloudOptionsMap.get(1):
			case cloudOptionsMap.get(2): this.publishToCloud(this.projectId);
				break;
			default: this.publish();
			}
		},

		closePublish(e){
			this.toggleStatusHandler(e, ['overlay','publishproject']);
			this.resetStoreForPublishingProject();
		}

	}


}
