import ace from 'brace';
import 'brace/ext/language_tools';
//import ace tools
import 'brace/ext/searchbox';
import 'brace/mode/css';
import 'brace/mode/handlebars';
import 'brace/mode/html';
import 'brace/mode/javascript';
import 'brace/mode/json';
import 'brace/mode/less';
import 'brace/mode/markdown';
import 'brace/mode/sass';
import 'brace/mode/scss';
import 'brace/mode/text';
import 'brace/mode/xml';
//import snippets for supported modes
import 'brace/snippets/css';
import 'brace/snippets/javascript';
import 'brace/snippets/markdown';
import { Base64 } from 'js-base64';
import Resizer from 'kt-resizer';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import AceEditor from 'react-ace';
import { connect } from 'react-redux';
import { addKitsuneErrors, codeChange, fileChanged, fileSourceCompile, fileSourceUpdate } from '../../actions/editor';
import { footerUpdate } from '../../actions/footer';
import { schemaFetchDetails } from '../../actions/schema';
import { config } from '../../config';
//import supported modes
import './brace-kitsune-syntax/kitsune';
import './brace-kitsune-syntax/kitsuneSnippets';
//import kitsune theme
import './brace-kitsune-syntax/kitsuneTheme';
import { EDITOR_PLUGINS, renderPluginComponents, renderPluginIcons } from './plugins';
import TabActions from './tabs/tabActions';

const resizeAce = (path) => setTimeout(function () {
	const editorQuery = document.querySelector(`#kitsune-editor-${path.replace(/\//g, '-')}`);
	if (editorQuery !== null) {
		const editor = ace.edit(`kitsune-editor-${path.replace(/\//g, '-')}`);
		editor.resize(true);
	}
}, 200);

const REF_EDITOR = 'editorRef';

class Editor extends Component {
	constructor(props) {
		super(props);
		this.onChange = this.onChange.bind(this);
		this.onLoad = this.onLoad.bind(this);
		this.renderFile = this.renderFile.bind(this);
		this.handleKeyBindings = this.handleKeyBindings.bind(this);
		this.mouseWheelHandler = this.mouseWheelHandler.bind(this);
		this.handleKeyUp = this.handleKeyUp.bind(this);
		this.togglePlugin = this.togglePlugin.bind(this);
		this.handleResize = this.handleResize.bind(this);

		this.autoCompile = '';
		this.ctrlKey = false;
		this.footerUpdater = '';

		this.state = {
			fullScreen: false,
			fontSize: 14,
			activePlugin: '',
			flex: {
				editor: 1,
				plugin: 1
			}
		};
	}

	handleResize(diff) {
		const diffRatio = (diff / this.refs[REF_EDITOR].offsetWidth);

		this.setState({
			flex: {
				editor: (this.state.flex.editor - diffRatio),
				plugin: (this.state.flex.plugin + diffRatio)
			}
		})
	}

	togglePlugin(plugin) {
		// Already active, and clicked again: hide
		if (plugin === this.state.activePlugin) {
			this.setState({ activePlugin: '', flex: { editor: 1, plugin: 1 } });
		} else {
			// Switch from another plugin, or open up fresh
			this.setState({ activePlugin: plugin });

			if (plugin === EDITOR_PLUGINS.SCHEMA_VIEWER && !this.props.schemaClasses) {
				this.props.dispatch(schemaFetchDetails(this.props.mappedSchemaId, true, true));
			}
		}
	}

	handleKeyBindings(e) {
		this.ctrlKey = e.key === 'Control' || e.key === 'Meta';
		if ((e.ctrlKey || e.metaKey) && e.key === '-') {
			e.preventDefault();
			const { fontSize } = this.state;
			const newSize = fontSize - 1 <= 1 ? 1 : fontSize - 1;
			this.setState({ fontSize: newSize });
		}
		if ((e.ctrlKey || e.metaKey) && e.key === '=') {
			e.preventDefault();
			const { fontSize } = this.state;
			const newSize = fontSize + 1;
			this.setState({ fontSize: newSize });
		}
		if ((e.ctrlKey || e.metaKey) && e.key === '0') {
			e.preventDefault();
			this.setState({ fontSize: 14 });
		}
	}

	handleKeyUp(e) {
		if (e.key === 'Control' || e.key === 'Meta') {
			this.ctrlKey = false;
		}
	}

	mouseWheelHandler(e) {
		const event = window.event || e;
		const delta = Math.max(-1, Math.min(1, (event.wheelDelta || -event.detail)));
		if (this.ctrlKey) {
			e.preventDefault();
			const { fontSize } = this.state;
			const newSize = fontSize + delta <= 1 ? 1 : fontSize + delta;
			this.setState({ fontSize: newSize });
		}
	}

