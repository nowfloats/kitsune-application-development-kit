import { mapGetters, mapActions } from "vuex";
import threeDots from '../../loaders/threeDots/ThreeDots.vue';
import vueGoogleAutocomplete from 'vue-google-autocomplete';
import helpers from '../../mixin';

export default {
	name: 'editAccount',

	components: {
		threeDots,
		vueGoogleAutocomplete
	},

	data() {
		return {
			newDetails: {
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
			showSaveButton: false,
			addressDetail:'',
			autoCompleteParts: {
				City: '',
				State: '',
				Country: '',
				Pin: ''
			}
		}
	},

	mixins: [helpers],

	methods: {
		...mapActions([
			'editUserDetails'
		]),
		submitForm(){
			this.doesFormHasErrors()
				.then(() => this.editUserDetails(this.newDetails))
				.catch(() => this.showSaveButton = false)
		},

		updateAddressDetails() {
			this.updateAddressDetail('addressDetail', 'address');
		},

		getAddressData: function (addressData, placeResultData, id) {
			this.autoCompleteParts = {
				...this.getAddressDetails('addressDetail', 'address', addressData, placeResultData)
			};
			this.updateAddress();
		},

		updateAddress() {
			this.newDetails.Address = {
				...this.autoCompleteParts,
				AddressDetail: this.addressDetail
			};
		}
	},

	computed: {
		...mapGetters({
			user: 'getUserDetails',
			updateStatus: 'getUserInfoUpdated',
			componentStatus: 'componentStatus'
		}),

		getAddress() {
			return this.user.Address === null || this.user.Address === undefined;
		}
	},

	created() {
		const { DisplayName, Email, PhoneNumber, GSTIN, Address } = this.user;
		this.newDetails = {
			Name: DisplayName,
			UserEmail: Email,
			PhoneNumber,
			GSTIN,
			Address: Address ? Address : {
				AddressDetail: '',
				City: '',
				State: '',
				Country: '',
				Pin: ''
			}
		};
		this.addressDetail = this.getAddress ? '' : this.user.Address.AddressDetail;
	},

	mounted() {
		document.querySelector('#addressDetail').value = this.addressDetail;
	}
}
