<template>
	<div>
		<multiselect v-model="customer"
								 :options="getCustomerList"
								 :custom-label="nameWithLang"
								 placeholder="select customer"
								 label="CustomerName"
								 track-by="WebsiteDomain"
								 :show-labels="false"
								 @input="publishProject">
			<template slot="option" scope="props">
				<span>{{props.option.WebsiteDomain.toLowerCase()}}</span>
			</template>
		</multiselect>
	</div>
</template>

<script>
	import CustomDropdown from './index'

	export default CustomDropdown
</script>

<style src="vue-multiselect/dist/vue-multiselect.min.css"></style>
<style lang="scss">
	@import "../../sass/components/customdropdown";
</style>
