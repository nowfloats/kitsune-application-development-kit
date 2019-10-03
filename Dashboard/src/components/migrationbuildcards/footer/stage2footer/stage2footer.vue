<template>
	<div class="stage2footer-container">
		<section>
			<span>{{numberOfSelectedDomain}}</span>/<span>{{totalDomainsFound}}</span>
			<span>links selected</span>
		</section>
		<button @click="submitDomains"
						:disabled="disableButton"
						:class="[ 'btn kbtn-primary',{ 'position-for-loader': disableButton}]">
			<span v-if="!disableButton">proceed</span>
			<threeDots v-if="disableButton"></threeDots>
		</button>
	</div>
</template>

<script>
	import Stage2Footer from './index'

	export default Stage2Footer
</script>

<style lang="scss">
	@import '../../../../sass/components/migrationbuildcardsscss/stage2/footer';
</style>
