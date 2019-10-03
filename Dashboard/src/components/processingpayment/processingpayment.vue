<template>
	<div class="processing-payment-base-container">
		<div class="processing-payment-container">
			<span class="pulsate" v-show="!getPaymentDetails.isDetailsreceived">₹</span>
			<span v-if="getPaymentDetails.isDetailsreceived && getPaymentDetails.isPaymentSuccessfull">payment successful</span>
			<span v-if="getPaymentDetails.isDetailsreceived && !getPaymentDetails.isPaymentSuccessfull">payment failed</span>
			<button class="btn kbtn"
				@click="toggleStatusHandler($event, ['overlay', 'processingpayment'])">ok
			</button>
		</div>
	</div>
</template>

<script>
	import processingPayment from  './index'

	export default processingPayment;
</script>

<style scoped lang="scss">
	@import '../../sass/components/processingpayment';
</style>
