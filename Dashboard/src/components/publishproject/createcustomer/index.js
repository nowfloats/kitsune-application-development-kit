import PublishHeader from '../publishheader/stage0/stage0.vue';
import createCustomerBody from '../publishbody/stage1/stage1.vue';

export default {
	name: 'CreateCustomer',

	props : [
		'projectDetails'
	],

	components: {
		PublishHeader,
		createCustomerBody
	}
}
