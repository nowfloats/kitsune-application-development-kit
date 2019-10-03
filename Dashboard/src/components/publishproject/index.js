import Publishstage0 from './publishstage0/PublishStage0.vue';
import CreateCustomer from './createcustomer/CreateCustomer.vue';
import ConfirmPublish from './confirmpublish/ConfirmPublish.vue';
import PublishLoader from './loading/loading.vue';
import chooseCloud from './chooseCloud/chooseCloud';
import cloudParam from './cloudParam/cloudParam';
import domainDetails from './domainDetails/domainDetails';
import gcpUpload from './gcpUpload/gcpUpload';
import { mapGetters,mapActions } from 'vuex'
import helpers from '../mixin';

export default {
	name : 'PublishProject',

	components: {
		Publishstage0,
		CreateCustomer,
		ConfirmPublish,
		PublishLoader,
		chooseCloud,
		cloudParam,
		domainDetails,
		gcpUpload
	},

	mixins: [helpers],

	computed:{
		...mapGetters([
			'getPublishDetails'
		]),

		projectDetails(){
			return this.getPublishDetails;
		},

		showLoader(){
			return this.projectDetails.isGettingCustomerList
				|| this.projectDetails.isPublishing
				|| this.projectDetails.isCreatingCustomer
				|| !this.projectDetails.isEligibleForPublishing;
		}
	},

	methods: {

		...mapActions([
			'toggleStatus',
			'resetCloudDetails'
		]),

		getStage(stage){
			return this.getPublishDetails.stage == stage &&
				!(this.showLoader);
		},

		closeAll(e){
			this.toggleStatusHandler(e, ['overlay','publishproject']);
		},
	},

	destroyed() {
		this.resetCloudDetails();
	}

}
