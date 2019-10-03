<template>
	<div class="publish-base-container" @click.self="closeAll($event)">
		<PublishLoader v-if="showLoader" :projectDetails="projectDetails" ></PublishLoader>
		<Publishstage0  v-if="getStage(0)" :projectDetails="projectDetails"></Publishstage0>
		<CreateCustomer v-if="getStage(1)" :projectDetails="projectDetails"></CreateCustomer>
		<ConfirmPublish v-if="getStage(2)" :projectDetails="projectDetails"></ConfirmPublish>
		<chooseCloud v-if="getStage(3)" :projectDetails="projectDetails"></chooseCloud>
		<cloudParam v-if="getStage(4)"></cloudParam>
		<domainDetails v-if="getStage(5)" :projectDetails="projectDetails"></domainDetails>
		<gcpUpload v-if="getStage(6)" :projectDetails="projectDetails"></gcpUpload>
	</div>
</template>

<script>
	import PublishProject from './index'

	export default PublishProject
</script>

<style lang="scss">
	@import '../../sass/components/publish/basepublish';
</style>
