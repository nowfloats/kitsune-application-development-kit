import { mapActions } from 'vuex';
import publishFooter from '../../footer/pfooter.vue';
import { nameRegex, phoneNumberRegex, emailRegex } from '../../../../../config/config';

export default {
	name: 'PublishCreateCustomer',

	components: {
		publishFooter
	},

	data() {
		return {
			customer: {
				customerName : '',
				customerEmail : '',
				phoneNumber: ''
			},
			isNameValid : true,
			isPhoneValid: true,
			isEmailValid: true
		}
	},

	methods: {

		...mapActions([
			'setIsNewCustomerInPublishProject',
			'setCustomerDetailsForNewCustomerInPublishProject',
			'setStageForPublishing'
		]),

		validateCustomerName() {
			this.isNameValid = nameRegex.test(this.customer.customerName.trim());
		},

		validateCustomerEmail() {
			this.isEmailValid = emailRegex.test(this.customer.customerEmail.trim());
		},

		validateCustomerPhone() {
			this.isPhoneValid = phoneNumberRegex.test(this.customer.phoneNumber.trim());
		},

		backBtnHandler() {
			this.setStageForPublishing(5);
		},

		submitForm() {
			this.validateCustomerEmail();
			this.validateCustomerName();
			this.validateCustomerPhone();

			if(this.isNameValid && this.isPhoneValid && this.isEmailValid){
				this.setIsNewCustomerInPublishProject(true);
				this.setCustomerDetailsForNewCustomerInPublishProject(this.customer);
				this.setStageForPublishing(2);
			}
		}
	}
}
