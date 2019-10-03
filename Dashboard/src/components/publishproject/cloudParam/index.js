import { mapActions, mapGetters } from 'vuex';
import { cloudOptionsMap } from '../../../../config/config';
import aliCloudIcon from '../../../assets/icons/alibaba-cloud.svg';
// import gcpIcon from '../../../assets/icons/GCP.svg';
import pinganIcon from '../../../assets/icons/pingan.svg';
import azureIcon from '../../../assets/icons/azure.svg';
import awsIcon from '../../../assets/icons/AWS.svg';
import helpers from '../../mixin';

export default {
	name: 'cloudParam',

	mixins: [helpers],

	data: () => ({
		aliCloud: {
			accountId: '',
			key: '',
			region: '',
			secret: ''
		},
		gcpCloud: {
			secret: ''
		},
		showField1Error: false,
		showField2Error: false,
		showField3Error: false,
		showField4Error: false
	}),

	computed: {
		...mapGetters({
			selectedCloud: 'selectedCloud',
			publishDetails: 'getPublishDetails',
			aliCloudDetails: 'aliCloudDetails',
			gcpCloudDetails: 'gcpCloudDetails',
			gcpTokenGeneratorUrl: 'gcpTokenGeneratorUrl'
		}),

		disableBtn() {
			if(this.selectedCloud == cloudOptionsMap.get(1)) {
				const { accountId, key, region, secret } = this.aliCloud;
				return !accountId || !key || !region || !secret;
			} else if (this.selectedCloud == cloudOptionsMap.get(2)) {
				const { secret } = this.gcpCloud;
				return !secret;
			}
		},

		cloudOptions: () => cloudOptionsMap,

		paramModalData() {
			switch(this.selectedCloud) {
			case cloudOptionsMap.get(0) : return {
				icon: awsIcon,
				header: 'AWS account credentials'
			};
			case cloudOptionsMap.get(1) : return {
				icon: aliCloudIcon,
				header: 'AliCloud account credentials'
			};
			// case cloudOptionsMap.get(2) : return {
			// 	icon: gcpIcon,
			// 	header: 'GCP authentication'
			// };
			case cloudOptionsMap.get(2) : return {
				icon: pinganIcon,
				header: 'PingAn authentication'
			};
			case cloudOptionsMap.get(3) : return {
				icon: azureIcon,
				header: 'Azure account credentials'
			};
			default: return {
				icon: awsIcon,
				header: 'AWS account credentials'
			};
			}
		}
	},

	methods: {
		...mapActions([
			'setAliCloudDetails',
			'setStageForPublishing',
			'toggleStatus',
			'setGCPDetails'
		]),

		closeModalHandler(event) {
			this.toggleStatusHandler(event, ['overlay', 'publishproject']);
		},

		backBtnHandler() {
			this.selectedCloud == cloudOptionsMap.get(2) ? this.setStageForPublishing(6) : this.setStageForPublishing(3);
		},

		returnPropertyValue(cloud, property) {
			switch(cloud) {
			case cloudOptionsMap.get(1): return this.aliCloud[property];
			case cloudOptionsMap.get(2): return this.gcpCloud[property];
			}
		},

		validateField(fieldNumber, property, cloud) {
			switch(fieldNumber) {
			case 0: this.showField1Error = this.returnPropertyValue(cloud, property) == '';
				break;
			case 1: this.showField2Error = this.returnPropertyValue(cloud, property) == '';
				break;
			case 2: this.showField3Error = this.returnPropertyValue(cloud, property) == '';
				break;
			case 3: this.showField4Error = this.returnPropertyValue(cloud, property) == '';
				break;
			}
		},

		proceedBtnHandler() {
			if(!this.disableBtn) {
				if(this.selectedCloud == cloudOptionsMap.get(1)) {
					this.aliCloud = { ...this.aliCloud, 
						provider: this.selectedCloud };
					this.setAliCloudDetails(this.aliCloud);
				} else if (this.selectedCloud == cloudOptionsMap.get(2)) {
					this.setGCPDetails({
						...this.gcpCloud,
						provider: this.selectedCloud
					});
				}
				this.setStageForPublishing(5);
			}
		}
	},

	mounted() {
		if(this.selectedCloud == cloudOptionsMap.get(1)) {
			const { accountId, region, secret, key } = this.aliCloudDetails;
			this.aliCloud = {
				accountId: accountId ? accountId : '',
				region: region ? region : '',
				secret: secret ? secret : '',
				key: key ? key : ''
			}
		} else if (this.selectedCloud == cloudOptionsMap.get(2)) {
			document.getElementById('gcpParamForm').addEventListener('submit', (event) => {
				event.preventDefault();
				this.proceedBtnHandler();
			}, false);
			const { secret } = this.gcpCloudDetails;
			this.gcpCloud = {
				secret: secret ? secret : ''
			}
		}
	}
}