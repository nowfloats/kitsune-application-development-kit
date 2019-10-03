import projectsCards from './projectscards/ProjectsCards.vue'
import zerothCase from '../../zerothcase/ZerothCase.vue'
import { mapGetters, mapActions } from 'vuex'

export default {
	name: 'projectscontent',

	components : {
		projectsCards,
		zerothCase
	},

	data(){
		return {
			searchQuery : ''
		}
	},

	computed: {
		...mapGetters({
			projects: 'getAllProjects',
			searchResults: 'getProjectsBySearchQuery',
			lazyLoad: 'lazyLoad'
		}),

		searchResult(){
			return this.searchResults;
		},

		hasProjects() {
			return this.projects.length > 0;
		}
	},

	watch: {
		lazyLoad() {
			if(this.lazyLoad.stop) {
				document.querySelector('#projects-container').removeEventListener('scroll', this.scroll);
			}
		}
	},

	methods: {
		...mapActions([
			'sortProjectsByCreatedOn',
			'getAllProjects'
		]),

		scroll() {
			const container = document.querySelector('#projects-container');
			let bottomOfWindow = container.scrollTop + container.offsetHeight
				=== container.scrollHeight;
			const { showLoader } = this.lazyLoad;
			if (bottomOfWindow && !showLoader) {
				this.getAllProjects({ limit: 100, skip: this.projects.length, lazyLoad: true });
			}
		}
	},

	created () {
		Event.listen('sortByProjectsCreatedOn',(payload)=>{
			this.sortProjectsByCreatedOn(payload);
		});
	},

	mounted() {
		document.querySelector('#projects-container').addEventListener('scroll', this.scroll);
	}
}

