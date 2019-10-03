import vuesticChart from '../charts/vuesticChart.vue'
import { mapGetters, mapActions } from "vuex"
import multiSelect from 'vue-multiselect'
import { daysInMonths } from "../../../config/config";
import threeDots from '../loaders/threeDots/ThreeDots.vue'

const defaultGraphConfig = {
	defaultBorderColor: 'rgb(219, 219, 219)',
	defaultDataBackgroundColor: 'rgb(98,98,98)',
	defaultDataRadius: 3,
	defaultDataHoverRadius: 5
};

const currentDateDataColor = 'rgb(240,100,0)';

const arbitraryDateValue = 115;

export default {
	name: 'billing',

	data() {
		return {
			customTooltips: {},
			isLoading: false,
			chartData: {},
			monthsInAnYear: ['January', 'February', 'March', 'April', 'May', 'June', 'July',
				'August', 'September', 'October', 'November', 'December'],
			selectMonth: [],
			selectYear: [],
			selectMonthInvoice: [],
			selectedMonth: new Date().toLocaleString('en-GB', { month: 'long' }),
			selectedYear: new Date().getFullYear(),
			selectedMonthInvoice: new Date(new Date().setMonth(new Date().getMonth() - 1))
				.toLocaleString('en-GB', { month: 'long' }),
			selectedYearInvoice: new Date(new Date().setMonth(new Date().getMonth() - 1))
				.getFullYear(),
			payload: {
				fromDate: new Date(new Date().getFullYear(), new Date().getMonth(), 1).toISOString().split('T')[0],
				endDate: new Date(new Date().getFullYear(), new Date().getMonth() + 1, 1).toISOString().split('T')[0]
			},
			component: 'request',
			invoicePayload: {
				month: new Date(new Date().setMonth(new Date().getMonth() - 1)).getMonth() + 1,
				year: new Date(new Date().setMonth(new Date().getMonth() - 1)).getFullYear()
			},
			showNoRequestMsg: true,
			showNoStorageMsg: true
		}
	},

	components: {
		vuesticChart,
		multiSelect,
		threeDots
	},

	computed: {
		...mapGetters ({
			transactions: 'getTransactionHistory',
			usage: 'getUsage',
			storage: 'getStorage',
			invoiceDetails: 'getInvoice',
			fetchingData: 'isFetchingUsageData',
			user: 'getUserDetails',
			fetchingInvoiceDetails: 'isFetchingInvoiceDetails',
			fetchingTransactions: 'isFetchingTransactionData'
		}),

		showTransactionsZeroth() {
			return this.transactions.length
		},

		reorderTransactions() {
			let reverseTransaction = this.transactions.slice();
			return reverseTransaction.reverse();
		},

		optionsForRequest() {
			return this.getOptionsForGraph('web requests', this.customTooltips);
		},

		optionsForStorage() {
			return this.getOptionsForGraph('web storage (in MB)', this.customTooltips);
		}
	},

	watch: {
		transactions() {
			const createdOn = new Date(this.user.CreatedOn),
				currentDate = new Date();
			if(currentDate.getFullYear() > createdOn.getFullYear()) {
				for(let year = createdOn.getFullYear(); year <= currentDate.getFullYear(); year++ ) {
					this.selectYear = [...this.selectYear, year];
				}
				this.selectMonth = this.monthsInAnYear;
				this.selectMonthInvoice = this.monthsInAnYear;
			}
			else {
				this.selectYear = [...this.selectYear, currentDate.getFullYear()];
				for(let month = createdOn.getMonth(); month <= currentDate.getMonth(); month++ ) {
					this.selectMonth = [...this.selectMonth, this.monthsInAnYear[month]];
					if(month < currentDate.getMonth()) {
						this.selectMonthInvoice = [...this.selectMonthInvoice, this.monthsInAnYear[month]];
					}
				}
			}
		},
		usage() {
			if(this.usage !== null) {
				this.showNoRequestMsg = false;
				this.chartData = this.generateChartData({ dataForGraph: this.usage, labelForGraph: 'requests' });
			}
			else {
				this.showNoRequestMsg = true;
			}
		},

		storage() {
			if(this.storage !== null) {
				this.showNoStorageMsg = false;
				this.chartData = this.generateChartData({ dataForGraph: this.storage, labelForGraph: 'storage' });
			}
			else {
				this.showNoStorageMsg = true;
			}
		}
	},

	methods: {
		...mapActions([
			'setTransactionHistory',
			'setUsageDetails',
			'setStorageDetails',
			'setInvoiceDetails'
		]),

		getOptionsForGraph: (label) => {
			//custom tooltips
			let customTooltips = function(tooltip) {
				// Tooltip Element
				let tooltipEl = document.getElementById('chartjs-tooltip');
				if (!tooltipEl) {
					tooltipEl = document.createElement('div');
					tooltipEl.id = 'chartjs-tooltip';
					tooltipEl.innerHTML = "<table></table>";
					this._chart.canvas.parentNode.appendChild(tooltipEl);
				}

				// Hide if no tooltip
				if (tooltip.opacity === 0) {
					tooltipEl.style.opacity = 0;
					return;
				}
				// Set caret Position
				tooltipEl.classList.remove('above', 'below', 'no-transform');
				tooltip.yAlign = "left";
				tooltip.xAlign = "left";
				if (tooltip.yAlign) {
					tooltipEl.classList.add(tooltip.yAlign);
				} else {
					tooltipEl.classList.add('no-transform');
				}

				function getBody(bodyItem) {
					return bodyItem.lines;
				}
				// Set Text
				if (tooltip.body) {
					let bodyLines = tooltip.body.map(getBody);
					let innerHtml = '<thead>';
					innerHtml += '</thead><tbody>';
					bodyLines.forEach(function (body, i) {
						innerHtml += '<tr><td>' + body + '</td></tr>';//
					});
					innerHtml += '</tbody>';

					let tableRoot = tooltipEl.querySelector('table');
					tableRoot.innerHTML = innerHtml;
				}
				let positionY = this._chart.canvas.offsetTop;
				let positionX = this._chart.canvas.offsetLeft;

				// Display, position, and set styles for font
				tooltipEl.style.opacity = 1;
				tooltipEl.style.cursor = 'pointer';
				tooltipEl.style.left = positionX + tooltip.caretX + 'px';
				tooltipEl.style.top = positionY + tooltip.caretY+15 + 'px';
			}
			return {
				legend: {
					display: false
				},
				tooltips: {
					enabled: false,
					mode: 'index',
					position: 'nearest',
					custom: customTooltips,
				},
				scales: {
					xAxes: [{
						gridLines: {
							display: false,
						}
					}],
					yAxes: [{
						gridLines: {
							display: true
						},
						scaleLabel: {
							display: true,
							labelString: label
						}
					}]
				},
				responsive: true,
				maintainAspectRatio: false
			}
		},

		getDateOfTransaction(dateTime) {
			const transactionDate = new Date(dateTime),
				date = transactionDate.getDate(),
				month = transactionDate.toLocaleString('en-GB', { month: 'short' }),
				year  = transactionDate.getYear() % 100
			return `${date} ${month} '${year}`
		},

		changeComponentHandler(component) {
			this.changeComponent(component);
			this.selectInput();
		},

		//generates chart data with points color based on the color given color
		generateDataSet(borderColor, dateToProcess, labelForGraph, dataForGraph) {
			const month = dateToProcess.getMonth(),
				year = dateToProcess.getFullYear(),
				labels = this.getDaysInMonth(month, year),
				selectedMonthNumber = new Date(Date.parse(`${this.selectedMonth} 1, ${this.selectedYear}`))
				.getMonth();
			return {
				datasets: [
					{
						label: labelForGraph,
						borderDash: [5, 4],
						fill: false,
						borderColor: borderColor,
						backgroundColor: borderColor,
						...this.formattedData(dataForGraph, labels, month, year, selectedMonthNumber)
					},
				],
				labels
			};
		},

		generateChartData({ dataForGraph, labelForGraph }) {
			const { defaultBorderColor } = defaultGraphConfig,
				currentDate = new Date().getTime();
			// taking index as 1 because 0th index is the data for previous month
			// but if prev month's last date had no requests so, the 0th index will contain data
			// for this month
			const dateToProcess = dataForGraph[1] ? dataForGraph[1].Key1 : dataForGraph[0].Key1;
			return (dataForGraph[1] !== undefined
				|| new Date(dataForGraph[0].Key1).getTime() === currentDate)
				? this.generateDataSet(defaultBorderColor, new Date(dateToProcess), labelForGraph, dataForGraph)
				: {
					datasets: [
						{
							label: labelForGraph,
							fill: false,
							borderColor: defaultBorderColor,
							backgroundColor: defaultBorderColor
						},
					],
				};
		},

		//to check if the date in label is the current date
		removeGraphAfterToday({ label, month, year, mapValue }) {
			const todaysDate = new Date(),
				presentMonth = () => label > todaysDate.getDate()
					&& month === todaysDate.getMonth()
					&& year === todaysDate.getFullYear();

			//if mapValue is present return mapValue
			//else if date is >= currentDate, then return NaN(to remove graph)
			//else return 0 to signify 0 request on that day
			return presentMonth()
				? mapValue ? mapValue : NaN
				: mapValue ? mapValue : 0
		},

		isNotCurrentMonth: (month, selectedMonthNumber, year) =>
			month < selectedMonthNumber || (month > selectedMonthNumber && year < Number(this.selectedYear)),

		formattedData(dataForGraph, labels, month, year, selectedMonthNumber) {
			const indexMap = new Map(),
				today = new Date();
			dataForGraph.forEach(({ Key1, DataCount }) => {
				let date = new Date(Key1),
					month = date.getMonth(),
					day = date.getDate(),
					year = date.getFullYear();
				let getIndex = this.isNotCurrentMonth.call(this, month, selectedMonthNumber, year) ?
					-1 : day;
				indexMap.set(getIndex, DataCount % 1 === 0 ? DataCount : DataCount.toFixed(2));
			});
			//check if today's date, if true return date else arbitrary value (115)
			const todaysDate = (selectedMonthNumber === today.getMonth() &&
				this.selectedYear === today.getFullYear()) ? today.getDate() : arbitraryDateValue;
			let dataObect = {
				data: [],
				pointBackgroundColor: [],
				pointBorderColor: [],
				pointRadius: [],
				pointHoverRadius: []
			};
			const { defaultDataBackgroundColor, defaultDataRadius, defaultDataHoverRadius } = defaultGraphConfig;
			return labels.reduce((acc, label, index) => {
				//getting the date from the label string
				let formatLabel = Number(label.toString().split(' ')[0]);
				// if value from indexmap is undefined and,
					// if index is 0, then take first value of indexMap that is -1
					// else if index is last, then take last value of indexmap,
					// else if its current day, remove graph from that day
					// else put the value as 0
				// else simply return the indexmap value
				acc.data.push(indexMap.get(label) === undefined
					? index === 0
						? indexMap.get(-1) ? indexMap.get(-1) : 0
						: this.removeGraphAfterToday({
							label: formatLabel,
							month,
							year,
							mapValue: indexMap.get(formatLabel)
						})
					: indexMap.get(label));
				//the below steps are for making the current date's datapoint look different than rest
				acc.pointBackgroundColor.push(formatLabel === todaysDate ? currentDateDataColor : defaultDataBackgroundColor);
				acc.pointBorderColor.push(formatLabel === todaysDate ? currentDateDataColor : defaultDataBackgroundColor);
				acc.pointRadius.push(formatLabel === todaysDate ? (defaultDataRadius + 2) : defaultDataRadius);
				acc.pointHoverRadius.push(formatLabel === todaysDate ? (defaultDataHoverRadius + 2) : defaultDataHoverRadius);
				return acc;
			}, dataObect);
		},

		getDateOfMonth : (year, month, date) => new Date(year, month, date).toISOString().split('T')[0],

		selectInput() {
			if(this.selectedMonth !== null && this.selectedYear !== null) {
				let month = (new Date(Date.parse(`${this.selectedMonth} 1, ${this.selectedYear}`)).getMonth()),
					fromDate = this.month === 0
						? this.getDateOfMonth(this.selectedYear - 1, month, 1)
						: this.getDateOfMonth(this.selectedYear, month, 1);
				let endDate = this.getDateOfMonth(this.selectedYear, month + 1, 1);
				this.payload = {
					fromDate,
					endDate
				};
				this.component === 'request' ? this.setUsageDetails(this.payload) : this.setStorageDetails(this.payload);
			}
		},

		getMonthName: (date, format) => date.toLocaleString('en-GB', { month: format }),

		getDaysInMonth(month, year) {
			let date = new Date(year, month, 1),
				currentMonth = this.getMonthName(date, 'short'),
				prevDate = new Date(year, month, 0),
				currentYear = date.getFullYear(),
				isLeapYear = currentYear % 100 === 0 ? currentYear % 400 === 0 : currentYear % 4 === 0,
				//if its february and leap year, take the 12 key in dayInMonths map, which has 29 days
				noOfDays = daysInMonths.get(date.getMonth() === 1 && isLeapYear ? 12 : date.getMonth()),
				daysInCurrentMonth = [...Array(noOfDays).keys()];
			daysInCurrentMonth.shift();
			return [
				`${prevDate.getDate()} ${this.getMonthName(prevDate, 'short')}`,
				...daysInCurrentMonth,
				`${noOfDays} ${currentMonth}`
			];
		},

		changeComponent(component) {
			this.component = component;
		},

		selectInputInvoice(){
			if(this.selectedMonthInvoice !== null && this.selectedYearInvoice !== null) {
				let newMonth = new Date(Date.parse(`${this.selectedMonthInvoice}/2/
				${this.selectedYearInvoice}`)).getMonth()+1;
				let newPayload = {
					month: newMonth,
					year: this.selectedYearInvoice
				};
				this.invoicePayload = newPayload;
				this.setInvoiceDetails(this.invoicePayload)
			}
		},

		downloadInvoice() {
			if(this.invoiceDetails.status === 'Found'){
				window.open(this.invoiceDetails.S3Link, '_blank');
			}
		}
	},

	created() {
		this.setTransactionHistory()
		this.setUsageDetails(this.payload)
		this.setInvoiceDetails(this.invoicePayload)
	}
}
