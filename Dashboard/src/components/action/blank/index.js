import { mapGetters, mapActions } from "vuex"
import { regex } from '../../../../config/config'
import store from '../../../store/index'
import ThreeDots from '../../loaders/threeDots/ThreeDots.vue'
import FileUpload from 'vue-upload-component';
import parallelLimit from "async-es/parallelLimit";
import reflect from "async-es/reflect";

export default {
	name: 'blank',

	components: {
		ThreeDots,
		FileUpload
	},

	//TODO add validation for project creation
	data() {
		return {
			project: {
				name: ''
			},
			projectNameRegex: regex.projectName,
			projectError: {
				regex: false,
				empty: false
			},
			newFiles: [],
			displayAlert: false
		}
	},

	computed: {
		...mapGetters([
			'isActionInputInFocus',
			'uploadFiles',
			'newProjectId',
			'loader',
			'uploadOnProjectDetails',
			'isFolderDropped',
			'componentStatus',
			'user'
		]),

		recommendationMsg() {
			return `we don't recommend folder selection because webkit browsers like chrome`+
				` don't always preserve the folder structure`
		},

		willProjectUpdate() {
			return this.uploadOnProjectDetails.projectId !== ''
		}
	},

	methods: {
		...mapActions([
			'callToggleContainerActiveClass',
			'changeContainerCounter',
			'createProject',
			'uploadFolder',
			'toggleLoader',
			'callSetUploadFiles',
			'setUploadOnProjectDetails',
			'setIsFolderDropped',
			'uploadCallback',
			'getAllProjects',
			'toggleActionModal'
		]),
		validateProjectName() {
			this.projectError.regex = false
			this.projectError.empty = false
			if (this.project.name !== undefined || this.project.name !== null) {
				if(!this.projectNameRegex.test(this.project.name.trim())){
					this.projectError.regex = false
				}
				else if (this.project.name.trim() == ""){
					this.projectError.regex = false
				}
				else
					this.projectError.regex = true
			}
		},

		createBlankProject() {
			this.changeDisplayAlert();
			if(this.project.name.trim() == ""){
				this.projectError.empty = true
			}
			else {
				if(!this.projectError.regex){
					if(this.uploadOnProjectDetails.projectId == '') {
						this.createProject(this.project);
						this.toggleLoader(true);
					}
					else {
						const { projectId } = this.uploadOnProjectDetails;
						this.uploadAction(projectId)
							.then(tasks => {
								this.$router.replace({ path: '/projects' });
								parallelLimit(tasks, 3);
							})
					}
				}
			}
			this.$router.replace({ path: '/projects' });
		},
		populateDataForFiles(projectId, file, fileIndex) {
			let filePath = file.name;
			return {
				SourcePath : filePath.substring(filePath.indexOf('/')+1),
				FileContent : "",
				IsStatic: true,
				fileIndex: fileIndex,
				ProjectId: projectId,
				file: file.file
			}
		},
		changeFolder(files) {
			if(files.length !== 0)
				store.dispatch('callSetUploadFiles', files)
			this.$refs.projectName.focus();
			this.project.name = files[0].name.split('/')[0]
		},

		uploadAction(projectId) {
			return new Promise((resolve, reject) => {
				let tasks = [];
				let count = 1;
				this.uploadFiles.forEach((file, index) => {
					let dataForFiles = this.populateDataForFiles(projectId, file, index);
					let reader = new FileReader();
					reader.readAsDataURL(file.file);
					reader.onloadend = () => {
						let dataResult = reader.result.substr(reader.result.indexOf(",") + 1);
						dataForFiles.FileContent = dataResult.indexOf('data') == 0 ? "" : dataResult;
						++count;
						tasks = tasks.concat((() => {
							return reflect(callback => {
								return store.dispatch('uploadFolder', dataForFiles)
									.then(success => {
										store.dispatch('uploadCallback', {
											success,
											folderData: dataForFiles,
											length: this.uploadFiles.length });
										callback(null);
									})
							});
						})());
						if(count == this.uploadFiles.length + 1) {
							resolve(tasks);
							this.toggleLoader(false);
							this.toggleActionModal();
						}
					};
				})
			})
		},

		changeDisplayAlert() {
			this.displayAlert = false;
		}
	},

	watch: {
		newProjectId: function() {
			store.dispatch('getAllProjects');
			if(this.uploadFiles.length !== 0) {
				this.uploadAction(this.newProjectId)
				.then(tasks => {
					this.$router.replace({ path: '/projects' });
					parallelLimit(tasks, 3);
				})
			} else {
				this.$router.replace({ path: '/projects' });
			}
		},
	},

	beforeMount() {
		this.project.name = this.uploadFiles.length !== 0 ? this.uploadFiles[0].name.split('/')[0] : '';
	},

	mounted() {
		if(!this.isFolderDropped && this.uploadFiles.length !== 0) {
			setTimeout(() => {
				this.displayAlert = true
			}, 500)
		}
	},

	beforeDestroy() {
		this.changeDisplayAlert();
	}
}
