import { mapActions, mapGetters } from 'vuex';
import helpers from '../mixin';
import { anaMinBalance } from '../../../config/config';
import { stripePublicKey } from '../../../config/config';
import Vue from 'vue';
import VueStripeCheckout from 'vue-stripe-checkout';

Vue.use(VueStripeCheckout, stripePublicKey);

export default {
	name : 'addmoney',

	data() {
		return {
			amount : null,
			wrongInput : false,
			buttonText : 'pay',
			errorMessage : 'please enter valid amount',
			minimumAmountError : 'minimum amount is 10',
			minimumAnaAmount: `minimum amount is ${anaMinBalance}`,
			stripeKitBranding: 'https://www.getkitsune.com/assets/images/icons/kit-mascot.png',
			stripeCurrency: 'inr',
			currencyOptions: [
				{ text: 'Indian Rupee', value: 'inr' },
				{ text: 'Bangladesh Taka', value: 'bdt' },
				{ text: 'Singapore Dollars', value: 'sgd' },
				{ text: 'Thailand Baht', value: 'thb' },
				{ text: 'US Dollar', value: 'usd' }
			],
			currencySymbol: '&#8377;',
			stripeStatementDescriptor: 'wallet recharge',
			stripeKitsuneCompanyName: 'Kitsune Cloud Solutions'
		}
	},

	mixins: [helpers],

	computed: {
		...mapGetters({
			paymentDetails: 'paymentDetails',
			requestedForAna: 'requestedForAna',
			user: 'user',
			lowBalanceDetails: 'getLowBalanceDetails'
		}),

		getPaymentDetails() {
			return this.paymentDetails;
		},

		getRequiredBalance() {
			const { amount_to_add } = this.lowBalanceDetails;
			return amount_to_add ? amount_to_add : 1500;
		},

		getErrorMessage(){
			const { Wallet } = this.user.user;
			const balance = Wallet ? Wallet.Balance : 0;
			if(this.isNumber(this.amount)) {
				return this.requestedForAna ?
					(this.convertFormattedStringToNumber(this.amount) + balance) < anaMinBalance
						? this.minimumAnaAmount : ''
					: (this.convertFormattedStringToNumber(this.amount) < 10 ? this.minimumAmountError : '');
			}
			return this.errorMessage;
		},

		getAnaMinBalance: () => anaMinBalance.toLocaleString('en-IN')

	},

	methods: {
		...mapActions([
			'getPaymentRedirectLink',
			'toggleStatus',
			'toggleRequestedForAna',
			'processInternationalPayments'
		]),

		updpateUIForProcesssing() {
			this.buttonText = 'processing';
			this.wrongInput = false;
		},

		// TODO update for errors in api call or response
		addMoneyToAccount(){
			this.validateValue();
			if(!this.wrongInput){
				this.updpateUIForProcesssing();
				this.getPaymentRedirectLink(this.convertFormattedStringToNumber(this.amount));
			}
		},

		incrementValue(value) {
			this.amount = this.convertFormattedStringToNumber(this.amount) + value + '';
			this.amountKeyUpHandler();
		},

		validateValue() {
			const { Wallet } = this.user.user;
			const balance = Wallet ? Wallet.Balance : 0;
			const valid = this.isNumber(this.convertFormattedStringToNumber(this.amount)) && this.requestedForAna 
				? (this.convertFormattedStringToNumber(this.amount) + balance) >= anaMinBalance
				: this.convertFormattedStringToNumber(this.amount) >= 10;
			this.wrongInput = !valid;
		},

		isNumber: n => !isNaN(parseFloat(n)) && isFinite(n),

		keyUpHandler(event){
			if(event.keyCode === 27 ){
				this.closeAddMoneyModal(event);
			}
		},

		closeAddMoneyModal(e){
			this.toggleStatusHandler(e, ['overlay', 'addmoney']);
			this.toggleRequestedForAna(false);
		},

		amountKeyUpHandler() {
			this.validateValue();
			this.formatAmount();
		},

		formatAmount() {
			this.amount = this.convertFormattedStringToNumber(this.amount).toLocaleString('en-IN');
		},

		convertFormattedStringToNumber(val) {
			let formattedAmount = Number(val.toString().replace(/,/g, ''));
			return formattedAmount ? formattedAmount : 0;
		},

		async checkout () {
			// token - is the token object
			// args - is an object containing the billing and shipping address if enabled
			const { token, args } = await this.$refs.checkoutRef.open();
			console.log(token);
			console.log(args);
		},
		done ({ token, args }) {
			// token - is the token object
			// args - is an object containing the billing and shipping address if enabled
			// do stuff...
			console.log("browser checkout complete function START -----");
			this.updpateUIForProcesssing();
			this.processInternationalPayments({ amount: this.convertFormattedStringToNumber(this.amount), 
				stripe_token: token.id, currency: this.stripeCurrency });
			console.log("browser checkout complete function END -----");
		},
		opened () {
			// do stuff 
		},
		closed () {
		},
		canceled () {
			//handle cancel. Cancel also triggers after close.
		},
		currencyChanged(){
			if(this.stripeCurrency == 'usd')
				this.currencySymbol = '$';
			else if(this.stripeCurrency == 'inr')
				this.currencySymbol = '&#8377;';
			else if(this.stripeCurrency == 'bdt')
				this.currencySymbol = '&#2547;';
			else if(this.stripeCurrency == 'sgd')
				this.currencySymbol = 'S$';
			else if(this.stripeCurrency == 'thb')
				this.currencySymbol = '&#3647;';
		},
		getINRConvertedAmount(){
			var amountSet = this.convertFormattedStringToNumber(this.amount);
			if(this.stripeCurrency == 'usd')
				return Math.round(amountSet*69);
			else if(this.stripeCurrency == 'bdt')
				return Math.round(amountSet*0.8);
			else if(this.stripeCurrency == 'sgd')
				return Math.round(amountSet*50);
			else if(this.stripeCurrency == 'thb')
				return Math.round(amountSet*2);
		},
		getINRConversionRateText(){
			if(this.stripeCurrency == 'usd')
				return 'USD 1 = INR 69';
			else if(this.stripeCurrency == 'bdt')
				return 'BDT 1 = INR 0.80';
			else if(this.stripeCurrency == 'sgd')
				return 'SGD 1 = INR 50';
			else if(this.stripeCurrency == 'thb')
				return 'THB 1 = INR 2';
		}
	},

	created(){
		window.addEventListener('keyup',this.keyUpHandler);
	},

	mounted() {
		this.amount = this.getRequiredBalance;
		this.formatAmount();
	},

	destroyed(){
		window.removeEventListener('keyup',this.keyUpHandler);
	}
}
