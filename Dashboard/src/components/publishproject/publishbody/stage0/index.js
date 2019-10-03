import CustomDropdown from '../../../customdropdown/CustomDropdown.vue';
import { siteNameRegex } from '../../../../../config/config';
import publishFooter from '../../footer/pfooter.vue';
import threeDots from '../../../loaders/threeDots/ThreeDots.vue';
// import { domainNameRegex } from "../../../../../config/config";

import { mapGetters, mapActions } from 'vuex'

export default {
	name: 'PublishingStage0Body',

	props:[
		'projectDetails'
	],

	data() {
		return {
			selectedExisting: false,
			selectedCreateNew: false,
			selectedUseSubDomain: false,
			hoveredOnSelectedExisting: false,
			hoveredOnSelectedCreateNew: false,
			hoveredOnUseSubDomain: false,
			domainEntered: false,
			domainName: '',
			subDomain: '',
			valid : true,
			showSiteError: false,
			showDomainError: false,
			showLoader: false
		}
	},

	components: {
		CustomDropdown,
		publishFooter,
		threeDots
	},

	methods: {
		...mapActions([
			'setStageForPublishing',
			'setDomainNameForNewCustomerInPublishProject',
			'checkIfWebsiteTagExists'
		]),

		extractSubDomain() {
			this.subDomain = this.domainName.replace('https://','').replace('http://','').
			replace('www.','').split('.').slice(0,-1).join('');
		},

		selectExisting() {
			this.selectedExisting = true;
			this.selectedCreateNew = false;
			this.selectedUseSubDomain = false;
		},

		selectCreateNew() {
			this.selectedCreateNew = true;
			this.selectedExisting = false;
			this.selectedUseSubDomain = false;
			this.subDomain = '';
		},

		selectUseSubDomain() {
			this.selectedUseSubDomain = true;
			this.selectedExisting = false;
			this.selectedCreateNew = false;
			this.domainName = '';
		},

		hoverOnSelectedExisting() {
			this.hoveredOnSelectedExisting = true;
			this.hoveredOnSelectedCreateNew = false;
			this.hoveredOnUseSubDomain = false;
		},

		hoverOnSelectedCreateNew() {
			this.hoveredOnSelectedCreateNew = true;
			this.hoveredOnSelectedExisting = false;
			this.hoveredOnUseSubDomain = false;
		},

		hoverOnUseSubDomain() {
			this.hoveredOnUseSubDomain = true;
			this.hoveredOnSelectedCreateNew = false;
			this.hoveredOnSelectedExisting = false;
		},

		reset() {
			this.hoveredOnSelectedCreateNew = false;
			this.hoveredOnSelectedExisting = false;
			this.hoveredOnUseSubDomain = false;
		},

		submitData(){
			if(this.selectedUseSubDomain && this.subDomain) {
				this.showLoader = true;
				this.checkIfWebsiteTagExists(this.subDomain)
					.then( response => {
						this.showLoader = false;
						if(response.status === 204) {
							this.showSiteError = true;
						}
					})
					.catch( response => {
						this.showLoader = false;
						const { data } = response.response
						if(data !== "") {
							this.showSiteError = false;
							this.submitDomain();
						}else {
							//TODO error handler
						}
					})
			}else if(this.selectedCreateNew) {
				// if(domainNameRegex.test(this.domainName)){
				// 	this.showDomainError = false;
				// 	this.extractSubDomain();
				// 	this.domainEntered = true;
				// 	this.selectedUseSubDomain = true;
				// }else {
				// 	this.showDomainError = true;
				// }
				this.areCloudDetailsPresent ? this.setStageForPublishing(5) : this.setStageForPublishing(3);
			}
			else {
				const { WebsiteId } = this.projectDetails.customerForPublishing;
				if ( WebsiteId ) {
					this.submitExistingCustomer();
				}
			}
		},

		submitExistingCustomer() {
			this.setStageForPublishing(2);
		},

		submitDomain() {
			if(siteNameRegex.test(this.subDomain)){
				this.setDomainNameForNewCustomerInPublishProject({ subDomain: this.subDomain, domain: this.domainName });
				this.setStageForPublishing(1);
			}
			else {
				this.checkDomain();
			}
		},

		backBtnHandler() {
			this.domainName = '';
			this.subDomain = '';
			this.domainEntered = false;
			this.selectedUseSubDomain = false;
			this.selectedCreateNew = false;
		},

		checkDomain() {
			this.valid = siteNameRegex.test(this.subDomain);
			return this.valid;
		},

		checkExistingcustomer() {
			const { WebsiteId } = this.projectDetails.customerForPublishing;
			return !!WebsiteId;
		}

	},

	computed: {
		...mapGetters({
			publishDetails: 'getPublishDetails',
			areCloudDetailsPresent: 'areCloudDetailsPresent'
		}),

		hasCustomers() {
			return this.projectDetails.customerList.length > 0;
		},

		isCustomerIdValid() {
			const { WebsiteId } = this.projectDetails.customerForPublishing;
			return !!WebsiteId;
		},

		isDomainValid() {
			return siteNameRegex.test(this.subDomain);
		},

		isValidAll() {
			return (this.isDomainValid || this.isCustomerIdValid);
		}

	}

}
