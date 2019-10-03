<template>
	<div class="stage3body-conatiner">
		<div class="stage-information">
			<span class="stage-name dots-loading">importing files from site</span>
			<span class="stage-description">migrating assets to your new host, and setting up for optimization.</span>
		</div>
		<div class="stage-progress">
			<div class="processed">
				<span>
					<span>styles</span>
					<span>
						<span>{{downloadDetails.stylesDownloaded}}</span>/<span>{{downloadDetails.stylesFound}}</span>
					</span>
				</span>
				<ProgressBar :wrapperRequired="true" size="extra-thin" min="0" :max="downloadDetails.stylesFound" :value="downloadDetails.stylesPercentage"></ProgressBar>
			</div>
			<div class="processed">
				<span>
					<span>assets</span>
					<span>
						<span>{{downloadDetails.assetsDownloaded}}</span>/<span>{{downloadDetails.assetsFound}}</span>
					</span>
				</span>
				<ProgressBar :wrapperRequired="true" size="extra-thin" min="0" :max="downloadDetails.assetsFound" :value="downloadDetails.assetsPercentage"></ProgressBar>
			</div>
			<div class="processed">
				<span>
					<span>scripts</span>
					<span>
						<span>{{downloadDetails.scriptsDownloaded}}</span>/<span>{{downloadDetails.scriptsFound}}</span>
					</span>
				</span>
				<ProgressBar :wrapperRequired="true" size="extra-thin" min="0" :max="downloadDetails.scriptsFound" :value="downloadDetails.scriptsPercentage"></ProgressBar>
			</div>
		</div>
	</div>
</template>

<script>
	import Stage3Body from './index'

	export default Stage3Body
</script>

<style lang="scss">
	@import "../../../../sass/components/migrationbuildcardsscss/stage3/body";
</style>
