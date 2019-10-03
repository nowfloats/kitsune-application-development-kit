import Stage0 from './stage0/stage0.vue';
import Stage1 from './stage1/stage1.vue';
import Stage2 from './stage2/stage2.vue';
import Stage3 from './stage3/stage3.vue';
import Stage4 from './stage4/stage4.vue';
import Stage5 from './stage5/stage5.vue';
import Stage6 from './stage6/stage6.vue';
import Stage7 from './stage7/stage7.vue';
import error from './errror/error.vue';
import { mapGetters, mapActions } from 'vuex';
import helpers from '../mixin';


export default {
	name : 'migrationbuildcards',

	data: () => ({
		isExtraOptionsActive: false
	}),

	components: {
		Stage0,
		Stage1,
		Stage2,
		Stage3,
		Stage4,
		Stage5,
		Stage6,
		Stage7,
		error
	},

	computed: {
		...mapGetters({
			migration : 'getMigrationDetails',
			stage : 'getMigrationStage',
			componentStatus: 'componentStatus'
		}),

		isError(){
			const { isErrorInStartingCrawl, isErrorInGettingStageDetails, isNetworkError } = this.migration;
			return (isErrorInStartingCrawl || isErrorInGettingStageDetails || isNetworkError);
		}

	},

	mixins: [helpers],

	methods: {
		...mapActions([
			'minimizeMigrationCards',
			'resetMigrationStatus',
			'toggleStatus'
		]),

		getStatus(index) {
			return (this.stage === index && this.migration.isCrawlingStarted);
		},

		minimizeProject() {
			this.minimizeMigrationCards();
			if(this.stage === 8) {
				this.resetMigrationStatus();
			}
		},

		keyUpHandler(event) {
			event.stopPropagation();
			if(event.keyCode === 27 && !this.componentStatus.abortCrawl) {
				this.minimizeProject();
			}
		},

		toggleExtraOptions(bool) {
			this.isExtraOptionsActive = bool;
		},

		extraOptionsClickHandler(event, optionName) {
			this.toggleExtraOptions(false);
			try {
				switch (optionName) {
				case 'minimize': this.minimizeProject();
					break;
				case 'abort': this.toggleStatusHandler(event, ['abortCrawl']);
					break;
				default: throw 'choose a correct option for migration';
				}
			}
			catch (e) {
				console.log(e);
			}
		}
	},

	mounted() {
		window.addEventListener('keyup', this.keyUpHandler, false);
	},

	beforeDestroy() {
		window.removeEventListener('keyup', this.keyUpHandler);
	},
}
