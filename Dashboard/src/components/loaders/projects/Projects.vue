<template>
	<div class="projectLoader container-fluid">
		<div class="cards-row row">
			<cards class="col-md-4 col-sm-6 col-lg-3"></cards>
			<cards class="col-md-4 col-sm-6 col-lg-3"></cards>
			<cards class="col-md-4 col-sm-6 col-lg-3"></cards>
			<cards class="col-md-4 col-sm-6 col-lg-3"></cards>
		</div>
	</div>
</template>

<script>
	import Projects from './index'

	export default Projects
</script>

<style lang="scss">
	@import "../../../sass/loaders/_projects.scss";
</style>
