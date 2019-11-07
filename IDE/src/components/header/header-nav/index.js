import React, { Component } from 'react';
import { connect } from 'react-redux';
import PropTypes from "prop-types";
import { config, httpDomainWithoutSSL } from "../../../config";
import { modalOpen, modalThemesFetch } from "../../../actions/modal";
import { openProjectLabel } from "../../modals/open-project";
import PromptMessage, { promptMessageLabel } from "../../modals/prompt";
import CreateProject, { createProjectLabel } from "../../modals/create-project";
import CreateFile, { createFileLabel } from "../../modals/create-file";
import EditProject, { editProjectLabel } from "../../modals/edit-project";
import BuildRequired, { buildRequiredLabel } from "../../modals/build-required";
import ComponentConfirmation, { componentConfirmationLabel } from "../../modals/component-confirmation";
import ComponentDisable, { componentDisableLabel } from "../../modals/component-disable";
import { buildProject } from "../../../actions/build";
import CustomerList, { customerListLabel } from "../../modals/customer-list";
import ReportBug, { MODAL_LABEL } from "../../modals/report-bug";
import { toastr } from "react-redux-toastr";
import { checkFileChanged, editorClear, fileSourceUpdate, singleFileSourceUpdate, setHelper } from "../../../actions/editor";
import ace from "brace/index";
import { previewPage, updateProjectData } from "../../../actions/projectTree";
import UploadFile, { uploadFileLabel, uploadApplicationLabel } from "../../modals/upload-file";
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome'
import { faCheck } from '@fortawesome/free-solid-svg-icons'
import { controlKey, shiftKey, altKey, supportedAceCommands } from './ace-commands';

class HeaderNav extends Component {
	constructor(props) {
		super(props);
		this.openExistingProjects = this.openExistingProjects.bind(this);
		this.createProject = this.createProject.bind(this);
		this.editProject = this.editProject.bind(this);
		this.buildProject = this.buildProject.bind(this);
		this.publishProject = this.publishProject.bind(this);
		this.publishToAll = this.publishToAll.bind(this);
		this.newFile = this.newFile.bind(this);
		this.closeProject = this.closeProject.bind(this);
		this.executeAceCommand = this.executeAceCommand.bind(this);
		this.closeFile = this.closeFile.bind(this);
		this.previewProject = this.previewProject.bind(this);
		this.renderComponents = this.renderComponents.bind(this);

		this.helper = {
			saveFile: this.props.updateSource,
			saveAllFiles: this.props.saveAllFiles,
			buildProject: this.buildProject
		};
		props.setHelpers(this.helper);

		this.state = {
			openCommand: `${controlKey}O`,
			saveCommand: `${controlKey}S`,
			saveAllFilesCommand: `${controlKey}${shiftKey}K`,
			findCommand: `${controlKey}F`,
			replaceCommand: `${controlKey}H`,
			buildCommand: `${controlKey}B`,
			previewCommand: `${controlKey}P`,
			publishCommand: `${controlKey}${shiftKey}P`,
			publishToAllCommand: `${controlKey}${altKey}P`,
			uploadedfileName: '',
			uploadedCode: '',
			uploadType: ''
		};
	}

	//open an existing project
	openExistingProjects() {
		const { checkFileChanged, openProjects } = this.props;
		checkFileChanged(openProjects);
	}

	//creating a new project
	createProject() {
		const { checkFileChanged, createProject } = this.props;
		checkFileChanged(createProject);
	}

	//edit opened project
	editProject() {
		const { checkFileChanged, editProject } = this.props;
		checkFileChanged(editProject);
	}

	//create new file

	newFile() {
		const { checkFileChanged, newFile } = this.props;
		checkFileChanged(newFile);
	}

	//build opened project
	buildProject() {
		const { checkFileChanged, buildProject } = this.props;
		checkFileChanged(buildProject);
	}

	//publish current project
	publishProject() {
		const { checkFileChanged, publishProject } = this.props;
		checkFileChanged(publishProject);
	}

