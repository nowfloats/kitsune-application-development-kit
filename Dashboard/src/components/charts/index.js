import lineChart from './lineChart'

export default {
	name: 'vuestic-chart',

	props: ['chartData', 'options', 'type'],

	components: {
		lineChart
	}
}
