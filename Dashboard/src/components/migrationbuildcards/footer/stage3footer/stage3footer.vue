<template>
	<div class="stage3footer-base-container">
		<ProgressBar size="extra-thin" min="0" max="100" :value="percentageCompleted"></ProgressBar>
		<div class="stage3footer-container">
			<span v-if="requiredPercentage">{{percentageCompleted}}%</span>
			<span v-else>&nbsp;</span>
		</div>
	</div>
</template>

<script>
	import Stage3footer from './index'

	export default Stage3footer
</script>

<style lang="scss">
	@import '../../../../sass/components/migrationbuildcardsscss/stage3/footer';
</style>
