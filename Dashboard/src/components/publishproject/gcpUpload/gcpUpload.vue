<template>
    <div class="base-modal-container">
		<div class="gcp-upload-body">
			<div class="gcp-upload-header">
				<div class="close-icon" @click="closeHandler($event)"></div>
				<img src="../../../assets/icons/pingan.svg" alt="edit icon">
				<section>
					<p class="header">upload credentials.json for</p>
                    <p class="subheader">PingAn Cloud</p>
				</section>
			</div>
			<div class="gcp-upload-content" v-if="fileToUpload">
				<file-upload 
                    class="gcp-upload-upload toUpload"
                    ref="upload"
                    @input-file="uploadCredentials"
                    :drop="true"
                    v-model="creds">
                    <img src="../../../assets/icons/cloud-upload-dark.svg" class="toUpload" alt="upload icon">
                    <p>drag or <span>click here</span> to upload</p>
                    <p>the credentials.json for PingAn</p>
                </file-upload>
                <p @click="backButtonHandler($event)">
                    <span class="previous-step">back to previous step</span>
                </p>
			</div>
            <div class="gcp-upload-content" v-else>
                <div class="gcp-upload-upload uploaded">
                    <img src="../../../assets/icons/cloud-upload-dark.svg" class="uploaded" alt="upload icon">
                    <div>
                        <p class="file-name"><span>{{file.name}}</span> ({{file.size}})</p>
                        <p>
                            <file-upload 
                                ref="change"
                                @input-file="uploadCredentials"
                                v-model="creds">
                                <span class="file-options upload">change file</span>
                            </file-upload>
                            <span class="file-options" @click="removeFile()">remove file</span>
                        </p>
                    </div>
                </div>
                <div class="gcp-upload-footer">
                    <button class="btn kbtn-secondary" @click="removeFile()">back</button>
                    <button class="btn kbtn-primary" @click="saveFile($event)">proceed</button>
                </div>
			</div>
		</div>
	</div>
</template>

<script>
    import gcpUpload from './index';

    export default gcpUpload;
</script>

<style lang="scss" scoped>
    @import "../../../sass/components/publish/gcpUpload";
</style>
