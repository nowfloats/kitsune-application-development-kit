<template>
	<div class="modal">
		<slot></slot>
		<slot name="header"></slot>
		<slot name="content"></slot>
		<slot name="footer"></slot>
	</div>
</template>

<script>
	import modal from './index'

	export default modal
</script>

<style>
	@import '../../sass/components/_modal.scss';
</style>
