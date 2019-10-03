import { mapGetters, mapActions } from 'vuex'
import FileUpload from "vue-upload-component"

export default {
	name: 'defaultComponent',

	components: {
		FileUpload
	},

	computed: {
		...mapGetters({
			user: 'getUserDetails',
			updateStatus: 'getUserProfileUpdate',
			component: 'getaccountComponent'
		})
	},

	methods: {
		...mapActions([
			'editAccountComponent'
		]),
		changeSettingsComponent(component) {
			this.editAccountComponent(component)
			this.$router.push( { path: '/settings/account' })
		}
	}
}
