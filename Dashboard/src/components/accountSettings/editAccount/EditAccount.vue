<template>
	<div class="edit-account">
		<div class="profile-pic row">
			<img :src="user.ProfilePic" alt="">
			<!--<div class="change-pic">
				<button>change photo</button>
				<p>preferabel size of photo is 200px x 200px</p>
			</div>-->
		</div>
		<form novalidate>
			<div class="k-form">

				<div class="row">
					<div class="k-form-field override-margin col-md-6 col-xs-12 form-group">
						<label class="floating-label active"
									 for="name"><span v-if="errors.has('name') && componentStatus.hamburger" class="error">
							your name is required</span>
							<span v-else>your name*</span>
						</label>
						<input v-model="newDetails.Name"
									 type="text"
									 placeholder="your name*"
									 id="name"
									 name="name"
									 data-vv-name="name"
									 v-validate="'required|alpha_spaces'"
									 @input="validateField('name', newDetails.Name)"
									 :class="{ 'disabled' : updateStatus }"
									 :disabled="updateStatus"
									 required>
						<span :title="errors.first('name')"
									data-toggle="tooltip"
									:class="[ 'error-image', { 'active' : errors.has('name') } ]"></span>
					</div>
					<div class="override-margin k-form-field col-md-6 col-xs-12 form-group">
						<label class="floating-label active"
									 for="role">your role</label>
						<input class="disabled" id="role" disabled value="admin" type="text">
						<p></p>
					</div>
				</div>
				<div class="row">
					<div class="override-margin k-form-field col-md-6 col-xs-12 form-group">
						<label class="floating-label active"
									 for="email">email address</label>
						<input class="disabled"
									 id="email"
									 placeholder="email address"
									 v-model.trim="newDetails.UserEmail"
									 disabled type="text">
						<p></p>
					</div>
					<div class="override-margin k-form-field col-md-6 col-xs-12 form-group">
						<label class="floating-label active"
									 for="phone">
										<span  v-if="errors.has('phone') && componentStatus.hamburger" class="error">
											mobile number is invalid</span>
							<span v-else>mobile number*</span>
						</label>
						<input v-model.trim="newDetails.PhoneNumber"
									 id="phone"
									 name="phone"
									 data-vv-as="mobile number"
									 data-vv-name="phone"
									 v-validate="'required|digits:10'"
									 :class="{ 'disabled' : updateStatus }"
									 :disabled="updateStatus"
									 @input="validateField('phone', newDetails.PhoneNumber)"
									 placeholder="mobile number*"
									 required
									 type="text">
						<span :title="errors.first('phone')"
									data-toggle="tooltip"
									:class="[ 'error-image', { 'active' : errors.has('phone') } ]"></span>
						<!--/<span v-show="errors.has('newDetails.PhoneNumber')" class="help is-danger">{{ errors.first('newDetails.PhoneNumber') }}</span>-->
					</div>
				</div>
				<div class="row">
					<div class="k-form-field override-margin col-md-6 col-xs-12 form-group">
						<label class="floating-label active"
									 for="addressDetail">
							<span  v-if="errors.has('address') && componentStatus.hamburger" class="error">
								address is invalid</span>
							<span v-else="">your address*</span>
						</label>
						<vue-google-autocomplete
							id="addressDetail"
							ref="addressDetail"
							classname="input"
							:class="{ 'disabled' : updateStatus }"
							types=""
							data-vv-name="address"
							v-validate="'required'"
							v-on:inputChange="updateAddressDetails"
							@change="updateAddressDetails"
							placeholder="address*"
							v-model.trim="addressDetail"
							v-on:placechanged="getAddressData">
						</vue-google-autocomplete>
						<span :title="errors.first('address')"
									data-toggle="tooltip"
									:class="[ 'error-image', { 'active' : errors.has('address') } ]"></span>
					</div>
					<div class="k-form-field override-margin col-md-6 col-xs-12 form-group">
						<label class="floating-label active"
									 for="city">
							<span  v-if="errors.has('city') && componentStatus.hamburger" class="error">city is invalid</span>
							<span v-else>city*</span>
						</label>
						<input v-model.trim="newDetails.Address.City"
									 :disabled="updateStatus"
									 placeholder="city*"
									 name="city"
									 @input="validateField('city', newDetails.Address.City)"
									 data-vv-name="city"
									 v-validate="'required|alpha_spaces'"
									 required
									 :class="{ 'disabled' : updateStatus }"
									 id="city"
									 type="text">
						<span :title="errors.first('city')"
									data-toggle="tooltip"
									:class="[ 'error-image', { 'active' : errors.has('city') } ]"></span>
					</div>
				</div>
				<div class="row">
					<div class="k-form-field override-margin col-md-6 col-xs-12 form-group">
						<label class="floating-label active"
									 for="state">
							<span  v-if="errors.has('state') && componentStatus.hamburger" class="error">state is invalid</span>
							<span v-else="">state*</span>
						</label>
						<input v-model.trim="newDetails.Address.State"
									 :disabled="updateStatus"
									 name="state"
									 data-vv-name="state"
									 v-validate="'required|alpha_spaces'"
									 placeholder="state*"
									 @input="validateField('state', newDetails.Address.State)"
									 :class="{ 'disabled' : updateStatus }"
									 id="state" type="text">
						<span :title="errors.first('state')"
									data-toggle="tooltip"
									:class="[ 'error-image', { 'active' : errors.has('state') } ]"></span>
					</div>
					<div class="k-form-field override-margin col-md-6 col-xs-12 form-group">
						<label class="floating-label active"
									 for="pincode">
							<span  v-if="errors.has('pincode') && componentStatus.hamburger" class="error">pincode is invalid</span>
							<span v-else="">pincode*</span>
						</label>
						<input v-model.trim="newDetails.Address.Pin"
									 :disabled="updateStatus"
									 name="pincode"
									 data-vv-name="pincode"
									 v-validate="'required'"
									 placeholder="pincode*"
									 @input="validateField('pincode', newDetails.Address.Pin)"
									 :class="{ 'disabled' : updateStatus }"
									 id="pincode" type="text">
						<span :title="errors.first('pincode')"
									data-toggle="tooltip"
									:class="[ 'error-image', { 'active' : errors.has('pincode') } ]"></span>
					</div>
				</div>
				<div class="row">
					<div class="override-margin k-form-field col-md-6 col-xs-12 form-group">
						<label class="floating-label active"
									 for="country">
							<span  v-if="errors.has('country') && componentStatus.hamburger" class="error">country is invalid</span>
							<span v-else>country*</span>
						</label>
						<input v-model.trim="newDetails.Address.Country"
									 id="country"
									 name="country"
									 data-vv-name="country"
									 v-validate="'required|alpha_spaces'"
									 @input="validateField('country', newDetails.Address.Country)"
									 :class="{ 'disabled' : updateStatus }"
									 :disabled="updateStatus"
									 placeholder="country*"
									 type="text">
						<span :title="errors.first('country')"
									data-toggle="tooltip"
									:class="[ 'error-image', { 'active' : errors.has('country') } ]"></span>
					</div>
					<div class="override-margin k-form-field col-md-6 col-xs-12 form-group">
						<label class="floating-label active"
									 for="gstin">GSTIN</label>
						<input id="gstin"
									 name="gstin"
									 v-model="newDetails.GSTIN"
									 placeholder="GSTIN"
									 @input="validateField('gstin', newDetails.GSTIN)"
									 :class="{ 'disabled' : updateStatus }"
									 :disabled="updateStatus"
									 data-vv-name="gstin"
									 v-validate="'alpha_num'"
									 type="text">
						<span :title="errors.first('gstin')"
									data-toggle="tooltip"
									:class="[ 'error-image', { 'active' : errors.has('gstin') } ]"></span>
					</div>
				</div>
				<div class="save-user-details">
					<button :class="['kbtn-primary', !showSaveButton ? 'save-disabled' : { 'loader-container':updateStatus} ]"
									type="submit"
									:disabled="!showSaveButton"
									@click.prevent="submitForm">
						<span v-if="!updateStatus">save</span>
						<threeDots v-if="updateStatus"></threeDots>
					</button>
					<!--<button class="loader-container" v-if="!UpdateStatus"></button>-->
				</div>
			</div>
		</form>
	</div>
</template>

<script>
	import editAccount from './index'

	export default editAccount
</script>

<style scoped lang="scss">
	@import "../../../sass/components/_editAccount.scss";
</style>
