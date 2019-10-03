<template>
	<div class="account-info">
		<section class="account-header">
			<div class="profile-pic">
				<img :src="user.ProfilePic" alt="user profile pic">
			</div>
			<div class="details">
				<p>{{user.DisplayName}}</p>
				<p>{{user.Email}}</p>
			</div>
			<div class="add-money-container">
				<span class="text-white">
					<!--TODO currency-->
					<span>INR</span>
					<span>{{user.Wallet.Balance}}</span>
				</span>
				<button>ADD MONEY</button>
			</div>
		</section>
	</div>
</template>

<script>
	import Account from './index'

	export default Account
</script>

<style scoped lang="scss">
	@import "../../sass/components/accountInfo";
</style>
