import alert from '../alert/Alert.vue'
import { mapGetters, mapActions } from 'vuex'
import helpers from '../mixin'

export default {
	name: 'updates',

	components: {
		alert
	},

	mixins: [helpers],

	computed: {
		...mapGetters({
			componentStatus: 'componentStatus',
			updateItems: 'updateList'
		}),
		updateCount() {
			return this.updateItems.length
		}
	},

	methods: mapActions([
		'toggleStatus'
	])
}
