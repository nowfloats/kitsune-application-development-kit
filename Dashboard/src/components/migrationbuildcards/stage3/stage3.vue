<template>
	<div class="migration-card-size">
		<MigrationHeader></MigrationHeader>
		<div class="overflow-hidden">
			<transition  appear
									 appear-active-class="animated customSlideInRight">
				<Stage3Body :downloadDetails="getDownloadDetails"></Stage3Body>
			</transition>
		</div>
		<Stage3Footer :totalPercentage="getTotalPercentage" :requiredPercentage="true"></Stage3Footer>
	</div>
</template>

<script>
	import Stage3 from './index'

	export default Stage3;
</script>

<style lang="scss">
	@import "../../../sass/components/migrationbuildcardsscss/migrationbuildcards";
</style>
