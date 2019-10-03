import React, { Component } from 'react'
import PropTypes from 'prop-types';
import { connect } from 'react-redux'
import { modalClose } from '../../../actions/modal'
import CloseIcon from '../../../images/close-thin.svg'
import { toggleProjectComponent } from "../../../actions/projectTree";

export const componentDisableLabel = 'RIA confirmation';

class ComponentDisable extends Component {
	constructor(props) {
		super(props);
		this.closeModal = this.closeModal.bind(this);
		this.disableComponent = this.disableComponent.bind(this);
	}

	closeModal() {
		const { dispatch } = this.props;
		dispatch(modalClose());
	}

	disableComponent() {
		const { dispatch, componentID, componentName , gatewayName } = this.props;
		dispatch(toggleProjectComponent(componentID, false, componentName, gatewayName));
		this.closeModal();
	}

	render() {
		const { componentName, gatewayName } = this.props;
		return(
			<div>
				<div className='k-modal-head'>
					<h1>disable <span className='component-name'>{componentName === 'payment gateway' ?
						gatewayName : componentName}</span></h1>
					<img src={CloseIcon} className='closeIcon' onClick={this.closeModal} />
				</div>
				<div className='k-modal-content k-modal-small'>
					<p>Are you sure you want to disable <span className='component-name'>{componentName === 'payment gateway' ?
						gatewayName : componentName} </span>
						for this project?</p>
				</div>
				<div className='k-modal-prompt-footer'>
					<button onClick={this.closeModal}>cancel</button>
					<button onClick={this.disableComponent}>yes</button>
				</div>
			</div>
		);
	}
}

ComponentDisable.propTypes = {
	dispatch: PropTypes.func,
	componentID: PropTypes.string,
	componentName: PropTypes.string,
	gatewayName: PropTypes.string
};

export default connect()(ComponentDisable)
