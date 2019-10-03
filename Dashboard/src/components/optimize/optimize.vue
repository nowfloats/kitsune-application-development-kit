<template>
	<div class="optimize-base-container">
		<div class="optimize-container">
			<div class="optimize-container-header">
				<div class="information">
					<span class="stage-name">optimize project</span>
					<span class="project-name">{{getProjectName}}</span>
				</div>
				<button @click="closeHandler($event)" class="close"></button>
			</div>
			<div class="optimize-container-body">
				<span v-if="!apiCallCompleted" class="text-description dots-loader">gathering information</span>
				<span v-if="apiCallCompleted && apiCallSuccess && !optimizeErrorStatus" class="text-description">optimization started</span>
				<span v-if="(apiCallCompleted && !apiCallSuccess) || optimizeErrorStatus" class="text-description">error while optimization</span>
				<button @click="closeHandler($event)" class="btn kbtn-primary">close</button>
			</div>
		</div>
	</div>

</template>

<script>
	import optimize from './index'

	export default optimize;
</script>

<style scoped lang="scss">
	@import '../../sass/components/optimize';
</style>
