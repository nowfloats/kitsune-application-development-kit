<template>
	<div class="stage5body-container">
		<div class="stage-information">
			<span class="stage-name dots-loading">optimizing files for site</span>
			<span class="stage-description">
				optimizing your files for faster performance and auto-SEO. to understand more about this process, please go to <a href="http://docs.kitsune.tools" target="_blank">docs.kitsune.tools</a>
			</span>
		</div>
		<div class="stage-progress">
			<div class="processed">
				<span>
					<span>minifying css</span>
					<span>
						<span>{{optimize.stylesOptimized}}</span>/<span>{{optimize.totalStyles}}</span>
					</span>
				</span>
				<ProgressBar :wrapperRequired="true" size="extra-thin" min="0" :max="optimize.totalStyles" :value="optimize.stylesProcessedPercentage"></ProgressBar>
			</div>
			<div class="processed">
				<span>
					<span>optimizing images</span>
					<span>
						<span>{{optimize.assetsOptimized}}</span>/<span>{{optimize.totalAssets}}</span>
					</span>
				</span>
				<ProgressBar :wrapperRequired="true" size="extra-thin" min="0" :max="optimize.totalAssets" :value="optimize.assetsProcessedPercentage"></ProgressBar>
			</div>
			<div class="processed">
				<span>
					<span>minifying js</span>
					<span>
						<span>{{optimize.scriptsOptimized}}</span>/<span>{{optimize.totalScripts}}</span>
					</span>
				</span>
				<ProgressBar :wrapperRequired="true" size="extra-thin" min="0" :max="optimize.totalScripts" :value="optimize.scriptsProcessedPercentage"></ProgressBar>
			</div>
			<div class="processed">
				<span>
					<span>extracting keywords</span>
					<span>
						<span>{{optimize.linksOptimized}}</span>/<span>{{optimize.totalLinks}}</span>
					</span>
				</span>
				<ProgressBar :wrapperRequired="true" size="extra-thin" min="0" :max="optimize.totalLinks" :value="optimize.linksProcessedPercentage"></ProgressBar>
			</div>
		</div>
	</div>
</template>

<script>
	import Stage5Body from './index'

	export default Stage5Body
</script>

<style lang="scss">
	@import '../../../../sass/components/migrationbuildcardsscss/stage5/body';
</style>
