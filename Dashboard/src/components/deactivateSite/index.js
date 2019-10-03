import { mapGetters, mapActions } from 'vuex';
import helpers from '../mixin';
import threeDots from '../loaders/threeDots/ThreeDots';

export default {
	name: "deactivateSite",

	mixins: [helpers],

	components: {
		threeDots
	},

	computed: {
		...mapGetters({
			site: 'deactivateProject',
			showLoader: 'isApiCallInProgress'
		})
	},

	methods: {
		...mapActions([
			'toggleStatus',
			'deactivateSite'
		]),

		closeHandler(event) {
			this.toggleStatusHandler(event, ['overlay', 'deactivateSite']);
		},

		deactivateSiteHandler(event) {
			this.deactivateSite(event);
		}
	}
}