	onLoad() {
		const { dispatch, editor } = this.props;

		setTimeout(() => {
			if (document.querySelector(`#kitsune-editor-${editor.path.replace(/\//g, '-')}`) !== null) {
				let kErrors = JSON.parse(sessionStorage.getItem('kErrors'));
				dispatch(addKitsuneErrors(kErrors, editor.path))
			}
		}, 1000);
		if (document.querySelector(`#kitsune-editor-${editor.path.replace(/\//g, '-')}`) !== null) {
			let editor = ace.edit(`kitsune-editor-${editor.path.replace(/\//g, '-')}`);
			let self = this;
			editor.container.onwebkitfullscreenchange = function () {
				if (!self.state.fullScreen) {
					editor.setStyle('full-screen-change');
					editor.resize()
				} else {
					editor.unsetStyle('full-screen-change')
					editor.resize()
				}
				self.state.fullScreen = !self.state.fullScreen;
			}
		}
	}

	onChange(newValue) {
		const { editor, dispatch, isEditorFetching, tabIndex } = this.props;
		const { fileConfig } = editor;

		if (this.autoCompile !== '') {
			clearTimeout(this.autoCompile)
		}
		if (this.footerUpdater !== '') {
			clearTimeout(this.footerUpdater)
		}

		if (isEditorFetching) {
			dispatch(codeChange(tabIndex, newValue));
		} else {
			if (!fileConfig.isStatic) {
				this.autoCompile = setTimeout(() => {
					const { path } = editor;
					dispatch(fileSourceCompile(path, Base64.encode(newValue)));
				}, 1000)
			}

			this.footerUpdater = setTimeout(() => {
				const { editor, tabIndex } = this.props;
				const { code, fileChanged: isFileChanged } = editor;
				const { NOTIFICATION } = config.INTERNAL_SETTINGS.footerTabs;
				const initCode = localStorage.getItem(`initialCode-${tabIndex}`);

				dispatch(codeChange(tabIndex, newValue));

				//if the current code is same as initial code and store says that file is changed
				if (initCode === code && isFileChanged) {
					//update the store value of fileChange
					dispatch(fileChanged(false, tabIndex));
				}
				//else if current code is not same as initial code and store say that file has not been changed
				else if (initCode !== code && !isFileChanged) {
					//update the store value of fileChange
					dispatch(fileChanged(true, tabIndex));
				}
				const editorErrors = ace.edit(`kitsune-editor-${editor.path.replace(/\//g, '-')}`).getSession().getAnnotations();
				const footerErrors = sessionStorage.getItem('footerErrors');
				if (footerErrors !== JSON.stringify(editorErrors) && fileConfig.isStatic) {
					dispatch(footerUpdate(editorErrors, NOTIFICATION));
				}
			}, 200)
		}
	}

	componentDidUpdate({ editor, isFooterActive }) {
		const { name: fileName } = editor;
		const { editor: { name: newFileName, path }, isFooterActive: newIsFooterActive } = this.props;

		//if file was not open before but is open now
		if (fileName === '' && newFileName !== '') {
			//add resize event listener
			window.addEventListener('resize', () => resizeAce(path));
			// Reset any active plugin
			this.setState({ activePlugin: '' });
		}
		//else if file as open before but is closed now
		else if (fileName !== '' && newFileName === '') {
			//remove resize event listner
			window.removeEventListener('resize', () => resizeAce(path));
		}
		if (isFooterActive !== newIsFooterActive) {
			resizeAce(path);
		}
	}

	renderFile() {
		const { editor } = this.props;
		const { type, code, fileConfig, path } = editor;
		const { fontSize } = this.state;

		let readOnlyEditor = false;

		switch (type) {
			case 'image':
				const { contentType } = fileConfig.File.ContentType;
				return (
					<div className='image-viewer'>
						<img src={`data:${contentType};base64,${code}`} />
					</div>
				)
			case 'svg':
				return (
					<div className='svg-viewer'>
						<img className='svg-container' src={`data:image/svg+xml;base64,${code}`} />
					</div>
				);
			case 'application':
				readOnlyEditor = true;
			default:
				const editorCommands = [
					{
						name: 'save',
						bindKey: { win: 'Ctrl-S', mac: 'Command-S' },
						exec: () => this.props.dispatch(fileSourceUpdate()),
						readOnly: true
					}
				];

				// Overrides:
				// The mode, and value getter can be customized for different file types
				// Example, the application uploads require a zip file to map to prettified-JSON
				return (
					<div className="editor-container" style={{ flex: this.state.flex.editor }}>
						<AceEditor
							name={`kitsune-editor-${path.replace(/\//g, '-')}`}
							readOnly={readOnlyEditor}
							mode={this.getEditorModeForType(type)}
							onChange={this.onChange}
							onLoad={this.onLoad}
							value={Base64.decode(code)}
							commands={editorCommands}
							enableBasicAutocompletion
							enableLiveAutocompletion
							fontSize={fontSize} />
					</div>
				);
		}
	}

