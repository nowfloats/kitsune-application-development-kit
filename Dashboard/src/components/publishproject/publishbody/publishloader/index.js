import { mapActions } from 'vuex'
import helpers from '../../../mixin'
import publishFooter from '../../footer/pfooter.vue';

export default {
	name: 'PublishLoader',

	props: ['projectDetails'],

	components: {
		publishFooter
	},

	computed:{

		isApiError(){
			return this.projectDetails.isAPIError || this.projectDetails.isCustomerCreationError;
		},

		customerCreationError() {
			return this.projectDetails.isCustomerCreationError;
		}

	},

	mixins: [helpers],

	methods: {
		...mapActions([
			'toggleStatus',
			'resetStoreForPublishingProject'
		]),

		minimizePublishModal(e) {
			this.toggleStatusHandler(e, ['overlay','publishproject']);
			this.resetStoreForPublishingProject();
		},

		addMoney(event){
			this.toggleStatusHandler(event, ['overlay','publishproject']);
			this.toggleStatusHandler(event, ['overlay','addmoney']);
			this.resetStoreForPublishingProject();
		}

	}

}
