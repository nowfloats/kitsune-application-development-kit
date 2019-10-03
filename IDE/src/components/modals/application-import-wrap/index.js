import React, { Component } from 'react';
import { connect } from 'react-redux';
import PropTypes from 'prop-types';
import CloseIcon from '../../../images/close-thin.svg';
import CustomerIcon from '../../../images/info.svg';

class ApplicationImportWrap extends Component {
    constructor(props) {
        super(props);
        this.state = {
            endpointString: ''
        };
        this.startUpload = this.startUpload.bind(this);
        this.endpointStringChange = this.endpointStringChange.bind(this);
	}

	endpointStringChange(event) {
        this.setState({ endpointString: event.target.value });
	}

	startUpload() {
        const { closeModal } = this.props;
        alert('will call api now');
        closeModal();
	}
	
	render() {
        const { closeModal, appType } = this.props;
        const { endpointString } = this.state;
        
		return (
			<div className='k-modal-medium path-selector'>
                <div className='k-modal-head'>
                    <div>
                        <img src={CustomerIcon} className='modal-header-icon' />
                        <h1 className='modal-header-title'>use {appType} as an endpoint</h1>
                    </div>
                    <img src={CloseIcon} className='closeIcon' onClick={closeModal} />
                </div>
                <div className='k-modal-content'>
                    <div className='k-modal-container'>
                        <label className='k-modal-label'>enter the {appType} public URL for kitsune to access the application:</label>
                        <input className='k-modal-input'
                            type='text'
                            autoFocus
                            value={endpointString}
                            onChange={this.endpointStringChange}/>
                    </div>
                </div>
                <div className='k-modal-prompt-footer'>
                    <button onClick={this.startUpload}>SUBMIT</button>
                </div>
            </div>
		);
	}
}

ApplicationImportWrap.propTypes = {
    appType: PropTypes.string,
    closeModal: PropTypes.func
};

export default connect()(ApplicationImportWrap);
