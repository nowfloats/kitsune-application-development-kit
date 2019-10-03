<template>
	<div class="project-header sticky-title">
		<div class="projects-count">
			<span class="sticky-title-text border-right active"
				v-if="!isLiveSites && !isChatBots">serverless-app projects ({{totalProjects}})</span>
			<span :class="['sticky-title-text active', { 'live-sites': isLiveSites }]"
				v-if="isLiveSites">live serverless apps ({{getActiveLiveSites}})
			</span>
			<span :class="['sticky-title-text active', { 'live-sites': isChatBots }]"
				v-if="isChatBots">chatbots ({{getActiveLiveSites}})
			</span>
		</div>
		<div v-if="!isLiveSites && !isChatBots" @mouseleave="deactivateSearch(true)"
				 :class="['search',{ 'active': enableSearch }]">
			<button   @mouseenter="activateSearch" @click="activateSearch"></button>
			<input type="text" ref="search" :autofocus="enableSearch" :class="{ 'active':enableSearch }"
						 v-model="searchQuery" @keyup="search" placeholder="search..."/>
			<button @click="deactivateSearch(false)" :class="{ 'active':enableSearch }"></button>
		</div>
		<div v-if="!isLiveSites && !isChatBots" class="sort-container">
			<a :class="['sort-by',{ 'active':!	enableSearch }]" @click="sortByProjectsCreatedOn">
				<i :class="{ 'ascen':!isDecreasingOrder }"></i>
				<span>created on</span>
			</a>
		</div>
	</div>
</template>

<script>
	import projectsheader from './index.js'

	export default projectsheader
</script>

<style lang="scss">
	@import "../../../sass/components/projectsheader";
</style>
