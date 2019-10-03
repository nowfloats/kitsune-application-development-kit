import { mapGetters,mapActions } from 'vuex';
import Account from "../accountInfo/AccountInfo.vue";
import helpers from '../mixin';
import { overlayModalNames } from "../../../config/config";

export default {
	name: 'overlay',

	components: {
		Account
	},

	mixins: [helpers],

	computed: {
		...mapGetters({
			componentStatus : 'componentStatus'
		}),

		getOverlayNames: () => overlayModalNames
	},

	methods: {
		...mapActions([
			'toggleStatus'
		]),

		escHandler(event) {
			event.stopPropagation();
			if(event.keyCode === 27 && !this.componentStatus.abortCrawl) {
				this.toggleStatusHandler(event, this.getOverlayNames);
			}
		}
	},

	mounted() {
		window.addEventListener('keyup', this.escHandler, false);
	},

	beforeDestroy() {
		window.removeEventListener('keyup', this.escHandler);
	}
}
