import React, { Component } from 'react';
import ace from 'brace';
import PropTypes from 'prop-types';
import Header from '../header/index';
import SideBar from '../sidebar/index';
import Footer from '../footer/index';
import Schema from '../schema/index';
import Editor from '../editor/index';
import ThirdParty from '../third-party/index';
import { connect } from 'react-redux';
import { checkFileChanged, editorClear } from '../../actions/editor';
import { modalOpen, modalThemesFetch } from '../../actions/modal';
import { projectFetch, updateProjectData } from '../../actions/projectTree';
import { config, httpDomainWithoutSSL } from '../../config';
import { footerInit } from '../../actions/footer';
import PageStates from '../page-states/index';
import Resizer from 'kt-resizer';
import { openProjectLabel } from '../modals/open-project/index';
import WebForm from '../web-form/index';
import { toastr } from "react-redux-toastr";
import { buildProject } from "../../actions/build";
import CustomerList, { customerListLabel } from "../modals/customer-list";
import BuildRequired, { buildRequiredLabel } from "../modals/build-required";
import PromptMessage, { promptMessageLabel } from "../modals/prompt";
import MultiTab from '../editor/multiTab';

class Home extends Component {
	constructor(props) {
		super(props);
		const { sidebar, editor, resizer } = config.INTERNAL_SETTINGS.widths;
		const { header, footer, footer: footerSettings, resizer: resizerSettings } = config.INTERNAL_SETTINGS.heights;
		this.removeFile = this.removeFile.bind(this);
		this.handleResize = this.handleResize.bind(this);
		this.handleResizeEnd = this.handleResizeEnd.bind(this);
		this.handleFooterResize = this.handleFooterResize.bind(this);
		this.handleFooterResizeEnd = this.handleFooterResizeEnd.bind(this);
		this.autoLoadProject = this.autoLoadProject.bind(this);
		this.renderWebForm = this.renderWebForm.bind(this);
		this.handleKeyBindings = this.handleKeyBindings.bind(this);
		this.state = {
			sidebar: {
				width: sidebar.width,
				minwidth: sidebar.minwidth,
				maxwidth: `calc(100% - ${editor.minwidth + resizer.width}px)`,
				resizing: false
			},
			editor: {
				width: `calc(100% - ${sidebar.width + resizer.width}px)`,
				minwidth: editor.minwidth
			},
			playground: {
				height: `calc(100% - ${header.height}px - ${footer.collapsedHeight}px)`
			},
			footer: {
				height: footer.collapsedHeight,
				collapsed: true,
				resizing: false
			}
		};
		sessionStorage.setItem('footer', JSON.stringify({
			footer: footerSettings.height,
			playground: `calc(100% - ${header.height}px - ${footerSettings.height}px - ${resizerSettings.height}px)`
		}));
	}

	//function to handle resizing of sidebar
	handleResize(diff) {
		const { sidebar, editor } = this.state;
		const { isSchemaOpen, webFormReducer } = this.props;
		const { isOpen } = webFormReducer;
		const { FILE_NONE } = config.INTERNAL_SETTINGS.pageStates;
		const { resizer, editor: editorW } = config.INTERNAL_SETTINGS.widths;
		const { pageState: pageState, editor: editorR } = this.props;
		const { activeTabs, visibleIndex } = editorR;
		const activeTab = activeTabs[visibleIndex];
		let resizeValue = sidebar.width - diff;
		sidebar.resizing = true;
		sidebar.width = resizeValue;
		editor.width = `calc(100% - ${(sidebar.width + resizer.width)}px)`;
		editor.minwidth = (isSchemaOpen || isOpen) ? 960 : editorW.minwidth;

		this.setState({ sidebar: { ...sidebar }, editor: { ...editor } });
		if (pageState.pageState === FILE_NONE.name && activeTab) {
			const editorExists = document.getElementById(`kitsune-editor-${activeTab.path.replace(/\//g, '-')}`);
			if (editorExists) {
				let editorInstance = ace.edit(`kitsune-editor-${activeTab.path.replace(/\//g, '-')}`);
				editorInstance.resize(true);
			}
		}
	}

	//function to handle edge cases(max-width and min-width) of resizing the sidebar
	handleResizeEnd() {
		const { innerWidth: windowWidth } = window;
		const { sidebar, editor } = this.state;
		const { resizer } = config.INTERNAL_SETTINGS.widths;
		sidebar.resizing = false;

		if (sidebar.width < sidebar.minwidth) {
			sidebar.width = sidebar.minwidth;
			editor.width = `calc(100% - ${sidebar.minwidth + resizer.width}px)`;
		} else if (sidebar.width > (windowWidth - (editor.minwidth + resizer.width))) {
			sidebar.width = windowWidth - (editor.minwidth + resizer.width);
			editor.width = `calc(100% - ${windowWidth - (editor.minwidth + resizer.width)}px)`;
		}
		this.setState({ sidebar: { ...sidebar }, editor: { ...editor } });
	}

