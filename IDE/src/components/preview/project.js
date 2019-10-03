import React, { Component } from 'react';
import PropTypes from 'prop-types';
import axios from 'axios';
import { config } from '../../config';
import Select from 'react-select';
import logo from '../../images/logo.svg';
import timeIcon from '../../images/time.svg';
import closeIcon from '../../images/close-grey.svg';
import editIcon from '../../images/edit.svg';
import datasetIcon from '../../images/change-dataset.svg';
import desktopIcon from '../../images/desktop-icon.svg'
import tabletIcon from '../../images/tablet-icon.svg'
import phoneIcon from '../../images/phone-icon.svg'
import desktopIconActive from '../../images/desktop-icon-active.svg'
import tabletIconActive from '../../images/tablet-icon-active.svg'
import phoneIconActive from '../../images/phone-icon-active.svg'
import { toastr } from "react-redux-toastr";
import { connect } from 'react-redux';

class PreviewProject extends Component {
	constructor(props) {
		super(props);
		this.getCustomerList = this.getCustomerList.bind(this);
		this.handleCustomerChange = this.handleCustomerChange.bind(this);
		this.getKAdminUrl = this.getKAdminUrl.bind(this);
		this.kAdminLoginHandle = this.kAdminLoginHandle.bind(this);
		this.removeFrame = this.removeFrame.bind(this);
		this.triggerResponsive = this.triggerResponsive.bind(this);
		const { desktop } = config.INTERNAL_SETTINGS.responsivePreview;
		this.state = {
			isLoading: true,
			customerID: this.props.params.customerid,
			loadTime: '0.00',
			options: [],
			defaultCustomer: null,
			isDefault: false,
			responsive: desktop.name
		}
	}

	triggerResponsive(target) {
		const { none } = config.INTERNAL_SETTINGS.responsivePreview;
		this.setState(this.state.responsive === target ? { responsive: none.name } : { responsive: target });
	}

	handleCustomerChange(val) {
		this.setState({
			customerID: val.value,
			isDefault: val.value === this.state.defaultCustomer
		});
	}

	removeFrame() {
		window.location = `//${this.state.customerID}.demo.getkitsune.com?v=${Date.now()}`;
	}

	kAdminLoginHandle() {
		const adminTab = window.open('', '_blank');
		adminTab.document.write('loading kadmin...');
		this.getKAdminUrl()
			.then(({ data }) => adminTab.location.href = data.RedirectUrl);
	}

	getKAdminUrl = () => new Promise((resolve, reject) => {
		const { customerID } = this.state;
		axios.get(`${config.API.kAdminLogin}${customerID}`)
			.then(response => {
				resolve(response);
			})
			.catch(error => {
				toastr.error('unable to fetch your login details');
				reject(error);
			})
	});

	getCustomerList = () => new Promise((resolve, reject) => {
		const { userID } = this.props.login;
		axios.get(`${config.API.userToken}${userID}`)
			.then(response => {
				const { Id: userAuth } = response.data;
				axios.defaults.headers.common['Authorization'] = userAuth;
				axios.get(`${config.API.customers}${this.props.params.projectid}`)
					.then(response => {
						const options = response.data.Websites.map(item => ({
							className: 'data-options',
							value: item.WebsiteId,
							label: item.WebsiteTag
						}));
						const defaultCustomer = response.data.Websites[response.data.Websites.length - 1].WebsiteId;
						resolve({ options, defaultCustomer });
					})
					.catch(error => {
						toastr.error('unable to fetch your datasets');
						reject(error);
					});
			});
	});

