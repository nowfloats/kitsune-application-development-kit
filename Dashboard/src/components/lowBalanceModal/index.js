import helpers from "../mixin";
import { mapGetters, mapActions } from "vuex";

export default {
	name: "LowBalanceModal",

	mixins: [helpers],

	computed: {
		...mapGetters({
			lowBalanceDetails: 'getLowBalanceDetails',
			user: 'getUserDetails'
		}),

		daysLeft() {
			const { balanceWentZeroDate } = this.lowBalanceDetails;
			return 10 - (new Date().getDate() - new Date(balanceWentZeroDate).getDate())
		},

		rechargeDate() {
			const { balanceWentZeroDate } = this.lowBalanceDetails;
			let dateToRecharge = new Date(balanceWentZeroDate);
			return new Date(dateToRecharge.setDate(dateToRecharge.getDate() + 10))
				.toLocaleString('un-US', { month: 'long', day: 'numeric', year: 'numeric' });
		},

		getBalance() {
			const { Wallet } = this.user;
			return Wallet ? Wallet.Balance - Wallet.UnbilledUsage : 0;
		}
	},

	methods: {
		...mapActions([
			'toggleStatus'
		]),

		closeModal(e) {
			this.toggleStatusHandler(e, ['overlay', 'lowBalance']);
		},

		addMoneyHandler(e) {
			this.closeModal(e);
			const { Address, PhoneNumber } = this.user;
			Address && PhoneNumber
				? this.toggleStatusHandler(e, ['overlay', 'addmoney']) 
				: this.toggleStatusHandler(e, ['overlay', 'billingform']);
		}
	}
};