	//function to handle resizing of footer
	handleFooterResize(diff) {
		const { innerHeight: windowHeight } = window;
		const { footer, playground } = this.state;
		const
			{ header, resizer, footer: footerSettings, playground: playgroundSettings } = config.INTERNAL_SETTINGS.heights;
		let resizeValue = footer.height + diff;
		const { name: FILE_NONE } = config.INTERNAL_SETTINGS.pageStates.FILE_NONE;
		const { pageState: pageState, editor: editorR } = this.props;
		const { activeTabs, visibleIndex } = editorR;
		footer.resizing = true;
		footer.height = resizeValue;
		playground.height = `calc(100% - ${header.height}px - ${footer.height}px - ${resizer.height}px)`;
		this.setState({ footer, playground });
		if (pageState.pageState === FILE_NONE &&
			activeTabs.length > 0 &&
			footer.height > footerSettings.minheight &&
			footer.height < (windowHeight - (playgroundSettings.minheight + resizer.height + header.height))) {
			setTimeout(() => {
				let editorInstance = ace.edit(`kitsune-editor-${activeTabs[visibleIndex].path.replace(/\//g, '-')}`);
				editorInstance.resize(true);
			}, 200);
		}
	}

	//function to handle edge cases(max-width and min-width) of resizing the footer
	handleFooterResizeEnd() {
		const { innerHeight: windowHeight } = window;
		const { footer, playground } = this.state;
		const
			{ header, footer: footerSettings, resizer, playground: playgroundSettings } = config.INTERNAL_SETTINGS.heights;
		footer.resizing = false;

		if (footer.height < footerSettings.minheight) {
			footer.height = footerSettings.minheight;
			playground.height = `calc(100% - ${header.height + footer.height + resizer.height}px)`;
		} else if (footer.height > (windowHeight - (playgroundSettings.minheight + resizer.height + header.height))) {
			footer.height = windowHeight - (playgroundSettings.minheight + resizer.height + header.height);
			playground.height = `calc(100% - ${footer.height + header.height + resizer.height}px)`;
		}
		this.setState({ footer, playground })
	}

	componentDidMount() {
		const { dispatch } = this.props;
		document.addEventListener('contextmenu', event => event.preventDefault());
		window.document.body.onkeydown = e => {
			if ((e.ctrlKey || e.metaKey) && e.keyCode === 79) {
				e.preventDefault();
				dispatch(modalThemesFetch(openProjectLabel));
			}
		};
		this.autoLoadProject();
	}

	/**
	 * Project routing: /project/:projectid
	 */
	autoLoadProject() {
		const { params } = this.props;
		let projectId = params ? params.projectid : null;
		projectId = projectId || window.sessionStorage.getItem('currentThemeId');

		if (projectId) {
			const { dispatch } = this.props;
			dispatch(projectFetch(projectId, false));
		}
	}

	removeFile() {
		const { dispatch, footer } = this.props;
		const { saveLog } = footer;

		dispatch(checkFileChanged(() => {
			dispatch(editorClear());
			dispatch(footerInit(saveLog));
			localStorage.removeItem('initialCode');
		}));
	}

	renderWebForm(webForm, editorStyles, sidebar) {
		if (webForm.isOpen)
			return (<WebForm style={editorStyles} className={sidebar.resizing ? 'notransition' : ''} />)
	}

	handleKeyBindings(e) {
		const { dispatch } = this.props;

		if ((e.ctrlKey || e.metaKey) && e.keyCode === 66) {
			e.preventDefault();
			dispatch(buildProject())
				.then(() => {
					toastr.success('build successful', `your project has been successfully built.`);
					dispatch(updateProjectData());
				})
				.catch(error => toastr.error('build failed', error.message));
		} else if ((e.ctrlKey || e.metaKey) && e.shiftKey && e.keyCode === 80) {
			e.preventDefault();
			dispatch(checkFileChanged(() => dispatch(modalOpen(<CustomerList />, customerListLabel, null))));
		} else if ((e.ctrlKey || e.metaKey) && e.altKey && e.keyCode === 80) {
			e.preventDefault();
			const { name: PUBLISH_ALL } = config.INTERNAL_SETTINGS.promptMessages.PUBLISH_ALL;
			dispatch(checkFileChanged(() =>
				dispatch(modalOpen(<PromptMessage promptItem={PUBLISH_ALL} />, promptMessageLabel, null))));
		} else if ((e.ctrlKey || e.metaKey) && e.keyCode === 80) {
			e.preventDefault();
			dispatch(checkFileChanged(() => {
				const { defaultCustomer, projectID, fileTimeStamp, buildTimeStamp } = this.props;
				if (fileTimeStamp > buildTimeStamp) {
					dispatch(modalOpen(<BuildRequired />, buildRequiredLabel, null))
				} else {
					window.open(`${httpDomainWithoutSSL}/preview/project=${projectID}/customer=${defaultCustomer}`, '_blank');
				}
			}))
		} else if ((e.ctrlKey || e.metaKey) && e.keyCode === 83) {
			e.preventDefault();
		}
	}

