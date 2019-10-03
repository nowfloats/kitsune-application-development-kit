<template>
	<div class="header-container">
		<span class="optimizing">optimizing site <span class="site-name">{{projectName}}</span></span>
		<div class="stage-indicators">
			<ul>
				<li v-if="isSixStagesRequired">
					<span :class="[ 'circle', { 'completed'  : getStatus(1)}, { 'current rotate' : isCurrentStage(1) || isCurrentStage(0) }]"></span>
					<span :class="['stage-number',{ 'completed' : getStatus(1) },{ 'current' : isCurrentStage(1) || isCurrentStage(0) }]">
						{{getStageNumber(1)}}
					</span>
					<span :class="['stage-name',{ 'current' : isCurrentStage(1) || isCurrentStage(0) }]">{{ getStatus(1) ? getStageName(1).completed : getStageName(1).pending }}</span>
				</li>

				<li v-if="isSixStagesRequired">
					<span :class="[ 'circle', { 'completed'  : getStatus(2) }, { 'current' : isCurrentStage(2) }]"></span>
					<span :class="['stage-number',{ 'completed' : getStatus(2) },{ 'current' : isCurrentStage(2) }]">
						{{getStageNumber(2)}}
					</span>
					<span :class="['stage-name',{ 'current' : isCurrentStage(2) }]">{{ getStatus(2) ? getStageName(2).completed : getStageName(2).pending }}</span>
				</li>

				<li v-if="isSixStagesRequired">
					<span  :class="[ 'circle', { 'completed'  : getStatus(3) }, { 'current rotate' : isCurrentStage(3) }]"></span>
					<span :class="['stage-number',{ 'completed' : getStatus(3) },{ 'current' : isCurrentStage(3) }]">
						{{getStageNumber(3)}}
					</span>
					<span :class="['stage-name',{ 'current' : isCurrentStage(3) }]">{{ getStatus(3) ? getStageName(3).completed : getStageName(3).pending }}</span>
				</li>

				<li>
					<span  :class="[ 'circle', { 'completed'  : getStatus(4) && getStatus(5) },{ 'current rotate' : isCurrentStage(4) || isCurrentStage(5)}]"></span>
					<span :class="['stage-number',{ 'completed' : getStatus(4) && getStatus(5) },{ 'current' : isCurrentStage(4) || isCurrentStage(5)}]">
						{{getStageNumber(4)}}
					</span>
					<span :class="['stage-name',{ 'current' : isCurrentStage(4) || isCurrentStage(5)}]">{{ getStatus(4) && getStatus(5) ? getStageName(4).completed : getStageName(4).pending }}</span>
				</li>

				<li>
					<span  :class="[ 'circle', { 'completed'  : getStatus(6) }, { 'current rotate' : isCurrentStage(6) }]"></span>
					<span :class="['stage-number',{ 'completed' : getStatus(6) },{ 'current' : isCurrentStage(6) }]">
						{{getStageNumber(5)}}
					</span>
					<span :class="['stage-name',{ 'current' : isCurrentStage(6) }]">{{ getStatus(5) ? getStageName(5).completed : getStageName(5).pending }}</span>
				</li>

				<li>
					<span :class="[ 'circle', { 'completed'  : getStatus(7) }, { 'current rotate' : isCurrentStage(7) }]"></span>
					<span :class="['stage-number',{ 'completed' : getStatus(7) },{ 'current' : isCurrentStage(7) }]">
						{{getStageNumber(6)}}
					</span>
					<span :class="['stage-name',{ 'current' : isCurrentStage(7) }]">{{ getStatus(6) ? getStageName(6).completed : getStageName(6).pending }}</span>
				</li>
			</ul>
		</div>
	</div>
</template>

<script>
	import Stage1Header from './index'

	export default Stage1Header;
</script>

<style lang="scss">
	@import "../../../../sass/components/migrationbuildcardsscss/stage1/header";
</style>
