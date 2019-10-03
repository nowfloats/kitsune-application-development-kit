import React, { Component } from 'react';
import PropTypes from 'prop-types';
import _ from 'lodash';
import { connect } from 'react-redux';
import { ContextMenu, Item, Separator } from 'kt-contexify';
import { checkFileChanged, editorClear, fileSourceFetch, makePageDefault } from "../../actions/editor";
import { showLoading } from "../../actions/loader";
import { config, riaComponentID, httpDomainWithoutSSL } from "../../config";
import CustomerList, { customerListLabel } from '../modals/customer-list/index';
import RenameFile, { renameFileLabel } from "../modals/rename-file";
import { setPath } from '../../actions/upload';
import CreateFile, { createFileLabel } from '../modals/create-file/index';
import BuildRequired, { buildRequiredLabel } from "../modals/build-required";
import { previewPage } from '../../actions/projectTree';
import { modalThemesFetch, modalOpen } from "../../actions/modal";
import { openProjectLabel } from "../modals/open-project/index";
import EditProject, { editProjectLabel } from "../modals/edit-project/index";
import PromptMessage, { promptMessageLabel } from "../modals/prompt/index";
import UploadFile, { uploadFileLabel } from "../modals/upload-file";
import { resetSchema, schemaFetchDetails } from "../../actions/schema";
import { getJsonOfWebForm, isWebFormOpen, setCanClone, setIsEditable, setCurrentWebForm } from "../../actions/webForm";
import mapperFunction from "../web-form/form-builder/mapper";

class Context extends Component {
	constructor(props) {
		super(props);
		//Pages Functions
		this.openFile = this.openFile.bind(this);
		this.renameFile = this.renameFile.bind(this);
		this.deleteFile = this.deleteFile.bind(this);
		this.previewPage = this.previewPage.bind(this);
		this.makeDefault = this.makeDefault.bind(this);
		//Schema Functions
		this.openLanguage = this.openLanguage.bind(this);
		//Preview Functions
		this.previewProject = this.previewProject.bind(this);
		this.previewComponent = this.previewComponent.bind(this);
		//Webform Functions
		this.openWebForm = this.openWebForm.bind(this);
		this.state = {
			filePicker: '*'
		};
	}

	previewPage(node) {
		const { previewPage } = this.props;
		previewPage(node);
	}

	//preview project
	previewProject() {
		const {
			defaultCustomer,
			projectID,
			buildNeeded,
			fileTimeStamp,
			buildTimeStamp,
			previewProject
		} = this.props;
		previewProject({ defaultCustomer, projectID, buildNeeded, fileTimeStamp, buildTimeStamp });
	}

	previewComponent({ Path }) {
		const { defaultCustomer, projectID } = this.props;
		const { origin } = window.location;
		window.open(`${origin}/preview/project=${projectID}/component=${riaComponentID}/path=${encodeURIComponent(Path)}/
		customer=${defaultCustomer}`, '_blank');
	}

	openFile(node) {
		const { openFile } = this.props;
		openFile(node.name, node.Path);
	}

	renameFile(node) {
		const { renameFile } = this.props;
		renameFile(node);
	}

	deleteFile(node) {
		const { deleteFile } = this.props;
		const payload = { name: node.name, path: node.Path };
		deleteFile(payload);
	}

	openLanguage({ SchemaId }) {
		const { fetchLanguage } = this.props;
		fetchLanguage(SchemaId);
	}

	makeDefault(node) {
		const { makeDefault } = this.props;
		const { Path } = node;
		makeDefault(Path);
	}

	openWebForm(webForm) {
		const { isWebFormEditable, isLanguageOpen, fetchWebForm } = this.props;
		fetchWebForm(webForm, isWebFormEditable, isLanguageOpen);
	}

