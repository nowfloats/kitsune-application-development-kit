import MigrationHeader from '../header/stage1header/stage1header.vue';
import Stage2Body from '../body/stage2body/stage2body.vue';
import Stage2Footer from '../footer/stage2footer/stage2footer.vue';

export default {
	name : 'stage2',

	props: ['data'],

	data() {
		return {
			numberOfSelectedDomains : 0
		}
	},

	computed: {

		getNumberOfDomainsFound() {
			return (this.data == undefined || this.data == null || Object.keys(this.data).length === 0)
				? 0 : this.data.length;
		},

	},

	components: {
		MigrationHeader,
		Stage2Body,
		Stage2Footer
	},

	methods: {
		setnumberOfSelectedLinks(payload) {
			this.numberOfSelectedDomains = payload;
		}

	}
}
