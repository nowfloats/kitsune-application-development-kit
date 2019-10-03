<template>
		<div class="project-item-container col-xs-12 col-lg-3 col-md-4 col-sm-6"
				 v-show="!isHidden">
			<div :class="['project-item', { 'cursor-auto' : !isProjectIdle }]">
				<div :title="!isProjectIdle ? 'please wait while the crawl is in progress' : 'edit in IDE'"
					v-tippy="{ size: 'small' }">
					<div @click="openProjectInIDE" class="project-image">
						<img class="blur-image" :src="ImageLink">
						<img class="real-image" :src="ImageLink">
						<div class="overlay"></div>
					</div>
					<p @click="openProjectInIDE"
						 :class="['project-name', { 'cursor-auto' : !isProjectIdle }]">{{projectName}}</p>
				</div>
			<projectsFooter v-if="!isChatBot"
				:isProjectsMenu="isProjectsMenu"
											:createdOn="createdOn"
											:projectId="projectId"
											:projectName="projectName"
											:projectType="projectType"
											:isContextMenuRequired="isContextMenuRequired"
											:kitsuneUrl="kitsuneUrl"
											:domainVerified="domainVerified"
											:version="version"
											:projectStatus="projectStatus"
											:customerId="customerId"
											:projectStage="projectStage"
			@OpenIDEEvent="openProjectInIDE">
			</projectsFooter>
			</div>
		</div>
</template>

<script>
	import projectscards from './index'

	export default projectscards
</script>

<style scoped lang="scss">
		@import '../../../../sass/components/projectscards';
</style>
