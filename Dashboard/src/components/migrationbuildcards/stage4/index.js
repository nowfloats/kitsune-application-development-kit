import MigrationHeader from '../header/stage1header/stage1header.vue';
import MigrationFooter from '../footer/stage4footer/stage4footer.vue';
import Stage4Body from '../body/stage4body/stage4body.vue';

export default {
	name: 'stage4',

	props: ['data', 'optimizer'],

	components: {
		MigrationHeader,
		Stage4Body,
		MigrationFooter
	}
}
