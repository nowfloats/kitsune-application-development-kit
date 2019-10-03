import projectsContent from './projectscontent/ProjectsContent.vue'
import projectsHeader from './projectsheader/ProjectsHeader.vue'
import zerothCase from '../zerothcase/ZerothCase.vue'
import { mapGetters } from 'vuex'
import googleSignIn from '../googlesignin/googlesignin.vue'
import skeletonLoader from '../loaders/projects/Projects.vue'

export default {
	name: 'projects',

	components :{
		projectsContent,
		projectsHeader,
		googleSignIn,
		zerothCase,
		skeletonLoader
	},

	computed: {
		...mapGetters({
			totalProjects : 'getTotalProjectsCount',
			loader: 'loader',
			componentStatus : 'componentStatus'
		}),

		hasProjects(){
			return this.totalProjects > 0;
		}
	}

}
