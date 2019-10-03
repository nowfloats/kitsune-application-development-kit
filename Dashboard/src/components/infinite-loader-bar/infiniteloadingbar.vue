<template>
	<div class="loader-container">
		<div id="loginLoadingScreen" :class="['k-loading-screen', { 'extra-height' : extraHeight }]">
			<div :class="['k-loading-bar',{ 'extra-height' : extraHeight }]"></div>
		</div>
	</div>
</template>

<script>
	import InfiniteLoaderBar from './index'

	export default InfiniteLoaderBar
</script>

<style lang="scss">
	@import '../../sass/components/infiniteloaderbar';
</style>
