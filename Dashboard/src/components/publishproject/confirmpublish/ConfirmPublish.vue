<template>
	<div class="confirm-customer-container">
		<publishHeader modalHeading="confirm publish"></publishHeader>
		<transition  appear
								 appear-active-class="animated customSlideInRight">
			<publishBody :projectName="projectDetails.projectName" 
				:projectId="projectDetails.projectId"
				:customerDomain="getDomainName">
			</publishBody>
		</transition>
	</div>
</template>

<script>
	import ConfirmPublish from './index'

	export default ConfirmPublish;
</script>

<style lang="scss">
	@import '../../../sass/components/publish/_confirmpublish.scss';
</style>
