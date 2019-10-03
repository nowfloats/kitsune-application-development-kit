import { mapActions, mapGetters } from 'vuex'

export default {
	name : 'errorBody',

	computed:{
		...mapGetters({
			migration: 'getMigrationDetails'
		}),

		isErrorInStartingCrawl(){
			return this.migration.isErrorInStartingCrawl;
		},

		isNetworkError() {
			return this.migration.isNetworkError;
		}

	},

	methods:{

		...mapActions([
			'retryCrawling'
		]),

		retryMigration(){
			this.retryCrawling();
		}

	}
}
