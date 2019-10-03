<template>
	<div class="user-dropdown">
		<div class="profile-header">
			<span>PROFILE</span>
			<i class="back-arrow-profile" @click="toggleStatusHandler($event, ['overlay', 'profile'])"></i>
		</div>
		<div class="user-details">
			<div class="user-avatar">
				<img :src="user.ProfilePic" alt="user profile pic">
			</div>
			<div>
				<h2>{{user.DisplayName}}</h2>
			</div>
			<div>
				<span>{{user.Email}}</span>
			</div>
		</div>
		<wallet id="profile-wallet"></wallet>
		<div class="user-menu">
			<ul>
				<li :class="item.imageUrl" v-for="(item, index) in listItems" @click="dropDownOperations($event, index)">
					<a>
						<!--TODO update anchor to cover all space in li-->
						<span>{{item.label}}</span>
					</a>
				</li>
			</ul>
		</div>
		<div class="user-copyright">&#169; {{getYear}} team kitsune</div>
	</div>
</template>

<script>
	import dropdown from "./index";

	export default dropdown
</script>

<style scoped lang="scss">
	@import "../../sass/components/dropdown";
</style>