	//publish to all
	publishToAll() {
		const { checkFileChanged, publishToAll } = this.props;
		checkFileChanged(publishToAll);
	}

	//preview the project
	previewProject() {
		const { defaultCustomer, projectID, buildNeeded, fileTimeStamp, buildTimeStamp, previewProject } = this.props;
		previewProject({ defaultCustomer, projectID, buildNeeded, fileTimeStamp, buildTimeStamp });
	}

	//closing the project
	closeProject() {
		const { currentPageState, closeProject } = this.props;
		const { name: fileNoneState } = config.INTERNAL_SETTINGS.pageStates.FILE_NONE;
		currentPageState === fileNoneState ? closeProject() :
			toastr.warning('sorry you have no open projects', 'please try opening a project first');
	}

	/*		Ace Commands	 */

	executeAceCommand(command) {
		const { visibleTab } = this.props;

		let editor = ace.edit(`kitsune-editor-${visibleTab.path.replace(/\//g, '-')}`);
		editor.execCommand(command);
	}

	/*		END		*/

	closeFile() {
		const { checkFileChanged, closeTab, visibleIndex } = this.props;
		checkFileChanged(() => closeTab(visibleIndex));
	}

	renderComponents() {
		const { components, toggleComponent, gateways } = this.props;
		return (
			<li>
				<p>Components</p>
				<ul>
					{components.map(({ componentID, componentName, isMapped }, i) => {
						switch (componentName) {
							case 'partial views':
								return (
									<div key={`header-comp-pv-${i}`}>
										<li className='line-separator'>
											<hr />
										</li>
										<li key={componentID}
											title={isMapped ? 'click to disable component' : `click to enable ${componentName} component`}
											onClick={() => toggleComponent(componentID, componentName, isMapped)}>
											<p className={isMapped ? 'is-mapped' : ''}>
												{componentName}
												<span className={isMapped ? 'component_selected_icon' : 'component_not_selected_icon'}>
													<FontAwesomeIcon icon={faCheck} />
												</span>
											</p>
										</li>
									</div>
								);
							case 'payment gateway':
								return (
									<li key={componentID}>
										<p>{componentName}</p>
										<ul>
											<li>
												<p>Bangladesh</p>
												<ul>
													<li title={isMapped ? 'click to disable aamarpay' : 'click to enable aamarpay'}
														onClick={() => toggleComponent(componentID, componentName, isMapped, 'aamarpay',
															gateways.aamarpay)}>
														<p className={gateways.aamarpay ? 'is-mapped' : ''}>aamarpay
														<span className={gateways.aamarpay ? 'component_selected_icon' : 'component_not_selected_icon'}>
																<FontAwesomeIcon icon={faCheck} />
															</span>
														</p>
													</li>
												</ul>
											</li>
											<li>
												<p>India</p>
												<ul>
													<li title={isMapped ? 'click to disable instamojo' : 'click to enable instamojo'}
														onClick={() => toggleComponent(componentID, componentName, isMapped, 'instamojo',
															gateways.instamojo)}>
														<p className={gateways.instamojo ? 'is-mapped' : ''}>instamojo
														<span className={gateways.instamojo ? 'component_selected_icon' : 'component_not_selected_icon'}>
																<FontAwesomeIcon icon={faCheck} />
															</span>
														</p>
													</li>
													<li title={isMapped ? 'click to disable paytm' : 'click to enable paytm'}
														onClick={() => toggleComponent(componentID, componentName, isMapped, 'paytm',
															gateways.paytm)}>
														<p className={gateways.paytm ? 'is-mapped' : ''}>paytm
														<span className={gateways.paytm ? 'component_selected_icon' : 'component_not_selected_icon'}>
																<FontAwesomeIcon icon={faCheck} />
															</span>
														</p>
													</li>
													<li title={isMapped ? 'click to disable payu' : 'click to enable payu'}
														onClick={() => toggleComponent(componentID, componentName, isMapped, 'payu',
															gateways.payu)}>
														<p className={gateways.payu ? 'is-mapped' : ''}>payu
														<span className={gateways.payu ? 'component_selected_icon' : 'component_not_selected_icon'}>
																<FontAwesomeIcon icon={faCheck} />
															</span>
														</p>
													</li>
												</ul>
											</li>
											<li>
												<p>International</p>
												<ul>
													<li title='coming soon' onClick={() => toastr.info('coming soon')}>
														<p>stripe</p>
													</li>
													<li title='coming soon' onClick={() => toastr.info('coming soon')}>
														<p>paddle</p>
													</li>
												</ul>
											</li>
										</ul>
									</li>
								);
							case 'super app':
								return (
									<div key={`header-comp-sa-${i}`}>
										<li className='line-separator'>
											<hr />
										</li>
										<li key={componentID}
											title={isMapped ? 'click to disable component' : `click to enable ${componentName} component`}
											onClick={() => toggleComponent(componentID, componentName, isMapped)}>
											<p className={isMapped ? 'is-mapped' : ''}>
												{componentName}
												<span className={isMapped ? 'component_selected_icon' : 'component_not_selected_icon'}>
													<FontAwesomeIcon icon={faCheck} />
												</span>
											</p>
										</li>
									</div>
								);
							default:
								return (
									<li key={componentID}
										title={isMapped ? 'click to disable component' : `click to enable ${componentName} component`}
										onClick={() => toggleComponent(componentID, componentName, isMapped)}>
										<p className={isMapped ? 'is-mapped' : ''}>
											{componentName}
											<span className={isMapped ? 'component_selected_icon' : 'component_not_selected_icon'}>
												<FontAwesomeIcon icon={faCheck} />
											</span>
										</p>
									</li>
								);
						}
					})}
				</ul>
			</li>
		);
	}

