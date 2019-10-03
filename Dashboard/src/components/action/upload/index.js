import { mapGetters, mapActions } from "vuex"
import FileUpload from 'vue-upload-component';
import { actionStates } from "../../../../config/config";
// const MyWorker = require("worker-loader?inline!./uploadFolder.js");
// import * as myWorker from "worker-loader!./uploadFolder.js";

export default {
	name: 'upload',

	components: {
		FileUpload
	},

	//TODO add validation for project creation
	data() {
		return {
			newFiles: [],
			isDropped: false,
			addDragOverClass: false
		}
	},

	computed: {
		...mapGetters([
			'isActionInputInFocus',
			'uploadFiles',
			'uploadOnProjectDetails'
		]),

		recommendationMsg() {
			return `we don't recommend folder selection because webkit browsers like chrome`+
				` don't always preserve the folder structure`
		}
	},

	methods: {
		...mapActions([
			'callToggleContainerActiveClass',
			'callSetUploadFiles',
			'setContainerCounter',
			'setIsFolderDropped'
		]),

		inputFolder() {
			this.$refs.upload.active = false;
			this.callSetUploadFiles(this.newFiles);
			const { ACTION_CREATE } = actionStates;
			this.setContainerCounter(ACTION_CREATE);
		},

		dragAndDropHandler() {
			this.setIsFolderDropped(true);
		},

		dragOverClassHandler() {
			this.addDragOverClass = true;
		},

		dragLeaveClassHandler() {
			this.addDragOverClass = false;
		}
	}
}
