import { mapGetters } from 'vuex'

export default {
	name : 'stage0header',

	computed: {

		...mapGetters({
			projectName : 'getCrawledProjectName'
		})


	}
}
