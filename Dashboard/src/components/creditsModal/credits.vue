<template>
	<div class="credits-modal-base">
		<div class='credits-modal-parent'>
			<div class="credits-modal-container">
				<div class='credits-modal-header'>
					<h1>kitsun<span>e</span></h1>
					<p class='info'>kitsune developer dashboard</p>
					<p class='release'>v2.4</p>
				</div>
				<div class='credits-modal-footer'>
					<div class='credits-copyright'>
						<p>&copy; {{getCurrentYear}} team kitsune all right reserved</p>
					</div>
					<img :src="getImage" alt='Kitsune Fox' />
				</div>
			</div>
		</div>
	</div>
</template>

<script>
	import credits from './index';

	export default credits;
</script>

<style scoped lang="scss">
	@import "../../sass/components/credits";
</style>
