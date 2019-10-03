import { mapGetters, mapActions } from 'vuex'
import helpers from '../../mixin'

export default {
	name: 'sidebar',

	computed: mapGetters({
		menuItems: 'menuItems',
		componentStatus: 'componentStatus',
		totalProjects : 'getTotalProjectsCount'
	}),

	mixins: [helpers],

	methods: {
		...mapActions([
			'toggleStatus'
		]),
		goHome(e) {
			if(this.componentStatus.sidebar) {
				this.hideSidebar(e);
			}
			this.toggleStatusHandler(e, ['overlay', 'credits'])
		},
		hideSidebar(e) {
			if(this.componentStatus.hamburger) {
				this.toggleStatusHandler(e, ['overlay', 'sidebar'])
			}
		}
	}
}
