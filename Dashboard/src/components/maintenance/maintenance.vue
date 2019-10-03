<template>
	<div class="credits-container">
		<div class="credits-header">
			<div class="kitsune-login"><span class="dashboard-info">DASHBOARD</span></div>
		</div>
		<div class="credits-footer">
			<div class="ocean">
				<div class="wave-1"></div>
				<div class="wave-2"></div>
				<div class="wave-3"></div>
				<div class="wave-4"></div>
			</div>
			<div class="description">
				<p class="title">{{getData.title}}</p>
				<p class="description">{{getData.description}}</p>
			</div>
			<!--<div class="credits">by logging in you agree to our terms and conditions</div>-->
		</div>
	</div>
</template>

<script>
	import googlesignin from './index'

	export default googlesignin;
</script>

<style lang="scss">
	@import '../../sass/components/_googlesignin.scss';
</style>
