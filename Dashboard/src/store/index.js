import Vue from 'vue';
import Vuex from 'vuex';

import menu from './modules/menu';
import app from './modules/app';
import projects from './modules/components/projects';
import payment from './modules/components/payment';
import user from './modules/components/user';
import action from './modules/components/action';
import billing from './modules/components/billing';
import migration from './modules/components/migration';
import toastr from './modules/components/toastr';
import ana from './modules/components/ana';
import notifications from './modules/components/notifications';

import * as getters from './getters';

Vue.use(Vuex)

const store = new Vuex.Store({
	strict: true,  // process.env.NODE_ENV !== 'production',
	getters,
	modules: {
		menu,
		app,
		projects,
		user,
		payment,
		action,
		migration,
		billing,
		toastr,
		ana,
		notifications
	},
	state: {},
	mutations: {}
})

export default store
