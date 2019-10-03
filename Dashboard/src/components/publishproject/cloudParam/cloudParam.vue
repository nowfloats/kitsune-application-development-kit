<template>
    <div class="base-modal-container">
        <div class="cloud-param-body">
			<div class="cloud-param-header">
				<div class="close-icon" @click="closeModalHandler($event)"></div>
				<img :src="paramModalData.icon" alt="cloud icon">
				<section>
					<p class="header">{{paramModalData.header}}</p>
				</section>
			</div>
			<div class="cloud-param-content">
				<div class="enter-params" v-if="selectedCloud == cloudOptions.get(1)">
					<form>
                        <div class="form-group">
                            <input v-model="aliCloud.accountId"
                                type="text"
                                @keyup.enter="proceedBtnHandler"
                                class="form-control"
                                placeholder="account id"
                                @keyup="validateField(0, 'accountId', cloudOptions.get(1))">
                            <span :class="['floating-label', { active : aliCloud.accountId != ''}]">account id</span>
                            <span title="this field is required" :class="[ 'error-message', { active: showField1Error }]"></span>
                        </div>
                        <div class="form-group">
                            <input v-model="aliCloud.key"
                                type="text"
                                @keyup.enter="proceedBtnHandler"
                                class="form-control"
                                placeholder="key"
                                @keyup="validateField(1, 'key', cloudOptions.get(1))">
                            <label :class="['floating-label', { active : aliCloud.key != ''}]">key</label>
                            <span title="this field is required" :class="[ 'error-message', { active: showField2Error }]"></span>
                        </div>
                        <div class="form-group">
                            <input v-model="aliCloud.region"
                                type="text"
                                @keyup.enter="proceedBtnHandler"
                                class="form-control"
                                placeholder="region"
                                @keyup="validateField(2, 'region', cloudOptions.get(1))">
                            <label :class="['floating-label', { active : aliCloud.region != ''}]">region</label>
                            <span title="this field is required" :class="[ 'error-message', { active: showField3Error }]"></span>
                        </div>
                        <div class="form-group">
                            <input v-model="aliCloud.secret"
                                type="text"
                                @keyup.enter="proceedBtnHandler"
                                class="form-control"
                                placeholder="secret"
                                @keyup="validateField(3, 'secret', cloudOptions.get(1))">
                            <label :class="['floating-label', { active : aliCloud.secret != ''}]">secret</label>
                            <span title="this field is required" :class="[ 'error-message', { active: showField4Error }]"></span>
                        </div>
                    </form>
				</div>
                <div class="enter-params" v-if="selectedCloud == cloudOptions.get(2)">
                    <div class="steps">
                        <p><span class="weight-500">step 1)</span> login &amp; authenticate your GCP account <a :href="gcpTokenGeneratorUrl" class="url" target="_blank">here</a></p>
                        <p><span class="weight-500">step 2)</span> copy &amp; paste the Firebase CLI token below</p>
                    </div>
                    <form id="gcpParamForm">
                        <div class="form-group">
                            <input v-model="gcpCloud.secret"
                                type="text"
                                @keyup.enter="proceedBtnHandler"
                                class="form-control"
                                placeholder="paste your token here"
                                @keyup="validateField(0, 'secret', cloudOptions.get(2))">
                            <span :class="['floating-label', { active : gcpCloud.secret != ''}]">Firebase CLI token</span>
                            <span title="this field is required" :class="[ 'error-message', { active: showField1Error }]"></span>
                        </div>
                        <!-- <div class="form-group">
                            <input v-model="gcpCloud.mongoCreds"
                                type="text"
                                @keyup.enter="proceedBtnHandler"
                                class="form-control"
                                placeholder="mongo credentials"
                                @keyup="validateField(1, 'mongoCreds', cloudOptions.get(2))">
                            <label :class="['floating-label', { active : gcpCloud.mongoCreds != ''}]">mongo credentials</label>
                            <span title="this field is required" :class="[ 'error-message', { active: showField2Error }]"></span>
                        </div> -->
                    </form>
                </div>
				<div class="cloud-footer">
					<button class="btn kbtn-secondary" @click="backBtnHandler()">back</button>
					<button class="btn kbtn-primary"
                        :disabled="disableBtn"
                        @click="proceedBtnHandler()">proceed</button>
				</div>
			</div>
		</div>
    </div>
</template>
<script>
    import cloudParam from './index';

    export default cloudParam;
</script>
<style lang="scss" scoped>
	@import '../../../sass/components/publish/cloudParam';
	@import '../../../sass/components/publish/createcustomerbody';
</style>
