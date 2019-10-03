<template>
	<div class="abort-crawl" @keyup.esc="clickHandler($event)" @click.self="clickHandler($event)">
		<div class="abort-container">
			<div class="abort-container-header">
				<div class="close-icon" @click="clickHandler($event)"></div>
				<img src="../../assets/icons/publish-dark.svg" alt="abort icon">
				<section>
					<p class="header">abort crawling process</p>
					<p class="subheader">{{projectName}}</p>
				</section>
			</div>
			<div class="abort-container-content">
				<p>do you really want to abort the crawling process?</p>
				<section>
					<button class="btn kbtn-secondary" @click="abortCrawlHandler($event)">yes</button>
					<button class="btn kbtn-primary" @click="clickHandler($event)">no</button>
				</section>
			</div>
		</div>
	</div>
</template>

<script>
	import abortCrawl from './index';

	export default abortCrawl;
</script>

<style scoped lang="scss">
	@import "../../sass/components/abortCrawl";
</style>
