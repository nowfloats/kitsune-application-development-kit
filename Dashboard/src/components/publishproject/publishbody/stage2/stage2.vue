<template>
	<div class="publish-stage2Body-container">
		<span>publish project '{{projectName}}' to '{{customerDomain}}' </span>
		<publishFooter>
			<div slot="actionButtons">
				<button class="btn kbtn" @click="closePublish">no</button>
				<button class="btn kbtn-primary" @click="publishProject">yes</button>
			</div>
		</publishFooter>
	</div>
</template>

<script>
	import PublishStage2Body from './index'

	export default PublishStage2Body;
</script>

<style lang="scss">
	@import '../../../../sass/components/publish/publishstage2body';
</style>
