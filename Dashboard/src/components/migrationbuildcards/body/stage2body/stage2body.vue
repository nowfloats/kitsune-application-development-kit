<template>
	<div class="stage2-body-container">
		<div class="stage-information">
			<span class="stage-name">please recognize  links</span>
			<span class="stage-description">these are the links that we've found associated with your website. please identify the content you wish to optimize.</span>
		</div>
		<div class="url-options" v-if="domainsFound && !gettingDomains">
			<span v-for="(item,index) in listOfDomains">
				<input ref="checkboxes"
							 :value="item"
							 v-model="listOfSelectedDomains"
							 type="checkbox"/> {{item.toLowerCase()}}
			</span>
		</div>
		<div class="no-domains-found" v-if="!domainsFound && !gettingDomains">
			<span>
				 no domains found please click proceed to continue.
			</span>
		</div>
		<div class="no-domains-found" v-if="gettingDomains">
			<span>
				 gathering information
			</span>
		</div>
	</div>
</template>

<script>
	import Stage2Body from './index'

	export default Stage2Body;
</script>

<style lang="scss">
	@import '../../../../sass/components/migrationbuildcardsscss/stage2/body';
</style>