	render() {
		const {
			fileName,
			currentPageState,
			openUploader,
			openApplicationZipUploader,
			projectStatus,
			updateSource,
			saveAllFiles,
			deleteFile,
			isProjectOpen,
			reportBug,
			visibleTab
		} = this.props;
		const {
			openCommand,
			saveCommand,
			saveAllFilesCommand,
			findCommand,
			replaceCommand,
			buildCommand,
			previewCommand,
			publishCommand,
			publishToAllCommand
		} = this.state;
		const { name: fileNoneState } = config.INTERNAL_SETTINGS.pageStates.FILE_NONE;
		const isFileNoneState = currentPageState === fileNoneState;
		const noProjectClassName = isFileNoneState ? '' : 'disable disable-submenu';
		const noProjectTitle = isFileNoneState ? '' : 'try opening a project first';
		const publishStatus = projectStatus === 1;
		const buildClass = (projectStatus <= 0) && !publishStatus ? '' : 'disable disable-submenu';
		const isFileOpen = fileName ? true : false;
		const isAceActive = (visibleTab && document.querySelector(`#kitsune-editor-${visibleTab.path.replace(/\//g, '-')}`) !== null);
		const noFileClassName = isFileOpen ? '' : 'disable disable-submenu';
		const isFileOpenTitle = isFileOpen ? '' : 'try opening a file first';

		return (
			<ul className='header-nav'>
				<li>
					<p>Project</p>
					<ul>
						<li onClick={this.openExistingProjects}>
							<p>open project</p>
							<span>{openCommand}</span>
						</li>
						<li onClick={this.createProject}>
							<p>new project</p>
						</li>
						<li className={noProjectClassName} onClick={isFileNoneState ? this.editProject : ''} title={noProjectTitle}>
							<p>rename project</p>
						</li>
						<li className='line-separator'>
							<hr />
						</li>
						<li className={noProjectClassName} title={noProjectTitle}>
							<p>import existing application</p>
							<ul>
								<li className={noProjectClassName}
									onClick={() => openApplicationZipUploader('Drupal')}
									title='import an existing Drupal application into kitsune'>
									<p>drupal</p>
								</li>
								<li className={noProjectClassName}
									onClick={() => openApplicationZipUploader('Laravel')}
									title='import an existing Laravel application into kitsune'>
									<p>laravel</p>
								</li>
								<li className={noProjectClassName}
									onClick={() => openApplicationZipUploader('Wordpress')}
									title='import an existing Wordpress application into kitsune'>
									<p>wordpress</p>
								</li>
							</ul>
						</li>
						<li className='line-separator'>
							<hr />
						</li>
						<li className={`${noProjectClassName}`}
							onClick={isFileNoneState ? this.previewProject : ''}
							title={noProjectTitle}>
							<p>preview project</p>
							<span>{previewCommand}</span>
						</li>
						<li className={buildClass} title='build the current project' onClick={this.buildProject}>
							<p>build project</p>
							<span>{buildCommand}</span>
						</li>
						<li className={noProjectClassName} onClick={this.publishProject} title={noProjectTitle}>
							<p>{publishStatus ? 'publishing project' : 'publish project'}</p>
							<span>{publishCommand}</span>
						</li>
						<li className={noProjectClassName} onClick={this.publishToAll} title={noProjectTitle}>
							<p>{publishStatus ? 'publishing to all' : 'publish to all'}</p>
							<span>{publishToAllCommand}</span>
						</li>
						<li className='line-separator'>
							<hr />
						</li>
						<li className={noProjectClassName} onClick={isFileNoneState ? this.closeProject : ''}
							title={noProjectTitle}>
							<p>close project</p>
						</li>
					</ul>
				</li>
				<li>
					<p>File</p>
					<ul>
						<li className={noProjectClassName} onClick={this.newFile} title={noProjectTitle}>
							<p>new file</p>
						</li>
						<li className={noProjectClassName} onClick={openUploader} title={noProjectTitle}>
							<p>upload file</p>
						</li>
						<li className='line-separator'>
							<hr />
						</li>
						<li className={noFileClassName} onClick={isFileOpen ? updateSource : ''} title={isFileOpenTitle}>
							<p>save file</p>
							<span>{saveCommand}</span>
						</li>
						<li className={noFileClassName} onClick={isFileOpen ? saveAllFiles : ''} title={isFileOpenTitle}>
							<p>save all files</p>
							<span>{saveAllFilesCommand}</span>
						</li>
						<li className='line-separator'>
							<hr />
						</li>
						<li className={noFileClassName} onClick={isFileOpen ? deleteFile : ''} title={isFileOpenTitle}>
							<p>delete {isFileOpen ? `'${fileName}'` : 'file'}</p>
						</li>
						<li className={noFileClassName} onClick={isFileOpen ? this.closeFile : ''} title={isFileOpenTitle}>
							<p>close file</p>
						</li>
						<li className={noFileClassName} onClick={isFileOpen ? this.closeFile : ''} title={isFileOpenTitle}>
							<p>close all files</p>
						</li>
					</ul>
				</li>
				<li className={isFileOpen ? '' : 'disable-menu'}>
					<p>Edit</p>
					<ul>
						<li className={noFileClassName} onClick={isFileOpen ? () => this.executeAceCommand('find') : ''} title={isFileOpenTitle}>
							<p>find</p>
							<span>{findCommand}</span>
						</li>
						<li className={noFileClassName} onClick={isFileOpen ? () => this.executeAceCommand('replace') : ''} title={isFileOpenTitle}>
							<p>replace</p>
							<span>{replaceCommand}</span>
						</li>
					</ul>
				</li>
				<li className={isAceActive ? '' : 'hide'}>
					<p>Code</p>
					<ul>
						{/* Map all supported ace commands configured in the ace-commands file */}
						{supportedAceCommands.map((commandConf, i) => {
							if (!Array.isArray(commandConf)) {
								commandConf = [commandConf];
							}

							const result = commandConf.map((conf, j) => (
								<li key={`ace-command-${i}-${j}`} className={noFileClassName} onClick={isAceActive ? () => this.executeAceCommand(conf.command) : ''}>
									<p>{conf.desc}</p>
									<span>{conf.shortcut}</span>
								</li>
							))
							const separator = <li key={`ace-separator-${i}`} className='line-separator'><hr /></li>;

							// At the end of a command section, add a separator, unless it's the last item
							if (i !== (supportedAceCommands.length - 1)) result.push(separator);

							return result;
						})}
					</ul>
				</li>
				{isProjectOpen ? this.renderComponents() : null}
				<li>
					<p>Help</p>
					<ul>
						<li onClick={() => window.open('https://docs.getkitsune.com/', '_blank')}>
							<p>view documentation</p>
						</li>
						<li onClick={() => window.open('https://www.youtube.com/watch?v=iDhhZMtN6mw&list=PLNaZhYxuy2QSRCHv88kBdc_VjhcC5Aoh1', '_blank')}>
							<p>view video tutorials</p>
						</li>
						<li className='line-separator'>
							<hr />
						</li>
						<li onClick={() =>
							window.open('https://www.getkitsune.com/create-static-serverless-web-application',
								'_blank')}>
							<p>build a static website</p>
						</li>
						<li onClick={() =>
							window.open('https://docs.kitsune.tools/dynamic/creating-dynamic-websites/',
								'_blank')}>
							<p>build a dynamic website</p>
						</li>
						<li className='line-separator'>
							<hr />
						</li>
						<li onClick={reportBug}>
							<p>report an issue</p>
						</li>
						<li onClick={() => window.open('https://www.getkitsune.com/', '_blank')}>
							<p>view kitsune's official website</p>
						</li>
					</ul>
				</li>
			</ul>
		);
	}
}

