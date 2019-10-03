<template>
	<section>
		<section v-if="isDataReceivedForUserandProjectsFromAPI">
			<div class="components" v-if="isRequiredDataReceived" v-resize>
				<div>
					<transition enterActiveClass="animated slideInLeft"
											leaveActiveClass="animated slideOutLeft">
						<sidebar v-if="componentStatus.sidebar"></sidebar>
					</transition>
					<sidebar v-if="!componentStatus.hamburger"></sidebar>
				</div>
				<section class="content">
					<navbar></navbar>
					<main id="content-body" class="content-body" role="main">
						<router-view></router-view>
						<fullLoader v-if="componentStatus.fullLoader"></fullLoader>
						<billingForm v-if="componentStatus.billingform"></billingForm>
						<addMoney v-if="componentStatus.addmoney"></addMoney>
						<optimizeProject v-if="componentStatus.optimizeProject"></optimizeProject>
						<deleteProject v-if="componentStatus.deleteproject"></deleteProject>
						<previewProjectModal v-if="componentStatus.previewProject"></previewProjectModal>
						<publishProject v-if="componentStatus.publishproject"></publishProject>
						<processingPayment v-if="componentStatus.processingpayment"></processingPayment>
						<liveSiteModal v-if="componentStatus.customerDetails"></liveSiteModal>
						<credits v-if="componentStatus.credits"></credits>
						<verifyDomain v-if="componentStatus.verifyDomain"></verifyDomain>
						<dnsDetails v-if="componentStatus.dnsDetails"></dnsDetails>
						<migrationBuildCards v-if="componentStatus.migrationbuildcards"></migrationBuildCards>
						<LowBalanceModal v-if="componentStatus.lowBalance"></LowBalanceModal>
						<abortCrawl v-if="componentStatus.abortCrawl"></abortCrawl>
						<renameProject v-if="componentStatus.renameProject"></renameProject>
						<deactivateSite v-if="componentStatus.deactivateSite"></deactivateSite>
						<overlay v-if="componentStatus.overlay"
							:class="{ 'overlay-profile' : componentStatus.profile,
							'overlay-modal' : componentStatus.modal,
							'overlay-billing-info' : componentStatus.billingform
							|| componentStatus.addmoney
							|| componentStatus.processingpayment
							|| componentStatus.migrationbuildcards
							|| componentStatus.publishproject
							|| componentStatus.optimizeProject
							|| componentStatus.customerDetails
							|| componentStatus.verifyDomain
							|| componentStatus.dnsDetails
							|| componentStatus.deleteproject
							|| componentStatus.credits
							|| componentStatus.lowBalance}">
						</overlay>
						<action v-if="componentStatus.action"></action>
						<transition enter-active-class="animated slideInRight"
												leave-active-class="animated slideOutRight">
							<updates v-if="componentStatus.updates"></updates>
						</transition>
					</main>
				</section>
			</div>
		</section>
		<section v-else>
			<Skeleton></Skeleton>
		</section>
	</section>
</template>

<script>
	import layout from './index.js'

	export default layout
</script>

<style scoped lang="scss">
	@import "../../sass/components/_layout.scss";
</style>

<style lang="scss">
	// toaster css
	@import '~izitoast/dist/css/iziToast.css';
	@import '../../sass/components/toastr';
</style>
