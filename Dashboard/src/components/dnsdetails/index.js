import { mapGetters,mapActions } from 'vuex';
import helpers from '../mixin';
import { domainNameRegex } from "../../../config/config";

const aRecord = '35.154.83.253';
const domainPlaceholder = 'please set a domain';

export default {
	name: 'dnsDetails',

	mixins : [helpers],

	data() {
		return {
			updateBtnClicked: true,
			updateSectionVisible: false,
			kitsuneUrls: ['.getkitsune', '.nowfloats'],
			domainStatus: 'mapped',
			newDomain: '',
			showDomainError: false,
			showLoader: true,
		}
	},

	computed:{
		...mapGetters({
			liveSites : 'getAllLiveProjects',
			dns : 'getDnsDetailsDetails',
			pendingDomains: 'getPendingDomains'
		}),

		getARecord: () => aRecord,

		customerDetails(){
			return this.liveSites.find((pro)=>{
				return pro.WebsiteId == this.dns.websiteId
			})
		},

		websiteUrl() {
			const { WebsiteUrl : websiteUrl } = this.customerDetails;
			if (websiteUrl) {
				return websiteUrl.toLowerCase();
			}
			return '';
		},

		getRequestedDomain() {
			const { pendingDomains } = this.dns;
			if(pendingDomains.length > 0) {
				const { DomainName } = pendingDomains[pendingDomains.length - 1];
				return DomainName.toLowerCase();
			}
			return '';
		},

		getDomainPlaceholder: () => domainPlaceholder


	},

	methods:{

		...mapActions({
			toggleStatus: 'toggleStatus',
			getPendingDomains: 'getPendingDomains',
			updateDomain: 'updateDomain',
			getMapDomainMappingAndMap : 'getMapDomainMappingAndMap',
			showToastr: 'showToastr'
		}),

		checkDomainStatus() {
			this.showLoader = false;
			let isKitsuneUrl = false,
				self = this;
			this.kitsuneUrls.map(url => {
				isKitsuneUrl = isKitsuneUrl || self.websiteUrl.indexOf(url) !== -1;
			});
			if(this.pendingDomains.length === 0) {
				this.domainStatus = isKitsuneUrl ? 'kitsuneUrl' : 'mapped';
			} else {
				this.domainStatus = 'requested';
			}
			switch (this.domainStatus) {
			case 'requested' : this.newDomain = this.getRequestedDomain;
				break;
			case 'mapped' : this.newDomain = this.websiteUrl;
				break;
			default: this.newDomain = domainPlaceholder;
			}
		},

		closeDnsDetails(event){
			this.toggleStatusHandler(event,['overlay','dnsDetails']);
		},

		keyUpHandler(event){
			if(event.keyCode == 27){
				this.closeDnsDetails(event);
			}
		},

		switchModals(event) {
			this.toggleStatusHandler(event, ['overlay', 'dnsDetails']);
			this.toggleStatusHandler(event,['overlay','verifyDomain']);
		},

		verifyClickHandler(event) {
			this.switchModals(event);
			this.getMapDomainMappingAndMap();
		},

		changeClickHandler() {
			this.newDomain = this.newDomain === domainPlaceholder ? '' : this.newDomain;
			this.updateBtnClickHandler(false);
			window.setTimeout(() => {
				this.$refs.domainName.focus();
			}, 500);
		},

		updateBtnClickHandler(bool) {
			if(bool) {
				window.setTimeout(() => this.updateBtnClicked = bool, 100);
				this.updateSectionVisible = !bool;
				this.checkDomainStatus();
			} else {
				this.updateBtnClicked = bool;
				window.setTimeout(() => this.updateSectionVisible = !bool, 100)
			}
			this.showDomainError = bool;
		},

		stripProtocol() {
			return this.newDomain.replace(/(https?:\/\/)/ig, '');
		},

		updateConfirmHandler(event) {
			if(domainNameRegex.test(this.newDomain)){
				this.showDomainError = false;
				this.updateBtnClicked = false;
				this.showLoader = true;
				const { websiteId } = this.dns;
				this.updateDomain({ websiteId: websiteId, domain: this.stripProtocol() })
					.then(() => {
						this.getDetails();
						this.updateBtnClickHandler(true);
						this.showDomainError = false;
					})
					.catch(() => {
						this.closeDnsDetails(event);
						this.showToastr({
							isError: true,
							title : 'domain change error',
							message : `we are unable to change domain. please try again`
						});
					})
			}else {
				this.showDomainError = true;
			}
		},

		getDetails() {
			this.getPendingDomains()
				.then(() => {
					this.checkDomainStatus();
				})
				.catch(() => {
					this.showLoader = false;
				});
		},

		updateCancelHandler() {
			this.updateBtnClicked = false;
		},

		addEventListenerTobodyToCloseOnEscKey(){

			let body = document.getElementsByTagName('body')[0];
			body.addEventListener('keyup',this.keyUpHandler);

		},

		removedEventListenerTobodyToCloseOnEscKey(){
			let body = document.getElementsByTagName('body')[0];
			body.removeEventListener('keyup',this.keyUpHandler);
		}

	},

	created(){
		this.addEventListenerTobodyToCloseOnEscKey();
	},

	destroyed(){
		this.removedEventListenerTobodyToCloseOnEscKey();
	},

	mounted() {
		this.getDetails();
	}

}