HeaderNav.propTypes = {
	setHelpers: PropTypes.func,
	checkFileChanged: PropTypes.func,
	openProjects: PropTypes.func,
	openUploader: PropTypes.func,
	openApplicationZipUploader: PropTypes.func,
	createProject: PropTypes.func,
	editProject: PropTypes.func,
	buildProject: PropTypes.func,
	newFile: PropTypes.func,
	publishProject: PropTypes.func,
	publishToAll: PropTypes.func,
	buildNeeded: PropTypes.func,
	closeProject: PropTypes.func,
	closeTab: PropTypes.func,
	updateSource: PropTypes.func,
	saveAllFiles: PropTypes.func,
	deleteFile: PropTypes.func,
	previewProject: PropTypes.func,
	toggleComponent: PropTypes.func,
	reportBug: PropTypes.func,
	fileName: PropTypes.string,
	currentPageState: PropTypes.string,
	projectStatus: PropTypes.number,
	defaultCustomer: PropTypes.string,
	projectID: PropTypes.string,
	fileTimeStamp: PropTypes.number,
	buildTimeStamp: PropTypes.number,
	isProjectOpen: PropTypes.bool,
	components: PropTypes.array,
	gateways: PropTypes.object
};

const mapDispatchToProps = dispatch => {
	const { name: CLOSE_PROJECT } = config.INTERNAL_SETTINGS.promptMessages.CLOSE_PROJECT;
	const { name: DELETE_FILE } = config.INTERNAL_SETTINGS.promptMessages.DELETE_FILE;
	const { name: PUBLISH_ALL } = config.INTERNAL_SETTINGS.promptMessages.PUBLISH_ALL;
	return ({
		newFile: () => {
			dispatch(modalOpen(<CreateFile />, createFileLabel, null));
		},
		openUploader: () => dispatch(checkFileChanged(() => {
			dispatch(modalOpen(<UploadFile />, uploadFileLabel, null));
		})),
		openApplicationZipUploader: appType => dispatch(checkFileChanged(() => {
			dispatch(modalOpen(<UploadFile appType={appType} />, uploadApplicationLabel, null));
		})),
		previewFile: () => {
			let previewTab = window.open('', '_blank');
			previewTab.document.write('loading...');
			dispatch(previewPage(null, previewTab));
		},
		checkFileChanged: callback => dispatch(checkFileChanged(callback)),
		setHelpers: helpers => dispatch(setHelper(helpers)),
		openProjects: () => dispatch(modalThemesFetch(openProjectLabel)),
		createProject: () => dispatch(modalOpen(<CreateProject />, createProjectLabel, null)),
		editProject: () => dispatch(modalOpen(<EditProject />, editProjectLabel, null)),
		buildProject: () => {
			dispatch(buildProject())
				.then(() => {
					toastr.success('build successful', `your project has been successfully built.`);
					dispatch(updateProjectData());
				})
				.catch(error => toastr.error('build failed', error.message));
		},
		publishProject: () => dispatch(modalOpen(<CustomerList />, customerListLabel, null)),
		publishToAll: () => dispatch(modalOpen(<PromptMessage promptItem={PUBLISH_ALL} />, promptMessageLabel, null)),
		closeProject: () => dispatch(modalOpen(<PromptMessage promptItem={CLOSE_PROJECT} />, promptMessageLabel, null)),
		buildNeeded: () => dispatch(modalOpen(<BuildRequired />, buildRequiredLabel, null)),
		updateSource: () => dispatch(fileSourceUpdate(true)),
		saveAllFiles: () => dispatch(fileSourceUpdate(false)),
		closeTab: index => dispatch(editorClear(index)),
		deleteFile: () => dispatch(modalOpen(<PromptMessage promptItem={DELETE_FILE} />, promptMessageLabel, null)),
		previewProject: props => dispatch(checkFileChanged(() => {
			const { defaultCustomer, projectID, buildNeeded, fileTimeStamp, buildTimeStamp } = props;
			if (fileTimeStamp > buildTimeStamp) {
				buildNeeded();
			} else {
				window.open(`${httpDomainWithoutSSL}/preview/project=${projectID}/customer=${defaultCustomer}`, '_blank');
			}
		})),
		toggleComponent: (componentID, componentName, boolean, gatewayName, isGatewayMapped) => {
			if (componentName === 'payment gateway') {
				isGatewayMapped
					? dispatch(modalOpen(<ComponentDisable componentID={componentID}
						componentName={componentName}
						gatewayName={gatewayName} />,
						componentDisableLabel, null))
					: dispatch(modalOpen(<ComponentConfirmation componentID={componentID}
						componentName={componentName}
						gatewayName={gatewayName} />, componentConfirmationLabel, null))
			} else {
				boolean
					? dispatch(modalOpen(<ComponentDisable componentID={componentID} componentName={componentName} />,
						componentDisableLabel, null))
					: dispatch(modalOpen(<ComponentConfirmation componentID={componentID} componentName={componentName} />,
						componentConfirmationLabel, null))
			}
		},
		reportBug: () => dispatch(modalOpen(<ReportBug />, MODAL_LABEL, null))
	})
};

const mapStateToProps = state => {
	const { activeTabs, visibleIndex } = state.editorReducer;
	const { name, lastUpdateTimeStamp } = activeTabs[visibleIndex] || {};
	return {
		fileName: name,
		visibleIndex,
		visibleTab: activeTabs[visibleIndex],
		currentPageState: state.pageStateReducer.pageState,
		projectStatus: state.projectTreeReducer.data.ProjectStatus !== undefined ?
			state.projectTreeReducer.data.ProjectStatus : 99,
		isNFSchema: state.projectTreeReducer.isNFSchema,
		defaultCustomer: state.publishReducer.defaultCustomer,
		projectID: state.projectTreeReducer.data ? state.projectTreeReducer.data.ProjectId : null,
		fileTimeStamp: lastUpdateTimeStamp,
		buildTimeStamp: state.buildReducer.lastBuildStamp,
		isProjectOpen: state.projectTreeReducer.name !== '',
		components: state.projectTreeReducer.components,
		gateways: state.projectTreeReducer.gateways
	}
};

export default connect(mapStateToProps, mapDispatchToProps)(HeaderNav);
