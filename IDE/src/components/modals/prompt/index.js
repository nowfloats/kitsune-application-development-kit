import React, { Component } from 'react';
import PropTypes from 'prop-types';
import { connect } from 'react-redux';
import { modalClose } from '../../../actions/modal';
import CloseIcon from '../../../images/close-thin.svg';
import ProjectIcon from '../../../images/project-white.svg';
import LanguageIcon from '../../../images/language-white.svg';
import WebFormIcon from '../../../images/webform-white.svg';
import { showLoading, hideLoading } from '../../../actions/loader';
import { cookieDomain, config } from '../../../config';
import { pageStateUpdate } from '../../../actions/pageState';
import { checkDeveloperNetBalance, projectInit, publishAll } from '../../../actions/projectTree';
import { editorClear, fileDelete, fileSourceUpdate, folderDelete } from '../../../actions/editor';
import { toastr } from 'react-redux-toastr';
import { footerInit, isSaved } from '../../../actions/footer';
import { resetSchema, saveSchema } from '../../../actions/schema';
import { isWebFormOpen } from "../../../actions/webForm";
import { buildProject } from "../../../actions/build";
import { AUTH_CONFIG } from "../../login/auth0-variables";

export const promptMessageLabel = 'Prompt Messsage';

class PromptMessage extends Component {
	constructor(props) {
		super(props);
		this.executeAction = this.executeAction.bind(this);
		this.noAction = this.noAction.bind(this);
		this.logOut = this.logOut.bind(this);
		this.deleteFile = this.deleteFile.bind(this);
		this.deleteFolder = this.deleteFolder.bind(this);
	}

	noAction() {
		const {
			noAction,
			callback,
			footerSaveLog,
			promptItem
		} = this.props;
		const { name: SAVE_PROJECT } = config.INTERNAL_SETTINGS.promptMessages.SAVE_PROJECT;
		const { name: SAVE_LANGUAGE } = config.INTERNAL_SETTINGS.promptMessages.SAVE_LANGUAGE;
		const { name: SAVE_WEBFORM } = config.INTERNAL_SETTINGS.promptMessages.SAVE_WEBFORM;
		switch (promptItem) {
			case SAVE_PROJECT:
			case SAVE_LANGUAGE:
			case SAVE_WEBFORM:
				noAction(footerSaveLog, callback);
				break;
			default:
				toastr.success('your action was successfully completed.');
		}
	}

	logOut() {
		let cookieDomainConfig = cookieDomain;
		if (window.location.hostname === 'localhost')
			cookieDomainConfig = 'localhost';

		let deleteCookie = function (name) {
			document.cookie = name + '=;expires=Thu, 01 Jan 1970 00:00:01 GMT;  path=/ ; domain=' + cookieDomainConfig;
		};
		deleteCookie('userName');
		deleteCookie('userId');
		deleteCookie('userImage');

		sessionStorage.clear();
		this.props.auth.logout();
		window.location = `
			https://${AUTH_CONFIG.domain}/v2/logout?returnTo=https://www.getkitsune.com&client_id=${AUTH_CONFIG.clientId}
		`;
	}

	deleteFile() {
		const { filePath, payload, deleteFile } = this.props;
		let path = payload === undefined ? filePath : payload.path;
		deleteFile(path);
	}

	deleteFolder() {
		const { payload, deleteFolder } = this.props;
		deleteFolder(payload.Path);
	}

	executeAction() {
		const { promptItem, callback,
			closeProject,
			saveSchema,
			schemaDetails,
			webFormSave,
			saveProject,
			publishToAll,
			closeModal,
			developerWallet
		} = this.props;
		const {
			CLOSE_PROJECT,
			LOG_OUT,
			SAVE_PROJECT,
			SAVE_LANGUAGE,
			SAVE_WEBFORM,
			DELETE_FILE,
			DELETE_FOLDER,
			PUBLISH_ALL
		} = config.INTERNAL_SETTINGS.promptMessages;
		switch (promptItem) {
			case CLOSE_PROJECT.name:
				closeProject();
				break;
			case LOG_OUT.name:
				this.logOut();
				break;
			case SAVE_PROJECT.name:
				saveProject().then(() => callback());
				break;
			case SAVE_LANGUAGE.name:
				saveSchema(schemaDetails).then(() => callback());
				break;
			case SAVE_WEBFORM.name:
				webFormSave(callback);
				break;
			case DELETE_FILE.name:
				this.deleteFile();
				break;
			case DELETE_FOLDER.name:
				this.deleteFolder();
				break;
			case PUBLISH_ALL.name:
				publishToAll(developerWallet);
				closeModal();
				break;
			default:
				toastr.success('your action was successfully completed.');
				if (callback) callback();
		}
	}

