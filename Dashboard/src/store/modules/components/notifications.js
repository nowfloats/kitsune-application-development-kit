import { addUpdates, updateBadgeNumber } from "../../mutation-types";
import { notificationCallback } from "../../../../config/config";

const state = {
	updateItems: [
		{
			read: false,
			type: 'user-alert',
			title: 'Welcome to kitsune dashboard',
			content: 'Hello there, add some funds and start experimenting',
			url: 'javascript:void(0)',
			actionName: 'add money',
			callback: notificationCallback.addMoney,
			extraInfo: '',
			className: 'alert'
		}
	],
	badgeNumber: 0
};

const getters = {
	updateList: state => state.updateItems,
	badgeNumber: state => state.badgeNumber
};

const actions = {
	addUpdatesToPanel: ({ commit }, payload) => {
		commit(addUpdates, payload);
		commit(updateBadgeNumber);
	},
	updateBadge: ({ commit }, payload) => {
		commit(updateBadgeNumber, payload)
	}
};

const mutations = {
	[addUpdates] (state, payload) {
		state.updateItems.unshift(payload);
	},
	[updateBadgeNumber] (state, payload) {
		state.badgeNumber = payload ? 0 : ++state.badgeNumber;
	}
};

export default {
	state,
	actions,
	mutations,
	getters
}
