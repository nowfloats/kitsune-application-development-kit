<template>
	<section class="upload-action">
		<progressBar size="very-thin"
								 min="0"
								 max="100"
								 :value="getValue">
		</progressBar>
		<div class="upload-info">
			<p>uploading ...</p>
			<p>{{getFileName}}</p>
		</div>
	</section>
</template>

<script>
	import projectFooterUpload from './index'

	export default projectFooterUpload
</script>

<style lang="scss">
	@import "../../../../../../sass/components/_projectFooterUpload.scss";
</style>
