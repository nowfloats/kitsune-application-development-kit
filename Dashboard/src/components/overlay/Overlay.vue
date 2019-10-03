<template>
	<div class="overlay"
			 @click="toggleStatusHandler($event, getOverlayNames)">
	</div>
</template>

<script>
	import overlay from './index'

	export default overlay
</script>

<style lang="scss">
	@import '../../sass/components/_overlay.scss';
</style>
