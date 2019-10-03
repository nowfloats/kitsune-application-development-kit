import { mapGetters } from "vuex";

export default {
	name : 'maintenance',

	computed: {
		...mapGetters({
			apiStatus: 'apiStatus'
		}),

		getData() {
			if(this.apiStatus && this.apiStatus.Detail) {
				const { title, description } = this.apiStatus.Detail;
				return {
					title,
					description
				}
			} else {
				return '';
			}
		},
	},

	created() {
		if(this.apiStatus == null || !this.apiStatus.isDown) {
			this.$router.replace({ path: '/projects' });
		}
	}
}
