<template>
	<div class="projects-content row">
		<ProjectsCards v-for="project in getActiveLiveSites"
									 :projectName="project.WebsiteUrl"
									 :projectId="project.ProjectId"
									 :createdOn="project.CreatedOn"
									 :projectStatus="project.ProjectStatus"
									 :screenShotUrl="project.ScreenShotUrl"
									 :kitsuneUrl="project.WebsiteUrl"
									 :domainVerified="false"
									 :isContextMenuRequired="true"
									 :customerId="project.WebsiteId"
									 :isProjectsMenu="false">
		</ProjectsCards>
	</div>
</template>

<script>
	import LiveSites from './index'

	export default LiveSites;
</script>

<style lang="scss">
	@import '../../../sass/components/projectscontent';
</style>
