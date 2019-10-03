import React, { Component } from 'react'
import PropTypes from 'prop-types';
import axios from 'axios'
import { config } from '../../config'
import { toastr } from 'react-redux-toastr'
import logo from '../../images/logo.svg'
import timeIcon from '../../images/load-time.png'
import desktopIcon from '../../images/desktop-icon.svg'
import tabletIcon from '../../images/tablet-icon.svg'
import phoneIcon from '../../images/phone-icon.svg'
import desktopIconActive from '../../images/desktop-icon-active.svg'
import tabletIconActive from '../../images/tablet-icon-active.svg'
import phoneIconActive from '../../images/phone-icon-active.svg'
import pencilIcon from '../../images/pencil-icon.png'
import { connect } from 'react-redux'

class Preview extends Component {

	constructor(props) {
		super(props)
		const { profileImage } = this.props.login;
		this.toggleForm = this.toggleForm.bind(this)
		this.applyDataset = this.applyDataset.bind(this)
		this.getPreview =  this.getPreview.bind(this)
		this.onchangeDataset = this.onchangeDataset.bind(this)
		this.enterDataset = this.enterDataset.bind(this);
		this.triggerResponsive = this.triggerResponsive.bind(this);
		const { none } = config.INTERNAL_SETTINGS.responsivePreview;
		this.state = {
			html : '',
			isLoading: true,
			userImage: profileImage,
			formToggle: false,
			dataset: this.props.params.dataset,
			loadTime: '0.00',
			responsive: none.name
		}
	}

	triggerResponsive(target) {
		const { none } = config.INTERNAL_SETTINGS.responsivePreview;
		this.setState(this.state.responsive === target ? { responsive: none.name } : { responsive: target });
	}

	onchangeDataset(event) {
		this.setState({ dataset: event.target.value });
	}

	enterDataset(event) {
		if(event.key === 'Enter')
			this.applyDataset();
	}

	toggleForm() {
		this.setState({
			formToggle: !this.state.formToggle
		})
	}


	getPreview() {
		//helper function to read a cookie value
		function readCookie(name) {
			var nameEQ = name + "=";
			var ca = document.cookie.split(';');
			for(var i=0;i < ca.length;i++) {
				var c = ca[i];
				while (c.charAt(0)==' ') c = c.substring(1,c.length);
				if (c.indexOf(nameEQ) == 0) return c.substring(nameEQ.length,c.length);
			}
			return null;
		}

	//retrieve data from the cookie
		let userId = readCookie('userId')
		let self = this;
		const url = `${config.API.project}/${this.props.params.projectid}/Preview`;
		let header = {
			params: {
				userEmail: userId,
				dataset: this.state.dataset,
				sourcePath: this.props.params.pagename
			}
		}
		axios.get(url, header)
			.then(function (response) {
				if(response.data !== null) {
					const { domContentLoadedEventEnd, navigationStart } = window.performance.timing;
					let loadtime = ((domContentLoadedEventEnd- navigationStart)/1000).toString()
					self.setState({
						html: response.data,
						isLoading: false,
						loadTime: loadtime
					})
				} else {
					self.setState({
						html: '<h1>there seems to be a problem with this service. please try again.</h1>',
						isLoading: false
					})
				}
			})
			.catch(function (error) {
				self.setState({
					html: '<h1>there seems to be a problem with this service. please try again</h1>',
					isLoading: false
				})
			});
	}

	applyDataset() {
		//TODO: Do this with react-router
		let locationArray = window.location.pathname.split('/');
		if(locationArray[4] !== this.state.dataset) {
			locationArray[4] = `data=${this.state.dataset}`;
			history.pushState({ newState: this.state.dataset }, document.title, locationArray.join('/'))
			this.setState({
				html: '',
				isLoading: true,
				formToggle: false,
				loadTime: '0.00'
			})
			this.getPreview();
		} else {
			toastr.warning('dataset has not been changed', 'there has been no change in dataset.')
		}
	}

	componentDidMount(){
		this.getPreview();
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
								<img src={logo} alt='Kitsune Logo' />
							</a>
							<div className='preview-timestamp'>
								<img src={timeIcon} alt='time icon' />
								<span>{this.state.loadTime}s</span>
							</div>
						</section>
						<section className='preview-right'>
							<div className={this.state.formToggle ? 'hidden' : 'uneditable-dataset'}>
								<span onClick={this.toggleForm}>{this.state.dataset}</span>
								<img onClick={this.toggleForm} src={pencilIcon} alt='edit' />
							</div>
							<div className={this.state.formToggle ? 'editable-dataset' : 'hidden'}>
								<label htmlFor='dataset'>sample customer dataset</label>
								<input id='dataset' type='text' value={this.state.dataset} onKeyPress={this.enterDataset}
									onChange={this.onchangeDataset} />
								<button onClick={this.applyDataset}>apply</button>
							</div>
							<div className='preview-responsive'>
								<img src={responsive === desktop.name ? desktopIconActive : desktopIcon}
									alt='desktop icon'
									onClick={() => this.triggerResponsive(desktop.name)} />
								<img src={responsive === tablet.name ? tabletIconActive : tabletIcon}
									alt='tablet icon'
									onClick={() => this.triggerResponsive(tablet.name)} />
								<img src={responsive === phone.name ? phoneIconActive : phoneIcon}
									alt='phone icon'
									onClick={() => this.triggerResponsive(phone.name)} />
							</div>
						</section>
					</nav>
					<iframe id='preview' srcDoc={this.state.html} name={Date.now()} style={iFrameStyle}>
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

Preview.propTypes = {
	params: PropTypes.shape({
		projectid: PropTypes.string,
		pagename: PropTypes.string,
		dataset: PropTypes.string
	}),
	login: PropTypes.object
}

const mapStateToProps = (state) => {
	return {
		login: state.login
	}
};

export default connect(mapStateToProps)(Preview)
