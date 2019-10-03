import { mapGetters, mapActions } from 'vuex';
import helpers from '../mixin';
import vueGoogleAutocomplete from 'vue-google-autocomplete';
import threeDots from '../loaders/threeDots/ThreeDots.vue';

export default {
	name : 'billingform',

	mixins: [helpers],

	data() {
		return {
			userDetails : {
				PhoneNumber : '',
				Address : {
					AddressDetail: '',
					City: '',
					State: '',
					Country: '',
					Pin: ''
				},
				Name: '',
				UserEmail: '',
				GSTIN: ''
			},
			autocompleteParts: {
				City: '',
				State: '',
				Country: '',
				Pin: ''
			},
			buttonText : 'proceed',
			addressDetail: '',
			showSaveButton: false,
		};
	},

	components: {
		vueGoogleAutocomplete,
		threeDots
	},

	computed: {
		...mapGetters([
			'user',
			'getUserProfileUpdate'
		]),

		getUserProfileUpdateStatus() {
			return this.getUserProfileUpdate;
		},

		disableSaveButton() {
			return !this.showSaveButton || this.getUserProfileUpdateStatus.isUserDetailsUpdateRequested;
		},

		getAddressStatus() {
			const { Address } = this.user.user;
			return Address === null || Address === undefined;
		}
	},

	methods: {
		...mapActions([
			'toggleStatus',
			'updateUserDetails',
			'toggleRequestedForAna',
			'editUserDetails'
		]),

		submitForm() {
			this.doesFormHasErrors()
				.then(() => this.updateUserDetails(this.userDetails))
				.catch(() => this.showSaveButton = false);
		},

		updateAutocompleteAddress() {
			this.userDetails.Address = {
				...this.autocompleteParts,
				AddressDetail: this.addressDetail
			};
		},

		getAddress: function (addressData, placeResultData, id) {
			this.autocompleteParts = { ...this.getAddressDetails('address', 'address', addressData, placeResultData) };
			this.updateAutocompleteAddress();
		},

		closeBillingForm(event) {
			this.toggleStatusHandler(event, ['overlay','billingform']);
			this.toggleRequestedForAna(false);
		},

		keyUpHandler(event) {
			if(event.keyCode === 27 ) {
				this.closeBillingForm(event);
			}
		},

		addEventListenerToBodyForEscKey() {
			let body = document.getElementsByTagName('body')[0];
			body.addEventListener('keyup', this.keyUpHandler);

		},

		removeEventListenerToBodyForEscKey() {
			let body = document.getElementsByTagName('body')[0];
			body.removeEventListener('keyup', this.keyUpHandler);
		},

		updateAddressDetails() {
			this.updateAddressDetail('address', 'address');
		}

	},

	created() {
		const { DisplayName, PhoneNumber, Email,  GSTIN, Address } = this.user.user;
		this.userDetails = {
			Name: DisplayName,
			PhoneNumber,
			UserEmail: Email,
			GSTIN,
			Address: Address ? Address : {
				AddressDetail: '',
				City: '',
				State: '',
				Country: '',
				Pin: ''
			}
		};
		this.addressDetail = this.getAddressStatus ? '' : this.user.user.Address.AddressDetail;
		this.addEventListenerToBodyForEscKey();
	},

	mounted() {
		document.querySelector('#address').value = this.addressDetail;
	},

	destroyed() {
		this.removeEventListenerToBodyForEscKey();
	}
}
