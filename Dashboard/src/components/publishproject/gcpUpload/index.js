import FileUpload from 'vue-upload-component';
import helpers from '../../mixin';
import { mapActions, mapGetters } from 'vuex';

export default {
	name: 'gcpUpload',

	components: {
		FileUpload
	},

	props: ['projectDetails'],

	data: () => ({
		creds: [],
		fileToUpload: true
	}),

	mixins: [helpers],

	computed: {
		...mapGetters([
			'gcpCredsFile'
		]),

		file() {
			if(this.creds.length) {
				let { name, size } = this.creds[0].file;
				size = (size/1024).toFixed(2).concat(` KB`);
				return {
					name,
					size
				};
			}
			return '';
		}
	},

	methods: {
		...mapActions([
			'toggleStatus',
			'setStageForPublishing',
			'getGCPTokenGeneratorUrl',
			'uploadFile',
			'setGCPCredsFile'
		]),

		uploadCredentials(newFile, oldFile) {
			this.fileToUpload = false;
		},

		removeFile() {
			this.creds.length = 0;
			this.fileToUpload = true;
		},

		backButtonHandler() {
			this.setStageForPublishing(3);
		},

		closeHandler(event) {
			this.toggleStatusHandler(event, ['overlay', 'publishproject']);
		},

		saveFile() {
			let data = {
				SourcePath : '.credentials/credentials.json',
				FileContent : "",
				IsStatic: true,
				ProjectId: this.projectDetails.projectId
			};
			let reader = new FileReader();
			reader.readAsDataURL(this.creds[0].file);
			reader.onloadend = () => {
				let dataResult = reader.result.substr(reader.result.indexOf(",") + 1);
				data.FileContent = dataResult.indexOf('data') == 0 ? "" : dataResult;
				this.uploadFile(data)
					.then(res => {						
						this.setGCPCredsFile(this.creds[0]);
						this.setStageForPublishing(4);
					})
					.catch(err => console.log(err));
			};
		}
	},

	mounted() {
		this.getGCPTokenGeneratorUrl();
		if(this.gcpCredsFile) {
			this.creds.push(this.gcpCredsFile);
			this.fileToUpload = false;
		}
	}
}