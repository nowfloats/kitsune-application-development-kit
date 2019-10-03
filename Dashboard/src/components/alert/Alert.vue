<template>
	<div class="alert-item" :class="[updates.type, {'user-read' : updates.read } ]">
		<div class="alert-item-title">
			<span>{{updates.title}}</span>
		</div>
		<div class="alert-item-content">
			<p>{{updates.content}}</p>
		</div>
		<hr class="alert-line">
		<div class="alert-item-action">
			<a v-if="updates.callback"
				 href="javascript:void(0)"
				 @click="updatesAction($event)">
				{{updates.actionName.toUpperCase()}}
			</a>
			<a :href="updates.url"
				 v-else
				 class="alert-button"
				 target="_blank">
				{{updates.actionName.toUpperCase()}}
			</a>
			<span v-if="updates.extraInfo !== undefined">{{updates.extraInfo}}</span>
		</div>
	</div>
</template>

<script>
	import alert from "./index"

	export default alert
</script>

<style scoped lang="scss">
	@import "../../sass/components/alert";
</style>
