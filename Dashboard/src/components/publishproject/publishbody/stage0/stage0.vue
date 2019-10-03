<template>
	<div :class="['publish-stage0-body', { 'decrease-height': domainEntered || !hasCustomers }]">
		<div :class="[ 'publish-options' , { 'hide': !hasCustomers }]" v-if="!domainEntered">
			<div :class="['text-underline', { 'disabled':hoveredOnSelectedCreateNew || selectedCreateNew || hoveredOnUseSubDomain || selectedUseSubDomain }]"
					 v-if="!selectedExisting"
					 @mouseenter="hoverOnSelectedExisting"
					 @mouseleave="reset"
					 @click="selectExisting">
				<input type="radio"/><span class="radio-button-label">update an existing live site</span>
				<span v-if="hoveredOnSelectedExisting" class="checked-box"></span>
			</div>

			<div class="select-customer" v-if="selectedExisting">
				<label class="float-label" v-if="isCustomerIdValid">select customer</label>
				<CustomDropdown></CustomDropdown>
			</div>

		</div>
		<div :class="['publish-options padding', { 'reduce-padding': !hasCustomers }]" v-if="!domainEntered">
			<div :class="['text-underline', { 'disabled':hoveredOnSelectedExisting || selectedExisting || hoveredOnUseSubDomain || selectedUseSubDomain }]"
					 @mouseenter="hoverOnSelectedCreateNew"
					 @mouseleave="reset"
					 @click="selectCreateNew">
				<input checked type="radio"/>create a new customer
				<span v-if="hoveredOnSelectedCreateNew || selectedCreateNew" class="checked-box "></span>
			</div>

			<!-- <div class="new-site" v-if="selectedCreateNew">
				<input type="text" v-model.trim="domainName"
							 autofocus
							 @keyup.enter="submitData"
							 placeholder="enter domain name"/>
				<label :class="['floating-label',{ 'active': domainName!='' }]">enter domain name</label>
			</div>
			<p class="site-error" v-if="showDomainError && selectedCreateNew">please enter a valid domain</p> -->

		</div>

		<!-- <div :class="['publish-options padding margin-bot', { 'reduce-padding': !hasCustomers}]" >
			<div :class="['text-underline',
			{ 'disabled':hoveredOnSelectedExisting || selectedExisting || selectedCreateNew || hoveredOnSelectedCreateNew }]"
					 v-if="!selectedUseSubDomain"
					 @mouseenter="hoverOnUseSubDomain"
					 @mouseleave="reset"
					 @click="selectUseSubDomain">
				<input checked type="radio"/> as a kitsune sub-domain
				<span v-if="hoveredOnUseSubDomain" class="checked-box "></span>
			</div>
			<p class="kitsuneSubdomain"
				 v-if="selectedUseSubDomain && domainName !== ''">
				<img src="../../../../assets/icons/info.svg" alt="info icon">
				we are registering <span class="kitsuneDomain">{{domainName}}</span> as your domain for this project. before you update
				domain's DNS details, please choose a preferred kitsune sub-domain.</p>
			<div :class="['new-site', { 'margin-bot': selectedUseSubDomain && domainName !== '' }]"
					 v-if="selectedUseSubDomain">
				<input type="text" :class="{ 'error': !valid }"
							 autofocus
							 @keyup.enter="submitData"
							 @keyup="checkDomain"
							 v-model.trim="subDomain"
							 placeholder="enter kitsune sub-domain"/>
				<span class="static-text">.getkitsune.com</span>
				<label :class="['floating-label', { 'active': subDomain !== '' }]">kitsune sub-domain</label>
			</div>
			<p class="site-error" v-if="showSiteError && selectedUseSubDomain">site name already exists</p>

		</div> -->
		<publishFooter :class="{'hide' : !isValidAll}">
			<div slot="actionButtons">
				<button class="btn publish-back-button"
								v-if="selectedUseSubDomain && domainName !== ''"
								@click="backBtnHandler">back</button>
				<button :class="['btn kbtn-primary' , { 'loader-active': showLoader }]"
								:disabled="showLoader"
								@click="submitData">
					<span v-if="!showLoader">next</span>
					<threeDots v-else></threeDots>
				</button>
			</div>
		</publishFooter>
	</div>
</template>

<script>
	import PublishingStage0Body from './index'

	export default PublishingStage0Body;
</script>

<style lang="scss">
	@import '../../../../sass/components/publish/stage0body';
	@import '../../../../sass/components/publish/common';
</style>