	render() {
		let menus = config.INTERNAL_SETTINGS.contextMenus;
		const {
			openProject,
			openUploader,
			editProject,
			deleteFolder,
			publishProject,
			closeProject,
			newFile } = this.props;
		return (
			<div>
				<ContextMenu id={menus.project} animation={this.props.animation} theme={this.props.theme}>
					<Item label='open another project' onClick={openProject}>
						open another project
					</Item>
					<Item label='upload here' onClick={openUploader}>
						upload here
					</Item>
					<Item label='rename project' onClick={editProject}>
						rename project
					</Item>
					<Item label='publish project' onClick={publishProject}>
						publish project
					</Item>
					<Item label='preview project' onClick={this.previewProject}>
						preview project
					</Item>
					<Item label='close project' onClick={closeProject}>
						close project
					</Item>
				</ContextMenu>

				<ContextMenu id={menus.folder} animation={this.props.animation} theme={this.props.theme}>
					<Item label='new file' onClick={newFile}>
						new file
					</Item>
					<Item label='upload here' onClick={openUploader}>
						upload here
					</Item>
					<Separator />
					<Item label='delete folder' onClick={deleteFolder}>
						delete folder
					</Item>
				</ContextMenu>

				<ContextMenu id={menus.pages} animation={this.props.animation} theme={this.props.theme}>
					<Item label='open file' onClick={this.openFile}>
						open file
					</Item>
					<Item label='rename file' onClick={this.renameFile}>
						rename file
					</Item>
					<Item label='make default page' onClick={this.makeDefault}>
						make default page
					</Item>
					<Separator />
					<Item label='delete file' onClick={this.deleteFile}>
						delete file
					</Item>
				</ContextMenu>

				<ContextMenu id={menus.component} animation={this.props.animation} theme={this.props.theme}>
					<Item label='open file' onClick={this.openFile}>
						open file
					</Item>
					<Item label='preview page' onClick={this.previewComponent}>
						preview page
					</Item>
					<Separator />
					<Item label='delete file' onClick={this.deleteFile}>
						delete file
					</Item>
				</ContextMenu>

				<ContextMenu id={menus.assets} animation={this.props.animation} theme={this.props.theme}>
					<Item label='open file' onClick={this.openFile}>
						open file
					</Item>
					<Item label='rename file' onClick={this.renameFile}>
						rename file
					</Item>
					<Separator />
					<Item label='delete file' onClick={this.deleteFile}>
						delete file
					</Item>
				</ContextMenu>

				<ContextMenu id={menus.language} animation={this.props.animation} theme={this.props.theme}>
					<Item label='open language' onClick={this.openLanguage}>
						open language
					</Item>
				</ContextMenu>

				<ContextMenu id={menus.webForm} animation={this.props.animation} theme={this.props.theme}>
					<Item label='open webForm' onClick={this.openWebForm}>
						open webForm
					</Item>
				</ContextMenu>
			</div>
		);
	}
}

Context.propTypes = {
	animation: PropTypes.string,
	theme: PropTypes.string,
	openProject: PropTypes.func,
	openUploader: PropTypes.func,
	editProject: PropTypes.func,
	previewProject: PropTypes.func,
	publishProject: PropTypes.func,
	buildNeeded: PropTypes.func,
	newFile: PropTypes.func,
	closeProject: PropTypes.func,
	previewPage: PropTypes.func,
	openFile: PropTypes.func,
	deleteFile: PropTypes.func,
	deleteFolder: PropTypes.func,
	renameFile: PropTypes.func,
	makeDefault: PropTypes.func,
	fetchLanguage: PropTypes.func,
	fetchWebForm: PropTypes.func,
	defaultCustomer: PropTypes.string,
	projectID: PropTypes.string,
	fileTimeStamp: PropTypes.number,
	buildTimeStamp: PropTypes.number,
	isLanguageOpen: PropTypes.bool,
	isWebFormEditable: PropTypes.bool
};

