<template>
	<section @click.self="openIDEForParent($event)" class="project-action-bar">
		<span @click="openIDEForParent($event)" class="project-created pull-left">
			<span  class="project-createdon-text">created on&nbsp;</span>
			{{CreatedOnDate}}
		</span>
		<button v-if="isContextMenuRequired" @click="openContextMenu($event)" class="project-context-menu-trigger pull-right"></button>
		<button v-if="!domainVerified" @click="buttonAction($event)"
						title="preview project"
						v-tippy="{ placement: 'bottom', arrow: true, arrowType: 'round', size: 'small', arrowTransform: 'scale(.8)' }"
						:class="[ 'project-btn-primary pull-right',{ 'live-site': !isContextMenuRequired }]">
			{{isProjectsMenu ? buttonNameForProjects : buttonNameForLiveSites}}
		</button>
		<transition enterActiveClass="animated fadeIn" leaveActiveClass="animated fadeOut">
			<contextMenu v-if="showContextMenu"
									 :projectId="projectId"
									 :projectName="projectName"
									 :customerId="customerId"
									 :kitsuneUrl="kitsuneUrl"
									 :version="version"
									 :isSiteActive="true"
									 :isProjectsMenu="isProjectsMenu">
			</contextMenu>
		</transition>
	</section>
</template>

<script>
	import projectFooterAction from './index'

	export default projectFooterAction
</script>

<style lang="scss">
	@import "../../../../../../sass/components/projectFooterAction";
</style>
