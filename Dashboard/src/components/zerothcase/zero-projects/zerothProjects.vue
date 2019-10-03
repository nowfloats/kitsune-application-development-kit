<template>
	<div class="zeroth-projects">
		<div class="zeroth-projects-button">
			<p class="zerothcase-title">create your first #serverless app</p>
			<span class="zeroth-subtitle">click on&nbsp;</span>
			<button class="zeroth-create-button" @click="createNewProject"></button>
			<span class="zeroth-subtitle">&nbsp;at the <span v-if="changeMessage">bottom</span>
				<span v-else>top</span>-right to get started.</span>
		</div>
	</div>
</template>

<script>
	import zerothProjects from './index'

	export default zerothProjects
</script>

<style scoped lang="scss">
	@import "../../../sass/components/_zerothProjects.scss";
</style>
