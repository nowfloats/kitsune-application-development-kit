<template>
	<div class="migration-card-size">
		<MigrationHeader></MigrationHeader>
		<div class="overflow-hidden">
			<transition  appear
									 appear-active-class="animated customSlideInRight">
				<Stage4Body :data="data" :analyzer="optimizer"></Stage4Body>
			</transition>
		</div>
		<MigrationFooter></MigrationFooter>
	</div>
</template>

<script>
	import Stage4 from './index'

	export default Stage4;
</script>

<style lang="scss">
	@import "../../../sass/components/migrationbuildcardsscss/migrationbuildcards";
</style>
