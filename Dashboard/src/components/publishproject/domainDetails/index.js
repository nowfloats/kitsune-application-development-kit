import { siteNameRegex, cloudOptionsMap } from '../../../../config/config';
import { mapActions, mapGetters } from 'vuex';
import helpers from '../../mixin';
import threeDots from '../../loaders/threeDots/ThreeDots.vue';

let domainForCloud = {};

cloudOptionsMap.forEach((value, key) => {
	switch(key) {
	case 0: domainForCloud[value] = '.getkitsune.com';
		break;
	case 1: domainForCloud[value] = '.getkitsune-alicloud.com';
		break;
	case 2: domainForCloud[value] = '.getkitsune.com';
		break;
	case 3: domainForCloud[value] = '.getkitsune.com';
		break;
	default: throw 'domain for selected cloud not found';
	}
})

domainForCloud = Object.freeze(domainForCloud);

export default {
	name: 'domainDetails',

	props: ['projectDetails'],

	mixins: [helpers],

	components: {
		threeDots
	},

	data: () => ({
		subDomain: '',
		showError: false,
		showLoader: false,
		showSiteError: false
	}),

	computed: {
		...mapGetters({
			selectedCloud: 'selectedCloud',
			areCloudDetailsPresent: 'areCloudDetailsPresent',
			chooseOwnAccount: 'chooseOwnAccount'
		}),

		projectName() {
			return this.projectDetails.projectName
		},

		disableBtn() {
			return this.subDomain == '' || this.showError || this.showLoader;
		},

		getDomain() {
			switch(this.selectedCloud) {
			case cloudOptionsMap.get(0): return domainForCloud[cloudOptionsMap.get(0)];
			case cloudOptionsMap.get(1): return domainForCloud[cloudOptionsMap.get(1)];
			default: throw 'select correct cloud to get a domain';
			}
		}
	},

	methods: {
		...mapActions([
			'setStageForPublishing',
			'toggleStatus',
			'setDomainNameForNewCustomerInPublishProject',
			'checkIfWebsiteTagExists',
			'setDomainForCloud'
		]),

		validateSubDomain() {
			this.showError = !siteNameRegex.test(this.subDomain) || this.subDomain == '';
		},

		submitDomain(event) {
			event.stopPropagation();
			if(event.type == 'keyup' && event.keyCode !== 13) {
				return false;
			}
			if(!this.disableBtn) {
				this.showLoader = true;
				this.checkIfWebsiteTagExists({ subDomain: this.subDomain, projectId: this.projectDetails.projectId })
					.then( response => {
						this.showLoader = false;
						if(response.status === 204) {
							this.showSiteError = true;
						}
					})
					.catch( response => {
						const { data } = response.response
						if(data !== "") {
							this.showLoader = false;
							this.showSiteError = false;
							this.updateDomain();
						}else {
							//TODO error handler
						}
					})
			} else {
				this.showError = true;
				this.showSiteError = false;
			}
		},

		updateDomain() {
			this.setDomainNameForNewCustomerInPublishProject({ subDomain: this.subDomain, domain: '' });
			this.setStageForPublishing(1);
		},

		closeModalHandler(event) {
			this.toggleStatusHandler(event, ['overlay', 'publishproject']);
		},

		backBtnHandler() {
			if(this.areCloudDetailsPresent) {
				this.setStageForPublishing(0);
			} else {
				if(this.selectedCloud == cloudOptionsMap.get(0)) {
					this.setStageForPublishing(3);
				} else if(this.selectedCloud == cloudOptionsMap.get(1)) {
					this.chooseOwnAccount ? this.setStageForPublishing(4) : this.setStageForPublishing(3);
				} else if(this.selectedCloud == cloudOptionsMap.get(2)) {
					this.setStageForPublishing(4);
				}
			}
		},
	},

	mounted() {
		document.getElementById('subDomainForm').addEventListener('submit', (e) => {
			e.preventDefault();
			this.submitDomain(e);
		}, false);
		this.setDomainForCloud(this.getDomain);
	},

	beforeDestroy() {
		window.removeEventListener('keyup', this.submitDomain);
	}
}