const mapDispatchToProps = dispatch => {
	const { name: CLOSE_PROJECT } = config.INTERNAL_SETTINGS.promptMessages.CLOSE_PROJECT;
	const { text: OPEN_FILE } = config.INTERNAL_SETTINGS.loadingText.OPEN_FILE;
	const { name: DELETE_FILE } = config.INTERNAL_SETTINGS.promptMessages.DELETE_FILE;
	const { name: DELETE_FOLDER } = config.INTERNAL_SETTINGS.promptMessages.DELETE_FOLDER;
	return ({
		newFile: node => dispatch(checkFileChanged(() => {
			dispatch(setPath(_.trimStart(node.Path, '/')));
			dispatch(modalOpen(<CreateFile />, createFileLabel, null));
		})),
		openProject: () => dispatch(checkFileChanged(() => dispatch(modalThemesFetch(openProjectLabel)))),
		editProject: () => dispatch(checkFileChanged(() => dispatch(modalOpen(<EditProject />, editProjectLabel, null)))),
		publishProject: () => dispatch(checkFileChanged(() =>
			dispatch(modalOpen(<CustomerList />, customerListLabel, null)))),
		closeProject: () => dispatch(modalOpen(<PromptMessage promptItem={CLOSE_PROJECT} />, promptMessageLabel, null)),
		openUploader: node => dispatch(checkFileChanged(() => {
			dispatch(setPath(_.trimStart(node.Path, '/')));
			dispatch(modalOpen(<UploadFile />, uploadFileLabel, null));
		})),
		previewPage: node => {
			dispatch(checkFileChanged(() => {
				let previewTab = window.open('', '_blank');
				previewTab.document.write('loading...');
				dispatch(previewPage(node, previewTab));
			}));
		},
		openFile: (name, path) => {
			dispatch(checkFileChanged(() => {
				dispatch(showLoading(OPEN_FILE));
				dispatch(fileSourceFetch(name, path));
			}));
		},
		renameFile: node => dispatch(modalOpen(<RenameFile currentNode={node} />), renameFileLabel, null),
		deleteFile: payload => dispatch(modalOpen(<PromptMessage promptItem={DELETE_FILE} payload={payload} />,
			promptMessageLabel, null)),
		deleteFolder: payload => dispatch(checkFileChanged(() =>
			dispatch(modalOpen(<PromptMessage promptItem={DELETE_FOLDER} payload={payload} />,
				promptMessageLabel, null)))),
		buildNeeded: () => dispatch(modalOpen(<BuildRequired />, buildRequiredLabel, null)),
		makeDefault: path => dispatch(makePageDefault(path)),
		previewProject: props => dispatch(checkFileChanged(() => {
			const { defaultCustomer, projectID, buildNeeded, fileTimeStamp, buildTimeStamp } = props;
			if (fileTimeStamp > buildTimeStamp) {
				buildNeeded();
			} else {
				window.open(`${httpDomainWithoutSSL}/preview/project=${projectID}/customer=${defaultCustomer}`, '_blank');
			}
		})),
		fetchLanguage: languageID => dispatch(checkFileChanged(() => {
			dispatch(editorClear());
			dispatch(isWebFormOpen(false));
			dispatch(schemaFetchDetails(languageID));
		})),
		fetchWebForm: (webForm, isEditable, isOpen) => {
			const { text: OPEN_WEBFORM } = config.INTERNAL_SETTINGS.loadingText.OPEN_WEBFORM;
			dispatch(checkFileChanged(() => {
				dispatch(editorClear());
				isOpen ? dispatch(resetSchema()) : '';
				dispatch(isWebFormOpen(true))
				if (isEditable) {
					dispatch(setIsEditable(false))
				}
				dispatch(setCanClone(false));
				dispatch(showLoading(OPEN_WEBFORM));
				dispatch(setCurrentWebForm(webForm));
				let webFormProperties = mapperFunction.getWebformObject(webForm);
				dispatch(getJsonOfWebForm(webForm.ActionId, webFormProperties))
			}));
		}
	})
};

const mapStateToProps = state => {
	const { editorReducer } = state;
	const { activeTabs, visibleIndex } = editorReducer;
	return {
		defaultCustomer: state.publishReducer.defaultCustomer,
		projectID: state.projectTreeReducer.data ? state.projectTreeReducer.data.ProjectId : null,
		fileTimeStamp: activeTabs[visibleIndex] ? activeTabs[visibleIndex].lastUpdateTimeStamp : null,
		buildTimeStamp: state.buildReducer.lastBuildStamp,
		isLanguageOpen: state.schemaCreatorReducer.isOpen,
		isWebFormEditable: state.webFormReducer.isEditable
	};
};

export default connect(mapStateToProps, mapDispatchToProps)(Context);
