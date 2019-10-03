import lazyLoading from './lazyLoading';

export default {
	name: 'chatbots',
	path: '/chatbots',
	component: lazyLoading('chatbots-view/ChatbotsView'),
	meta: {
		default: false,
		title: 'chatbots',
		comingSoon: false,
		iconClass: 'ana-chatbots'
	}
};
