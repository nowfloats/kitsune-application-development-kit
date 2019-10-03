import Multiselect from 'vue-multiselect';
import { mapActions, mapGetters } from 'vuex';

export default {
	name: 'CustomDropdown',

	components: {
		Multiselect
	},

	data() {
		return {
			customer: ''
		}
	},

	computed: {
		...mapGetters([
			'lazyLoad',
			'getPublishCustomers'
		]),

		getCustomerList() {
			return this.getPublishCustomers;
		}
	},

	watch: {
		lazyLoad() {
			if(this.lazyLoad.stop) {
				document.querySelector('.multiselect__content').removeEventListener('scroll', this.scroll);
			}
		}
	},

	methods: {

		...mapActions({
			setCustomerForPublishingProject: 'setCustomerForPublishingProject',
			setStageForPublishing: 'setStageForPublishing',
			getCustomers: 'getCustomerList'
		}),

		nameWithLang ({ WebsiteDomain }) {
			return `${WebsiteDomain}`.toLowerCase();
		},

		publishProject(){
			this.customer ? this.setCustomerForPublishingProject(this.customer) : this.setCustomerForPublishingProject({});
		},

		scroll() {
			const container = document.querySelector('.multiselect__content');
			let bottomOfWindow = container.scrollTop + container.offsetHeight
				=== container.scrollHeight;
			const { stop } = this.lazyLoad;
			console.log(container.scrollTop, container.offsetHeight, container.scrollHeight, bottomOfWindow);
			if (bottomOfWindow && !stop) {
				this.getCustomers({ limit: 100, skip: this.getPublishCustomers.length });
			}
		}
	},

	mounted () {
		document.querySelector('.multiselect__content').addEventListener('scroll', this.scroll);
	}
}
