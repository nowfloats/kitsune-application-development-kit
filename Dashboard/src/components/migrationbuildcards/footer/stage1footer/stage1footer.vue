<template>
	<div class="base-footer-container">
		<infiniteLoaderBar></infiniteLoaderBar>
		<div class="footer-container">
			<ul>
				<li><span>{{linksFound}}</span><span>pages</span></li>
				<li><span>{{stylesFound}}</span><span>styles</span></li>
				<li><span>{{scriptsFound}}</span><span>scripts</span></li>
				<li><span>{{assetsFound}}</span><span>assets</span></li>
			</ul>
			<span class="percentage"></span>
		</div>
	</div>
</template>

<script>
	import Stage1Footer from './index'

	export default Stage1Footer;
</script>

<style lang="scss">
	@import "../../../../sass/components/migrationbuildcardsscss/stage1/footer";
</style>
