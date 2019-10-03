<template>
	<div class="create-button-container"
			 :class="{ 'create-container-active' : componentStatus.action }">
		<div class="create-button" :title="getTitle"
				v-tippy="{
					size: 'small',
					placement: 'top-end',
					offset: '75, 0'
				}"
				 :class="{ 'create-button-active' : componentStatus.action }"
				 v-if="showCreateButton"
				 @click="handler($event, ['action'])">
		</div>
		<img v-if="showArrowProjects" class="zeroth-arrow" src="../../assets/icons/zeroth-arrow.svg" alt="arrow">
		<!--for arrow in chatbots seciton-->
		<img v-if="showArrowChatbots" class="zeroth-arrow" src="../../assets/icons/zeroth-arrow.svg" alt="arrow">
	</div>
</template>

<script>
	import createButton from './index'

	export default createButton
</script>

<style scoped lang="scss">
	@import "../../sass/components/_create-button.scss";
</style>
