import { mapGetters, mapActions } from 'vuex'
import helpers from '../mixin';

export default {
	name: 'verifyDomain',

	mixins: [helpers],

	computed : {
		...mapGetters({
			verifyDomain : 'getVerifyDomainDetails',
			dns : 'getDnsDetailsDetails'
		}),

		getDomainStatus(){
			if(this.verifyDomain.domainDetails.IsMapped){
				return 'domain verified';
			}
			else {
				return 'domain not verified';
			}
		},

		getRequestedDomain() {
			if(this.dns.pendingDomains.length > 0) {
				const { DomainName } = this.dns.pendingDomains[0];
				return DomainName.toLowerCase();
			}
			return '';
		}
	},

	methods :{


		...mapActions([
			'toggleStatus'
		]),

		closeDomainVerificationModal(event){
			this.toggleStatusHandler(event,['overlay', 'verifyDomain']);
			this.toggleStatusHandler(event,['overlay', 'dnsDetails']);
		},

		keyUpHandler(event){
			if(event.keyCode == 27){
				this.closeDomainVerificationModal(event);
			}
		},

		addEventListenerTobodyToCloseOnEscKey(){
			let body = document.getElementsByTagName('body')[0];
			body.addEventListener('keyup', this.keyUpHandler);
		},

		removeEventListenerTobodyToCloseOnEscKey(){
			let body = document.getElementsByTagName('body')[0];
			body.removeEventListener('keyup', this.keyUpHandler);
		}
	},

	created(){
		this.addEventListenerTobodyToCloseOnEscKey();
	},

	destroyed(){
		this.removeEventListenerTobodyToCloseOnEscKey();
	}

}
