<template>
				<div class="projects-content container-fluid project-main-content" id=projects-container>
					<transition-group tag="div" class="list row" name="list">
							<projectsCards
								v-show="hasProjects"
								class="list-item"
								v-for="project in searchResult"
								:key="project.ProjectId"
								:projectName="project.ProjectName"
								:projectId="project.ProjectId"
								:createdOn="project.CreatedOn"
								:projectStatus="project.ProjectStatus"
								:projectType = "project.ProjectType"
								:screenShotUrl="project.ScreenShotUrl"
								:version="project.Version"
								:isProjectsMenu="true"
								:customerId="project.WebsiteId"
								:isContextMenuRequired="true"
								:isHidden="project.isHidden"
								:projectStage="project.ProjectStage">
							</projectsCards>
					</transition-group>
					<div v-if="lazyLoad.showLoader" class="lazyLoader">
						<img src="../../../assets/icons/coffee-cup.svg" alt="loader icon">
						<p>loading more ...</p>
					</div>
					<zerothCase v-show="!hasProjects" name="projects" action="create one"></zerothCase>
				</div>
</template>

<script>
	import projectscontent from './index'

	export default projectscontent
</script>

<style lang="scss">
	@import "../../../sass/components/projectscontent";
</style>
