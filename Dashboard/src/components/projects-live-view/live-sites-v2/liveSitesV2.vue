<template>
	<div class="data">
		<p class="count">{{index}}</p>
		<div class="content-main">
			<div class="cloud-container">
				<p class="live-status-container">
				<span :class="['live-status', site.IsActive? 'active' : 'inactive']"></span>
				</p>
				<p class="cloud-icon-container">
					<i :class="cloudIcon"></i>
				</p>
				<p class="ssl-icon-container">
					<span :class="['ssl-status', !site.IsSSLEnabled? 'active' : 'inactive']"><span v-if="!site.IsSSLEnabled">-</span></span>
				</p>
			</div>
			<div class="name-container">
				<p class="name url">{{site.WebsiteUrl}}</p>
				<p class="sub-info project-name">{{site.ProjectName}} (v{{site.ProjectVersion}})</p>
			</div>
		</div>
		<div class="content-footer">
			<span class="sub-info date">{{upTime}} minutes</span>
			<section>
				<button class="btn kbtn" @click="openSiteHandler">view site</button>
				<button @click.prevent="openContextMenu($event)" class="context-menu-btn"></button>
			</section>
			<transition enterActiveClass="animated fadeIn" leaveActiveClass="animated fadeOut">
				<contextMenu v-if="showContextMenu"
										 :projectId="site.ProjectId"
										 :projectName="site.ProjectName"
										 :customerId="site.WebsiteId"
										 :kitsuneUrl="site.WebsiteUrl"
										 :version="site.ProjectVersion"
										 :isSiteActive="site.IsActive"
										 :isProjectsMenu="false">
				</contextMenu>
			</transition>
		</div>
	</div>
</template>

<script>
	import liveSitesV2 from "./index";

	export default liveSitesV2
</script>

<style scoped lang="scss">
@import "../../../sass/components/liveV2";
</style>
