import { mapGetters, mapActions } from 'vuex';
import crawl from "./crawl/Crawl.vue";
import blank from "./blank/Blank.vue";
import upload from './upload/Upload';
import ana from './ana/Ana';
import { actionStates } from "../../../config/config";

export default {
	name: 'action',

	data() {
		return {
			uploadFiles: [],
			accept: 'image/png,image/gif,image/jpeg,image/webp',
			extensions: 'gif,jpg,jpeg,png,webp'
		}
	},

	components: {
		crawl,
		blank,
		upload,
		ana
	},

	computed: {
		...mapGetters([
			'isActionInputInFocus',
			'actionContainerCounter',
			'isFolderDropped',
			'componentStatus'
		]),
		showBackButton() {
			return this.actionContainerCounter === 2
		}
	},

	methods: {
		...mapActions([
			'setContainerCounter',
			'changeContainerCounter',
			'callSetUploadFiles',
			'setIsFolderDropped',
			'setUploadOnProjectDetails',
			'toggleShowArrowBots'
		]),
		changeDroppedFolderStatus(payload) {
			if (this.isFolderDropped) {
				this.setIsFolderDropped(payload)
			}
		},

		backToMainCounter() {
			this.changeContainerCounter();
			this.changeDroppedFolderStatus(false);
			this.callSetUploadFiles([]);
		},
		startBlankAction() {
			this.setUploadOnProjectDetails({ projectId: '', projectName: '' });
			this.setIsFolderDropped(false);
			const { ACTION_CREATE } = actionStates;
			this.setContainerCounter(ACTION_CREATE);
			this.callSetUploadFiles(this.uploadFiles);
		},

		escHandler(e) {
			if(e.keyCode === 27) {
				this.changeContainerCounter();
				this.changeDroppedFolderStatus(false);
				this.callSetUploadFiles([]);
			}
		}
	},

	mounted() {
		window.addEventListener('keyup', this.escHandler, false);
		this.toggleShowArrowBots(false);
	},

	beforeDestroy() {
		window.removeEventListener('keyup', this.escHandler);
		this.setUploadOnProjectDetails({ projectId: '', projectName: '' });
		this.toggleShowArrowBots(true);
	}
}
