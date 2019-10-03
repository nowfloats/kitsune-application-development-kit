import ProjectsHeader from  '../projects/projectsheader/ProjectsHeader.vue';
import LiveSitesV2 from './live-sites-v2/liveSitesV2';
import zerothCase from '../zerothcase/ZerothCase.vue';
import { mapGetters, mapActions } from 'vuex';
import { cloudOptionsMap } from '../../../config/config';
import aliCloudIcon from '../../assets/icons/alibaba-cloud.svg';
// import gcpIcon from '../../assets/icons/GCP.svg';
import pingAnIcon from '../../assets/icons/pingan.svg';
import azureIcon from '../../assets/icons/azure.svg';
import awsIcon from '../../assets/icons/AWS.svg';

const sortingArr = [
	[0, 'ProjectName'],
	[1, 'WebsiteUrl'],
	[2, 'UpdatedOn'],
	[3, 'IsActive']
];

const cloudDocsUrl = 'https://www.getkitsune.com';

const sortingMap = new Map(sortingArr);

export default {
	name : "projectsliveview",

	components: {
		ProjectsHeader,
		LiveSitesV2,
		zerothCase
	},

	data: () => ({
		sortingVariable: 1,
		orderArray: [0, 0, 0, 0],
		enableSearch: false,
		searchQuery: '',
		isMenuActive: false,
		activeTab: 'ALL'
	}),

	computed: {
		...mapGetters({
			getTotalLiveProjectsCount: 'getTotalLiveProjectsCount',
			liveSites: 'getAllLiveProjects',
			lazyLoad: 'lazyLoad',
			showNotification: 'showNotificationLiveSite'
		}),

		hasLiveSites(){
			return this.getTotalLiveProjectsCount > 0;
		},

		sortedArr() {
			return this.sortBy(sortingMap.get(this.sortingVariable), this.orderArray[this.sortingVariable]);
		},

		/**
		 * checks for the value at a particular place in orderArray
		 * if its 0(default) - adds no class
		 * if its true - adds .asc class
		 * if false - adds .desc class
		 * @returns {*[]}
		 */
		statusClass() {
			const bool = this.orderArray[3];
			return ['live-status-container', bool === 0 ? '' : bool ? 'asc' : 'desc'];
		},

		nameClass() {
			const bool = this.orderArray[0];
			return ['sub-info', 'project-name', 'header', bool === 0 ? '' : bool ? 'asc' : 'desc'];
		},

		urlClass() {
			const bool = this.orderArray[1];
			return ['name', 'url', 'header', bool === 0 ? '' : bool ? 'asc' : 'desc'];
		},

		updatedClass() {
			const bool = this.orderArray[2];
			return ['header', bool === 0 ? '' : ( bool ? 'desc' : 'asc')];
		},

		tabOptions: () => cloudOptionsMap,

		notificationData() {
			let icon = awsIcon;
			let message =`to host on ${this.activeTab}, select ${this.activeTab} logo while publishing. 
			you can choose to switch your hosting provider anytime.`

			switch(this.activeTab) {
			case cloudOptionsMap.get(0) : icon = awsIcon;
				break;
			case cloudOptionsMap.get(1) : icon = aliCloudIcon;
				break
			// case cloudOptionsMap.get(2) : icon = gcpIcon;
			// 	break;
			case cloudOptionsMap.get(2) : icon = pingAnIcon;
				break;
			case cloudOptionsMap.get(3) : icon = azureIcon;
				break;
			default: icon = awsIcon;
			}

			return {
				icon,
				message
			};
		},

		regex() {
			return new RegExp(`(^|,)${this.activeTab}($|,)`);
		},

		showNotificationPanel() {
			return this.sortedArr.length !== 0 && this.regex.test(this.showNotification);
		},

		cloudZerothCaseContent() {
			let cloudSelected = this.activeTab == cloudOptionsMap.get(1) ? 'Alibaba cloud' : this.activeTab;
			return `to host, select ${cloudSelected} while publishing a project. 
			you can also switch your hosting provider anytime. to learn more, click`
		},

		cloudDocsUrl : () => cloudDocsUrl
	},

	watch: {
		/**
		 * it fires if search param is changed
		 * so it changes the old value back to default i.e., 0
		 * @param newVal
		 * @param oldVal
		 */
		sortingVariable: function(newVal, oldVal) {
			this.$set(this.orderArray, oldVal, 0);
		},

		searchQuery: function(newVal) {
			this.filterArray();
		},

		lazyLoad() {
			if(this.lazyLoad.stop) {
				document.querySelector('#live-sites-content').removeEventListener('scroll', this.scroll);
			}
		}
	},

	methods: {
		...mapActions({
			getLiveSites: 'getAllLiveProjects',
			setShowNotificationLiveSite: 'setShowNotificationLiveSite'
		}),

		sortBy(value, ascending) {
			// let arr = this.liveSites.slice();
			let filteredArray = this.filterArray().sort((a, b) => a[value] > b[value]
				? 1 : (a[value] < b[value] ? -1 : 0));
			return ascending ? filteredArray : filteredArray.reverse();
		},

		menuClass(val) {
			const bool = this.orderArray[val];
			return [bool === 0 ? '' : bool ? 'asc menu' : 'desc']
		},

		filterArray() {
			let arr = this.liveSites.slice();
			arr = arr.filter(site => this.filterArrayFuntion(site));
			return arr;
		},

		filterArrayFuntion (site) {
			let cloudProvider = this.activeTab == 'ALL' ? true : cloudOptionsMap.get(site.CloudProvider) == this.activeTab;
			return site.WebsiteUrl.toLowerCase().includes(this.searchQuery.toLowerCase()) && cloudProvider;
		},

		/**
		 * checks if the value in param is the same value
		 * if yes, then reverses the value in the array
		 * if not, then sets the value as true(ascending order)
		 * @param event
		 * @param value
		 */
		changeSortingVariable(value, bool) {
			const order = bool !== undefined
				? bool :
				(this.sortingVariable === value ? !this.orderArray[value] : true);
			this.$set(this.orderArray, value, order);
			this.sortingVariable = value;
		},

		activateSearch() {
			this.enableSearch = true;
			this.$refs.search.focus();
		},

		// TODO clear
		deactivateSearch(hover) {
			if(!hover) {
				this.enableSearch = false;
				this.searchQuery = '';
			} else{
				if(this.searchQuery.trim() === ''){
					this.enableSearch = false;
					this.searchQuery = '';
				}
			}
		},

		toggleLiveSitesMenu(bool) {
			this.isMenuActive = bool;
		},

		hideMenu(event) {
			event.stopPropagation();
			this.isMenuActive = this.isMenuActive ? false : null;
		},

		menuClickHandler(event, val) {
			this.changeSortingVariable(val);
			this.hideMenu(event);
		},

		setActiveTab(payload) {
			this.activeTab = cloudOptionsMap.get(payload) ? cloudOptionsMap.get(payload) : 'ALL';
		},

		filterLiveSitesByCloud(payload) {
			return this.liveSites.filter(site => cloudOptionsMap.get(site.CloudProvider) == payload);
		},

		scroll() {
			const container = document.querySelector('#live-sites-content');
			let bottomOfWindow = container.scrollTop + container.offsetHeight
				=== container.scrollHeight;
			const { showLoader } = this.lazyLoad;
			if (bottomOfWindow && !showLoader) {
				this.getLiveSites({ limit: 100, skip: this.getTotalLiveProjectsCount, lazyLoad: true });
			}
		},

		hideNotificationForTab() {
			this.setShowNotificationLiveSite(this.showNotification.replace(this.regex, ','));
		}
	},

	mounted() {
		this.changeSortingVariable(2, false);
		window.addEventListener('click', this.hideMenu);
		document.querySelector('#live-sites-content').addEventListener('scroll', this.scroll);
	},

	beforeDestroy() {
		window.removeEventListener('click', this.hideMenu);
	}
}
