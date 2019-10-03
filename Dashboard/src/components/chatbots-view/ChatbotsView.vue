<template>
	<div class="project-components">
		<div v-if="hasBots">
			<div class="sticky-title">
				<ProjectsHeader isChatBots="true"></ProjectsHeader>
			</div>
			<div class="project-main-content container-fluid">
				<chatbots></chatbots>
			</div>
		</div>
		<zerothCase v-else name="chatbots"></zerothCase>
	</div>
</template>

<script>
	import ProjectLevelView from './index';

	export default ProjectLevelView;
</script>

<style scoped lang="scss">
	@import '../../sass/components/projects';
</style>
