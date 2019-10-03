<template>
	<section class="crawl"
					 @dragover.prevent.capture="activateDragEffect"
					 @dragleave.prevent.capture="deactivateDragEffect"
					 :class="{ 'active' : isActionInputInFocus }">
		<fieldset class="field-set">
			<legend class="legend">crawl and optimise an existing website</legend>
			<div class="url-enter">
				<button :class="['url-submit', { 'valid': isDomainNameValid && defaultButtonVisibility } ]" @click="startCrawl">start</button>
				<input type="text"
							 spellcheck="false"
							 autofocus
							 tabindex="1"
							 placeholder="http://www.yourwebsite.com"
							 @focus="callToggleContainerActiveClass(true)"
							 @blur="callToggleContainerActiveClass(false)"
							 v-model.trim="projectName"
							 ref="websiteName"
							 v-model="projectName"
							 @keyup="validateDomainName"
							 @keyup.enter="startCrawl($event)">
				<span :class="['floating-label', { 'appear':projectName.trim()!='' }]">your site url</span>
				<span :class="['project-name-error',{ 'appear': !isDomainNameValid }]">enter valid site name</span>
			</div>
			<p>- or -</p>
			<span class="upload-wrapper"
						@drop="dragAndDropHandler"
						@mouseenter="activateHoverEffect"
						@mouseleave="deactivateHoverEffect">
		<file-upload :class="['drag-drop-container', { 'on-hover-effect': isHoverEffectActive }]"
								 @input="inputFile"
								 ref="upload"
								 :directory=true
								 :multiple="true"
								 :drop="true"
								 v-model="uploadFiles"
								 put-action=""
								 post-action="">
				click or drag your website folder
			</file-upload>
		</span>
		</fieldset>
	</section>
</template>

<script>
	import Crawl from './index'

	export default Crawl
</script>

<style scoped lang="scss">
	@import "../../../sass/components/crawl";
</style>
