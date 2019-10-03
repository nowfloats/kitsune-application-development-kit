<template>
	<div class="progress-bar-container">
		<div :class="[ 'horizontal' , { 'extra-border': wrapperRequired }]">
			<!--<div v-if="size != 'thick'" class="value">{{animatedValue + '%'}}</div>-->
			<div class="progress" :class="size" >
				<div class="progress-bar" ref="progressBar" :style="'width: ' + value + '%'">
					<span v-if="size == 'thick'" :class="{hidden: animatedValue == min}" class="value">
						{{animatedValue + '%'}}
					</span>
				</div>
			</div>
		</div>
	</div>
</template>

<script>
	import ProgressBar from './index'

	export default ProgressBar;
</script>

<style lang="scss">
	@import "../../sass/components/progressbar";
</style>
