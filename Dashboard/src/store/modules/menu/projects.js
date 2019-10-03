import lazyLoading from './lazyLoading'

export default {
	name: 'serverless-app projects',
	path: '/projects',
	component: lazyLoading('projects/Projects'),
	meta: {
		default: true,
		title: 'serverless-app projects',
		comingSoon: false,
		iconClass: 'projects',
		isProjects : true
	}
}
