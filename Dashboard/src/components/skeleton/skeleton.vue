<template>
	<div class="skeleton">
		<aside class="sidebar">
		</aside>
		<div class="content">
			<header class="ske-navbar">
			</header>
			<div class="cards-holder container-fluid">
				<div class="cards-row row">
					<card class="card col-lg-3 col-md-4 col-sm-6"></card>
					<card class="card col-lg-3 col-md-4 col-sm-6"></card>
					<card class="card col-lg-3 col-md-4 col-sm-6"></card>
					<card class="card col-lg-3 col-md-4 col-sm-6"></card>
				</div>
			</div>
		</div>
	</div>
</template>

<script>
	import skeleton from './index';

	export default skeleton;
</script>

<style scoped lang="scss">
	@import "../../sass/components/skeletion";
</style>