	componentDidMount() {
		this.getCustomerList()
			.then(response => {
				const { domContentLoadedEventEnd, navigationStart } = window.performance.timing;
				let loadtime = ((domContentLoadedEventEnd - navigationStart)/1000).toString();
				//Check if customerid coming from route is 'default'
				const newState = {
					loadTime: loadtime,
					isLoading: false,
					options: response.options,
					defaultCustomer: response.defaultCustomer,
					isDefault: this.state.customerID === response.defaultCustomer
				};
				this.setState(this.props.params.customerid === 'default' ?
				//if yes, set customerID as defaultCustomer
				{
					...newState,
					customerID: response.defaultCustomer,
					isDefault: true
				} : newState);
			})
			.catch(error => {
				this.setState({ isLoading: false });
				console.log(error); //eslint-disable-line
			})
	}

	render() {
		if(!this.state.isLoading) {
			const { responsivePreview } = config.INTERNAL_SETTINGS;
			const { phone, tablet, desktop } = config.INTERNAL_SETTINGS.responsivePreview;
			const { responsive } = this.state;
			const iFrameStyle = {
				width: responsivePreview[responsive].width,
				height: responsivePreview[responsive].height
			};
			return (
				<div className='preview-container'>
					<nav className='preview-header'>
						<section className='preview-left'>
							<a className='preview-branding' href='#' alt='kitsune logo' >
								<img src={logo} alt='kitsune logo' />
							</a>
							<div className='preview-timestamp' title='load time of the site'>
								<img src={timeIcon} alt='time icon' />
								<span>{this.state.loadTime}s</span>
							</div>
							<div className='preview-responsive'>
								<img src={responsive === desktop.name ? desktopIconActive : desktopIcon}
									alt='desktop icon'
									onClick={() => this.triggerResponsive(desktop.name)}
									title={responsive === desktop.name ?
										'click to disable' : 'preview of how the site will look on desktop'} />
								<img src={responsive === tablet.name ? tabletIconActive : tabletIcon}
									alt='tablet icon'
									onClick={() => this.triggerResponsive(tablet.name)}
									title={responsive === tablet.name ?
										'click to disable' : 'preview of how the site will look on tablet'} />
								<img src={responsive === phone.name ? phoneIconActive : phoneIcon}
									alt='phone icon'
									onClick={() => this.triggerResponsive(phone.name)}
									title={responsive === phone.name ?
										'click to disable' : 'preview of how the site will look on mobile'} />
							</div>
						</section>
						<section className='preview-center'>
							{
								this.state.options.length > 1 ?
									<div className='preview-button customer-select'
										title='change the dataset to preview your site for different customers'>
										<img src={datasetIcon} />
										<span>select customer</span>
										<Select name='dataset-select'
											value={this.state.options.find(option => option.value === this.state.customerID)}
											options={this.state.options}
											onChange={this.handleCustomerChange}
											className='k-modal-dropdown preview-dropdown' />
									</div> :
									null
							}
							{
								this.state.isDefault ?
									<div className='preview-button'
										onClick={this.kAdminLoginHandle}
										title='Click to edit content for your website'>
										<img src={editIcon} />
										<span>edit content</span>
									</div> :
									null
							}
						</section>
						<section className='preview-right'>
							<div className='preview-button' onClick={this.removeFrame}>
								<img src={closeIcon} />
								<span>remove frame</span>
							</div>
						</section>
					</nav>
					<iframe id='preview'
						src={`//${this.state.customerID}.demo.getkitsune.com?hideHeader?v=${Date.now()}`}
						name={Date.now()}
						style={iFrameStyle} >
						<h1>your browser does not support iframes.</h1>
					</iframe>
				</div>
			)
		} else {
			return (
				<div className='custom-loader'>
					<div className='loader-bars-container'>
						<div className='loader-bars'>
							<i />
							<i />
							<i />
							<i />
							<i />
							<i />
							<i />
							<i />
							<i />
							<i />
						</div>
					</div>
					<p>loading the preview...</p>
				</div>
			)
		}
	}
}

PreviewProject.propTypes = {
	params: PropTypes.shape({
		projectid: PropTypes.string,
		customerid: PropTypes.string
	}),
	login: PropTypes.object
};

const mapStateToProps = state => {
	return {
		login: state.login
	}
};

export default connect(mapStateToProps)(PreviewProject)

