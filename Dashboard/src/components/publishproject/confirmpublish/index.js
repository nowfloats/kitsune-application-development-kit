import publishHeader from '../publishheader/stage0/stage0.vue';
import publishBody from '../publishbody/stage2/stage2.vue';
import { mapGetters } from 'vuex';

export default {
	name: 'ConfirmPublish',

	props: ['projectDetails'],

	components: {
		publishHeader,
		publishBody
	},

	computed: {
		...mapGetters([
			'domainForCloud'
		]),

		getDomainName(){
			return this.projectDetails.isNewCustomer ?
				`${this.projectDetails.newCustomerDetails.WebsiteTag}.${this.domainForCloud}` :
				this.projectDetails.customerForPublishing.WebsiteDomain;
		}

	}

}
