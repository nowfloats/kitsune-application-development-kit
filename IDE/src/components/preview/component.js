import React, { Component } from 'react';
import PropTypes from 'prop-types';
import _ from 'lodash';
import axios from 'axios';
import { config } from '../../config';
import Select from 'react-select';
import logo from '../../images/logo.svg';
import timeIcon from '../../images/load-time.png';
import { toastr } from "react-redux-toastr";
import { connect } from 'react-redux';
import { Base64 } from 'js-base64';

class PreviewComponent extends Component {
	constructor(props) {
		super(props);
		const { profileImage } = this.props.login;
		this.getCustomerList = this.getCustomerList.bind(this);
		this.handleCustomerChange = this.handleCustomerChange.bind(this);
		this.handlePerfChange = this.handlePerfChange.bind(this);
		this.handleNotifChange = this.handleNotifChange.bind(this);
		this.getSettingsFile = this.getSettingsFile.bind(this);
		const { customerid } = this.props.params;
		this.state = {
			isLoading: true,
			userImage: profileImage,
			customerID: customerid,
			loadTime: '0.00',
			options: [],
			perfOptions: [],
			notifOptions: [],
			sidebar: {
				perfPeriod: null,
				notifType: null
			}
		};
	}

	handlePerfChange({ value }) {
		this.setState({
			sidebar: {
				...this.state.sidebar,
				perfPeriod: value
			}
		});
	}

	handleNotifChange({ value }) {
		this.setState({
			sidebar: {
				...this.state.sidebar,
				notifType: value
			}
		});
	}

	handleCustomerChange({ value }) {
		this.setState({ customerID: value });
	}

	getCustomerList = () => new Promise((resolve, reject) => {
		const { userID } = this.props.login;
		const { userToken } = config.API;
		axios.get(`${userToken}${userID}`)
			.then(({ data }) => {
				const { Id: userAuth } = data;
				axios.defaults.headers.common['Authorization'] = userAuth;
				const { customers } = config.API;
				const { projectid } = this.props.params;
				axios.get(`${customers}${projectid}`)
					.then(({ data }) => {
						const { Websites } = data;
						const options = Websites.map(({ WebsiteId, WebsiteTag }) => ({
							className: 'data-options',
							value: WebsiteId,
							label: WebsiteTag
						}));
						const defaultCustomer = Websites[Websites.length - 1].WebsiteId;
						resolve({ options, defaultCustomer });
					})
					.catch(error => {
						toastr.error('unable to fetch your datasets');
						reject(error);
					});
			});
	});


	getSettingsFile = () => new Promise((resolve, reject) => {
		const { userID } = this.props.login;
		const { projectAPI } = config.API;
		const { projectid } = this.props.params;
		const encodedPath = encodeURIComponent(`/kitsune-settings.json`);
		return axios.get(`${projectAPI}/v2/${projectid}/resource/?user=${userID}&sourcePath=${encodedPath}`)
			.then(({ data }) => resolve(JSON.parse(Base64.decode(data.File.Base64Data))))
			.catch(error => {
				toastr.error('unable to fetch your config');
				reject(error);
			});
	});

	componentDidMount() {
		this.getCustomerList()
			.then(({ options, defaultCustomer }) => {
				const { domContentLoadedEventEnd, navigationStart } = window.performance.timing;
				const { customerid } = this.props.params;
				const loadTime = ((domContentLoadedEventEnd - navigationStart)/1000).toString();
				if(customerid === 'default') {
					this.setState({
						loadTime,
						isLoading: false,
						options,
						customerID: defaultCustomer
					});
				} else {
					this.setState({
						loadTime,
						isLoading: false,
						options
					});
				}
			})
			.catch(() => {
				this.setState({ isLoading: false });
			});

		this.getSettingsFile()
			.then(({ reports }) => {
				const { notifications } = reports;
				const { path } = this.props.params;
				const pathArray = path.split('/');
				const fileName = pathArray[pathArray.length - 1];
				const fileObject = _.find(notifications, ({ email }) => email.body === fileName);
				const { perf: perfOptions, notif: notifOptions } = _.reduce(notifications, (result, { period, type }) => ({
					...result,
					perf: [
						...result.perf,
						{
							className: 'data-options',
							label: period,
							value: period
						}
					],
					notif: [
						...result.notif,
						{
							className: 'data-options',
							label: type,
							value: type
						}
					]
				}), { perf: [], notif: [] });
				this.setState({
					perfOptions,
					notifOptions,
					sidebar: {
						perfPeriod: fileObject.period,
						notifType: fileObject.type
					}
				});
			})
			.catch(() => {
				this.setState({ isLoading: false });
			});
	}

	render() {
		const { isLoading } = this.state;
		if(!isLoading) {
			const {
				customerID,
				sidebar,
				loadTime,
				options,
				userImage,
				notifOptions,
				perfOptions
			} = this.state;
			const { path } = this.props.params;
			const encodedData = Base64.encode(JSON.stringify({
				perf_report_period: sidebar.perfPeriod,
				notif_type: sidebar.notifType
			}));
			const iframeSrc = `https://${customerID}.demo.getkitsune.com${path}?ria_args=${encodedData}`;
			return (
				<div className='preview-container'>
					<nav className='preview-header'>
						<section className='preview-left'>
							<a className='preview-branding' href='#' alt='kitsune logo' >
								<img src={logo} alt='kitsune logo' />
							</a>
							<div className='preview-timestamp'>
								<img src={timeIcon} alt='time icon' />
								<span>{loadTime}s</span>
							</div>
						</section>
						<section className='preview-center'>
							<Select name='dataset-select'
								value={options.find(option => option.value === customerID)}
								options={options}
								onChange={this.handleCustomerChange}
								className='k-modal-dropdown preview-dropdown'
								classNamePrefix='k-modal-dropdown' />
						</section>
						<section className='preview-right'>
							<a className='preview-user' alt='kitsune logo' >
								<img src={userImage} alt='user image' />
							</a>
						</section>
					</nav>
					<section className='preview-content'>
						<aside>
							<span>notification type</span>
							<Select name='notif-select'
								value={notifOptions.find(option => option.value === sidebar.notifType)}
								options={notifOptions}
								onChange={this.handleNotifChange}
								className='k-modal-dropdown preview-dropdown'
								classNamePrefix='k-modal-dropdown' />
							<span>notification period</span>
							<Select name='perf-select'
								value={perfOptions.find(option => option.value === sidebar.perfPeriod)}
								options={perfOptions}
								onChange={this.handlePerfChange}
								className='k-modal-dropdown preview-dropdown'
								classNamePrefix='k-modal-dropdown' />
						</aside>
						<iframe id='preview' src={iframeSrc} name={Date.now()}>
							<h1>your browser does not support iframes.</h1>
						</iframe>
					</section>
				</div>
			);
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
			);
		}
	}
}

PreviewComponent.propTypes = {
	params: PropTypes.shape({
		projectid: PropTypes.string,
		customerid: PropTypes.string,
		componentid: PropTypes.string,
		path: PropTypes.string
	}),
	login: PropTypes.object
};

const mapStateToProps = state => ({
	login: state.login
});

export default connect(mapStateToProps)(PreviewComponent);

