import React, { Component } from 'react';
import { connect } from 'react-redux';
import PropTypes from 'prop-types';
import { existingAppDeploymentType } from '../../../actions/actionTypes';
import CloseIcon from '../../../images/close-thin.svg';
import { modalOpen } from '../../../actions/modal';
import FileDropper from '../upload-file/file-dropper';
import ApplicationImportWrap from '../application-import-wrap';
import CustomerIcon from '../../../images/person_white.svg';
import RadioSelect from '../../shared/radio-select';

class ApplicationUploadType extends Component {
    constructor(props) {
        super(props);
        this.state = {
            // Important: This is important to bind the defualt value for the radio button
            appUploadType: existingAppDeploymentType.CONTAINERISE
        };
        this.startUpload = this.startUpload.bind(this);
        this.optionChange = this.optionChange.bind(this);
    }

    optionChange(value) {
        this.setState({ appUploadType: value });
    }

    startUpload() {
        const { dispatch, appType, closeModal } = this.props;
        if (this.state.appUploadType === existingAppDeploymentType.CONTAINERISE) {
            let files = [];
            dispatch(modalOpen(<FileDropper appType={appType} closeModal={closeModal} files={files} />, 'upload app', null));
        } else if (this.state.appUploadType === existingAppDeploymentType.USE_AS_ENDPOINT) {
            dispatch(modalOpen(<ApplicationImportWrap appType={appType} closeModal={closeModal} />, 'wrap app', null));
        }
    }

    render() {
        const { closeModal, appType } = this.props;

        return (
            <div>
                <div className='k-modal-head'>
                    <div>
                        <img src={CustomerIcon} className='modal-header-icon customer-icon' />
                        <h1 className='modal-header-title'>how would you like to import {appType}?</h1>
                    </div>
                    <img src={CloseIcon} className='closeIcon' onClick={closeModal} />
                </div>
                <div className='k-modal-content k-modal-small k-modal-form'>
                    <div className='k-modal-container'>
                        <RadioSelect
                            onChange={this.optionChange}
                            labelClass='k-modal-primary-label'
                            selectedValue={this.state.appUploadType}>
                            <RadioSelect.Value name="appUploadOptions" value={existingAppDeploymentType.CONTAINERISE}>
                                containerise {appType} application code with kitsune, and optimise for CaaS deployment
                            </RadioSelect.Value>
                            <RadioSelect.Value name="appUploadOptions" value={existingAppDeploymentType.USE_AS_ENDPOINT}>
                                use {appType} application as an endpoint
                            </RadioSelect.Value>
                        </RadioSelect>
                    </div>
                </div>

                <div className='k-modal-footer'>
                    <button onClick={this.startUpload} >next</button>
                </div>
            </div>
        );
    }
}

ApplicationUploadType.propTypes = {
    appType: PropTypes.string,
    appUploadType: PropTypes.string,
    selectedOption: PropTypes.string,
    closeModal: PropTypes.func,
    dispatch: PropTypes.func
};

export default connect()(ApplicationUploadType);
