import MigrationHeader from '../header/stage7header/stage7header.vue';
import MigrationFooter from '../footer/stage7footer/stage7footer.vue';
import { mapActions, mapGetters } from 'vuex';

export default {
	name: 'stage7',

	components: {
		MigrationHeader,
		MigrationFooter
	},

	computed: {
		...mapGetters({
			migrationDetails: 'getMigrationDetails'
		})
	},

	methods:{
		...mapActions({
			setPreviewProjectDetails: 'setPreviewProjectDetails'
			// getCustomerListForProjectPreview: 'getCustomerListForProjectPreview'
		})

	},

	created() {
		this.setPreviewProjectDetails({ projectId: this.migrationDetails.projectId });
		// this.getCustomerListForProjectPreview();
	}
}
