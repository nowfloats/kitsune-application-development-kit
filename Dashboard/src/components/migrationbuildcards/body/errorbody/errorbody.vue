<template>
	<div class="errorbody">
		<img class="kitsune-animation-img" src="../../../../assets/icons/kitsune_animation.gif">
		<span v-if="isErrorInStartingCrawl" class="error-msg">unknown forces have stopped our noble work. please try again.</span>
		<span v-if="isNetworkError" class="error-msg">Kitsune has detected issues with your internet connectivity. please check your internet connection and hit retry.</span>
		<button v-if="isErrorInStartingCrawl || isNetworkError" @click="retryMigration" class="btn kbtn-primary">retry</button>
	</div>
</template>

<script>
	import errorBody from './index'

	export default errorBody;
</script>

<style lang="scss">
	@import '../../../../sass/components/migrationbuildcardsscss/error/body';
</style>
