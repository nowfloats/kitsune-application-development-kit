<template>
	<div class="base-modal-container">
		<div class="low-bal-body">
			<div class="low-bal-header">
				<div class="close-icon" @click="closeModal($event)"></div>
				<img src="../../assets/icons/wallet.svg" alt="edit icon">
				<section>
					<p class="header">recharge</p>
					<p class="subheader">an empty wallet could cost you.</p>
				</section>
			</div>
			<div class="low-bal-content">
				<p>
					<span>&#8377;</span>
					<span>{{getBalance}}</span>
				</p>
				<div class="low-bal-content-info">
					<p>your current balance has gone below zero. to keep your sites(s) from being deactivated
						in the next {{daysLeft}} days, you'll need to recharge by {{rechargeDate}}</p>
					<button class="btn kbtn-primary" @click="addMoneyHandler">ADD MONEY</button>
				</div>
			</div>
		</div>
	</div>
</template>

<script>
import LowBalanceModal from './index';

export default LowBalanceModal;
</script>

<style lang="scss">
@import "../../sass/components/lowBalance";
</style>
