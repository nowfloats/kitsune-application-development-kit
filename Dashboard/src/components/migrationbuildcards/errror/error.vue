<template>
	<div class="migration-card-size">
		<migrationHeader></migrationHeader>
		<div class="overflow-hidden">
			<transition  appear
									 appear-active-class="animated customSlideInRight">
				<migrationBody></migrationBody>
			</transition>

		</div>
	</div>
</template>

<script>
	import migrationError from './index'

	export default migrationError;
</script>

<style lang="scss">
	@import '../../../sass/components/migrationbuildcardsscss/migrationbuildcards';
</style>
