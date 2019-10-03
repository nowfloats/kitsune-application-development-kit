import { mapGetters, mapActions } from "vuex";
import helpers from '../mixin';
import { errorMessages } from "../../../config/config";

export default {
	name: "abort-crawl",

	computed: {
		...mapGetters({
			projectName: 'getCrawledProjectName',
			projectDetails: 'projectNameAndId'
		})
	},

	mixins: [helpers],

	methods: {
		...mapActions([
			'toggleStatus',
			'abortCrawl',
			'showToastr'
		]),

		clickHandler(event) {
			if(event.type === 'keyup' && event.keyCode !== 27) {
				return null;
			}
			event.stopPropagation();
			this.toggleStatus({
				event,
				components: ['abortCrawl']
			})
		},

		abortCrawlHandler(event) {
			const { projectId, projectName } = this.projectDetails;
			this.abortCrawl({
				projectId,
				projectName
			})
				.then(response => {
					this.toggleStatusHandler(event, ['overlay', 'abortCrawl', 'migrationbuildcards'])
					this.showToastr({
						isError: false,
						title : 'crawl aborted',
						message : `${response} for ${projectName}.`
					});
				})
				.catch(err => {
					this.showToastr({
						isError: true,
						title : 'ERROR',
						message : `${errorMessages.ABORT_CRAWL}`
					});
				})
		}
	},

	mounted() {
		window.addEventListener('keyup', this.clickHandler, false);
	},

	beforeDestroy() {
		window.removeEventListener('keyup', this.clickHandler);
	},
}
