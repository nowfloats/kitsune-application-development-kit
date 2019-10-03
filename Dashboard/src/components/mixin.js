import anaEmailBody from './emailBody.html';

var Helpers = {
	methods: {
		getSvg(name) {
			return require(`../assets/icons/${name}.svg`)
		},

		toggleStatusHandler(e, components) {
			this.toggleStatus({
				event: e,
				components: components
			})
		},
		validateField(fieldName, modelName) {
			this.$validator.validate(fieldName, modelName)
				.then(result => {
					if (result) {
						this.toggleSaveButton(true);
						return;
					} else {
						this.toggleSaveButton(false)
					}
				});
		},

		doesFormHasErrors() {
			return new Promise((resolve, reject) => {
				this.$validator.validateAll().then(result => {
					result ? resolve() : reject();
				})
			})
		},

		toggleSaveButton(payload) {
			this.showSaveButton = payload;
		},

		readCookie(name) {
			let nameEQ = name + "=";
			let ca = document.cookie.split(';');
			for (let i = 0; i < ca.length; i++) {
				let c = ca[i];
				while (c.charAt(0) == ' ') c = c.substring(1, c.length);
				if (c.indexOf(nameEQ) == 0) return c.substring(nameEQ.length, c.length);
			}
			return null;
		},

		generateAnaEmailBody: ({ Name, Password, UserName }) => anaEmailBody
			.replace('[[name]]', Name)
			.replace('[[username]]', UserName)
			.replace('[[password]]', Password),

		getAddressDetails(elementId, refName, addressData, placeResultData) {
			let address = document.getElementById(elementId).value;
			if(address.lastIndexOf(address.locality) < 0) {
				let parts = address.split(',');
				this.addressDetail = parts.slice(0, parts.length - 2).join(',');
			}
			else {
				this.addressDetail = address.substr(0, address.lastIndexOf(addressData.locality) - 2);
			}
			this.validateField(refName, address);
			const index = placeResultData.address_components.length - 3;
			return {
				City: addressData.locality,
				State: placeResultData.address_components[index].long_name,
				Country: addressData.country,
				Pin: addressData.postal_code
			};
		},

		updateAddressDetail(elementId, refName) {
			const address = document.getElementById(elementId).value;
			this.addressDetail = address;
			this.validateField(refName, address);
		}
	}
};
export default Helpers
