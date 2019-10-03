import liveSites from './liveSites'
import projects from './projects'
import billing from './billing'
import settings from './accountSettings'
import chatbots from './chatbots'

const state = {
	items: [
		projects,
		liveSites,
		chatbots,
		billing,
		settings
	]
}

export default {
	state
}
