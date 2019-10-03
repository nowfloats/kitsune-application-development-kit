<template>
	<aside class="sidebar">
		<v-touch @swipeleft="hideSidebar($event)">
			<section class="logo">
				<a @click="goHome($event)" title="Kitsune">
					<img src="../../../assets/icons/logo.svg" alt="kitsune logo">
				</a>
			</section>
			<section class="sidebar-menu">
				<ul class="sidebar-menu-group">
					<li v-for="(item, index) in menuItems"
							@click="hideSidebar($event)"
							:title="item.meta.title"
							v-tippy="{ placement: 'left', arrow: true, arrowType: 'round' }">
						<router-link :to="item.path"
												 class="sidebar-link"
												 v-if="item.path"
												 :class="item.meta.iconClass" >
							<p v-if="componentStatus.hamburger">{{item.meta.title}} </p>
							<span v-if="item.meta.comingSoon" class="coming-soon">(coming soon)</span>
						</router-link>
					</li>
				</ul>
			</section>
		</v-touch>
	</aside>
</template>

<script>
	import sidebar from './index'

	export default sidebar
</script>

<style scoped lang="scss">
	@import "../../../sass/components/sidebar";
</style>
