import ProjectsCards from '../../projects/projectscontent/projectscards/ProjectsCards.vue'
import { mapGetters } from  'vuex'

export default {
	name : "livesites",

	components: {
		ProjectsCards
	},

	computed: {
		...mapGetters([
			'getAllLiveProjects'
		]),
		getActiveLiveSites() {
			return this.getAllLiveProjects.filter(obj => {
				return obj.IsActive
			})
		}
	}

}
