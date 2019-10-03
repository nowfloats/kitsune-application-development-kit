<template>
	<div class="base-modal-container">
		<div class="add-money-body">
			<div class="add-money-header">
				<div class="close-icon" @click="closeAddMoneyModal($event)"></div>
				<img src="../../assets/icons/wallet.svg" alt="edit icon">
				<section>
					<p class="header">recharge your kitsune wallet</p>
				</section>
			</div>
			
			<div class="add-money-content">
				<div class="info" style="justify-content:flex-end;">
					<p style="margin-right:0"> Currency: 
						<select v-model="stripeCurrency" @change="currencyChanged">
							<option v-for="option in currencyOptions" v-bind:value="option.value">
								{{ option.text }}
							</option>
						</select>
					</p>
				</div><br/>
				<div class="input">
					<span v-html="currencySymbol"></span>
					<input class="k-form-field form-control" type="text"
								 v-model="amount"
								 @keyup="amountKeyUpHandler"
								 @keyup.enter="addMoneyToAccount"
								 :disabled="getPaymentDetails.isInstamojoPaymentLinkRequested"/>
					<span :title="getErrorMessage"
								data-toggle="tooltip"
								:class="[ 'error-image', { 'active' : wrongInput } ]"></span>
				</div>
				<div class="pre-defined">
					<p @click="incrementValue(100)"><span v-html="currencySymbol"></span><span>100</span></p>
					<p @click="incrementValue(500)"><span v-html="currencySymbol"></span><span>500</span></p>
					<p @click="incrementValue(1000)"><span v-html="currencySymbol"></span><span>1,000</span></p>
					<p @click="incrementValue(10000)"><span v-html="currencySymbol"></span><span>10,000</span></p>
				</div>
				<div class="info" v-if="requestedForAna">
					<p>your wallet needs to have a minimum of <span v-html="currencySymbol"></span>{{getAnaMinBalance}} for registering to ana cloud.</p>
					<button @click="addMoneyToAccount"
									:class="[ 'btn kbtn-primary']"
									:disabled="getPaymentDetails.isInstamojoPaymentLinkRequested || wrongInput">
						{{buttonText}}
					</button>
				</div>
				<div class="info" v-else>
					
					<p><span v-if="currencySymbol != '&#8377;'"><u>Based on the conversion rate ({{getINRConversionRateText()}}), INR {{getINRConvertedAmount()}} will get added to your kitsune wallet.</u></span><br/><br/>
					we suggest you to add minimum of <span v-html="currencySymbol"></span>{{getRequiredBalance}} to keep your account running soothly for this billing cycle.
						however you're free to add any amount starting from <span v-html="currencySymbol"></span>10.</p>
					<div>
						<vue-stripe-checkout
							ref="checkoutRef"
							:image="stripeKitBranding"
							:name="stripeKitsuneCompanyName"
							:description="stripeStatementDescriptor"
							:currency="stripeCurrency"
							:amount="convertFormattedStringToNumber(amount)*100"
							:allow-remember-me="false"
							@done="done"
							@opened="opened"
							@closed="closed"
							@canceled="canceled">
						</vue-stripe-checkout>
						<button @click="checkout"
								:class="['btn kbtn-primary']"
								:disabled="wrongInput">
							{{buttonText}}
						</button>
					</div>
					<!-- <button @click="addMoneyToAccount"
									:class="[ 'btn kbtn-primary']"
									:disabled="getPaymentDetails.isInstamojoPaymentLinkRequested || wrongInput">
						{{buttonText}}
					</button> -->
				</div>
			</div>
		</div>
	</div>
</template>

<script>
	import addMoney from './index'

	export default addMoney;
</script>

<style scoped lang="scss">
	@import "../../sass/components/addmoney";
	@import "../../sass/components/form";
</style>
