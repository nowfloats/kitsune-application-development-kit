import React, { Component } from 'react';
import PropTypes from 'prop-types';
import { connect } from 'react-redux';
import { showLoading } from '../../../actions/loader';
import {
	isWebFormOpen, setCurrentWebForm, filterWebFormlist,
	initializeCurrentWebForm, setIsEditable, getJsonOfWebForm,
	setJsonOfWebForm, setCanClone
} from '../../../actions/webForm';
import { ContextMenuProvider } from 'kt-contexify';
import { resetSchema } from '../../../actions/schema';
import { config } from '../../../config'
import webFormIcon from '../../../images/action-web-form.svg'
import plusIcon from '../../../images/plus.svg'
import { modalOpen } from '../../../actions/modal'
import CreateWebform from '../../modals/create-webform/index'
import { checkFileChanged, editorClear, fileOpen } from '../../../actions/editor'
import mapperFunction from '../../web-form/form-builder/mapper'
import { createWebformLabel } from "../../modals/create-webform";

class WebForms extends Component {

	constructor(props) {
		super(props);
		this.openWebForm = this.openWebForm.bind(this)
		this.createWebFormHandler = this.createWebFormHandler.bind(this)
		this.getListOfWebForms = this.getListOfWebForms.bind(this);
		this.filterWebForms = this.filterWebForms.bind(this)
	}

	openWebForm() {
		const { dispatch } =  this.props;
		dispatch(isWebFormOpen(true));
	}

	webFormSelectHandler(webForm) {
		const { dispatch, isOpen, webForm: webFormRed } = this.props;
		const { text: OPEN_WEBFORM } = config.INTERNAL_SETTINGS.loadingText.OPEN_WEBFORM;
		dispatch(checkFileChanged(() => {
			dispatch(isWebFormOpen(true))
			dispatch(fileOpen(webForm.Name, '', '', '', {}));
			if(webFormRed.isEditable){
				dispatch(setIsEditable(false))
			}
			dispatch(setCanClone(false));
			dispatch(showLoading(OPEN_WEBFORM));
			isOpen ? dispatch(resetSchema()) : '';
			dispatch(editorClear());
			dispatch(setCurrentWebForm(webForm));
			let webFormProperties = mapperFunction.getWebformObject(webForm);
			dispatch(getJsonOfWebForm(webForm.ActionId, webFormProperties))
		}));
	}

	getListOfWebForms(webForms) {
		const { webForm: webFormContext } = config.INTERNAL_SETTINGS.contextMenus;
		return webForms.map((webForm, index) => {
			return (
				<ContextMenuProvider id={webFormContext} node={Object.assign({}, webForm)} key={index}>
					<li className={`webform ${(this.props.webForm.currentWebForm.Name === webForm.Name ? 'active' : '')}`}
						key={webForm.ActionId}
						onClick={() => {this.webFormSelectHandler(webForm)}}>
						<img src={webFormIcon} />
						<span>{webForm.DisplayName}</span>
					</li>
				</ContextMenuProvider>
			);
		})
	}

	createWebFormHandler() {
		const { dispatch, isOpen, webForm } = this.props;
		dispatch(checkFileChanged(() => {
			dispatch(setCanClone(false));
			if(isOpen) {
				dispatch(resetSchema());
			}
			if(webForm.isOpen) {
				dispatch(isWebFormOpen(false));
			}
			dispatch(editorClear());
			dispatch(modalOpen(<CreateWebform />, createWebformLabel, null));
			dispatch(initializeCurrentWebForm(false));
			dispatch(setJsonOfWebForm([]));
		}));
	}

	filterWebForms(e) {
		const { webForm, dispatch } = this.props;
		const { webForms } = webForm;
		let searchQuery = e.target.value.trim();
		if(searchQuery) {
			dispatch(filterWebFormlist(webForms, searchQuery))
		} else {
			dispatch(filterWebFormlist(webForms, ''))
		}
	}

	render() {
		const { webForm, isActive, variant } = this.props;
		const { WEB_FORMS } = config.INTERNAL_SETTINGS.sidebarItems[variant];
		const { copyOfWebForms, webForms } = webForm;
		const placeHolder = `search your webForms here...`
		return (
			<div className={isActive === WEB_FORMS.key ? 'sidebar-webform' : 'hide'}>
				<div className={webForms.length ? 'hide' : 'schema-list-intro'}>
					<div className='schema-logo webform-logo' />
					<h2  className='intro-title'>kitsune webform builder</h2>
					<p className='intro-text'>
						Webform can be used to build forms and surveys for your application. 
						It also automatically generates serverless APIs with r/w access.
						To use a webform - just copy the generated code snippet and paste it into HTML, 
						or use the API using custom script/jquery.
					</p>
					<button className='new-schema btn' onClick={this.createWebFormHandler} >Create a WebForm</button>
				</div>
				<div className={webForms.length ? 'webForm-header' : 'hide'}>
					<button className='new-webForm-button'
						onClick={this.createWebFormHandler}>
						<img src={plusIcon} />
					</button>
					<div className='search-box'>
						<div className='input-group'>
							<input type='text'
								className='form-control'
								placeholder={placeHolder}
								onKeyUp={this.filterWebForms} />
							<span className='focus-border'><i /></span>
						</div>
					</div>
				</div>
				<ul className='webforms-list'>{this.getListOfWebForms(copyOfWebForms)}</ul>
			</div>
		);
	}
}

WebForms.propTypes = {
	dispatch: PropTypes.func,
	isActive: PropTypes.string,
	webForm: PropTypes.object,
	isOpen: PropTypes.bool
}

const mapStateToProps = (state) => {
	return {
		webForm: state.webFormReducer,
		isOpen: state.schemaCreatorReducer.isOpen,
	}
}

export default connect(mapStateToProps)(WebForms)
