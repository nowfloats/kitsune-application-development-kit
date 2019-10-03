import { mapGetters, mapActions } from 'vuex';
import helpers from '../mixin';
import { actionStates } from "../../../config/config";

export default {
	name: 'create-button',

	data () {
		return {
			styleInitial: {
				fill : '#f06428',
				transition: 'fill 1s ease'
			},
			styleActive: {
				fill : '#7e7e7e',
				transition: 'fill 1s ease'
			}
		}
	},

	mixins: [helpers],

	computed: {
		...mapGetters([
			'componentStatus',
			'actionContainerCounter',
			'getTotalProjectsCount',
			'actionContainerCounter',
			'isFolderDropped',
			'showArrowBots',
			'isAnaAccountPresent'
		]),

		getTitle(){
			return this.componentStatus.action
				?  'close'
				: 'create project';
		},

		showCreateButton() {
			return this.componentStatus.hamburger
				? this.$route.name == 'Projects'
				: true;
		},

		showArrowProjects() {
			return !this.getTotalProjectsCount &&
				!this.componentStatus.action &&
				this.$route.name === 'Projects';
		},

		showArrowChatbots() {
			return (!this.componentStatus.action
				&& this.showArrowBots
				&& !(this.getListOfBots !== undefined ? this.getListOfBots.totalElements > 0 : false)
				&& this.$route.name === 'chatbots'
				&& !this.isAnaAccountPresent);
		}
	},

	methods: {
		...mapActions([
			'toggleStatus',
			'setContainerCounter',
			'setIsFolderDropped',
			'resetContainerHistory',
			'setUploadOnProjectDetails'
		]),
		changeFillProperty : function () {
			return this.modalStatus.isActive
				? this.styleActive
				: this.styleInitial;
		},
		handler(e, components) {
			this.toggleStatusHandler(e, components);
			if(this.componentStatus.action) {
				const { ACTION_CRAWL } = actionStates;
				this.setContainerCounter(ACTION_CRAWL);
			} else {
				this.resetContainerHistory();
				this.setUploadOnProjectDetails({ projectId: '', projectName: '' });
			}

			if(this.isFolderDropped) {
				this.setIsFolderDropped(false);
			}
		}
	}
}
