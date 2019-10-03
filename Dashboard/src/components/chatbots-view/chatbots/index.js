import ProjectsCards from '../../projects/projectscontent/projectscards/ProjectsCards.vue';
import { mapGetters } from  'vuex';

export default {
	name : "chatbots",

	components: {
		ProjectsCards
	},

	computed: {
		...mapGetters([
			'getListOfBots'
		]),
		getBots() {
			return this.getAllLiveProjects !== undefined
				? this.getAllLiveProjects.content[0].flow
				: [];
		}
	}
}
