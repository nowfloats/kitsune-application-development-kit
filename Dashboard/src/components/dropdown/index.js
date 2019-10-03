import helpers from '../mixin'
import wallet from '../wallet/Wallet.vue'
import { mapGetters, mapActions } from "vuex"
import * as Constants from '../../../config/config.js'
import store from '../../store'
import { AUTH_CONFIG } from '../../store/modules/auth0-variables'

export default {
	name: 'dropdown',

	components: {
		wallet
	},

	mixins: [helpers],

	data() {
		return {
			listItems: [
				{
					label: 'account settings',
					url: 'javascript:void(0)',
					imageUrl: 'account-settings',
					newTab: true
				},
				{
					label: 'take guided tour',
					url: '#',
					imageUrl: 'guided-tour',
					newTab: true
				},
				{
					label: 'read documentation',
					url: 'http://docs.kitsune.tools/',
					imageUrl: 'read-documentation',
					newTab: true
				},
				{
					label: 'kitsune blog',
					url: 'http://blog.getkitsune.com/',
					imageUrl: 'kitsune-blog',
					newTab: true,
					badge: true
				},
				{
					label: 'kitsune IDE',
					url: 'http://ide.kitsune.tools/',
					imageUrl: 'open-IDE',
					newTab: true,
					badge: true
				},
				{
					label: 'logout',
					url: '#',
					imageUrl: 'logout',
					newTab: true,
					badge: true
				}
			]
		}
	},

	computed: {
		...mapGetters({
			user: 'getUserDetails'
		}),

		getYear: () => new Date().getFullYear()
	},

	methods: {
		...mapActions([
			'toggleStatus'
		]),
		dropDownOperations(e, index) {
			let cookieDomainConfig = Constants.cookieDomain;
			if(window.location.hostname === 'localhost')
				cookieDomainConfig = 'localhost';

			let deleteCookie = function(name) {
				document.cookie = name + '=;expires=Thu, 01 Jan 1970 00:00:01 GMT; path=/; domain='+cookieDomainConfig;
			};

			switch (index){
			case 0:
				this.toggleStatusHandler(e, ['overlay', 'profile'])
				//window.open(this.listItems[index].url, "_self");
				this.$router.replace( { path: '/settings/account' } )
				break;
			case 1:
			case 2:
			case 3:
			case 4:
				window.open(this.listItems[index].url);
				break;
			case 5:
				// TODO clear cookie using action
				deleteCookie('userName');
				deleteCookie('userId');
				deleteCookie('userImage');
				deleteCookie('userToken');
				store.state.app.auth.logout();
				window.location=`
					https://${AUTH_CONFIG.domain}/v2/logout?returnTo=https://www.getkitsune.com&client_id=${AUTH_CONFIG.clientId}
				`;
				break;
			default:
				// todo handle default case
				break;
			}
		}
	}
}
