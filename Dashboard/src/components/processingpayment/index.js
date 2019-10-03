import { mapGetters, mapActions } from 'vuex';
import helpers from '../mixin';

export default {
	name : 'processingpayment',

	mixins: [helpers],

	computed: {
		...mapGetters([
			'paymentDetails',
			'requestedForAna',
			'anaRegistrationDetails'
		]),

		getPaymentDetails(){
			return this.paymentDetails;
		}

	},

	methods: {
		...mapActions([
			'registerAnaAccount',
			'toggleStatus'
		])
	}
}
