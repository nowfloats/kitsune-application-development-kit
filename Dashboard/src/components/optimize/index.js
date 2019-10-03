import { mapGetters,mapActions } from 'vuex'
import helpers from '../mixin'

export default {
	name: 'optimize',

	computed: {
		...mapGetters({
			optimizeDetails : 'getOptimizeDetails',
			optimizeError: 'getOptimizationError'
		}),

		getProjectName(){
			return this.optimizeDetails.projectName.replace('http://','').replace('https://','').replace('/','');
		},

		apiCallCompleted(){
			return this.optimizeDetails.isApiCallCompleted;
		},

		apiCallSuccess(){
			return this.optimizeDetails.isApiCallCompleted && this.optimizeDetails.isApiCallSuccess;
		},

		optimizeErrorStatus() {
			return this.optimizeError;
		}

	},

	mixins : [helpers],

	methods:{
		...mapActions([
			'toggleStatus',
			'setOptimizeErrorStatus'
		]),

		closeHandler(event){
			this.setOptimizeErrorStatus(false);
			this.toggleStatusHandler(event,['overlay','optimizeProject']);
		}

	}

}
