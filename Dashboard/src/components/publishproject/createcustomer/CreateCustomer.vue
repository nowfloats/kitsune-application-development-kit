<template>
	<div class="create-customer-container container-fluid">
		<PublishHeader :isCreateCustomer="true" :projectName="projectDetails.newCustomerDetails.WebsiteTag" modalHeading="add a customer, who will own"></PublishHeader>
		<transition  appear
								 appear-active-class="animated customSlideInRight">
			<createCustomerBody></createCustomerBody>
		</transition>
	</div>
</template>

<script>
	import CreateCustomer from './index'

	export default CreateCustomer
</script>

<style lang="scss">
	@import '../../../sass/components/publish/createcustomer';
</style>

