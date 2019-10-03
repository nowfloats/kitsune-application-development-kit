<template>
	<div class="migration-card-size">
		<MigrationHeader></MigrationHeader>
		<div class="overflow-hidden">
			<transition  appear
									 appear-active-class="animated customSlideInRight">
				<Stage2Body @numberOfSelectedLinks="setnumberOfSelectedLinks" :listOfDomains="data"></Stage2Body>
			</transition>
		</div>
		<Stage2Footer :totalDomainsFound="getNumberOfDomainsFound" :numberOfSelectedDomain="numberOfSelectedDomains"></Stage2Footer>
	</div>
</template>

<script>
	import Stage2 from './index'

	export default Stage2;
</script>

<style lang="scss">
	@import '../../../sass/components/migrationbuildcardsscss/migrationbuildcards';
</style>
