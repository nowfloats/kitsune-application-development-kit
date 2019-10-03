<template>
	<div class="stage6body-container">
		<span class="stage-description dots-loading">generating kitsune html</span>
		<div class="stage-progress">
			<span class="information">
				<span>building pages</span>
				<span>{{replacer.totalFilesReplaced}}/{{replacer.totalFilesToBeReplaced}}</span>
			</span>
			<ProgressBar :wrapperRequired="true" size="extra-thin" min="0" :max="replacer.totalFilesToBeReplaced" :value="replacer.filesReplacedPercentage"></ProgressBar>
		</div>
	</div>
</template>

<script>
	import Stage6Body from './index'

	export default Stage6Body
</script>

<style lang="scss">
	@import '../../../../sass/components/migrationbuildcardsscss/stage6/body';
</style>
