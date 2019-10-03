import { mapGetters } from 'vuex'
import * as constants from "../../../../../config/config";

export default {
	name: 'stage7footer',

	computed:{
		...mapGetters({
			migration : 'getMigrationDetails'
		}),

		getPreviewLink() {
			return constants.projectIDEPreviewLink.replace('{projectId}', this.migration.projectId);
		}

	},

	methods:{

		previewSite(){
			window.open(this.getPreviewLink)
		}

	}
}
