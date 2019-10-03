<template>
	<section class="upload-section action-wrapper active">
		<section :class="['uploadInputSection', {'drag-active': addDragOverClass}]"
						 @dragover.prevent.capture="dragOverClassHandler"
						 @dragleave.prevent.capture="dragLeaveClassHandler"
						 @drop.capture="dragAndDropHandler">
			<file-upload class="upload-container"
									 ref="upload"
									 @input="inputFolder"
									 :directory=true
									 :multiple="true"
									 :drop="true"
									 v-model="newFiles"
									 put-action=""
									 post-action="">
				<span>drag your folder to upload it to</span>
				<p class="upload-project-name">{{this.uploadOnProjectDetails.projectName}}</p>
			</file-upload>
		</section>
		<section :class="['select-section', {'select-hide': addDragOverClass}]">
			<p><span class="underline"
							 data-toggle="tooltip"
							 data-placement="bottom"
							 data-container=".underline"
							 :title="recommendationMsg"
							 v-tippy="{ size: 'small', placement: 'bottom', arrow: true, arrowType: 'round' }">
				we recommend
			</span> drag and drop, however you can also select your folder</p>
			<div>
				<file-upload class="select-folder"
										 ref="upload"
										 @input="inputFolder"
										 :directory=true
										 :multiple="true"
										 v-model="newFiles"
										 put-action=""
										 post-action="">
					select folder
				</file-upload>
			</div>
		</section>
	</section>
</template>

<script>
	import upload from "./index"

	export default upload
</script>

<style scoped lang="scss">
	@import "../../../sass/components/upload";
</style>
