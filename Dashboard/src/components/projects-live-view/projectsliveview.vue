<template>
	<div class="live-sites-container">
		<div v-if="hasLiveSites">
			<header class="sticky-title">
				<h2 :class="{'hide-header' : enableSearch}">
					<span class="sticky-title-text active">live serverless-apps</span>
				</h2>
				<div :class="['search', {'active' : enableSearch}]">
					<button @mouseenter="activateSearch" @click="activateSearch"></button>
					<input type="text" ref="search" :autofocus="enableSearch" :class="{ 'active':enableSearch }"
								 v-model.trim="searchQuery" placeholder="search..."/>
					<button @click="deactivateSearch(false)" :class="{ 'active':enableSearch }"></button>
				</div>
				<div class="live-sites-menu-btn" @click.stop="toggleLiveSitesMenu(true)">
					<ul :class="['live-sites-menu', { active : isMenuActive }]">
						<li @click.stop="menuClickHandler($event, 1)" :class="menuClass(1)">website url</li>
						<li :class="menuClass(0)" @click="menuClickHandler($event, 0)">connected project</li>
						<li :class="menuClass(2)" @click="menuClickHandler($event, 2)">up time(in minutes)</li>
						<li :class="menuClass(3)" @click="menuClickHandler($event, 3)">status</li>
					</ul>
				</div>
			</header>

			<main class="live-sites-content wrapper">
					<div class="tabs-container">
						<div :class="['tabs', { 'active' : this.activeTab == 'ALL' }]"
							@click="setActiveTab(4)">
							<span>all serverless-apps ({{liveSites.length}})</span>
						</div>
						<div :class="['tabs', { 'active' : this.activeTab == tabOptions.get(1) }]"
							 @click="setActiveTab(1)">
							<img src="../../assets/icons/alibaba-cloud.svg" alt="alibaba cloud icon">
							<span>Alibaba Cloud ({{filterLiveSitesByCloud(tabOptions.get(1)).length}})</span>
						</div>
						<div :class="['tabs', { 'active' : this.activeTab == tabOptions.get(0) }]"
							@click="setActiveTab(0)">
							<img src="../../assets/icons/AWS.svg" alt="aws icon">
							<span>AWS ({{filterLiveSitesByCloud(tabOptions.get(0)).length}})</span>
						</div>
						<div :class="['tabs', { 'active' : this.activeTab == tabOptions.get(2) }]"
							@click="setActiveTab(2)">
							<img src="../../assets/icons/pingan.svg" alt="pingan icon">
							<span>GCP ({{filterLiveSitesByCloud(tabOptions.get(2)).length}})</span>
						</div>
						<div :class="['tabs', { 'active' : this.activeTab == tabOptions.get(3) }]"
							@click="setActiveTab(3)">
							<img src="../../assets/icons/azure.svg" alt="azure icon">
							<span>Microsoft Azure ({{filterLiveSitesByCloud(tabOptions.get(3)).length}})</span>
						</div>
					</div>
				<div class="sites-scroll" id="live-sites-content">
					<div class="data header" v-if="sortedArr.length > 0">

						<span class="count">#</span>
						<div class="content-main">
							<div class="cloud-container">
								<p :class="statusClass" @click="changeSortingVariable(3)">status</p>
								<p class="cloud-icon-container">cloud</p>
								<p class="ssl-icon-container">ssl</p>
							</div>
							<div class="name-container">
								<p :class="urlClass">
									<span @click="changeSortingVariable(1)">website url</span>
								</p>
								<p :class="nameClass">
									<span @click="changeSortingVariable(0)">project name</span>
								</p>
							</div>
						</div>
						<div class="content-footer">
							<span :class="updatedClass" @click="changeSortingVariable(2)">up time</span>
						</div>
					</div>
					<div class="notify" v-if="showNotificationPanel">
						<div class="content">
							<img :src="notificationData.icon" alt="cloud icon">
							<p>{{notificationData.message}}<a :href="cloudDocsUrl" target="_blank">learn more</a></p>
							<i class="close-icon" @click="hideNotificationForTab"></i>
						</div>
					</div>
					<LiveSitesV2 v-for="(site, index) in sortedArr"
						:key="index"
						:site="site"
						:index="index + 1">
					</LiveSitesV2>
					<div v-if="sortedArr.length == 0 && this.activeTab == tabOptions.get(2)"
						class="zeroth-live">
						<div class="cloud-coming">
							<img src="../../assets/icons/gcp-zeroth.svg" alt="">
							<p class="info">coming soon</p>
						</div>
					</div>
					<div v-if="sortedArr.length == 0 && this.activeTab == tabOptions.get(1)"
						class="zeroth-live">
						<div>
							<img src="../../assets/icons/alicloud-zeroth.svg" alt="">
							<p class="info">no site hosted on Alibaba Cloud</p>
							<p class="sub-info">{{cloudZerothCaseContent}}
								<a :href="cloudDocsUrl" target="_blank">here</a>
							</p>
						</div>
					</div>
					<div v-if="sortedArr.length == 0 && this.activeTab == tabOptions.get(0)"
						class="zeroth-live">
						<div>
							<img src="../../assets/icons/aws-zeroth.svg" alt="">
							<p class="info">no site hosted on AWS</p>
							<p class="sub-info">{{cloudZerothCaseContent}}
								<a :href="cloudDocsUrl" target="_blank">here</a>
							</p>
						</div>
						</div>
					<div v-if="sortedArr.length == 0 && this.activeTab == tabOptions.get(3)"
						class="zeroth-live">
						<div class="cloud-coming">
							<img src="../../assets/icons/azure-zeroth.svg" alt="">
							<p class="info">coming soon</p>
						</div>
					</div>
					<div v-if="lazyLoad.showLoader" class="lazyLoader">
						<img src="../../assets/icons/coffee-cup.svg" alt="loader icon">
						<p>loading more ...</p>
					</div>
				</div>
			</main>
		</div>
		<zerothCase v-else name="live-sites"></zerothCase>
	</div>
</template>

<script>
	import ProjectLevelView from './index';

	export default ProjectLevelView;
</script>

<style lang="scss">
	@import '../../sass/components/liveSites';
	@import "../../sass/components/liveV2";
</style>
