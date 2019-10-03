import lazyLoading from './lazyLoading'

export default {
	name: 'Settings',
	path: '/settings',
	component: lazyLoading('accountSettings/AccountSettings'),
	meta: {
		default: false,
		title: 'settings',
		comingSoon: false,
		iconClass: 'settings-sidebar'
	},
	children: [
		{
			name: '',
			path: '/settings',
			component: lazyLoading('accountSettings/default/Default'),
			meta: {
				title: ''
			}
		},
		{
			name: 'Account',
			path: '/settings/account',
			component: lazyLoading('accountSettings/editAccount/EditAccount'),
			meta: {
				title: 'account'
			}
		}
	]
}
