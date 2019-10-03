import Vue from 'vue';
import App from './App.vue';
import store from './store';
import router from './router';
import { sync } from 'vuex-router-sync';
import VueTouch from 'vue-touch';
import veeValidate from 'vee-validate';
import VueTippy from 'vue-tippy';
// import GSignInButton from 'vue-google-signin-button';
// import VueAnalytics from 'vue-analytics';

sync(store, router);

// Vue.use(GSignInButton);
Vue.use(VueTouch);
Vue.use(VueTippy,{
	flipDuration: 0,
	popperOptions: {
		modifiers: {
			preventOverflow: {
				enabled: false
			},
			hide: {
				enabled: false
			}
		}
	}
});

const config = {
	errorBagName: 'errors', // change if property conflicts
	fieldsBagName: 'fields',
	delay: 0,
	locale: 'en',
	dictionary: null,
	strict: true,
	classes: false,
	classNames: {
		touched: 'touched', // the control has been blurred
		untouched: 'untouched', // the control hasn't been blurred
		valid: 'valid', // model is valid
		invalid: 'invalid', // model is invalid
		pristine: 'pristine', // control has not been interacted with
		dirty: 'dirty' // control has been interacted with
	},
	events: 'input|blur',
	inject: true,
	validity: false,
	aria: true
};

Vue.use(veeValidate, config)

Vue.component('Multiselect', require('vue-multiselect').default);

// Removed as it doesn't work in china
// Vue.use(VueAnalytics, {
// 	id: 'UA-101466766-2',
// 	checkDuplicatedScript: true
// })

window.Event =  new class {
	constructor() {
		this.vue =  new Vue();
	}

	emit(event, data = null) {
		this.vue.$emit(event, data);
	}

	listen(event, callback) {
		this.vue.$on(event, callback);
	}

}

new Vue({
	el: '#app',
	router,
	store,
	template: '<App/>',
	components: { App },

	/*mounted() {
		let body =document.getElementsByTagName('body')[0];
		body.addEventListener('click',(event)=>{
			let target = event.target.className;
			if(target.indexOf('project-context-menu-trigger') <0)
				Event.emit('CloseContextMenu');
		});
	}*/

	mounted() {
		store.dispatch('checkApiStatus')
			.then(({ success, isDown }) => {
				if(success && isDown) {
					this.$router.replace({ name: 'maintenance' });
				} else {
					this.$router.replace({ name: 'dashboard' })
				}
			})
			.catch(() => this.$router.replace({ name: 'maintenance' }))
		console.log('like looking under the hood? - come work with us at team@getkitsune.com')
	}
})
