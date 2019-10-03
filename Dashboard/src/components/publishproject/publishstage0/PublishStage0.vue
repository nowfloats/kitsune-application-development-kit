<template>
	<div class="publish-stage0-container container-fluid">
		<PublishingStage0Header :projectName="projectDetails.projectName"></PublishingStage0Header>
		<transition  appear
								appear-active-class="animated customSlideInRight">
			<PublishingStage0Body :projectDetails="projectDetails"></PublishingStage0Body>
		</transition>
	</div>
</template>

<script>
	import Publishstage0 from './index'

	export default Publishstage0;
</script>

<style lang="scss">
	@import '../../../sass/components/publish/stage0';
</style>
