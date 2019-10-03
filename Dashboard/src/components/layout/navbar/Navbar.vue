<template>
	<header class="user-navbar">
		<greeting :name="user.DisplayName"></greeting>
		<button v-if="componentStatus.hamburger"
			@click="toggleStatusHandler($event, ['overlay', 'sidebar'])"
				 class="toggle-sidebar">
		</button>
		<nav class="navbar-container">
			<wallet v-if="!componentStatus.hamburger"></wallet>
			<section @click="updatesClickHandler($event)">
				<button title="updates"
								v-tippy="{ placement: 'top', size: 'small', arrow: true, arrowType: 'round' }"
								class="updates"></button>
				<badge></badge>
			</section>
			<section>
				<section @click="toggleStatusHandler($event, ['overlay', 'profile'])">
					<button title="user menu"
									v-tippy="{ placement: 'top', size: 'small', arrow: true, arrowType: 'round' }"
									class="profile"></button>
				</section>
				<transition enterActiveClass="slideInRight"
										leaveActiveClass="slideOutRight">
					<dropdown v-if="componentStatus.profile && componentStatus.hamburger"
										class="animated slideInRight">
					</dropdown>
				</transition>
				<dropdown v-if="componentStatus.profile && !componentStatus.hamburger"></dropdown>
			</section>
		</nav>
		<create-button v-if="showCreateButton"></create-button>
	</header>
</template>

<script>
	import Navbar from "./index.js";

	export default Navbar
</script>

<style scoped lang="scss">
	@import "../../../sass/components/_navbar";
</style>
