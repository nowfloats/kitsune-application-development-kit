import zerothLiveSites from './zeroth-live-sites/ZerothLiveSites.vue'
import zerothProjects from './zero-projects/zerothProjects.vue'
import zerothChatbots from './zeroth-chatbots/ZerothChatbots'

export default {
	name: 'zerothcase',

	props: [
		'name'
	],

	components: {
		zerothLiveSites,
		zerothProjects,
		zerothChatbots
	}
}
