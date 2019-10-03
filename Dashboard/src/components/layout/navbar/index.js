import { mapGetters, mapActions } from 'vuex'
import createButton from '../../create-button/CreateButton.vue'
import badge from '../../badge/Badge.vue'
import greeting from './greeting/Greeting.vue'
import dropdown from '../../dropdown/Dropdown.vue'
import wallet from '../../wallet/Wallet.vue'
import helpers from '../../mixin'

export default {
	name: 'navbar',

	components: {
		createButton,
		badge,
		greeting,
		dropdown,
		wallet
	},

	mixins: [helpers],

	computed: {
		...mapGetters({
			componentStatus: 'componentStatus',
			user: 'getUserDetails',
			actionContainerCounter: 'actionContainerCounter'
		}),
		showCreateButton() {
			if(!this.componentStatus.action)
				return true;
			else {
				if(this.actionContainerCounter !== 2 )
					return true;
				else
					return false
			}
		}
	},

	methods: {
		...mapActions([
			'toggleStatus',
			'updateBadge'
		]),

		updatesClickHandler(e) {
			this.updateBadge(true);
			this.toggleStatusHandler(e, ['overlay', 'updates']);
		},

		addMoneyAction(e) {
			let user = this.user;
			if(user.Address == null || user.Address == undefined){
				this.toggleStatusHandler(e, ['overlay','billingform']);
			}
			else{
				this.toggleStatusHandler(e, ['overlay','addmoney']);
			}
		}
	}
}
