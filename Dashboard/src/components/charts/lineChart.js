import { Line, mixins } from 'vue-chartjs'
import defaultOptions from './defaultOptions'
const { reactiveProp } = mixins

export default Line.extend({
	props: ['chartData', 'options'],

	mixins: [reactiveProp],

	data () {
		return {
			ChartOptions: {

			}
		}
	},

	mounted () {
		this.addPlugin({
			id: 'my-plugin',
			beforeRender : function(chart){
			}
		})
		let options = Object.assign({}, defaultOptions, this.ChartOptions, this.options)
		this.renderChart(this.chartData, options)
	}
})
