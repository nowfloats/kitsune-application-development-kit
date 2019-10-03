import { mapGetters, mapActions } from 'vuex';
import helpers from '../mixin';
import threeDots from '../loaders/threeDots/ThreeDots';
import aliCloudIcon from '../../assets/icons/alibaba-cloud.svg';
// import gcpIcon from '../../assets/icons/GCP.svg';
import pinganIcon from '../../assets/icons/pingan.svg';
import azureIcon from '../../assets/icons/azure.svg';
import awsIcon from '../../assets/icons/AWS.svg';
import { cloudOptionsMap } from '../../../config/config';

export default {
	name: 'livesitemodal',

	mixins: [helpers],

	components: {
		threeDots
	},

	computed:{
		...mapGetters({
			liveSites : 'getAllLiveProjects',
			customerDetails : 'getCustomerDetailsForLiveSite'
		}),

		domainName() {
			const { customer } = this.customerDetails;
			const { WebsiteUrl : websiteUrl } = customer;
			if (websiteUrl) {
				return websiteUrl.toLowerCase();
			}
			return '';
		},

		allDetails() {
			return this.customerDetails.customer;
		},

		userDetails() {
			const { customer } = this.customerDetails;
			const { WebsiteUsers : websiteUsers } = customer;
			let websiteUserDetails = null;
			if(websiteUsers && websiteUsers.length > 0) {
				websiteUserDetails = websiteUsers[0].Contact;
			} else {
				// todo error handling
			}
			return {
				fullName: websiteUserDetails ? websiteUserDetails.FullName : '',
				email: websiteUserDetails ? websiteUserDetails.Email : '',
				number: websiteUserDetails ? websiteUserDetails.PhoneNumber : '',
			};
		},

		websiteUrl() {
			const { WebsiteUrl } = this.customerDetails.customer;
			return WebsiteUrl ? WebsiteUrl.toLowerCase() : '';
		},
		
		cloudIcon() {
			switch(this.customerDetails.customer.CloudProvider) {
			case cloudOptionsMap.get(0) : return awsIcon;
			case cloudOptionsMap.get(1) : return aliCloudIcon;
			// case cloudOptionsMap.get(2) : return gcpIcon;
			case cloudOptionsMap.get(2) : return pinganIcon;
			case cloudOptionsMap.get(3) : return azureIcon;
			default: return awsIcon;
			}
		},

	},

	methods:{
		...mapActions([
			'toggleStatus'
		]),

		closeCustomerDetails(event){
			this.toggleStatusHandler(event,['overlay','customerDetails'])
		},

		keyUpHandler(event){
			if(event.keyCode == 27){
				this.closeCustomerDetails(event);
			}
		},

		addEventListenerTobodyToCloseOnEscKey(){
			let body = document.getElementsByTagName('body')[0];
			body.addEventListener('keyup',this.keyUpHandler);
		},

		removeEventListenerTobodyToCloseOnEscKey(){
			let body = document.getElementsByTagName('body')[0];
			body.removeEventListener('keyup',this.keyUpHandler);
		},

		formatDate(date) {
			let formattedDate = new Date(date);
			return `${formattedDate.toString().toLowerCase() !== 'Invalid Date'.toLowerCase() 
			? formattedDate.toLocaleString('en-US', { 
				month: 'short', 
				year: 'numeric', 
				day: 'numeric',
				hour: 'numeric',
				minute: 'numeric'
			}) 
			: ''}`;
		}
	},

	created(){
		this.addEventListenerTobodyToCloseOnEscKey();
	},

	destroyed(){
		this.removeEventListenerTobodyToCloseOnEscKey();
	}
}
