<template>
	<line-chart :options="options" :chart-data="chartData" v-if="type === 'line'"></line-chart>
</template>

<script>
	import vuesticChart from './index'

	export default vuesticChart
</script>

<style lang="scss">
	@import "../../sass/components/_vuesticChart.scss";
</style>
