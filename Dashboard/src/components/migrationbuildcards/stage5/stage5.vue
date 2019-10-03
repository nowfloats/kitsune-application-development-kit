<template>
	<div class="migration-card-size">
		<MigrationHeader></MigrationHeader>
		<div class="overflow-hidden">
			<transition  appear
									 appear-active-class="animated customSlideInRight">
				<Stage5Body :optimize="getOptimizeDetails"></Stage5Body>
			</transition>
		</div>
		<MigrationFooter :requiredPercentage="true" :totalPercentage="getTotalPercentage"></MigrationFooter>
	</div>
</template>

<script>
	import Stage5 from './index'

	export default Stage5
</script>

<style lang="scss">
	@import '../../../sass/components/migrationbuildcardsscss/migrationbuildcards';
</style>
