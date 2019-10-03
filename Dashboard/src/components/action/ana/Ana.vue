<template>
	<section :class="['start-blank',{ 'active' : isActionInputInFocus }]">
		<fieldset class="field-set">
			<legend v-if="!isAnaAccountPresent" class="legend">build a chatbot project</legend>
			<legend v-else class="legend">build a chatbot project</legend>
			<section class="inputSection">
				<input type="text"
							 :class="['project-name', { 'error': accountError.regex }]"
							 v-model="account.Name"
							 ref="projectName"
							 disabled
							 tabindex="4"
							 placeholder="subscription account name*"
							 spellcheck="false"
							 @focus="callToggleContainerActiveClass(true)"
							 @blur="callToggleContainerActiveClass(false)">
				<span :class="['fake-placeholder', { 'activePlaceholder': account.Name !== '' }]">
				<span>account name</span>
			</span>
			</section>
			<section class="inputSection" >
				<input type="text"
							 class="project-desc"
							 v-model.trim="account.PhoneNumber"
							 spellcheck="false"
							 tabindex="5"
							 @keyup="validateAnaAccount"
							 @keyup.enter="registerAccount"
							 placeholder="phone number*"
							 @focus="callToggleContainerActiveClass(true)"
							 @blur="callToggleContainerActiveClass(false)">
				<span v-if="accountError.regex"
							class="projectError">
					phone number should contain digits only
				</span>
				<span v-if="accountError.empty"
							class="projectError">Please enter a phone number</span>
				<span v-if="accountError.length"
							class="projectError">
					phone number should be a 10 digit number
				</span>
			</section>
			<a class="start-build"
				 @click="registerAccount($event)">
				<span>subscribe for ana-cloud</span>
			</a>
		</fieldset>
	</section>
</template>

<script>
	import blank from "./index";

	export default blank;
</script>

<style scoped lang="scss">
	@import "../../../sass/components/blank";
</style>
