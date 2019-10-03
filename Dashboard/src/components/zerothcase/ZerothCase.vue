<template>
		<section class="zerothcase">
			<zerothProjects v-if="name === 'projects'"></zerothProjects>
			<zerothLiveSites v-if="name === 'live-sites'"></zerothLiveSites>
			<zerothChatbots v-if="name === 'chatbots'"></zerothChatbots>
		</section>
</template>

<script>
	import zerothcase from './index';

	export default zerothcase;
</script>

<style scoped lang="scss">
	@import "../../sass/components/zerothcase";
</style>