	componentWillReceiveProps({ footer, pageState }) {
		const { footer: footerS, playground } = this.state;
		const { footer: currentFooter, pageState: currentPageState } = this.props;
		const { header, footer: footerSettings } = config.INTERNAL_SETTINGS.heights;
		if (footer.isActive === null && currentFooter.isActive !== null) {
			sessionStorage.setItem('footer', JSON.stringify({
				footer: footerS.height,
				playground: playground.height
			}));
			footerS.collapsed = true;
			footerS.height = footerSettings.collapsedHeight;
			playground.height = `calc(100% - ${header.height}px - ${footerSettings.collapsedHeight}px)`;
			this.setState({ footer: footerS, playground });
		} else if (footer.isActive !== null && currentFooter.isActive === null) {
			const { playground: playgroundPrev, footer: footerPrev } = JSON.parse(sessionStorage.getItem('footer'));
			footerS.collapsed = false;
			footerS.height = footerPrev;
			playground.height = playgroundPrev;
			this.setState({ footer: footerS, playground });
		}
		//check if project has been opened
		const { name: FILE_NONE } = config.INTERNAL_SETTINGS.pageStates.FILE_NONE;
		if (pageState.pageState === FILE_NONE && currentPageState.pageState !== FILE_NONE) {
			document.addEventListener('keydown', this.handleKeyBindings, false);
		} else if (currentPageState.pageState === FILE_NONE && pageState.pageState !== FILE_NONE) {
			document.removeEventListener('keydown', this.handleKeyBindings, false);
		}
	}

	render() {
		const { sidebar, editor, footer: footerS, playground } = this.state;
		const { pageState: pageState, footer: footer, webFormReducer: webForm } = this.props;
		const { FILE_NONE } = config.INTERNAL_SETTINGS.pageStates;
		let sidebarStyles = {
			left: {
				width: sidebar.width,
				minWidth: sidebar.minwidth,
				maxWidth: sidebar.maxwidth
			},
			right: {
				minWidth: sidebar.minwidth,
			}
		},
			editorStyles = {
				width: editor.width,
				minWidth: editor.minwidth,
			},
			playgroundStyles = {
				height: playground.height
			},
			footerStyle = {
				height: footerS.height
			};
		return (
			<div>
				<ThirdParty />
				<Header />
				<div id='playground' style={playgroundStyles} className={footer.isActive !== null ? 'active' : ''}>
					<SideBar style={sidebarStyles.left} className={`${pageState.pageState !== FILE_NONE.name ? 'hide sidebar' :
						'sidebar'} ${sidebar.resizing ? 'notransition' : ''}`} />
					<Resizer className={pageState.pageState !== FILE_NONE.name ? 'animated-hide' : ''}
						onResize={this.handleResize} onResizeEnd={this.handleResizeEnd} direction='x' />
					<MultiTab
						style={{ ...editorStyles }}
						className={sidebar.resizing || footerS.resizing ? 'notransition' : ''}
					/>
					<Schema style={{ ...editorStyles }} footerResizerStyle={playgroundStyles} />
					<PageStates style={{ ...editorStyles }} className={sidebar.resizing || footerS.resizing ? 'notransition' : ''} />
					{this.renderWebForm(webForm, { ...editorStyles }, sidebar)}
				</div>
				<Resizer className={footerS.collapsed ? 'footer-resizer animated-hide' : 'footer-resizer'} direction='y'
					onResize={this.handleFooterResize} onResizeEnd={this.handleFooterResizeEnd} />
				<Footer style={footerStyle} errorLogs={footer.log} footerToggle={footer.toggle} />
			</div>
		);

	}
}

Home.propTypes = {
	dispatch: PropTypes.func,
	params: PropTypes.object,
	editor: PropTypes.shape({
		activeTabs: PropTypes.array,
		visibleIndex: PropTypes.number,
	}),
	pageState: PropTypes.shape({
		pageState: PropTypes.string
	}),
	footer: PropTypes.shape({
		log: PropTypes.array,
		isActive: PropTypes.string,
		type: PropTypes.string,
		saveLog: PropTypes.object
	}),
	isSchemaOpen: PropTypes.bool,
	webFormReducer: PropTypes.shape({
		isOpen: PropTypes.bool
	}),
	defaultCustomer: PropTypes.string,
	projectID: PropTypes.string,
	fileTimeStamp: PropTypes.number,
	buildTimeStamp: PropTypes.number,
};

const mapStateToProps = (state) => {
	const { activeTabs, visibleIndex } = state.editorReducer;
	const activeEditor = activeTabs[visibleIndex];
	return {
		editor: state.editorReducer,
		pageState: state.pageStateReducer,
		footer: state.footerReducer,
		isSchemaOpen: state.schemaCreatorReducer.isOpen,
		webFormReducer: state.webFormReducer,
		login: state.login,
		defaultCustomer: state.publishReducer.defaultCustomer,
		projectID: state.projectTreeReducer.data ? state.projectTreeReducer.data.ProjectId : null,
		fileTimeStamp: activeEditor ? activeEditor.lastUpdateTimeStamp : null,
		buildTimeStamp: state.buildReducer.lastBuildStamp
	}
};

export default connect(mapStateToProps)(Home)
