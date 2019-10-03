import lazyLoading from './lazyLoading'
//
export default {
	name: 'Live Serverless-Apps',
	path: '/live-serverless-apps',
	component: lazyLoading('projects-live-view/projectsliveview'),
	meta: {
		default: false,
		title: 'live serverless-apps',
		comingSoon: false,
		iconClass: 'live-sites'
	}
}
