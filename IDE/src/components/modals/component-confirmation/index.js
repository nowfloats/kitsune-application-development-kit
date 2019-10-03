import React, { Component } from 'react';
import PropTypes from 'prop-types';
import { connect } from 'react-redux';
import { modalClose } from '../../../actions/modal';
import CloseIcon from '../../../images/close-thin.svg';
import { toggleProjectComponent } from "../../../actions/projectTree";

export const componentConfirmationLabel = 'component confirmation';

class ComponentConfirmation extends Component {
	constructor(props) {
		super(props);
		this.closeModal = this.closeModal.bind(this);
		this.enableComponent = this.enableComponent.bind(this);
		this.renderContent = this.renderContent.bind(this);
	}

	closeModal() {
		const { dispatch } = this.props;
		dispatch(modalClose());
	}

	enableComponent() {
		const {
			dispatch,
			componentID,
			componentName,
			gatewayName
		} = this.props;
		dispatch(toggleProjectComponent(componentID, true, componentName, gatewayName));
		this.closeModal();
	}

	renderContent() {
		const { componentName } = this.props;
		switch(componentName) {
		case 'reports':
			return (
				<div className='k-modal-content k-modal-small'>
					<p><span className='component-name'>reports</span> component enables you to send proactive reports to your
						customers. eg: weekly performance report, order confirmation report, etc.</p>
					<p>this module also gives you the complete control of customising the look & feel of the report via
						<a href='http://docs.kitsune.tools/components/reports/#report-language-reference'
							target='_blank'> reports language.</a>
					</p>
					<p>
						<a href='http://docs.kitsune.tools/components/reports/' target='_blank'>click here </a>
						to view the detailed documentation.
					</p>
				</div>
			);
		case 'payment gateway': //eslint-disable-line
			return (
				<div className='k-modal-content k-modal-small'>
					<p>the <span className='component-name'>payment gateway</span> component helps you enable commerce on your
						website. the component supports Instamojo, PayU, PayTM as out of the box payment providers.</p>
					<p>Using this component is the most secure and simplest way to enable payments on any website.
						It just takes few minutes to
						<a href='https://docs.kitsune.tools/components/k-pay/#configuration'
							target='_blank'> configure your kitsune-settings.json</a> and enable payment via
						<a href='https://docs.kitsune.tools/components/k-pay/#usage' taget='_blank'> k-pay</a></p>
				</div>
			);
		case 'partial views':
			return (
				<div className='k-modal-content k-modal-small'>
					<p><span className='component-name'>partial views</span> help you reuse repeating portions of your html like
						the header and footer. enabling this component will add the required folder and a sample file to your
						project.</p>
					<p>please visit <a href='https://docs.kitsune.tools/components/partial-views/' target='_blank' >
						partial views docs</a> to understand how to use partial views.</p>
				</div>
			);

		case 'call tracker':
			return (
				<div className='k-modal-content k-modal-small'>
					<p><span className='component-name'>call tracker</span> component allows you to mask all phone number data
						types with a virtual number. all calls made to these numbers are tracked and recorded so that your customers
						get a genuine view of their ROI.</p>
					<p>you can configure the websites for which you want
						to support call tracking in the kitsune-settings.json file.</p>
				</div>
			);
		}
	}
	render() {
		const { componentName, gatewayName } = this.props;
		return(
			<div>
				<div className='k-modal-head'>
					<h1>enable <span className='component-name'>{componentName === 'payment gateway' ?
						gatewayName : componentName}</span>{componentName === 'payment gateway' ? '' : ' component'}</h1>
					<img src={CloseIcon} className='closeIcon' onClick={this.closeModal} />
				</div>
				{this.renderContent()}

				<div className='k-modal-prompt-footer'>
					<button onClick={this.closeModal}>cancel</button>
					<button onClick={this.enableComponent}>enable</button>
				</div>
			</div>
		);
	}
}

ComponentConfirmation.propTypes = {
	dispatch: PropTypes.func,
	componentID: PropTypes.string,
	componentName: PropTypes.string,
	gatewayName: PropTypes.string
};

export default connect()(ComponentConfirmation)
