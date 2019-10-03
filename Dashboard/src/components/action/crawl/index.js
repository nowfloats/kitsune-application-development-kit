import { mapGetters, mapActions } from "vuex"
import { domainNameRegex, actionStates } from '../../../../config/config'
import FileUpload from "vue-upload-component"

export default {
	name: 'crawl',

	components: {
		FileUpload
	},

	computed: {
		...mapGetters([
			'isActionInputInFocus',
		]),

		getProjectName(){
			return this.projectName.replace(/(\/+$)/igm,'');
		}

	},

	data() {
		return {
			isHoverEffectActive: false,
			projectName : '',
			uploadFiles: [],
			isDomainNameValid: true,
			defaultButtonVisibility: false
		}
	},

	methods: {
		...mapActions([
			'setContainerCounter',
			'callToggleContainerActiveClass',
			'changeContainerCounter',
			'startCrawling',
			'callSetUploadFiles',
			'setIsFolderDropped',
			'setCurrentProjectName',
			'resetMigrationStatus'
		]),

		getInputFile() {
			this.$refs.inputFile.click()
		},

		activateHoverEffect() {
			this.isHoverEffectActive = true;
		},

		deactivateHoverEffect() {
			this.isHoverEffectActive = false;
		},

		activateDragEffect() {
			this.isHoverEffectActive = true
			this.callToggleContainerActiveClass(true)
		},

		deactivateDragEffect() {
			this.isHoverEffectActive = false;
			this.callToggleContainerActiveClass(false)
		},

		startCrawl(event){
			event.stopPropagation();
			this.validateDomainName();
			if(this.isDomainNameValid){
				this.resetMigrationStatus();
				this.startCrawling({ Url : this.getProjectName });
				this.setCurrentProjectName(this.getProjectName);
			}
		},

		inputFile() {
			this.$refs.upload.active = false;
			this.callSetUploadFiles(this.uploadFiles);
			const { ACTION_CREATE } = actionStates;
			this.setContainerCounter(ACTION_CREATE);
		},

		validateDomainName(){
			this.defaultButtonVisibility = true;
			this.isDomainNameValid = domainNameRegex.test(this.getProjectName);
		},

		dragAndDropHandler() {
			this.setIsFolderDropped(true)
		}
	},

	mounted() {
		this.$refs.websiteName.focus();
	}
}
