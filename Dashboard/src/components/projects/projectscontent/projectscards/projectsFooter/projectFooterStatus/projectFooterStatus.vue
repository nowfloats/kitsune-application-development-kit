<template>
	<div :class="['project-footer-status-container', { 'border-top' : isIdentifyingDomainName }]">
		<infiniteLoader v-if="!isIdentifyingDomainName" class="download" :extraHeight="true"></infiniteLoader>
		<span :class="{'process-name-center': !isRequired}" class="process-name dots-loading">{{getProjectStatus}}</span>
		<span v-if="isRequired" class="stage-name">{{getProcessStage}}</span>
		<button v-if="isRequired && !isQueued"
						@click="maximizeMigration"
						class="btn kbtn-primary maximize">
			view details
		</button>
	</div>
</template>

<script>
	import projectFooterStatus from  './index'

	export default projectFooterStatus;
</script>

<style lang="scss">
	@import '../../../../../../sass/components/projectFooterStatus';
</style>
