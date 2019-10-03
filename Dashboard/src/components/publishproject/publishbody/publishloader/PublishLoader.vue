<template>
	<div class="loading-body-container">
		<span v-if="projectDetails.isGettingCustomerList && !isApiError" class="processing dots-loading">
			gathering information
		</span>

		<transition leave-class="animated slideOutUp">
			<span v-if="projectDetails.isCreatingCustomer && !isApiError" class="processing">
				adding {{projectDetails.newCustomerDetails.CustomerName}} to your customer list
			</span>
		</transition>

		<transition enterActiveClass="animated slideInUp">
			<span class="publishing" v-if="projectDetails.isPublishing && !isApiError">
				<span class="processing">publishing, it might take a while</span>
				<div>
					<button @click="minimizePublishModal" class="btn kbtn">minimize</button>
				</div>
			</span>
		</transition>

		<transition enterActiveClass="animated slideInUp">
			<span class="publishing" v-if="!projectDetails.isEligibleForPublishing && !isApiError">
				<span class="processing">please add money to continue</span>
				<div>
					<button @click="addMoney($event)" class="btn kbtn">add money</button>
				</div>
			</span>
		</transition>

		<transition enterActiveClass="animated slideInUp">
			<span class="publishing" v-if="isApiError">

				<span v-if="!customerCreationError" class="processing">error while publishing project</span>
				<span v-if="customerCreationError" class="processing">error creating customer</span>

			</span>
		</transition>
	</div>
</template>

<script>
	import PublishLoader from './index'

	export default PublishLoader;
</script>

<style lang="scss">
 @import "../../../../sass/components/publish/loadingbody";
</style>
