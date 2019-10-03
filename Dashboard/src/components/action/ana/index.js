import { mapGetters, mapActions } from "vuex";
import { regex, anaMinBalance } from '../../../../config/config';
import helpers from '../../mixin';
import store from "../../../store";
import axios from "axios/index";

export default {
	name: 'ana',

	mixins: [helpers],

	//TODO add validation for project creation
	data: () => ({
		account: {
			Name: '',
			PhoneNumber: ''
		},
		phoneNumberRegex: regex.phoneNumber,
		accountError: {
			regex: false,
			empty: false,
			length: false
		}
	}),

	computed: {
		...mapGetters([
			'isActionInputInFocus',
			'user',
			'isAnaAccountPresent'
		])
	},

	methods: {
		...mapActions([
			'toggleStatus',
			'toggleRequestedForAna',
			'setAnaRegistrationDetails',
			'registerAnaAccount',
			'triggerEmail',
			'callToggleContainerActiveClass'
		]),
		validateAnaAccount() {
			this.accountError.regex = false;
			this.accountError.empty = false;
			this.accountError.length = false;

			const { PhoneNumber } = this.account;
			if(this.phoneNumberRegex.test(PhoneNumber)) {
				this.accountError.regex = true;
				this.accountError.length = false;
			} else if (this.account.PhoneNumber.length !== 10) {
				this.accountError.regex = false;
				this.accountError.length = true;
			}
		},
		getDetails() {
			const { DisplayName, PhoneNumber } = this.user.user;
			return {
				Name: DisplayName,
				PhoneNumber: PhoneNumber
			}
		},
		registerAccount(e) {
			if(this.isAnaAccountPresent) {
				const { ERROR } = store.state.toastr.toasterTypes;
				store.dispatch('addToaster', {
					type : ERROR,
					title : 'already subscribed',
					message: 'you have already subscribed for ANA-cloud'
				});
			} else {
				const { PhoneNumber } = this.account;
				if(PhoneNumber) {
					const { regex, length, empty } = this.accountError;
					const { user } = this.user;
					const { Wallet } = user;
					if(!regex && !length && !empty) {
						if(Wallet === null || Wallet === undefined) {
							this.toggleRequestedForAna(true);
							this.setAnaRegistrationDetails(this.createPayloadForRegistration(user, this.account));
							this.toggleStatusHandler(e, ['overlay','billingform']);
						} else if(Wallet.Balance <= anaMinBalance) {
							this.toggleRequestedForAna(true);
							this.setAnaRegistrationDetails(this.createPayloadForRegistration(user, this.account));
							this.toggleStatusHandler(e, ['overlay', 'addmoney']);
						} else {
							this.registerAnaAccount(this.createPayloadForRegistration(user, this.account));
							this.$router.replace({ path: '/chatbots' });
						}
					}
					this.toggleStatusHandler(e, ['action']);
				} else {
					this.accountError = {
						empty: true,
						regex: false,
						length: false
					};
				}
			}
		},

		passwordGenerator: () => Math.random().toString(36).slice(2),

		createPayloadForRegistration ({ DisplayName, Email }, { PhoneNumber }) {
			return {
				business: {
					id: axios.defaults.headers.common['Authorization'],
					name: DisplayName,
					email: Email,
					phone: PhoneNumber
				},
				user: {
					name: DisplayName,
					email: Email,
					password: this.passwordGenerator()
				}
			}
		}
	},

	mounted() {
		this.account = { ...this.getDetails() };
	}
}
