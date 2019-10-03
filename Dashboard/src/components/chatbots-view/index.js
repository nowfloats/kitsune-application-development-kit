import ProjectsHeader from  '../projects/projectsheader/ProjectsHeader.vue';
import chatbots from './chatbots/Chatbots';
import zerothCase from '../zerothcase/ZerothCase.vue';
import { mapGetters, mapActions } from 'vuex';

export default {
	name : "projectsliveview",

	components: {
		ProjectsHeader,
		chatbots,
		zerothCase
	},

	computed: {
		...mapGetters([
			'getListOfBots'
		]),

		hasBots() {
			return this.getListOfBots !== undefined
				? this.getListOfBots.totalElements > 0
				: false;
		}
	},

	methods: {
		...mapActions([
			'toggleShowArrowBots'
		])
	},

	mounted() {
		this.toggleShowArrowBots(true);
	},

	destroyed(){
		this.toggleShowArrowBots(false);
	}
}
