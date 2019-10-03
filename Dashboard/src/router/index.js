/*
import Vue from 'vue'
import Router from 'vue-router'

import menuModule from 'vuex-store/modules/menu'

Vue.use(Router)

export default new Router({
	routes: [
		...generateRoutesFromMenu(menuModule.state.items),
		{ path: '*', redirect: { name: getDefaultRoute(menuModule.state.items).name } }
	]
})

function generateRoutesFromMenu (menu = [], routes = []) {
	const getRoutes = function (item) {
		if (item.path) {
			routes.push(item)
		}
		if (item.children) {
			generateRoutesFromMenu(item.children, routes)
		}
	}
	menu.forEach(item => getRoutes(item))
	return routes
}

function getDefaultRoute (menu = []) {
	let defaultRoute
	const getDefaultRoute = function (item) {
		if (item.meta.default) {
			defaultRoute = item
		} else if (item.children) {
			let defaultChild = item.children.find((i) => i.meta.default)
			defaultRoute = defaultChild || defaultRoute
		}
	}
	menu.forEach((item) => {
		getDefaultRoute.apply(null, item)
	})

	return defaultRoute
}
*/

import Vue from 'vue'
import Router from 'vue-router'
import lazyLoading from '../store/modules/menu/lazyLoading'
import menuModule from 'vuex-store/modules/menu';
import store from '../store/index';

Vue.use(Router);

const router = new Router({
	routes: [
		{ name: '404', path: '*', component: lazyLoading('fourNotFour/fourNotFour') },
		{ name: 'payment', path: '/payment(/)?*', component: lazyLoading('layout/Layout') },
		{ name: 'dashboard', path: '/', component: lazyLoading('layout/Layout'),
			children: [
				...generateRoutesFromMenu(menuModule.state.items)
			], redirect: { name: getDefaultRoute(menuModule.state.items).name }
		},
		{ name: '', path: '/login', component: lazyLoading('googlesignin/googlesignin') },
		{ name: 'maintenance', path: '/maintenance', component: lazyLoading('maintenance/maintenance') }
	],
	mode: 'history'
});

router.beforeEach((to, from, next) => {
	const { apiStatus } = store.getters;
	if(apiStatus) {
		const { success, isDown } = apiStatus;
		if(success && isDown) {
			to.name !== 'maintenance' ? router.replace({ name: 'maintenance' }) : next();
		} else {
			next();
		}
	}
	if(apiStatus === null) {
		next();
	}
});

export default router

function generateRoutesFromMenu (menu = [], routes = []) {
	for (let i = 0, l = menu.length; i < l; i++) {
		let item = menu[i]
		if (item.path) {
			routes.push(item)
		}
		if (item.children) {
			generateRoutesFromMenu(item.children, routes)
		}
	}
	return routes
}

function getDefaultRoute (menu = []) {
	let defaultRoute

	menu.forEach((item) => {
		if (item.meta.default) {
			defaultRoute = item
		} else if (item.children) {
			let defaultChild = item.children.find((i) => i.meta.default)
			defaultRoute = defaultChild || defaultRoute
		}
	})

	return defaultRoute
}
