<template>
	<div class="dns-details-base-container">
		<div class="dns-details-container">

			<div class="modal-header">
				<div class="k-title">
					<p class="modal-heading">
						<span v-if="domainStatus === 'mapped' && !updateSectionVisible">dns details</span>
						<span v-if="updateBtnClicked && domainStatus !== 'mapped'">map domain to your site</span>
						<span v-if="updateSectionVisible">edit domain</span>
					</p>
					<p v-if="domainStatus !== 'mapped'" class="modal-subheading">{{websiteUrl}}</p>
				</div>
				<button @click="closeDnsDetails($event)"></button>
			</div>

			<div class="modal-body" v-if="!showLoader">
				<section>
					<transition enter-active-class="customSlideInLeft">
						<section class="details-section" v-if="updateBtnClicked">
							<div class="domain-section">
								<p>{{newDomain}}</p>
								<span v-if="domainStatus !== 'mapped'" :class="['hovering-label', {'active': newDomain !== ''}]">domain to be pointed</span>
								<button class="dns-button"
												@click="changeClickHandler">
									<span v-if="getRequestedDomain || domainStatus == 'mapped'">change</span>
									<span v-else>set</span>
								</button>
							</div>
							<section class="unmapped-section" v-if="domainStatus !== 'mapped'">
								<p class="details-docs">dns details to be filled
									<a href="http://docs.kitsune.tools/setting-custom-subdomains" target="_blank">(how?)</a>
								</p>
								<section class="dns-info">
									<div class="info">
										<div class="details">
											<p>cname</p>
											<p>{{websiteUrl}}</p>
										</div>
										<div class="details">
											<p>a record</p>
											<p>{{getARecord}}</p>
										</div>
									</div>
									<div class="action">
										<span>if in case you've already mapped domain, click verify</span>
										<button :class="['dns-button', { 'primary' : newDomain !== getDomainPlaceholder }]"
														:disabled="newDomain === getDomainPlaceholder"
														:title="newDomain === getDomainPlaceholder ? getDomainPlaceholder : ''"
														@click="verifyClickHandler">verify</button>
									</div>
								</section>
							</section>
						</section>
					</transition>
					<transition enter-active-class="customSlideInRight">
						<section class="update-section" v-if="updateSectionVisible">
							<div class="domain-section update">
								<input type="text"
											 autofocus
											 ref="domainName"
											 placeholder="domain to be pointed"
											 v-model.trim="newDomain"
											 spellcheck="false"
											 @keyup.enter="updateConfirmHandler" />
								<span v-if="domainStatus !== 'mapped'" :class="['hovering-label-primary', {'active': newDomain !== ''}]">domain to be pointed</span>
								<span title="domain cannot contain special characters or sub-paths"
											data-toggle="tooltip"
											:class="[ 'domain-error', {'active' : showDomainError} ]"></span>
							</div>
							<section>
								<button class="btn kbtn-secondary" @click="updateBtnClickHandler(true)">cancel</button>
								<button class="btn kbtn-primary" @click="updateConfirmHandler($event)">done</button>
							</section>
						</section>
					</transition>
				</section>
				<div v-if="domainStatus === 'mapped' && !updateSectionVisible">
					<span class="support-info">
						{{newDomain}} has been mapped to your website. If you would like to change the domain, please click on change.
					</span>
				</div>
			</div>

			<div class="modal-body" v-if="showLoader">
				<div class="dots-loading">gathering information</div>
			</div>

		</div>
	</div>
</template>

<script>
	import dnsDetails from './index'

	export default dnsDetails;
</script>

<style lang="scss">
	@import '../../sass/components/dnsdetails';
</style>
