import { mapGetters } from 'vuex';

export default {
	name: 'badge',

	computed: {
		...mapGetters({
			badge: 'badgeNumber'
		}),

		badgeNumber() {
			return this.badge > 99 ? '99+' : this.badge;
		}
	}
}
