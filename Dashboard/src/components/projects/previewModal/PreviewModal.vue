<template>
	<div class="modal-base-container" @click.self="closePreviewProjectModal($event)">
		<div class="k-modal-container preview-modal container-fluid">
			<!--<div class="k-modal-header">
				<div class="k-modal-title">
					preview project
				</div>
				<button @click="closePreviewProjectModal($event)"></button>
			</div>-->
			<div class="k-header-preview">
				<div class="k-title">
					<span class="modal-heading">preview project</span>
					<span class="project-name">"{{previewProject.projectName}}"</span>
				</div>
				<button @click="closePreviewProjectModal($event)"></button>
			</div>
			<div>
				<div class="k-modal-content preview-modal-content">
					<multiselect :options="getCustomerList"
												:show-labels=false
												@input="generatePreviewLink"
												placeholder="select customer"
											 label="CustomerName"
												v-model="customer">
					</multiselect>
				</div>
				<div class="k-modal-footer">
					<button class="btn" @click="previewProjectInNewTab($event)" :disabled="disableButton">preview</button>
				</div>
			</div>
		</div>
	</div>
	<!--<div class="publish-stage0-container container-fluid">
		<PublishingStage0Header :projectName="projectDetails.projectName"></PublishingStage0Header>
		<transition  appear
								 appear-active-class="animated customSlideInRight">
			<PublishingStage0Body :projectDetails="projectDetails"></PublishingStage0Body>
		</transition>
	</div>-->
</template>

<script>
	import PreviewProjectModal from './index'

	export default PreviewProjectModal;
</script>

<style lang="scss">
	@import '../../../sass/components/previewModal';
</style>
