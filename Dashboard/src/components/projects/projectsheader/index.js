import { mapGetters, mapActions } from 'vuex'

export default {
	name: 'projectsheader',

	props: ['isLiveSites', 'isChatBots'],

	data() {
		return {
			searchQuery : '',
			enableSearch : false,
			isDecreasingOrder : true
		}
	},

	computed: {
		...mapGetters({
			totalProjects: 'getTotalProjectsCount',
			totalLiveSites: 'getAllLiveProjects'
		}),
		getActiveLiveSites() {
			return this.totalLiveSites.filter(ele => ele.IsActive).length;
		}
	},
	methods : {
		...mapActions([
			'setProjectSearchQueryInProjects'
		]),

		sortByProjectsCreatedOn() {
			Event.emit('sortByProjectsCreatedOn', this.isDecreasingOrder);
			this.isDecreasingOrder = !this.isDecreasingOrder;
		},

		search(){
			this.setProjectSearchQueryInProjects(this.searchQuery);
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
				this.search();
			}else{
				if(this.searchQuery.trim() === ''){
					this.enableSearch = false;
					this.searchQuery = '';
					this.search();
				}
			}
		}

	},

	beforeDestroy() {
		this.setProjectSearchQueryInProjects('');
	}

}
