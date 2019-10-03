import { mapGetters, mapActions } from 'vuex';
import sidebar from './sidebar/Sidebar.vue';
import navbar from './navbar/Navbar.vue';
import overlay from '../overlay/Overlay.vue';
import modal from '../modal/Modal.vue';
import updates from '../updates/Updates.vue';
import resize from '../../directives/resizeHandler';
import action from '../action/Action.vue';
import billingForm from '../billingform/billingform.vue';
import addMoney from '../addmoney/addmoney.vue';
import processingPayment from '../processingpayment/processingpayment.vue';
import migrationBuildCards from '../migrationbuildcards/migrationbuildcards.vue';
import deleteProject from '../deleteprojectmodal/DeleteProjectModal.vue';
import previewProjectModal from '../projects/previewModal/PreviewModal.vue';
import publishProject from '../publishproject/PublishProject.vue';
import helpers from '../mixin';
import optimizeProject from '../optimize/optimize.vue';
import liveSiteModal from '../livesitemodal/livesitemodal.vue';
import verifyDomain from '../verifydomain/verifyDomain.vue';
import dnsDetails from '../dnsdetails/dnsdetails.vue';
import GoogleSignIn from '../googlesignin/googlesignin.vue';
import Skeleton from '../skeleton/skeleton.vue';
import credits from '../creditsModal/credits';
import LowBalanceModal from '../lowBalanceModal/LowBalanceModal';
import abortCrawl from '../abort-crawl/abortCrawl';
import renameProject from '../rename-project-modal/RenameProject';
import fullLoader from '../loaders/fullScreenLoader/fullScreen';
import deactivateSite from '../deactivateSite/DeactivateSite';

export default {
	name: 'layout',

	components: {
		sidebar,
		navbar,
		modal,
		overlay,
		updates,
		action,
		billingForm,
		addMoney,
		processingPayment,
		migrationBuildCards,
		deleteProject,
		publishProject,
		optimizeProject,
		liveSiteModal,
		verifyDomain,
		dnsDetails,
		GoogleSignIn,
		Skeleton,
		previewProjectModal,
		credits,
		LowBalanceModal,
		abortCrawl,
		renameProject,
		fullLoader,
		deactivateSite
	},

	mixins: [helpers],

	computed: {
		...mapGetters([
			'componentStatus',
			'actionContainerCounter',
			'areMandatoryAPIcallsCompleted',
			'menuItems',
			'getLowBalanceDetails'
		]),

		isDataReceivedForUserandProjectsFromAPI() {
			return this.areMandatoryAPIcallsCompleted;
		},

		isRequiredDataReceived() {
			return this.areMandatoryAPIcallsCompleted;
		}

	},

	directives: {
		resize: resize
	},

	methods: {
		...mapActions([
			'toggleStatus',
			'getPaymentStatus',
			'getUserDetails',
			'getAllProjects',
			'setDefaultUserEmail',
			'getUserDetails',
			'getAllProjects',
			'getAllLiveProjects',
			'getUserId',
			'triggerLowBalanceModal',
			'checkApiStatus'
		]),

		getParameterByName(name, url) {
			if (!url) url = window.location.href;
			name = name.replace(/[\[\]]/g, "\\$&");
			var regex = new RegExp("[?&]" + name + "(=([^&#]*)|&|#|$)"),
				results = regex.exec(url);
			if (!results) return null;
			if (!results[2]) return '';
			return decodeURIComponent(results[2].replace(/\+/g, " "));
		},

		userLoggedIn() {
			//retrieve data from the cookie
			const { readCookie } = helpers.methods;
			let user = readCookie('userId');

			if(user == null || user === undefined) {
				this.$router.replace({ path: '/login' });
				return false;
			}
			this.setDefaultUserEmail(user);
			this.getUserId();
			return true;
		}
	},

	created() {
		this.userLoggedIn();
		const PAYMENT = '/payment';
		const url = window.location.href;
		const parameter = url.indexOf(PAYMENT) !== -1
			? url.substr(url.indexOf(PAYMENT) + PAYMENT.length)
			: undefined;
		if(parameter) {
			this.getPaymentStatus(parameter);
			window.history.pushState({}, document.location.origin, "/projects");
		}
	},

	mounted() {
		//retrieve data from the cookie
		const { readCookie } = helpers.methods;
		let userId = readCookie('userId');

		if (userId === null) {
			this.$router.replace({ path: '/login' })
		}
	}
}
