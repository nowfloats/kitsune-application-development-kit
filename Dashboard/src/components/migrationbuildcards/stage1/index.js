import Stage1Header from '../header/stage1header/stage1header.vue';
import Stage1Body from '../body/stage1body/stage1body.vue';
import Stage1Footer from '../footer/stage1footer/stage1footer.vue';
import { mapActions } from 'vuex';

export default {
	name : 'stage1',

	props: ['data'],

	components: {
		Stage1Header,
		Stage1Body,
		Stage1Footer
	},

	methods: {
		...mapActions([
			'getAnalysisDetails'
		])
	},

	mounted() {
		this.getAnalysisDetails();
	}
}
