<template>
	<div>
		<div v-if="!isAnaAccountPresent" class="zeroth-projects">
			<div class="zeroth-projects-button">
				<p class="zerothcase-title">build a chatbot project</p>
				<span class="zeroth-subtitle">click on&nbsp;</span>
				<button class="zeroth-create-button" @click="createNewProject"></button>
				<span class="zeroth-subtitle">&nbsp;at the
					<span v-if="changeMessage">bottom</span>
					<span v-else>top</span>
					-right to get started.
				</span>
			</div>
		</div>
		<div v-else class="zeroth-projects">
			<div class="zeroth-projects-button">
				<p class="zerothcase-title">create your first chatbot project</p>
				<span class="zeroth-subtitle">click
			<a class="ana-link" href="https://www.ana.chat/" target="_blank">here</a>
			to get started
			</span>
			</div>
		</div>
	</div>
</template>

<script>
	import zerothChatbots from './index';

	export default zerothChatbots;
</script>

<style scoped lang="scss">
	@import "../../../sass/components/zerothProjects";
</style>
