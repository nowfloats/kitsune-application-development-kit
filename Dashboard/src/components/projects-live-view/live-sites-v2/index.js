import contextMenu from '../../projects/projectscontent/projectscards/contextmenu/ContextMenu';
import helpers from '../../mixin';
import { cloudOptionsMap } from '../../../../config/config';

export default {
	name: "liveSitesV2",

	components: {
		contextMenu
	},

	mixins: [helpers],

	data: () => ({
		showContextMenu: false
	}),

	props: ['site', 'index'],

	computed: {
		cloudIcon() {
			return this.site.CloudProvider ? `cloud-icon ${cloudOptionsMap.get(this.site.CloudProvider)}` : 'cloud-icon aws';
		},

		upTime() {
			let time = new Date() - new Date(this.site.CreatedOn);
			let minutes = time/60000;
			let roundPower = Math.round(minutes).toString();
			roundPower = roundPower.length >= 4 ? 2 : 0;
			let roundNumber = Math.pow(10, Number(roundPower));
			return (Math.ceil(minutes/roundNumber)*roundNumber).toLocaleString('en-US');
		}
	},

	methods: {
		openContextMenu(event) {
			Event.emit('CloseContextMenu');
			this.showContextMenu = true;
			event.stopPropagation();
		},

		formattedDate(payload) {
			let date = new Date(payload);
			return `${date.toString().toLowerCase() !== 'Invalid Date'.toLowerCase() ? 
				date.toLocaleString('en-US', { 
					month: 'short', 
					year: 'numeric', 
					day: 'numeric',
					hour: 'numeric',
					minute: 'numeric'
				}) : ''}`;
		},

		openSiteHandler() {
			window.open(`//${this.site.WebsiteUrl}`)
		}
	},

	mounted() {
		Event.listen('CloseContextMenu',() => {
			this.showContextMenu = false;
		})
	}
}
