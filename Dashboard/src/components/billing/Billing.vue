<template>
	<div class="billing-container">
		<div class="sticky-title">
			<h2>
				<span class="sticky-title-text active">billing (kitsune managed cloud usage)</span>
			</h2>
		</div>
		<div class="billing-content wrapper">
			<div class="usage">
				<div class="billing-tab-container">
					<button class="billing-tab"
									@click="changeComponentHandler('request')"
									:class="{'active' : component === 'request' }">web requests</button>
					<button class="billing-tab"
									@click="changeComponentHandler('storage')"
									:class="{'active' : component === 'storage' }">storage</button>
				</div>
				<div class="select-duration row">
					<div class="col-md-3">
						<multi-select :options="selectMonth"
													:show-labels=false
													@input="selectInput"
													placeholder="select month"
													v-model="selectedMonth"></multi-select>
					</div>
					<div class="col-offset-md-2 col-md-3">
						<multi-select :options="selectYear"
													:show-labels=false
													@input="selectInput"
													placeholder="select year"
													v-model="selectedYear">
						</multi-select>
					</div>
				</div>
				<section class="content-section" v-if="component === 'request'">
					<div class="billing-graph">
						<vuesticChart :chart-data="chartData"
								v-if="!(showNoRequestMsg || fetchingData)"
								:options="optionsForRequest"
								type="line">
						</vuesticChart>
						<img src="../../assets/icons/web-requests-zeroth.svg"
								class="zeroth-image"
								alt=""
								v-if="showNoRequestMsg && !fetchingData">
						<p class="billing-message"
							  v-if="showNoRequestMsg && !fetchingData">
							no requests found for {{selectedMonth}}, {{selectedYear}}
						</p>
						<threeDots v-if="fetchingData"></threeDots>
					</div>
				</section>
				<section  class="content-section" v-if="component === 'storage'">
					<div class="billing-graph">
						<vuesticChart :chart-data="chartData"
							v-if="!(showNoStorageMsg || fetchingData)"
							:options="optionsForStorage"
							type="line">
						</vuesticChart>
						<img src="../../assets/icons/hosting-zeroth.svg"
								 class="zeroth-image"
								 alt="hosting zeroth image"
								 v-if="showNoStorageMsg && !fetchingData">
						<p class="billing-message"
							 v-if="showNoStorageMsg && !fetchingData">
							no storage available for {{selectedMonth}}, {{selectedYear}}
						</p>
						<threeDots v-if="fetchingData"></threeDots>
					</div>
				</section>
			</div>
			<div class="transaction-invoice">
				<div class="transaction">
					<p class="section-title">transaction history</p>
					<div class="transaction-header">
						<p>transactions</p>
						<p>date</p>
					</div>
					<div class="transaction-content">
						<section v-if="!fetchingTransactions">
							<div v-if="showTransactionsZeroth">
								<div v-for="({ Status, Amount, UpdatedOn }, index) in reorderTransactions"
										 class="transactions">
									<p :class="[Status === 'Credit' ? 'credit' : 'debit']">
										<span v-if="Status === 'Credit'">+</span>
										<span v-else="">- </span>INR {{Amount}}</p>
									<p>{{getDateOfTransaction(UpdatedOn)}}</p>
								</div>
							</div>
							<div class="transactions-zeroth" v-else>
								<img src="../../assets/icons/transaction-zeroth.svg" class="zeroth-image" alt="no transactions found">
								<p>no transactions yet</p>
							</div>
						</section>
						<threeDots v-else></threeDots>
					</div>
				</div>
				<section class="invoice">
					<div class="invoice-title">
						view invoice
					</div>
					<div class="invoice-body">
						<div class="invoice-select">
							<multi-select :options="selectMonthInvoice"
								:show-labels=false
								@input="selectInputInvoice"
								placeholder="month"
								v-model="selectedMonthInvoice">
							</multi-select>
						</div>
						<div class="invoice-select-year">
							<multi-select :options="selectYear"
								:show-labels=false
								@input="selectInputInvoice"
								placeholder="year"
								v-model="selectedYearInvoice">
							</multi-select>
						</div>
						<div>
							<button v-if="invoiceDetails.status == 'Found' || invoiceDetails.status == 'default'"
								:disabled="fetchingInvoiceDetails"
								@click="downloadInvoice"
								:class="[ fetchingInvoiceDetails ? 'loading' : 'invoice-btn']"
								class="btn kbtn">
								<span v-if="!fetchingInvoiceDetails">Download</span>
								<threeDots v-else></threeDots>
							</button>
							<p class="link-not-found" v-else>no invoice generated in {{selectedMonthInvoice}} {{selectedYearInvoice}}. please
								select another month</p>
						</div>
					</div>
				</section>
			</div>
		</div>
	</div>
</template>

<script>
	import billing from './index';

	export default billing;
</script>

<style scoped lang="scss">
	@import "../../sass/components/billing";
</style>

<!-- adding these internal styles so that these styles can override the above scoped styles -->
<!-- not using scss variables : variables are not available in css -->

<style lang="scss">
	.multiselect__input, .multiselect__single {
		padding: 0;
		margin-bottom: 0;
	}
	.multiselect__tags {
		min-height: 1rem;

		@media (min-width: 768px) {
			min-height: 2rem;
		}
	}
	.multiselect__select:before {
		border-color: #f06428 transparent transparent;
	}
</style>

<style lang="scss">
	#chartjs-tooltip {
		opacity: 1;
		position: absolute;
		background: rgba(0, 0, 0,0.7);
		color: #ffffff;
		border-radius: 2px;
		-webkit-transition: all .1s ease;
		transition: all .1s ease;
		pointer-events: none;
		-webkit-transform: translate(-50%, 0);
		transform: translate(-50%, 0);
		text-align: left;
		font-size: 0.8em;
		z-index: 1;
		cursor: pointer;
		width: 101px;

	&:before {
		 display: block;
		 content: '';
		 width: 0;
		 height: 0;
		 border-left: 13px solid transparent;
		 border-right: 13px solid transparent;
		 border-bottom: 13px solid transparentize(#000000, .3);
		 position: absolute;
		 top: -13px;
		 right: 37px;
		 border-radius: 2px;
		 z-index: 1;
	 }
	}
	#chartjs-tooltip td,#chartjs-tooltip th {
		padding: .4rem;
	}
	.chartjs-tooltip-key {
		display: block;
		width: 10px;
		height: 10px;
		margin-right: 10px;
	}
</style>

<style>
	.ball-loader-ball {
		background-color: #f06428;
	}
</style>
