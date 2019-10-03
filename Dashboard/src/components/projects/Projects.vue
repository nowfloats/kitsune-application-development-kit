<template>
		<div class="project-components">
			<div v-if="hasProjects && !componentStatus.skeletonLoader">
				<projectsHeader :isLiveSites="false"></projectsHeader>
				<div>
					<projectsContent></projectsContent>
				</div>
			</div>
			<zerothCase v-if="!hasProjects && !componentStatus.skeletonLoader"
									name="projects">
			</zerothCase>
			<skeletonLoader v-if="componentStatus.skeletonLoader"></skeletonLoader>
		</div>
</template>

<script>
	import projects from './index.js'

	export default projects;
</script>

<style lang="scss">
	@import "../../sass/components/_projects.scss";
</style>
