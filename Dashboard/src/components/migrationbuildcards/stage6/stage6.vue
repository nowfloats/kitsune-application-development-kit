<template>
	<div class="migration-card-size">
		<MigrationHeader></MigrationHeader>
		<div class="overflow-hidden">
			<transition  appear
									 appear-active-class="animated customSlideInRight">
				<Stage6Body :replacer="getReplacerDetails"></Stage6Body>
			</transition>
		</div>
		<MigrationFooter :requiredPercentage="true" :totalPercentage="getFilesReplacedPercentage"></MigrationFooter>
	</div>
</template>

<script>
	import Stage6 from './index'

	export default Stage6
</script>

<style lang="scss">
	@import '../../../sass/components/migrationbuildcardsscss/migrationbuildcards';
</style>
