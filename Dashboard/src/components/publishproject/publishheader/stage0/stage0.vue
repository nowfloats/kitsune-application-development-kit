<template>
	<div class="k-header">
		<div :class="['k-title', { 'create-customer' : isCreateCustomer }]">
			<span class="modal-heading">{{getHeaderValue}}</span>
			<span class="project-name" v-if="!isProjectName">'{{getProjectName}}'</span>
		</div>
		<button @click="toggleStatusHandler($event, ['overlay','publishproject'])"></button>
	</div>
</template>

<script>
	import PublishingStage0Header from './index'

	export default PublishingStage0Header
</script>

<style lang="scss">
	@import '../../../../sass/components/publish/stage0header';
</style>
