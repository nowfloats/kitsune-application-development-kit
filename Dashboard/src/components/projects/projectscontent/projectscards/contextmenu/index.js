import { mapGetters, mapActions } from'vuex'
import * as constants from '../../../../../../config/config'
import FileUpload from "vue-upload-component"
import helpers from '../../../../mixin'

export default {
	name : "context",

	props : [
		'projectId',
		'projectName',
		'isProjectsMenu',
		'customerId',
		'kitsuneUrl',
		'version',
		'isSiteActive'
	],

	components: {
		FileUpload
	},

	mixins: [helpers],

	data() {
		return {
			newFiles: [],
			ProjectsMenuItems : [
				{
					name : "publish",
					actionName : "publish",
					className : "publish"
				},
				{
					name: "optimize",
					actionName: "optimize",
					className: "optimize"
				},
				{
					actionName : "edit",
					name : "edit in IDE",
					className : "ide"
				},
				{
					actionName : "upload",
					name : "upload",
					className : "upload"
				},
				{
					actionName : "rename",
					name : "rename project",
					className : "rename"
				},
				{
					actionName : "download",
					name : "download",
					className : "download"
				},
				{
					actionName : "delete",
					name : "delete",
					className : "delete"
				}
			]
		}
	},

	computed: {
		...mapGetters({
			uploadFiles: 'uploadFiles'
		}),

		previewLink() {
			return constants.projectIDEPreviewLink.replace('{projectId}', this.projectId);
		},

		ideLink() {
			return constants.IDELink.replace('{projectid}', this.projectId);
		},

		LiveSiteMenuItems() {
			let menuItems = [
				{
					name : "live site info",
					className : "liveSiteInfo",
					actionName : 'liveSiteDetails'
				},
				{
					name : "DNS details",
					className : "publish",
					actionName: "dnsDetails"
				},
				{
					name : "data console",
					className : "dataConsole",
					actionName : 'dataConsole'
				}
			];
			this.isSiteActive
				? menuItems.push({
					name: 'deactivate site',
					className: 'deactivate',
					actionName: 'deactivateSite'
				})
				: menuItems.push({
					name: 'reactivate request',
					className: 'request',
					actionName: 'deactivateSite'
				});
			return menuItems;
		}
	},

	methods: {
		...mapActions({
			callSetUploadFiles: 'callSetUploadFiles',
			deleteProject: 'deleteProject',
			optimizeProject: 'optimizeProject',
			toggleStatus: 'toggleStatus',
			setProjectDeleteDetails: 'setProjectDeleteDetails',
			setPublishProjectDetails: 'setPublishProjectDetails',
			setPreviewProjectDetails: 'setPreviewProjectDetails',
			getCustomerList: 'getCustomerList',
			resetStoreForPublishingProject: 'resetStoreForPublishingProject',
			setProjectIdInDownloadProject : 'setProjectIdInDownloadProject',
			requestProjectDownloadLink: 'requestProjectDownloadLink',
			setCustomerIdForCustomerdetails: 'setCustomerIdForCustomerdetails',
			getCustomerDetails : 'getCustomerDetails',
			setWebsiteIdInDnsDetails: 'setWebsiteIdInDnsDetails',
			checkEligibleForPublishingAndGetCustomerList : 'checkEligibleForPublishingAndGetCustomerList',
			getCustomerListForProjectPreview: 'getCustomerListForProjectPreview',
			setContainerCounter: 'setContainerCounter',
			setUploadOnProjectDetails: 'setUploadOnProjectDetails',
			startOptimization: 'setProjectIdAndNameForOptimizeProjectAndStartBuild',
			setRenameProject: 'setRenameProject',
			getKAdminUrl: 'getKAdminUrl',
			getCloudProviderDetails: 'getCloudProviderDetails',
			setDeactivateSiteDetails: 'setDeactivateSiteDetails'
		}),

		populateDataForFiles(projectId, file, fileIndex) {
			let resourceType = '';
			switch (file.type){
			case 'text/html':
				resourceType = 'LINK';
				break;
			case 'application/javascript' :
				resourceType = 'SCRIPT';
				break;
			case 'text/css' :
				resourceType = 'STYLE';
				break;
			default :
				resourceType = 'FILE';
			}
			let filePath = file.name;
			return {
				SourcePath : filePath.substring(filePath.indexOf('/')+1),
				FileContent : "",
				ResourceType: resourceType,
				IsStatic: true,
				ProjectId: projectId,
				fileIndex,
				file: file.file
			}
		},

		inputFile(event) {
			this.$refs.upload.active = false;
			this.callSetUploadFiles(this.newFiles)
		},

		menuAction($event, index) {
			if(this.isProjectsMenu){
				this.projectsMenuAction($event, index);
			}
			else {
				this.LiveSiteMenuActions($event, index);
			}
		},

		projectsMenuAction(e, index) {

			switch (index)
			{
			case "optimize":
				this.toggleStatusHandler(e, ['overlay', 'optimizeProject']);
				this.startOptimization({ projectId : this.projectId, projectName : this.projectName });
				break;

			case "publish":
				// this.$ga.event('button', 'click', 'publish-click');
				this.getCloudProviderDetails(this.projectId);
				this.resetStoreForPublishingProject();
				this.setPublishProjectDetails({ projectId : this.projectId, projectName : this.projectName });
				this.checkEligibleForPublishingAndGetCustomerList();
				this.toggleStatusHandler(e, ['overlay','publishproject']);
				break;

			case "edit":
				window.open(this.ideLink);
				break;

			case "download":
				this.setProjectIdInDownloadProject(this.projectId);
				this.requestProjectDownloadLink();
				break;

			case "rename":
				this.setRenameProject({
					ProjectId: this.projectId,
					ProjectName: this.projectName
				});
				this.toggleStatusHandler(e, ['overlay', 'renameProject']);
				break;

			case "delete":
				this.deleteProject(e);
				break;

			case "upload":
				this.uploadFolder(e);
				break;

			default:
				// todo handle default case
				break;

			}

			Event.emit('CloseContextMenu');

			e.stopPropagation();
		},

		LiveSiteMenuActions(e, actionName){
			switch (actionName)
			{
			case 'liveSiteDetails':
				this.toggleStatusHandler(e,['overlay','customerDetails'])
				this.setCustomerIdForCustomerdetails(this.customerId);
				this.getCustomerDetails();
				break;
			case 'dnsDetails':
				this.toggleStatusHandler(e, ['dnsDetails','overlay']);
				this.setWebsiteIdInDnsDetails(this.customerId);
				break;
			case 'dataConsole':
				this.kAdminLoginHandle();
				break;
			case 'deactivateSite':
				this.isSiteActive ? this.deactivateSite(e) : this.reactivateSite();
				break;
			default:
				// todo handle default case
				break;
			}
			Event.emit('CloseContextMenu');


			e.preventDefault();
		},

		deactivateSite(event) {
			this.setDeactivateSiteDetails({
				name: this.kitsuneUrl,
				websiteIds: [ this.customerId ]
			});
			this.toggleStatusHandler(event, ['overlay', 'deactivateSite']);
		},

		//for future use
		reactivateSite() {
			//TODO logic for reactivating live site
		},

		kAdminLoginHandle() {
			const adminTab = window.open('', '_blank');
			adminTab.document.write('loading kadmin...');
			this.getKAdminUrl(this.customerId)
				.then(({ data }) => adminTab.location.href = data.RedirectUrl)
		},

		previewProjectHandler(e) {
			this.setPreviewProjectDetails({ projectId: this.projectId, projectName: this.projectName });
			this.getCustomerListForProjectPreview();
		},

		deleteProject(e){
			this.toggleStatusHandler(e, ['overlay','deleteproject']);
			this.setProjectDeleteDetails({
				projectId : this.projectId,
				projectName : this.projectName,
				projectVersion : this.version
			});
		},

		uploadFolder(e) {
			this.setUploadOnProjectDetails({ projectId: this.projectId, projectName: this.projectName })
			this.toggleStatusHandler(e, ['action']);
			const { ACTION_UPLOAD } = constants.actionStates;
			this.setContainerCounter(ACTION_UPLOAD);
		},

		contextEventHandler(event){
			let className = event.target.className;
			let elementId = event.target.id;
			if(className.indexOf('project-context-menu-trigger') < 0 &&
				(className.indexOf('contextFileUpload') <0 && elementId !== 'file')) {
				Event.emit('CloseContextMenu');
			}
		},

		attachCloseContextMenuEventToBody() {
			let body = document.getElementsByTagName('body')[0];
			body.addEventListener('click', this.contextEventHandler);
		},

		removeCloseContextMenuEventToBody() {
			let body = document.getElementsByTagName('body')[0];
			body.removeEventListener('click',this.contextEventHandler);
		},

		updateContextMenuPosition() {
			let ele = this.$el,
				rect = ele.getBoundingClientRect(),
				windowHeight = (window.innerHeight || document.documentElement.clientHeight),
				height = windowHeight - (rect.top + rect.height),
				computedProperties = window.getComputedStyle(ele),
				top = 0;

			if(height < 0) {
				height = -(height);
				top = parseInt(computedProperties.top);
				ele.setAttribute('style', `top : ${top - height -10}px`);
			}
		}

	},

	created(){
		this.attachCloseContextMenuEventToBody();
	},

	mounted() {
		this.updateContextMenuPosition();
	},

	destroyed(){
		this.removeCloseContextMenuEventToBody();
	}

}
