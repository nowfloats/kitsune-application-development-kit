import { mapGetters, mapActions } from 'vuex';
import helpers from '../mixin';
import { regex } from "../../../config/config";
import threeDots from '../loaders/threeDots/ThreeDots';

export default {
	name: "renameProject",

	data: () => ({
		showError: false,
		error: ''
	}),

	mixins: [helpers],

	components: {
		threeDots
	},

	computed: {
		...mapGetters({
			project: 'renameProjectDetails',
			showLoader: 'isApiCallInProgress'
		}),

		projectName() {
			return this.project.ProjectName;
		}
	},

	methods: {
		...mapActions([
			'toggleStatus',
			'renameProject',
			'setRenameProject'
		]),

		updateProjectNameInStore({ target }) {
			const { projectName } = regex;
			this.showError = projectName.test(target.value) || target.value.trim() === "";
			this.error = target.value.trim() === ""
				? 'please enter a name for the project'
				: 'project name cannot have special characters';
			this.setRenameProject({
				...this.project,
				ProjectName: target.value
			})
		},

		updateHandler(event) {
			if(!this.showError) {
				this.renameProject(this.project)
					.then(() => {
						this.toggleStatusHandler(event, ['overlay', 'renameProject']);
					})
					.catch(err => console.log(err));
			}
		},

		clickHandler(event) {
			this.toggleStatusHandler(event, ['overlay', 'renameProject']);
		}
	},

	mounted() {
		this.$refs.projectName.focus();
	}

}
