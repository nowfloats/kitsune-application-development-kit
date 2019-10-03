 <template>
	<div class="modal-base-container">
			<div class="k-modal-container container-fluid">
				<div class="k-modal-header">
					<div class="k-modal-title">
						delete project
					</div>
					<button @click="closeDeleteProjectModal($event)"></button>
				</div>
				<!-- v-if="project.projectVersion === 1" -->
				<div>
					<div class="k-modal-content">
						do you want to delete the project '{{projectName}}'?
					</div>
					<div class="k-modal-footer">
					<button class="btn" :disabled="disableButton" @click="closeDeleteProjectModal($event)">no</button>
					<button class="btn" :disabled="disableButton" @click="removeProject">yes</button>
				</div>
				</div>
				<!-- <div v-else>
					<div class="k-modal-content">
						this is a published project. to delete it please contact us at support@getkitsune.com
					</div>
					<div class="k-modal-footer">
						<button @click="closeDeleteProjectModal" class="btn">ok</button>
					</div>
				</div> -->
			</div>
	</div>
</template>

<script>
	import DeleteProjectModal from './index'

	export default DeleteProjectModal;
</script>

<style scoped lang="scss">
	@import '../../sass/components/deleteprojectmodal';
</style>
