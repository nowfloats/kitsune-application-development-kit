<template>
	<div class="uploadAlert">
		<div class="uploadStatus" :class="getStatus"></div>
		<div class="uploadContent" :class="getStatus">
			<p :class="getStatus">upload {{getStatus}}</p>
			<p>{{getDetails}}</p>
		</div>
		<a v-if="getStatus == 'failed'" class="uploadAction">retry</a>
	</div>
</template>

<script>
	import uploadAlert from './index'

	export default uploadAlert
</script>

<style scoped lang="scss">
	@import "../../sass/components/uploadAlert";
</style>
