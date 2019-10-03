import { mapGetters, mapActions } from "vuex"
import helpers from '../mixin'

export default {
	name: 'wallet',

	computed: {
		...mapGetters({
			user: 'getUserDetails',
			userBalance: 'getUserNetBalance',
			componentStatus: 'componentStatus'
		}),

		isBalanceInfinity() {
			return this.userBalance === Infinity
		},
		//for future reference
		/*balance(){
			let balance = this.userBalance;
			return (balance > 0) ? balance : 0.00;
		},*/

		getFormattedBalance(){
			return this.userBalance.toLocaleString('en-IN', { minimumFractionDigits: 2 } );
		}
	},

	mixins: [helpers],

	methods: {
		...mapActions([
			'toggleStatus'
		]),
		addMoneyAction(e) {
			// this.$ga.event('button', 'click', 'add-money');
			const { Address, PhoneNumber } = this.user;
			if(this.componentStatus.profile) {
				this.toggleStatusHandler(e, ['overlay', 'profile']);
			}
			if(Address && PhoneNumber) {
				this.toggleStatusHandler(e, ['overlay','addmoney']);
			} else {
				this.toggleStatusHandler(e, ['overlay','billingform']);
			}
		}
	}
}
