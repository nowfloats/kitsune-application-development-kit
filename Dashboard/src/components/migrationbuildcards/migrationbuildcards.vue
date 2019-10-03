<template>
	<section>
		<div @click.self="minimizeProject" class="migrationcards-basecontainer">
			<div class="minimize">
				<span @click="minimizeProject" v-if="stage == 8">close</span>
				<img src="../../assets/icons/dots-vertical.svg"
						 alt="triangle down icon"
						 class="vertical-dots"
						 @click="toggleExtraOptions(true)"
						 v-if="stage != 8"/>

				<transition enter-active-class="animated fadeIn" leave-active-class="animated fadeOut">
					<div v-if="isExtraOptionsActive" class="extra-options">
						<p @click="extraOptionsClickHandler($event, 'minimize')">minimize window</p>
						<p :class="{'disabled': stage >= 5}"
							 title="stop crawling"
							 v-tippy="{ placement: 'left', arrow: true, arrowType: 'round', size: 'small' }"
							 @click="extraOptionsClickHandler($event, 'abort')">stop crawling</p>
						<div class="extra-options-overlay" @click="toggleExtraOptions(false)"></div>
					</div>
				</transition>
			</div>
			<stage0 @minimize="minimizeProject" v-if="(!this.migration.isCrawlingStarted
						&& !this.migration.isMigrationCompleted
						&& !isError)"></stage0>
			<stage1 v-if="(getStatus(1) || getStatus(0)) && !isError" :data="migration.crawler.analysis"></stage1>
			<stage2 v-if="getStatus(2) && !isError" :data="migration.crawler.domainsFound"></stage2>
			<stage3 v-if="getStatus(3) && !isError" :data="migration.crawler.importingFiles"></stage3>
			<stage4 v-if="(getStatus(4) || getStatus(5)) && !isError" :data="migration.crawler.replacedLinks" :optimizer="migration.optimizer.analyzer"></stage4>
			<stage5 v-if="getStatus(6) && !isError" :data="migration.optimizer.optimize"></stage5>
			<stage6 v-if="getStatus(7) && !isError" :data="migration.optimizer.replacer"></stage6>
			<stage7 v-if="migration.isMigrationCompleted && !isError"></stage7>
			<error v-if="isError"></error>
		</div>
	</section>
</template>

<script>
	import MigrationBuildCards from './index'

	export default MigrationBuildCards;
</script>

<style lang="scss">
	@import '../../sass/components/migrationbuildcardsscss/migrationbuildcards';
</style>
