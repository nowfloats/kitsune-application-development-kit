import { mapActions } from "vuex";

export default {
	name: 'uploadAlert',

	props: ['details'],

	computed: {
		getStatus() {
			if(this.details.filesFailed !== undefined) {
				return this.details.filesFailed.length !== 0 ? 'failed' : 'success';
			}
		},
		getDetails() {
			return this.checkAllFilesUploaded ? 'all files uploaded' : `${this.details.filesFailed.length} files failed`;
		}
	},

	methods: {
		...mapActions([
			'resetUploadDetails'
		]),

		checkAllFilesUploaded() {
			return this.details.filesUploaded !== undefined && this.details.filesUploaded === this.details.totalFiles;
		}
	},

	destroyed() {
		this.resetUploadDetails();
	}
}
