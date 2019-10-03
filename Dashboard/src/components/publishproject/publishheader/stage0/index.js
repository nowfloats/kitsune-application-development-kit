import { mapActions, mapGetters } from 'vuex'
import helpers from '../../../mixin'

export default {
	name: 'PublishingStage0Header',

	props:['projectName','modalHeading', 'isCreateCustomer'],

	mixins: [helpers],

	computed:{
		...mapGetters([
			'domainForCloud'
		]),

		getHeaderValue(){
			return (this.modalHeading == null || this.modalHeading == undefined) ?
				'publishing project' : this.modalHeading;
		},

		isProjectName() {
			return (this.projectName == null || this.projectName == undefined);
		},

		getProjectName() {
			return this.isCreateCustomer ? `${this.projectName}${this.domainForCloud}` : this.projectName;
		}
	},

	methods:{

		...mapActions([
			'toggleStatus',
			'resetStoreForPublishingProject'
		])

	}

}
