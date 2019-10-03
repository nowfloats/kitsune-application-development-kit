<template>
	<div class="publish-loading-container">
		<PublishHeader></PublishHeader>
		<PublishLoaderBody :projectDetails="projectDetails"></PublishLoaderBody>
	</div>
</template>

<script>
	import LoadingPublish from './index'

	export default LoadingPublish
</script>

<style lang="scss">
	@import '../../../sass/components/publish/loading';
</style>
