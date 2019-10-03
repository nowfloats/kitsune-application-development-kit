<template>
	<ul class="project-context-menu">
	<li v-for="(item, index) in (isProjectsMenu ? ProjectsMenuItems : LiveSiteMenuItems)" class="project-context-menu-item">
	<a @click="menuAction($event, item.actionName)" :class="item.className">{{item.name}}
	</a>
	</li>
	</ul>
</template>

<script>
	import contextmenu from './index';

	export default contextmenu
</script>

<style scoped lang="scss">
	@import '../../../../../sass/components/projectcontextmenu';
</style>
