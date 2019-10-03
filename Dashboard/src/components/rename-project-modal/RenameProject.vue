<template>
	<div class="base-modal-container">
		<div class="rename-body">
			<div class="rename-header">
				<div class="close-icon" @click="clickHandler($event)"></div>
				<img src="../../assets/icons/edit-dark.svg" alt="edit icon">
				<section>
					<p class="header">rename project</p>
				</section>
			</div>
			<div class="rename-content">
				<div class="input-section">
					<span class="hovering-label active">new name for project</span>
					<input type="text"
								 autofocus
								 placeholder="new name for project"
								 :value="projectName"
								 spellcheck="false"
								 ref="projectName"
								 :disabled="showLoader"
								 @input="updateProjectNameInStore($event)"
								 @keyup.enter="updateHandler($event)" />
					<span :title="error"
								data-toggle="tooltip"
								:class="[ 'name-error', {'active' : showError} ]">

					</span>
				</div>
				<section class="rename-margin">
					<button :class="['btn kbtn-primary', { 'rename-loader' : showLoader }]"
						@click="updateHandler($event)"
						:disabled="showError || showLoader">
						<threeDots v-if="showLoader"></threeDots>
						<span v-else>update</span>
					</button>
				</section>
			</div>
		</div>
	</div>
</template>

<script>
	import renameProject from './index';

	export default renameProject;
</script>

<style scoped lang="scss">
	@import "../../sass/components/renameProject";
</style>
