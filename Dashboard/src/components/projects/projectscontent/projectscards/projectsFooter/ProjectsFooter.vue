<template>
	<section class="project-card-footer">
		<section class="project-status" v-if="!isProjectIdle && isProjectsMenu">
			<projectFooterStatus :projectStatus="projectStatus"
													 :projectName="projectName"
													 :projectId="projectId"
													 :projectType="projectType"
													 :projectStage="projectStage">
			</projectFooterStatus>
		</section>
		<section v-if="!showAlert && isProjectIdle">
			<projectFooterAction :projectId="projectId"
													 v-if="!details.isUploading"
													 :createdOn="createdOn"
													 :projectName="projectName"
													 :isProjectsMenu="isProjectsMenu"
													 :isContextMenuRequired="isContextMenuRequired"
													 :customerId="customerId"
													 :version="version"
													 :domainVerified="domainVerified"
													 @OpenIDEEvent="openIDEInParent"
													 :kitsuneUrl="kitsuneUrl">
			</projectFooterAction>
			<projectFooterUpload v-else :details="details"></projectFooterUpload>
		</section>
		<uploadAlert v-if="showAlert && isProjectIdle" :details="details"></uploadAlert>
	</section>
</template>

<script>
	import projectsFooter from './index'

	export default projectsFooter
</script>

<style lang="scss">
	@import "../../../../../sass/components/projectsFooter";
</style>
