<template>
	<div class="create-customer-body-container">
		<form class="customer-form">
			<div class="form-group">
				<input v-model="customer.customerName"
							 type="text"
							 class="form-control"
							 placeholder="customer name"
							 @keyup="validateCustomerName">
				<span :class="['floating-label',{ 'active': customer.customerName!='' }]">customer name</span>
				<span title="enter valid name" :class="[ 'error-message', { 'active' : !isNameValid } ]"></span>
			</div>
			<div class="form-group">
				<input v-model="customer.customerEmail"
							 type="text"
							 class="form-control"
							 placeholder="customer mail id"
							 @keyup="validateCustomerEmail">
				<label :class="['floating-label',{ 'active': customer.customerEmail!='' }]">customer mail id</label>
				<span title="enter valid email" :class="[ 'error-message', { 'active' : !isEmailValid } ]"></span>
			</div>
			<div class="form-group">
				<input v-model="customer.phoneNumber"
							 type="text"
							 class="form-control"
							 placeholder="customer mobile number"
							 @keyup="validateCustomerPhone">
				<label :class="['floating-label',{ 'active': customer.phoneNumber!='' }]">customer mobile number</label>
				<span title="enter valid phone" :class="[ 'error-message', { 'active' : !isPhoneValid } ]"></span>
			</div>
		</form>
		<publishFooter>
			<div slot="actionButtons">
				<button class="btn publish-back-button" @click="backBtnHandler">back</button>
				<button @click.prevent="submitForm" class="btn kbtn-primary">proceed</button>
			</div>
		</publishFooter>
	</div>
</template>

<script>
	import CreateCustomerBody from './index'

	export default CreateCustomerBody
</script>

<style lang="scss">
	@import '../../../../sass/components/publish/createcustomerbody';
</style>
