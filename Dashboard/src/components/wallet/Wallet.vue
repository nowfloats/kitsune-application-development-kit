<template>
	<section class="add-money-section" v-if="!isBalanceInfinity">
		<span class="text-white">
			<!--TODO currency-->
			<span>INR</span>
			<span>{{getFormattedBalance}}</span>
		</span>
		<button @click="addMoneyAction($event)">ADD MONEY</button>
	</section>
</template>

<script>
	import wallet from './index'

	export default wallet
</script>

<style scoped lang="scss">
	@import "../../sass/components/wallet";
</style>
