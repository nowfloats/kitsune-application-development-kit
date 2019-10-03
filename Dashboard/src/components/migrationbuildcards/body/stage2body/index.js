import { mapActions,mapGetters } from 'vuex';

export default {
	name : 'stage2body',

	props: ['listOfDomains'],

	methods: {

		...mapActions([
			'submitSelectedDomains',
			'setSelectedDomains'
		]),
	},

	computed: {

		...mapGetters({
			apiStagesStatus: 'getApiStatusForStages',
			domainList: 'domainList'
		}),

		listOfSelectedDomains: {
			get () { return this.domainList },
			set (val) { this.setSelectedDomains(val) }
		},

		isListOfDomainsAvaliable(){
			return !(this.listOfDomains == null || this.listOfDomains == undefined);
		},

		gettingDomains(){
			return this.apiStagesStatus.gettingDomains;
		},

		domainsFound(){
			if(this.isListOfDomainsAvaliable){
				return this.listOfDomains.length > 0;
			}
		}

	}
}
