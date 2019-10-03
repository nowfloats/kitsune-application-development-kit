import { mapGetters, mapActions } from "vuex";
import helpers from '../mixin';
import { notificationCallback } from "../../../config/config";

export default {
	name: 'alert',

	props: {
		updates: {
			type: Object
		}
	},

	computed: {
		...mapGetters({
			user: 'getUserDetails'
		})
	},

	mixins: [helpers],

	methods: {
		...mapActions([
			'toggleStatus'
		]),

		updatesAction(e) {
			const { callback } = this.updates;
			switch(callback) {
			case notificationCallback.addMoney:
				this.addMoneyAction(e);
				break;
			default:
				throw new Error("please provide a correct callback for notification's action");
			}
		},

		addMoneyAction(e) {
			const { Address, PhoneNumber } = this.user;
			Address && PhoneNumber
				? this.toggleStatusHandler(e, ['updates','addmoney'])
				: this.toggleStatusHandler(e, ['updates','billingform']);
		}
	}
}
