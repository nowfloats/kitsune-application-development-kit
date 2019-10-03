<template>
	<section class="start-blank"
					 :class="{ 'active' : isActionInputInFocus }">
		<fieldset class="field-set">
			<legend class="legend">
				<span v-if="!willProjectUpdate">build a serverless-app project</span>
				<span v-else>update project</span>
			</legend>
			<section class="inputSection">
				<input type="text"
							 :class="['project-name', { 'error': projectError.regex }]"
							 v-model="project.name"
							 @keyup="validateProjectName"
							 ref="projectName"
							 tabindex="2"
							 :disabled="willProjectUpdate"
							 placeholder="project name*"
							 spellcheck="false"
							 @focus="callToggleContainerActiveClass(true)"
							 @blur="callToggleContainerActiveClass(false)"
							 @keyup.enter="createBlankProject">
				<div v-if="!componentStatus.hamburger" :class="['alert-user',{ 'active': displayAlert }]">
					Is this the root folder you selected? if not, we suggest going back and drag&amp;drop your folder. (
					<span class="underline"
								data-toggle="tooltip"
								data-placement="bottom"
								data-container=".underline"
								:title="recommendationMsg"
								v-tippy="{ size: 'small', placement: 'bottom', arrow: true, arrowType: 'round' }">why?
				</span>)
					<button @click="changeDisplayAlert" class="close-recommend"></button>
				</div>
				<span v-if="projectError.regex"
							class="projectError">
				special characters not allowed in the first word
			</span>
				<span v-if="projectError.empty && !projectError.regex"
							class="projectError">Please enter a project name</span>
				<span :class="['fake-placeholder', { 'activePlaceholder': project.name !== '' }]">
				<span v-if="willProjectUpdate">uploading</span>
				<span v-else>project name</span>
			</span>
			</section>
			<section class="inputSection" >
				<input type="text"
							 v-if="!willProjectUpdate"
							 class="project-desc"
							 tabindex="3"
							 v-model="project.url"
							 spellcheck="false"
							 placeholder="few words about this project"
							 @focus="callToggleContainerActiveClass(true)"
							 @blur="callToggleContainerActiveClass(false)">
				<input type="text"
							 v-else
							 disabled
							 class="project-desc"
							 spellcheck="false"
							 :value="uploadOnProjectDetails.projectName">
				<span :class="['fake-placeholder', { 'activePlaceholder': willProjectUpdate }]">t0 project</span>
			</section>
			<a v-if="!loader" class="start-build"
				 @click="createBlankProject">
				<span v-if="uploadFiles.length === 0 && !willProjectUpdate">create and start building</span>
				<span v-if="uploadFiles.length !== 0">
				<span v-if="willProjectUpdate">start uploading</span>
				<span v-else>create and start uploading</span>
			</span>
			</a>
			<p v-if="loader" class="loader-container">
				<threeDots></threeDots>
			</p>
		</fieldset>
	</section>
</template>

<script>
	import blank from "./index"

	export default blank
</script>

<style scoped lang="scss">
	@import "../../../sass/components/blank";
</style>
