import { mapGetters, mapActions } from 'vuex'
import editAccount from './editAccount/EditAccount.vue'
import defaultComponent from './default/Default.vue'

export default {
	name: 'accountSettings',

	computed: {
		...mapGetters({
			user: 'getUserDetails',
			updateStatus: 'getUserProfileUpdate',
			component: 'getaccountComponent'
		}),

		subRouteName() {
			return this.$route.name;
		},

		checkRouteForBreadCrumbs() {
			return this.subRouteName !== 'settings' && this.subRouteName !== ''
		}
	},

	data() {
		return {
			setting: 'default'
		}
	},

	components: {
		editAccount,
		defaultComponent
	},

	methods: {
		...mapActions ([
			'editAccountComponent'
		]),
		goBackToDefault(component) {
			this.editAccountComponent(component)
			this.$router.push( { path: '/settings' })
		}
	}
}
