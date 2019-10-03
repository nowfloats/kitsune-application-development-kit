<template>
	<div class="updates-container">
		<div class="updates-header">
			<span>UPDATES ({{updateCount}})</span>
			<i class="back-arrow" @click="toggleStatusHandler($event, ['overlay', 'updates'])"></i>
		</div>
		<div class="updates-content">
			<div class="updates-item" v-for="update in updateItems">
				<alert :updates="update"></alert>
			</div>
		</div>
	</div>
</template>

<script>
	import updates from './index'

	export default updates
</script>

<style scoped lang="scss">
	@import "../../sass/components/updates";
</style>
