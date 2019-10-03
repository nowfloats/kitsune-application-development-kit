<template>
	<div class="base-modal-container">
		<div class="deactivate-body">
			<div class="deactivate-header">
				<div class="close-icon" @click="closeHandler($event)"></div>
				<img src="../../assets/icons/deactivate-site-dark.svg" alt="edit icon">
				<section>
					<p class="header">deactivate site</p>
					<p class="subheader">{{site.name.toLowerCase()}}</p>
				</section>
			</div>
			<div class="deactivate-content">
				<p class="info">are you sure you want to deactivate <span class="font-medium">{{site.name.toLowerCase()}}</span> ? the site will no longer be accessible once you click on confirm.</p>
				<section class="base-modal-footer-margin">
					<button class="btn kbtn-secondary" @click="closeHandler($event)">cancel</button>
					<button :class="['btn kbtn-primary', { 'rename-loader' : showLoader }]"
									@click="deactivateSiteHandler($event)"
									:disabled="showLoader">
						<threeDots v-if="showLoader"></threeDots>
						<span v-else>confirm</span>
					</button>
				</section>
			</div>
		</div>
	</div>
</template>

<script>
	import deactivateSite from './index';

	export default deactivateSite;
</script>

<style scoped lang="scss">
	@import "../../sass/components/deactivateSite";
</style>
