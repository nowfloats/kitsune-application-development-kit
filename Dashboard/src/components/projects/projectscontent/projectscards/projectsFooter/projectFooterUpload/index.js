import progressBar from '../../../../../progressbar/progressbar.vue'

export default {
	name: 'ProjectFooterUpload',

	components: {
		progressBar
	},

	props: [ 'details' ],

	computed: {
		getValue() {
			return Math.round((this.details.filesUploaded / this.details.totalFiles) * 100)
		},
		getFileName (){
			let index = this.details.fileName.lastIndexOf("/");
			return this.details.fileName.substr(++index)
		}
	},

	destroyed() {
		Event.emit('triggerUploadAlert', this.details.projectId)
	},
}
