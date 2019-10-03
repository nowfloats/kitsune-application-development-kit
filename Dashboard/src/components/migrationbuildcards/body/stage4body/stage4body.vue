<template>
	<div class="stage4body-container">
		<div class="stage-information">
			<span class="stage-name dots-loading">inserting placeholders</span>
			<span class="stage-description">
				optimizing your files for faster performance and auto-SEO. to understand more about this process, please go to <a href="http://docs.kitsune.tools" target="_blank">docs.kitsune.tools</a>
			</span>
		</div>
		<div class="stage-progress">
			<div v-if="isReplacerRequired" class="processed">
				<span>
					<span>replacing</span>
					<span>
						<span>{{getLinksReplaced}}</span>/<span>{{getTotalLinks}}</span>
					</span>
				</span>
				<ProgressBar :wrapperRequired="true" size="extra-thin" min="0" :max="getTotalLinks" :value="getLinksReplacedPercentage"></ProgressBar>
			</div>
			<div class="processed">
				<span>
					<span>analyzing</span>
					<span>
						<span>{{getFilesAnalyzed}}</span>/<span>{{getTotalFilesToAnalyze}}</span>
					</span>
				</span>
				<ProgressBar :wrapperRequired="true" size="extra-thin" min="0" :max="getTotalFilesToAnalyze" :value="getFilesAnalyzedPercentage"></ProgressBar>
			</div>
		</div>
	</div>
</template>

<script>
	import Stage4Body from './index'

	export default Stage4Body;
</script>

<style lang="scss">
	@import '../../../../sass/components/migrationbuildcardsscss/stage4/body';
</style>
