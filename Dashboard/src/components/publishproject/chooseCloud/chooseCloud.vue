<template>
    <div class="base-modal-container">
        <div class="cloud-body">
			<div class="cloud-header">
				<div class="close-icon" @click="closeModalHandler($event)"></div>
				<img src="../../../assets/icons/cloud-upload-dark.svg" alt="edit icon">
				<section>
					<p class="header">choose cloud platform</p>
					<p class="subheader">{{projectDetails.projectName}}</p>
				</section>
			</div>
			<div class="cloud-content">
				<div class="choose-platform">
					<div @click="setCloudOption(0)" :class="{ 'cloud-checked' : this.selectedCloud == cloudOptions.get(0) }">
						<img src="../../../assets/icons/AWS.svg"
							:class="['cloud']"
							alt="aws icon">
						<p class="cloud-name">amazon web services</p>
						<p :class="['radio', { 'cloud-checked' : this.selectedCloud == cloudOptions.get(0) }]"></p>
					</div>
					<div title="coming-soon" v-tippy="{ followCursor: true, size: 'small' }"
							 class="disable-pointer" :class="{ 'cloud-checked' : this.selectedCloud == cloudOptions.get(3) }">
						<img src="../../../assets/icons/azure.svg"
							:class="['cloud', 'gray-scale']"
							alt="azure icon">
						<p :class="['radio', { 'cloud-checked' : this.selectedCloud == cloudOptions.get(3) }]"></p>
						<div>
							<p class="cloud-name">microsoft azure</p>
							<p class="cloud-account">(coming soon)</p>
						</div>
					</div>
					<div @click="setCloudOption(1)" :class="{ 'cloud-checked' : this.selectedCloud == cloudOptions.get(1) }">
						<img src="../../../assets/icons/alibaba-cloud.svg"
							:class="['cloud']"
							alt="alicloud icon">
						<p class="cloud-name">alibaba cloud</p>
						<p :class="['radio', { 'cloud-checked' : this.selectedCloud == cloudOptions.get(1) }]"></p>
					</div>
					<div title="coming-soon" v-tippy="{ followCursor: true, size: 'small' }"
							 class="disable-pointer" :class="{ 'cloud-checked' : this.selectedCloud == cloudOptions.get(2) }">
						<img src="../../../assets/icons/pingan.svg"
							:class="['cloud', 'gray-scale']"
							 alt="pingan icon">
						<div>
							<p class="cloud-name">pingan cloud</p>
							<p class="cloud-account">(coming soon)</p>
						</div>
						<p :class="['radio', { 'cloud-checked' : this.selectedCloud == cloudOptions.get(2) }]"></p>
					</div>
				</div>
				<div class="cloud-footer">
					<div class="cloud-message">
						<div v-if="selectedCloud == cloudOptions.get(1)">
							<div title="you need to give credentials for your AliCloud account"
									 v-tippy="{ placement: 'top', size: 'small', arrow: true, arrowType: 'round' }" class="radio-container radio-options">
								<input type="radio" class="custom-radio-btn" id="myAccount" :value="true" v-model="ownAccount">
								<span class="radio-checked" v-if="ownAccount"></span>
								<label for="myAccount" :class="['message', { selected : ownAccount }]">use my own AliCloud account</label>
							</div>
							<div title="your site will be hosted on kitsune's AliCloud accont"
									 v-tippy="{ placement: 'top', size: 'small', arrow: true, arrowType: 'round' }" class="radio-container radio-options">
								<input type="radio" class="custom-radio-btn" id="kitsuneAccount" :value="false" v-model="ownAccount">
								<span class="radio-checked" v-if="ownAccount == false"></span>
								<label for="kitsuneAccount" :class="['message', { selected : ownAccount == false }]">use kitsune's AliCloud account</label>
							</div>
						</div>
						<div v-if="selectedCloud == cloudOptions.get(0) || selectedCloud == cloudOptions.get(2)"
							class="radio-options message">
							<img src="../../../assets/icons/info-dark.svg" alt="info icon" class="info-icon">
							<p class="cloud-account">{{infoMessage}}</p>
						</div>
						<p class="cloud-account"></p>
					</div>
					<div>
						<button class="btn kbtn-secondary" @click="backBtnHandler()">back</button>
						<button class="btn kbtn-primary" :disabled="disableBtn" @click="proceedBtnHandler()">proceed</button>
					</div>
				</div>
			</div>
		</div>
    </div>
</template>
<script>
    import chooseCloud from './index';

    export default chooseCloud;
</script>
<style lang="scss" scoped>
	@import "../../../sass/components/publish/chooseCloud";
</style>
