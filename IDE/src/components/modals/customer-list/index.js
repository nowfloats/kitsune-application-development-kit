import React, { Component } from 'react';
import { connect } from 'react-redux';
import PropTypes from 'prop-types';
import { checkDeveloperNetBalance, getCustomerList, projectPublish } from '../../../actions/projectTree';
import { modalClose } from '../../../actions/modal';
import CloseIcon from '../../../images/close-thin.svg';
import Select from 'react-select';
import { toastr } from "react-redux-toastr";
import { buildProject } from "../../../actions/build";
import CustomerIcon from '../../../images/person_white.svg';

export const customerListLabel = 'Customer List';

class CustomerList extends Component {
	constructor (props) {
		super(props);
		this.state = {
			websites: [],
			selectedWebsite: '',
			sendDefaultData: false,
			selectedDomain: ''
		};
		this.websiteChange = this.websiteChange.bind(this);
		this.checkboxChange = this.checkboxChange.bind(this);
		this.publishProject = this.publishProject.bind(this);
		this.closeModal = this.closeModal.bind(this);
		this.getSortedWebsites = this.getSortedWebsites.bind(this);
	}

	closeModal() {
		const { dispatch } = this.props;
		dispatch(modalClose());
	}

	componentWillMount() {
		const { dispatch } = this.props;
		dispatch(getCustomerList());
	}

	websiteChange({ value, label }) {
		this.setState({ selectedWebsite: value, selectedDomain: label });
	}

	checkboxChange(event){
		this.setState({ sendDefaultData: event.target.checked });
	}

	publishProject() {
		const { dispatch, developerWallet } = this.props;
		let { selectedWebsite: websiteID, selectedDomain: websiteDomain,
			sendDefaultData: copyDataFromDemoWebsite } = this.state;

		if(websiteDomain.includes(" - (default)")) {
			websiteDomain = websiteDomain.replace(" - (default)", "");
		}
		dispatch(modalClose());
		const isUserEligible = checkDeveloperNetBalance(developerWallet);
		if(isUserEligible) {
			dispatch(buildProject())
				.then(() => {
					toastr.success('build successful', `your project has been successfully built.`);
					dispatch(projectPublish({ websiteID, websiteDomain, copyDataFromDemoWebsite }));
				})
				.catch(error => toastr.error('build failed', error.message));
		} else {
			toastr.warning('insufficient funds');
		}
	}

	getSortedWebsites() {
		let websites = this.props.websites.sort((a,b) => Date.parse(a.createdOn) > Date.parse(b.createdOn));
		
		if(!websites[0].label.includes("(default)")) {
			websites[0].label = websites[0].label + " - (default)";
		}
		return websites;
	}

	render() {
		const websites = this.getSortedWebsites();
		const { selectedWebsite, sendDefaultData, selectedDomain } = this.state;
		const isPublishValid = selectedWebsite !== '';
		return (
			<div>
				<div className='k-modal-head'>
					<div>
						<img src={CustomerIcon} className='modal-header-icon customer-icon' />
						<h1 className='modal-header-title'>select website</h1>
					</div>
					<img src={CloseIcon} className='closeIcon' onClick={this.closeModal} />
				</div>
				<div className='k-modal-content k-modal-small k-modal-form'>
					<div className='k-modal-container'>
						<label className='k-modal-label'>website</label>
						<Select name='form-field-name'
							value={websites.find(option => option.value === selectedWebsite)}
							options={websites}
							onChange={this.websiteChange}
							className='k-modal-dropdown'
							classNamePrefix='k-modal-dropdown' />

						<div 
							className={`k-modal-checkbox ${selectedDomain.includes(" - (default)") ? 'hide-cross' : ''}`}>
							<input
								id='copy-website'
								type='checkbox'
								className='regular-checkbox'
								checked={sendDefaultData}
								onChange={this.checkboxChange}
							/>
							<label htmlFor='copy-website' />
							<span>copy data from the default website</span>
						</div>
						<div 
							className={(sendDefaultData && !selectedDomain.includes(" - (default)")) ? '' : 'hide-cross'}>	
							<span className='k-modal-warning' />
							<span className='k-modal-error'>this will overwrite all data present on the selected website</span>
						</div>
					</div>
				</div>
				
				<div className='k-modal-footer'>
					<button onClick={this.publishProject} disabled={!isPublishValid}>build & publish</button>
				</div>
			</div>
		);
	}
}

CustomerList.propTypes = {
	dispatch: PropTypes.func,
	websites: PropTypes.array,
	developerWallet: PropTypes.object
};

const mapStateToProps = state => {
	return {
		websites: state.publishReducer.websites,
		developerWallet: state.login.developerDetails.Wallet
	};
};

export default connect(mapStateToProps)(CustomerList);
