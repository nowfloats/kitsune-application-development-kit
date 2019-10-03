<template>
	<div class="verify-domain-base-container">
			<div class="verify-domain-container">
				<div class="modal-header">
					<div class="k-title">
						<span class="modal-heading">domain verification</span>
						<span class="project-name">{{getRequestedDomain}}</span>
					</div>
					<button @click="closeDomainVerificationModal($event)"></button>
				</div>
				<div class="modal-body">
					<p v-if="verifyDomain.isFetchingDetails" class="information">
						checking domain mapping
					</p>
					<p v-if="!verifyDomain.isFetchingDetails" class="information">
						{{getDomainStatus}}
					</p>
					<button class="btn kbtn-primary" @click="closeDomainVerificationModal($event)">go back</button>
				</div>
			</div>
	</div>
</template>

<script>
	import verifyDomain from './index'

	export default verifyDomain
</script>

<style scoped lang="scss">
	@import '../../sass/components/verifydomain';
</style>
