import { mapGetters } from 'vuex'

export default {
	name: 'account',

	computed: mapGetters({
		user: 'getUserDetails'
	})
}
