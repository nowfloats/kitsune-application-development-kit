import { mapActions, mapGetters } from 'vuex';
import helpers from '../../mixin';
import { cloudOptionsMap } from '../../../../config/config';

export default {
	name: 'chooseCloud',

	mixins: [helpers],

	props: ['projectDetails'],

	data: () => ({
		selectedCloud: '',
		ownAccount: null
	}),

	computed: {
		...mapGetters({
			selectedCloudOption: 'selectedCloud',
			chooseOwnAccount: 'chooseOwnAccount'
		}),

		cloudOptions: () => cloudOptionsMap,

		infoMessage() {
			return this.selectedCloud == cloudOptionsMap.get(0) 
				?`your site will be hosted on kitsune's ${this.selectedCloud.toUpperCase()} account`
				: `you need to give your GCP cloud account credentials in the next steps.`
		},

		disableBtn() {
			return this.selectedCloud == '' || (this.selectedCloud == cloudOptionsMap.get(1) && this.ownAccount == null)
		}
	},

	methods: {
		...mapActions([
			'setStageForPublishing',
			'toggleStatus',
			'setSelectedCloud',
			'setChooseOwnAccount',
			'setAliCloudDetails'
		]),

		backBtnHandler() {
			this.setStageForPublishing(0);
		},

		proceedBtnHandler() {
			this.ownAccount = this.selectedCloud == cloudOptionsMap.get(1) ? this.ownAccount : null;
			if(this.selectedCloud == cloudOptionsMap.get(0)) {
				this.setStageForPublishing(5);
			} else if(this.selectedCloud == cloudOptionsMap.get(2)) {
				this.setStageForPublishing(6);
			} else {
				this.ownAccount ? this.setStageForPublishing(4) : this.setStageForPublishing(5);
				this.setChooseOwnAccount(this.ownAccount);	
				this.setAliCloudDetails({
					accountId: '',
					key: '',
					region: '',
					secret: '',
					provider: this.selectedCloud
				})
			}			
			this.setSelectedCloud(this.selectedCloud);
			this.setChooseOwnAccount(this.ownAccount);
		},

		closeModalHandler(event) {
			this.toggleStatusHandler(event, ['overlay', 'publishproject']);
		},

		setCloudOption(payload) {
			this.selectedCloud = cloudOptionsMap.get(payload) ? cloudOptionsMap.get(payload) : '';
		}
	},

	mounted() {
		this.selectedCloud = this.selectedCloudOption ? this.selectedCloudOption : cloudOptionsMap.get(0);
		this.ownAccount = this.chooseOwnAccount == null ? null : this.chooseOwnAccount;
	}
}