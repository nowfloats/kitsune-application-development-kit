import lazyLoading from './lazyLoading'

export default {
	name: 'Billing',
	path: '/billing',
	component: lazyLoading('billing/Billing'),
	meta: {
		default: false,
		title: 'billing',
		iconClass: 'billing'
	}
}
