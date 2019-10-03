<template>
	<button class="badge-notify" v-if="badgeNumber !== 0">{{badgeNumber}}</button>
</template>

<script>
	import badge from './index';

	export default badge
</script>

<style scoped lang="scss">
	@import "../../sass/components/badge";
</style>
