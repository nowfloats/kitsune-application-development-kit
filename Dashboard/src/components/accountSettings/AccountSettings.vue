<template>
	<div class="account">
		<div class="sticky-title">
			<h2>
				<span :class="{ 'active': !checkRouteForBreadCrumbs}"
							class="sticky-title-text"
							@click.self="goBackToDefault('default')">settings</span>
			<span :class="{ 'active': checkRouteForBreadCrumbs}"
						class="sticky-title-text"
						v-if="checkRouteForBreadCrumbs">
				<i class="next"></i>{{subRouteName}}
			</span>
			</h2>
		</div>
		<div :class="{ 'activeLoader': updateStatus }" class="account-content wrapper">
			<!--<editAccount v-if="component === 'account'"></editAccount>-->
			<router-view></router-view>
		</div>
	</div>
</template>

<script>
	import accountSettings from './index'

	export default accountSettings
</script>

<style scoped lang="scss">
	@import "../../sass/components/accountSettings";
</style>
