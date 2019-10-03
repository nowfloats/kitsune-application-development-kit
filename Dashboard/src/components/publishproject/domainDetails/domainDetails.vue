<template>
    <div class="base-modal-container">
        <div class="domain-body">
			<div class="domain-header">
				<div class="close-icon" @click="closeModalHandler($event)"></div>
				<img src="../../../assets/icons/publish-dark.svg" alt="cloud icon">
				<section>
					<p class="header">choose sub domain for</p>
                    <p class="subheader">{{projectName}}</p>
				</section>
			</div>
			<div class="domain-content">
				<div class="enter-params">
					<form id="subDomainForm">
                        <div class="form-group">
                            <div class="input-container">
                                <input v-model="subDomain"
                                type="text"
                                @keyup.enter="submitDomain($event)"
                                class="form-control"
                                placeholder="sub domain"
                                @input="validateSubDomain()">
                                <span><nobr>{{getDomain}}</nobr></span>
                            </div>
                            <span :class="['floating-label', { active : subDomain != ''}]">sub domain</span>
                            <span title="this field is required" :class="[ 'error-message domain', { active: showError }]"></span>                            
			                <p class="site-error" v-if="showSiteError">sub domain already exists</p>
                        </div>                        
                    </form>
				</div>
				<div class="domain-footer">
					<button type="button" class="btn kbtn-secondary" @click="backBtnHandler()">back</button>
					<button type="button" :class="['btn kbtn-primary', { 'loader-active' : showLoader }]"
                        :disabled="disableBtn"
                        @click="submitDomain($event)">
                        <span v-if="!showLoader">proceed</span>
					    <threeDots v-else></threeDots>
                    </button>
				</div>
			</div>
		</div>
    </div>
</template>
<script>
    import domainDetails from './index';

    export default domainDetails;
</script>
<style lang="scss" scoped>
	@import '../../../sass/components/publish/domainDetails';
	@import '../../../sass/components/publish/createcustomerbody';
</style>