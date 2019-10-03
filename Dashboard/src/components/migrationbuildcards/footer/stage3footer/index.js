import ProgressBar from '../../../progressbar/progressbar.vue'

export default {
	name: 'stage3footer',

	props : ['totalPercentage', 'requiredPercentage'],

	components:{
		ProgressBar
	},

	computed: {

		percentageCompleted() {
			return parseInt(isNaN(this.totalPercentage) ? 0 : this.totalPercentage);
		}

	}
}