	/**
	 * Renders any inline plugins for the editor.
	 * This allows sharing state between the two
	 * using a controlled component approach
	 */
	renderPlugins() {
		const { editor, schemaClasses } = this.props;
		const { name, type, code } = editor;
		const { activePlugin } = this.state;
		const { editorPlugins } = config.INTERNAL_SETTINGS;
		const pluginProps = {};

		switch (type) {
			case 'json':
			case 'application':
				let jsonData = Base64.decode(code);

				Object.assign(pluginProps, {
					fileName: name,
					fileType: type,
					fileData: jsonData,
					onChange: data => this.onChange(JSON.stringify(data, null, '\t'))
				})
				break;
			case 'kitsune':
				Object.assign(pluginProps, {
					schemaClasses
				})
			default:
		}

		const plugins = renderPluginComponents(editorPlugins[type], pluginProps, activePlugin);
		if (!!plugins) {
			return [
				<Resizer key='active-plugin-resizer' disabled onResize={this.handleResize} />,
				<div key='active-plugin' className="plugins" style={{ flex: this.state.flex.plugin }}>
					{plugins}
				</div>
			]
		} else {
			return null;
		}
	}

	getEditorModeForType(original) {
		let result = original;
		switch (original) {
			case 'application':
				return 'json';
			default:
				break;
		}

		return result;
	}

	componentWillReceiveProps({ editor }) {
		if (!!editor && !!editor.type) {
			document.addEventListener('keydown', this.handleKeyBindings, false);
			document.addEventListener('keyup', this.handleKeyUp, false);
			document.addEventListener("mousewheel", this.mouseWheelHandler, false);
			document.addEventListener("DOMMouseScroll", this.mouseWheelHandler, false);
		}
	}

	componentWillUnmount() {
		const { editor } = this.props;
		if (!!editor && !!editor.type) {
			document.removeEventListener('keydown', this.handleKeyBindings, false);
			document.removeEventListener('keyup', this.handleKeyUp, false);
			document.removeEventListener("mousewheel", this.mouseWheelHandler, false);
			document.removeEventListener("DOMMouseScroll", this.mouseWheelHandler, false);
		}
	}

	render() {
		if (this.props.editor && this.props.editor.type !== '') {
			const { className: editorClass, style, editor, isVisible } = this.props;

			const { type } = editor;
			const { editorPlugins } = config.INTERNAL_SETTINGS;

			const plugins = [];
			if (type in editorPlugins) {
				plugins.push(...renderPluginIcons(editorPlugins[type], this.togglePlugin, this.state.activePlugin));
			}

			return (
				<div className={`editor ${editorClass}`} style={style}>
					{isVisible && <TabActions editor={editor} plugins={plugins} />}
					<div ref={REF_EDITOR} className={`contents ${isVisible ? '' : 'hide'}`}>
						{this.renderFile()}
						{this.renderPlugins()}
					</div>
				</div>
			);
		}

		return null;
	}
}

AceEditor.defaultProps = {
	name: 'kitsune-editor',
	focus: true,
	theme: 'kitsuneTheme',
	value: '',
	showGutter: true,
	onChange: null,
	onPaste: null,
	onLoad: null,
	onScroll: null,
	minLines: null,
	maxLines: null,
	readOnly: false,
	highlightActiveLine: true,
	showPrintMargin: false,
	tabSize: 2,
	cursorStart: 1,
	editorProps: { $blockScrolling: true },
	setOptions: {
		enableSnippets: true
	},
	wrapEnabled: false,
	annotations: []
};

Editor.propTypes = {
	dispatch: PropTypes.func,
	style: PropTypes.object,
	className: PropTypes.string,
	isFooterActive: PropTypes.string,
	isEditorFetching: PropTypes.bool,
	editor: PropTypes.object,
};

const mapStateToProps = state => {
	const { isActive } = state.footerReducer;
	const { data } = state.projectTreeReducer;
	const { isFetching: schemaFetching, schemaDetails } = state.schemaCreatorReducer;

	return {
		isFooterActive: isActive,
		mappedSchemaId: data.SchemaId,
		isSchemaFetching: schemaFetching,
		schemaClasses: schemaDetails.Classes,
		webForm: state.webFormReducer,
	}
};

export default connect(mapStateToProps)(Editor);
