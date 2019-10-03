import MigrationHeader from '../header/stage1header/stage1header.vue';
import MigrationFooter from '../footer/stage3footer/stage3footer.vue';
import Stage6Body from '../body/stage6body/stage6body.vue';

export default {
	name: 'stage6',

	props: ['data'],

	components: {
		MigrationHeader,
		MigrationFooter,
		Stage6Body
	},

	computed: {

		getReplacer(){
			return (this.data == null || this.data == undefined) ? false : true;
		},

		getTotalFilesToBeReplaced() {
			if (this.getReplacer) {
				return this.data.TOTAL ? this.data.TOTAL : 0;
			}
			return 0;
		},

		getStylesReplaced() {
			if (this.getReplacer) {
				return this.data.STYLE ? this.data.STYLE : 0;
			}
			return 0;
		},

		getLinksReplaced() {
			if (this.getReplacer) {
				return this.data.LINK ? this.data.LINK : 0;
			}
			return 0;
		},

		getTotalFilesReplaced() {
			if (this.getReplacer) {
				return this.getStylesReplaced + this.getLinksReplaced;
			}
			return 0;
		},

		getFilesReplacedPercentage() {
			return ( this.getTotalFilesReplaced / this.getTotalFilesToBeReplaced) * 100;
		},

		getReplacerDetails() {
			return {
				totalFilesToBeReplaced: this.getTotalFilesToBeReplaced,
				totalFilesReplaced : this.getTotalFilesReplaced,
				filesReplacedPercentage: this.getFilesReplacedPercentage
			}
		}

	}

}
