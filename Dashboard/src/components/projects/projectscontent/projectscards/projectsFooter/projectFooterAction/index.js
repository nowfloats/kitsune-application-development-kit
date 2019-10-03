import *  as Constants from '../../../../../../../config/config'
import contextMenu from '../../contextmenu/ContextMenu.vue';
import { mapActions } from 'vuex'
import helpers from '../../../../../mixin'



export default {
	name: 'projectFooterAction',

	components: {
		contextMenu
	},

	props: [
		'projectId',
		'createdOn',
		'details',
		'version',
		'projectName',
		'isProjectsMenu',
		'isContextMenuRequired',
		'kitsuneUrl',
		'customerId',
		'domainVerified'
	],

	data() {
		return {
			showContextMenu : false,
			buttonNameForProjects : 'preview',
			buttonNameForLiveSites : 'dns details'
		}
	},

	mixins: [helpers],

	computed: {
		CreatedOnDate() {
			let objDate = new Date(this.createdOn),
				month = objDate.toLocaleString("en-us", { month: "short" }),
				year = objDate.getFullYear(),
				date = objDate.getDate();
			return Constants.projectCardDateFormat.replace('{date}', date)
													.replace('{mon}',month)
													.replace('{year}', year);
		}
	},

	methods: {
		...mapActions({
			startOptimization : 'setProjectIdAndNameForOptimizeProjectAndStartBuild',
			toggleStatus: 'toggleStatus',
			setCustomerIdForVerifyDomain : 'setCustomerIdForVerifyDomain',
			setWebsiteIdInDnsDetails: 'setWebsiteIdInDnsDetails'
		}),

		openContextMenu(event) {
			Event.emit('CloseContextMenu');
			this.showContextMenu = true;
			event.stopPropagation();
		},

		buttonAction(event){
			event.stopPropagation();

			if(!this.isProjectsMenu){
				this.toggleStatusHandler(event,['overlay','dnsDetails']);
				this.setWebsiteIdInDnsDetails(this.customerId);
			}
			else{
				window.open(Constants.projectIDEPreviewLink.replace('{projectId}', this.projectId));
			}

		},

		openIDEForParent(event){
			event.stopPropagation();
			this.$emit('OpenIDEEvent');
		}

	},

	mounted() {
		Event.listen('CloseContextMenu',()=>{
			this.showContextMenu =false;
		})
	}
}
