import * as Constants from '../../../config/config.js'
import { mapActions } from 'vuex'
import store from '../../store';

export default {
	name : 'googlesignin',

	mounted () {
		const { hash } = store.state.route;
		if (/access_token|id_token|error/.test(hash)) {
			const { handleAuthentication, getProfile } = store.state.app.auth;

			handleAuthentication().then(() => {
				getProfile((err, profile) => {
					let cookieDomainConfig = Constants.cookieDomain;
					if(window.location.hostname === 'localhost')
						cookieDomainConfig = 'localhost';
					//helper function to create a cookie
					const createCookie = function(cookieName, cookieValue, days) {
						var dt, expires;
						dt = new Date();
						dt.setTime(dt.getTime()+(days*24*60*60*1000));
						expires = '; expires='+dt.toGMTString();
						document.cookie = cookieName+'='+cookieValue + expires+'; path=/ ; domain='+cookieDomainConfig;
					};

					//TODO: encrypt the data
					createCookie('userId', profile.email, 1);
					createCookie('userImage', profile.picture, 1);
					createCookie('userName', profile.name, 1);
					createCookie('userToken', profile.sub, 1);

					this.$router.replace({ path: '/projects' })
				});
			});
		}
	},

	methods: {

		...mapActions({
			setDeveloperInformation: 'setUserDataForUpdatingDatabaseForFirstTimeLogIn'
		}),

		login () {
			const { auth } = store.state.app;
			auth.login()
		},
	}
}
