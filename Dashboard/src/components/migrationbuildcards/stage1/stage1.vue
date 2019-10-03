<template>
	<div class="migration-card-size">
		<Stage1Header></Stage1Header>
		<div class="overflow-hidden">
			<transition  appear
									 appear-active-class="animated customSlideInRight">
				<Stage1Body></Stage1Body>
			</transition>
		</div>
		<Stage1Footer :data="data"></Stage1Footer>
	</div>
</template>

<script>
	import Stage1 from './index'

	export default Stage1;
</script>

<style lang="scss">
	@import '../../../sass/components/migrationbuildcardsscss/migrationbuildcards';
</style>
