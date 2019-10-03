<template>
	<div class="action-overlay">
		<div class="back-button"
				 v-if="showBackButton"
				 @click.self="backToMainCounter"
				 title="back">
		</div>
		<div class="action-container">
			<transition enterActiveClass="animated fadeInDown">
				<section class="first-container"
								 v-if="actionContainerCounter === 1 && !componentStatus.hamburger">
					<section class="crawl-section">
						<crawl></crawl>
					</section>
					<section class="select-action">
						<blank></blank>
						<ana></ana>
					</section>
				</section>
				<section class="first-container"
								 v-if="actionContainerCounter === 1 && componentStatus.hamburger">
					<crawl class="crawl-section"></crawl>
					<blank></blank>
					<ana></ana>
				</section>
			</transition>
			<transition enterActiveClass="animated fadeInUp">
				<section class="blank-section" v-if="actionContainerCounter === 2">
					<blank></blank>
				</section>
			</transition>
			<transition enterActiveClass="animated fadeInDown">
				<section class="upload-section" v-if="actionContainerCounter === 3">
					<upload></upload>
				</section>
			</transition>
		</div>
	</div>
</template>

<script>
	import action from './index'

	export default action
</script>

<style scoped lang="scss">
	@import "../../sass/components/action";
</style>
