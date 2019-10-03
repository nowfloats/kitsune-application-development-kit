import threeDots from '../../../loaders/threeDots/ThreeDots.vue';
import { mapActions } from "vuex";

export default {
	name : 'stage2footer',

	props: ['totalDomainsFound','numberOfSelectedDomain'],

	components :{
		threeDots
	},

	data() {
		return{
			disableButton : false
		}
	},

	methods:{
		...mapActions([
			'submitSelectedDomains'
		]),

		submitDomains() {
			this.disableButton = true;
			// Event.emit('submitSelectedDomains');
			this.submitSelectedDomains();
		}

	}
}
