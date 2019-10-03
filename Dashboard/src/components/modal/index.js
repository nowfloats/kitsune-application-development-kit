import { mapGetters, mapActions } from 'vuex'
import Account from '../accountInfo/AccountInfo.vue'
import helpers from '../mixin'

export default {
	name: 'modal',

	components: {
		Account
	},

	mixins: [helpers],

	computed: mapGetters([
		'componentStatus'
	]),

	methods: mapActions([
		'toggleStatus'
	])
}
