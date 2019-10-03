<template>
	<div class="greeting-container">
		<div v-if="componentStatus.hamburger">
			<h1 class="greeting">good {{greet}},</h1>
			<h1 class="greeting name">{{name}}.</h1>
		</div>
		<h1 v-if="!componentStatus.hamburger" class="greeting">good {{greet}}, {{name}}.</h1>
	</div>
</template>

<script>
	import greeting from "./index"

	export default greeting
</script>

<style scoped lang="scss">
	@import "../../../../sass/components/_greeting";
</style>
