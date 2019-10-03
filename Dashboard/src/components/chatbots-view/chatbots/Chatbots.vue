<template>
	<div class="projects-content row">
		<ProjectsCards v-for="bot in getBots"
									 :projectName="bot.Name"
									 projectId=""
									 createdOn=""
									 isChatBot="true"
									 projectStatus=""
									 screenShotUrl=""
									 kitsuneUrl=""
									 domainVerified=""
									 isContextMenuRequired="false"
									 customerId="bot.businessId"
									 isProjectsMenu="false">
		</ProjectsCards>
	</div>
</template>

<script>
	import chatbots from './index';

	export default chatbots;
</script>

<style scoped lang="scss">
	@import '../../../sass/components/projectscontent';
</style>
