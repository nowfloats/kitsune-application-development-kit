<template>
	<div class="base-modal-container">
		<div class="live-site-body">
			<div class="live-site-header">
				<div class="close-icon" @click="closeCustomerDetails($event)"></div>
				<img src="../../assets/icons/live-sites-dark.svg" alt="edit icon">
				<section>
					<p class="header">live site info</p>
				</section>
			</div>
			<div class="live-site-content">
				<section class="parts">
					<i class="domain"></i>
					<span>domain details</span>
					<div>
						<div class="type2">
							<p :class="['status', allDetails.IsActive ? 'active' : 'inactive']">
								<span v-if="allDetails.IsActive">active</span>
								<span v-else>inactive</span>
							</p>
							<p class="url">
								<i v-if="allDetails.IsSSLEnabled" class="ssl-icon"></i>
								<span class="ssl">https://</span>
								<span>{{websiteUrl}}</span>
							</p>
						</div>
						<p class="sub-info">last published on {{formatDate(allDetails.UpdatedOn)}}</p>						
					</div>
				</section>
				<section class="parts">
					<i class="cloud"></i>
					<span>hosting info</span>
					<div class="type1">
						<img :src="cloudIcon" class="parts-icon" alt="aws icon"/>
						<div>
							<p class="main-info">hosted on Amazon Web Services on {{formatDate(allDetails.CreatedOn)}}</p>
							<p class="sub-info">you can change hosting to other platforms like Alicloud, GCP and many more. check pricing here</p>
						</div>
					</div>
				</section>
				<section class="parts">
					<i class="customer"></i>
					<span>customer info</span>
					<div class="type1">
						<img src="../../assets/icons/customer-info.svg" class="parts-icon" alt="customer icon">
						<div>
							<p class="main-info">{{userDetails.fullName}}</p>
							<p>
								<span class="sub-info">{{userDetails.email}}</span>
								<span class="sub-info">{{userDetails.number}}</span>
							</p>
						</div>
					</div>
				</section>
				<section class="parts">
					<i class="project"></i>
					<span>project info</span>
					<div class="type1">
						<img src="../../assets/icons/projects-light.svg" alt="project icon" class="parts-icon projects">
						<div>
							<span class="main-info">{{allDetails.ProjectName}} (v{{allDetails.KitsuneProjectVersion}})</span>
							<span class="sub-info"></span>
						</div>
					</div>
				</section>			
			</div>
		</div>
	</div>
</template>

<script>
	import liveSiteView from './index'

	export default liveSiteView
</script>

<style scoped lang="scss">
	@import '../../sass/components/livesitemodal';
</style>
