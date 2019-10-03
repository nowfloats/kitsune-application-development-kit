<template>
	<div class="billing-form-base-container">
		<div class="billing-form-container container-fluid">
			<div class="header">
				<span class="heading">add ownership details</span>
				<button @click="closeBillingForm($event)" class="kclose"></button>
			</div>
			<div class="content ">
				<form>
					<div class="k-form container-fluid	">
						<div class="row">
							<div class="k-form-field col-md-6 col-xs-12 form-group">
								<label class="floating-label active"
											 for="name">your name*</label>
								<input type="text"
											 id="name"
											 name="name"
											 data-vv-name="name"
											 v-validate="'required|alpha_spaces'"
											 placeholder="name" v-model="userDetails.Name" required
											 :disabled="getUserProfileUpdateStatus.isUserDetailsUpdateRequested"
											 @input="validateField('name', userDetails.Name)">
								<span :title="errors.first('name')"
											data-toggle="tooltip"
											:class="[ 'error-image', { 'active' : errors.has('name') } ]"></span>
							</div>
							<div class="k-form-field col-md-6 col-xs-12 form-group">
								<label class="floating-label active"
											 for="phone">mobile number*</label>
								<input type="text"
											 v-model="userDetails.PhoneNumber" placeholder="mobile" required
											 id="phone"
											 data-vv-as="mobile number"
											 name="phone"
											 data-vv-name="phone"
											 v-validate="'required|digits:10'"
											 :disabled="getUserProfileUpdateStatus.isUserDetailsUpdateRequested"
											 @input="validateField('phone', userDetails.PhoneNumber)">
								<span :title="errors.first('phone')"
											data-toggle="tooltip"
											:class="[ 'error-image', { 'active' : errors.has('phone') } ]"></span>
							</div>
						</div>
						<div class="row">
							<div class="k-form-field col-md-6 col-xs-12 form-group">
								<label class="floating-label active"
											 for="address">your address*</label>
								<vue-google-autocomplete
									id="address"
									types=""
									ref="address"
									data-vv-name="address"
									v-on:inputChange="updateAddressDetails"
									@change="updateAddressDetails"
									v-validate="'required'"
									v-model.trim="addressDetail"
									:disabled="getUserProfileUpdateStatus.isUserDetailsUpdateRequested"
									placeholder="address*"
									v-on:placechanged="getAddress">
								</vue-google-autocomplete>
								<span :title="errors.first('address')"
											data-toggle="tooltip"
											:class="[ 'error-image', { 'active' : errors.has('address') } ]"></span>
							</div>
							<div class="k-form-field col-md-6 col-xs-12 form-group">
								<label class="floating-label active"
											 for="city">city*</label>
								<input type="text"
											 id="city"
											 name="city"
											 data-vv-name="city"
											 v-validate="'required|alpha_spaces'"
											 v-model.trim="userDetails.Address.City"
											 placeholder="city"
											 :disabled="getUserProfileUpdateStatus.isUserDetailsUpdateRequested"
											 @input="validateField('city', userDetails.Address.City)">
								<span :title="errors.first('city')"
											data-toggle="tooltip"
											:class="[ 'error-image', { 'active' : errors.has('city') } ]"></span>
							</div>
						</div>
						<div class="row">
							<div class="k-form-field col-md-6 col-xs-12 form-group">
								<label class="floating-label active"
											 for="state">state*</label>
								<input type="text"
											 id="state"
											 name="state"
											 data-vv-name="state"
											 v-validate="'required|alpha_spaces'"
											 v-model.trim="userDetails.Address.State" placeholder="state/province" required
											 :disabled="getUserProfileUpdateStatus.isUserDetailsUpdateRequested"
											 @input="validateField('state', userDetails.Address.State)">
								<span :title="errors.first('state')"
											data-toggle="tooltip"
											:class="[ 'error-image', { 'active' : errors.has('state') } ]"></span>
							</div>
							<div class="k-form-field col-md-6 col-xs-12 form-group">
								<label class="floating-label active"
											 for="pincode">pincode*</label>
								<input type="text"
											 id="pincode"
											 name="pincode"
											 data-vv-name="pincode"
											 v-validate="'required'"
											 v-model.trim="userDetails.Address.Pin"
											 placeholder="pin/zip"
											 :disabled="getUserProfileUpdateStatus.isUserDetailsUpdateRequested"
											 @input="validateField('pincode', userDetails.Address.Pin)">
								<span :title="errors.first('pincode')"
											data-toggle="tooltip"
											:class="[ 'error-image', { 'active' : errors.has('pincode') } ]"></span>
							</div>
						</div>
						<div class="row">
							<div class="k-form-field form-group col-md-6 form-group">
								<label class="floating-label active"
											 for="country">country*</label>
								<input type="text"
											 id="country"
											 data-vv-name="country"
											 v-validate="'required|alpha_spaces'"
											 name="country"
											 v-model.trim="userDetails.Address.Country"
											 placeholder="country"
											 :disabled="getUserProfileUpdateStatus.isUserDetailsUpdateRequested"
											 @input="validateField('country', userDetails.Address.Country)">
								<span :title="errors.first('country')"
											data-toggle="tooltip"
											:class="[ 'error-image', { 'active' : errors.has('country') } ]"></span>
							</div>
							<div class="k-form-field form-group col-md-6 form-group">
								<label class="floating-label active"
											 for="gstin">gstin</label>
								<input type="text"
											 id="gstin"
											 name="gstin"
											 data-vv-name="gstin"
											 v-validate="'alpha_num'"
											 v-model="userDetails.GSTIN"
											 placeholder="GSTIN"
											 @input="validateField('gstin', userDetails.GSTIN)"
											 :disabled="getUserProfileUpdateStatus.isUserDetailsUpdateRequested">
								<span :title="errors.first('gstin')"
											data-toggle="tooltip"
											:class="[ 'error-image', { 'active' : errors.has('gstin') } ]"></span>
							</div>
						</div>
					</div>
			</form>
		</div>
		<div class="footer">
			<button @click="submitForm" type="submit"
							:class="['btn kbtn-primary proceed' , { 'loader-active':getUserProfileUpdateStatus.isUserDetailsUpdateRequested }]"
							:disabled="disableSaveButton">
				<span v-if="!getUserProfileUpdateStatus.isUserDetailsUpdateRequested">Proceed</span>
				<threeDots v-if="getUserProfileUpdateStatus.isUserDetailsUpdateRequested"></threeDots>
			</button>
		</div>
	</div>
	</div>
</template>

<script>
	import billingForm from './index'

	export default billingForm;
</script>

<style scoped lang="scss">
	@import '../../sass/components/billingform';
	@import '../../sass/components/form';
</style>

