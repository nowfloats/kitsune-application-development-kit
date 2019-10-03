<template>
	<div>
		<div class="account-cards row">
			<div class="account-cards-wrapper col-md-4">
				<div @click="changeSettingsComponent('account')"
						 class="settings-card profile">
					<div class="settings-card-content profile">
						<div class="user-details">
							<p>{{user.DisplayName}}</p>
							<p>{{user.Email}}</p>
						</div>
						<img :src="user.ProfilePic" alt="user profile pic">
					</div>
					<div class="settings-card-footer">
						<span>account settings</span>
					</div>
				</div>
			</div>
			<div class="account-cards-wrapper col-md-4">
				<div class="settings-card coming-soon">
					<div class="settings-card-content">
						<p>team members</p>
					</div>
					<div class="settings-card-footer coming-soon">
						<span>coming soon</span>
					</div>
				</div>
			</div>
			<div class="account-cards-wrapper col-md-4">
				<div class="settings-card coming-soon">
					<div class="settings-card-content">
						<p>
							alerts and notification
						</p>
					</div>
					<div class="settings-card-footer coming-soon">
						<span>coming soon</span>
					</div>
				</div>
			</div>
		</div>
		<div class="account-activities coming-soon">
			<div class="header">
				<span>latest 5 activities</span>
			</div>
			<div class="content">
				coming soon
			</div>
		</div>
	</div>
</template>

<script>
	import defaultComponent from './index'

	export default defaultComponent
</script>

<style scoped lang="scss">
	@import '../../../sass/components/_settingCards';
</style>