	render() {
		const {
			SAVE_PROJECT,
			SAVE_LANGUAGE,
			SAVE_WEBFORM,
			DELETE_FILE,
			DELETE_FOLDER,
			LOG_OUT
		} = config.INTERNAL_SETTINGS.promptMessages;
		const {
			promptItem,
			isSchemaOpen,
			isWebFormOpen,
			fileName,
			payload,
			closeModal
		} = this.props;
		const {
			heading,
			text
		} = config.INTERNAL_SETTINGS.promptMessages[promptItem];
		const isSavePrompt = promptItem === SAVE_PROJECT.name || promptItem === SAVE_LANGUAGE.name ||
			promptItem === SAVE_WEBFORM.name;
		const isDeletePrompt = promptItem === DELETE_FILE.name || promptItem === DELETE_FOLDER.name;
		const isLogOutPrompt = promptItem === LOG_OUT.name;
		let name = payload === undefined ? fileName : payload.name;
		const icon = isSchemaOpen ? LanguageIcon :
			isWebFormOpen ? WebFormIcon : ProjectIcon;
		return (
			<div>
				<div className='k-modal-head'>
					<div>
						<img src={icon} className='modal-header-icon' />
						<h1 className='modal-header-title'>{heading}
							{
								isDeletePrompt ?
									<span className='delete-name'>{name}</span>
									: null
							}
						</h1>
					</div>
					<img src={CloseIcon} className='closeIcon' onClick={closeModal} />
				</div>
				<div className='k-modal-content k-modal-small'>
					<p>{text}</p>
				</div>
				<div className='k-modal-prompt-footer'>
					<button className={isLogOutPrompt ? 'hide' : ''}
						onClick={closeModal}>cancel</button>
					<button onClick={this.noAction} className={isSavePrompt ? '' : 'hide'}>no</button>
					<button onClick={this.executeAction}>
						<span className={isSavePrompt ? '' : 'hide'}> yes</span>
						<span className={isSavePrompt ? 'hide' : ''}>ok</span>
					</button>
				</div>
			</div>
		);
	}
}

PromptMessage.propTypes = {
	closeModal: PropTypes.func,
	noAction: PropTypes.func,
	saveProject: PropTypes.func,
	saveSchema: PropTypes.func,
	closeProject: PropTypes.func,
	deleteFile: PropTypes.func,
	deleteFolder: PropTypes.func,
	publishToAll: PropTypes.func,
	callback: PropTypes.func,
	filePath: PropTypes.string,
	fileName: PropTypes.string,
	footerSaveLog: PropTypes.object,
	promptItem: PropTypes.string,
	payload: PropTypes.object,
	schemaDetails: PropTypes.object,
	developerWallet: PropTypes.object,
	isSchemaOpen: PropTypes.bool,
	isWebFormOpen: PropTypes.bool,
	webFormSave: PropTypes.func,
	auth: PropTypes.object
};

const mapDispatchToProps = dispatch => {
	return ({
		closeModal: () => dispatch(modalClose()),
		noAction: (footerSaveLog, callback) => {
			dispatch(modalClose());
			dispatch(editorClear());
			dispatch(resetSchema());
			dispatch(isWebFormOpen(false));
			dispatch(footerInit(footerSaveLog));
			localStorage.removeItem('initialCode');
			callback();
		},
		saveProject: () => new Promise(resolve => {
			return (dispatch(fileSourceUpdate())).then(() => {
				dispatch(modalClose());
				dispatch(isSaved({ isSaved: true, lastSave: new Date() }));
				resolve();
			})
		}),
		saveSchema: details => new Promise((resolve, reject) => {
			dispatch(saveSchema(details))
				.then(() => {
					dispatch(resetSchema());
					resolve();
				})
				.catch(() => {
					reject();
				});
			dispatch(modalClose());
		}),
		closeProject: () => {
			const { GENERAL_LOADING } = config.INTERNAL_SETTINGS.loadingText;
			const { PROJECT_CLOSED } = config.INTERNAL_SETTINGS.pageStates;
			dispatch(showLoading(GENERAL_LOADING.text));
			dispatch(pageStateUpdate(PROJECT_CLOSED.name));
			dispatch(editorClear());
			dispatch(projectInit());
			dispatch(footerInit({ isSaved: false }));
			dispatch(hideLoading());
			dispatch(resetSchema());
			dispatch(isWebFormOpen(false));
			localStorage.removeItem('project-init');
			sessionStorage.removeItem('currentThemeId');
			sessionStorage.removeItem('projectName');
			sessionStorage.removeItem('projectResponsive');
			dispatch(modalClose());
		},
		deleteFile: path => {
			dispatch(fileDelete([encodeURIComponent(path)]));
			dispatch(modalClose());
		},
		deleteFolder: path => {
			dispatch(folderDelete(path));
			dispatch(modalClose());
		},
		publishToAll: developerWallet => {
			const isUserEligible = checkDeveloperNetBalance(developerWallet);
			if (isUserEligible) {
				dispatch(buildProject())
					.then(() => dispatch(publishAll()))
					.catch(error => toastr.error('build failed', error.message));
			} else {
				toastr.warning('insufficient funds');
			}
		}
	});
};

const mapStateToProps = state => {
	const { activeTabs, visibleIndex } = state.editorReducer;
	const editor = activeTabs[visibleIndex];
	return {
		fileName: editor ? editor.name : '',
		filePath: editor ? editor.path : '',
		footerSaveLog: state.footerReducer.saveLog,
		schemaDetails: state.schemaCreatorReducer.schemaDetails,
		isSchemaOpen: state.schemaCreatorReducer.isOpen,
		isWebFormOpen: state.webFormReducer.isOpen,
		webFormHelper: state.webFormReducer.helper,
		webFormSave: state.webFormReducer.helper.saveWebform,
		callback: state.modalReducer.callback,
		developerWallet: state.login.developerDetails.Wallet,
		auth: state.login.auth
	}
};

export default connect(mapStateToProps, mapDispatchToProps)(PromptMessage);
