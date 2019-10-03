import { decorators, Treebeard } from 'kt-treebeard';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { VelocityComponent } from 'velocity-react';
import { checkFileChanged, fileSourceFetch, getFileExtension, tabChanged } from '../../../actions/editor';
import { showLoading } from '../../../actions/loader';
import { projectGetInit, projectUpdate } from '../../../actions/projectTree';
import { config } from '../../../config';
import * as filters from './filter';

//Customising The header Decorator To Include Icons
decorators.Header = props => {
	const style = props.style;
	const { name: fileName } = props.node;
	const fileExtension = getFileExtension(fileName);
	const { extensionMap, extensionsIconMap } = config.INTERNAL_SETTINGS;
	//If our mapper doesn't recognize the extension, then open as a text file.
	const fileType = props.node.children ? (props.node.toggled ? 'folderOpen' : 'folder')
		: (extensionMap.get(fileExtension) === undefined ? 'text' : extensionMap.get(fileExtension));
	const projectName = sessionStorage.getItem('projectName');

	const iconType = extensionsIconMap.get(fileType) === undefined ? 'fas fa-file' : extensionsIconMap.get(fileType);

	const iconClass = projectName === fileName ? `${iconType} project-root` : `${iconType}`;
	const iconStyle = { marginRight: '5px' };
	if (props.node.highlightText === undefined) {
		return (
			<div style={style.base}>
				<div style={style.title} className='folder-parent'>
					<i className={iconClass} style={iconStyle} />
					<span>{props.node.name}</span>
				</div>
			</div>
		);
	} else {
		return (
			<div style={style.base}>
				<div style={style.title} className='folder-parent'>
					<i className={iconClass} style={iconStyle} />
					<span>
						{props.node.beforeHighlighted}
						<span className='search-highlight'>{props.node.highlighted}</span>
						{props.node.afterHighlighted}
					</span>
				</div>
			</div>
		);
	}
};

//Customising the toggle section
decorators.Toggle = props => {
	const style = props.style;
	const height = 8;
	const width = 6;
	let midHeight = height * 0.5;
	let points = `0,0 0,${height} ${width},${midHeight}`;
	return (
		<div style={style.base}>
			<div className='toggle-wrapper'>
				<svg height={height} width={width} visibility={props.node.children.length ? 'visible' : 'hidden'}>
					<polygon
						points={points}
						style={style.arrow} />
				</svg>
			</div>
		</div>
	);
};

//Customising the Container section
decorators.Container = props => {
	const { style, terminal, onClick, node, animations } = props;

	if (!animations) {
		return (
			<div onClick={onClick} className={node.active ? 'node-active' : ''} style={style.container[0]}>
				{!terminal ? <decorators.Toggle node={node} style={style.toggle} /> : null}
				<decorators.Header node={node} style={style.header} />
			</div>
		);
	} else {
		return (
			<div onClick={onClick} className={node.active ? 'node-active' : ''} style={style.container[0]}>
				{
					!terminal ?
						<VelocityComponent duration={animations.toggle.duration} animation={animations.toggle.animation}>
							<decorators.Toggle node={node} style={style.toggle} />
						</VelocityComponent> : null
				}
				<decorators.Header node={node} style={style.header} />
			</div>
		);
	}
};

// Proptype definitions
decorators.Container.propTypes = {
	style: PropTypes.object,
	terminal: PropTypes.bool,
	onClick: PropTypes.func,
	node: PropTypes.object,
	animations: PropTypes.object
};

decorators.Header.propTypes = {
	style: PropTypes.object.isRequired,
	node: PropTypes.shape({
		name: PropTypes.string.isRequired,
		children: PropTypes.array,
		highlightText: PropTypes.string,
		beforeHighlighted: PropTypes.string,
		highlighted: PropTypes.string,
		afterHighlighted: PropTypes.string,
		toggled: PropTypes.bool
	})
}

decorators.Toggle.propTypes = {
	style: PropTypes.object,
	node: PropTypes.object,
}

class ProjectTree extends Component {

	constructor(props) {
		super(props);
		this.onToggle = this.onToggle.bind(this);
		this.onFilterMouseUp = this.onFilterMouseUp.bind(this)
	}

	componentWillMount() {
		this.state = { data: { name: 'project-explorer', toggled: true, children: [], isFetching: false } };
		sessionStorage.setItem('existing-filter', '');
	}

	onToggle(node, toggled) {
		const { dispatch, editor } = this.props;
		if (this.state.cursor) { this.state.cursor.active = false; }
		if (node.children === null) {
			const { activeTabs, visibleIndex, isFetching } = editor;
			const existingTab = activeTabs.find(tab => tab.path === node.Path);
			const existingTabIndex = !!existingTab ? activeTabs.indexOf(existingTab) : -1;

			if (!!existingTab) {
				if (existingTabIndex !== visibleIndex) dispatch(tabChanged(existingTabIndex));
				node.active = true;
			} else if (!existingTab && !isFetching) {
				dispatch(showLoading(config.INTERNAL_SETTINGS.loadingText.OPEN_FILE.text));
				dispatch(fileSourceFetch(node.name, node.Path))
				node.active = true;
			}
		} else if (node.children !== null && node.children.length !== 0) {
			node.active = true;
			node.toggled = toggled;
		}
		this.setState({ cursor: node });
	}

	componentDidUpdate({ projectTree }) {
		const { ProjectId } = this.props.projectTree.data;
		const { ProjectId: prevProjectId } = projectTree.data;
		if (ProjectId !== prevProjectId) {
			this.searchBoxValue.value = '';
		}
	}

	onFilterMouseUp(e) {
		let previousFilter = sessionStorage.getItem('existing-filter').trim()
		const filter = e.target.value.trim();
		if (filter !== previousFilter) {
			sessionStorage.setItem('existing-filter', filter);
			let filtered = null;
			const { dispatch } = this.props;
			if (!filter) {
				return dispatch(projectGetInit());
			}
			if (e.keyCode === 8) {
				filtered = filters.filterTree(JSON.parse(localStorage.getItem('project-init')), filter);
			} else {
				filtered = filters.filterTree(this.state.data, filter);
			}
			filtered = filters.expandFilteredNodes(filtered, filter);
			filtered = filters.highlightFilteredText(filtered, filter)
			dispatch(projectUpdate(filtered))
		}
	}

	componentWillReceiveProps(nextProps) {
		const { editor, projectTree } = nextProps;
		const { activeTabs, visibleIndex } = editor;
		const activePath = activeTabs.length && visibleIndex < activeTabs.length ? activeTabs[visibleIndex].path : '';
		let activeNode = null;

		if (projectTree) {
			for (let node of projectTree.children) {
				if (node.Path === activePath) {
					node.active = true;
					activeNode = node;
				} else {
					node.active = false;
				}
			}

			this.setState({ data: projectTree, cursor: activeNode });
		}
	}

	render() {
		const { isActive, variant } = this.props;
		const { data } = this.state;

		let { PROJECT_EXPLORER } = config.INTERNAL_SETTINGS.sidebarItems[variant];
		let contextMenus = config.INTERNAL_SETTINGS.contextMenus;
		return (
			<div className={isActive === PROJECT_EXPLORER.key ? '' : 'hide'}>
				<div className='search-box'>
					<div className='input-group'>
						<input type='text'
							className='form-control'
							placeholder='search your files here...'
							ref={input => this.searchBoxValue = input}
							onKeyUp={this.onFilterMouseUp} />
						<span className='focus-border'><i /></span>
					</div>
				</div>
				<div className='project-files'>
					<Treebeard data={data} onToggle={this.onToggle} decorators={decorators}
						contextMenuId={contextMenus} />
				</div>
			</div>
		);
	}
}

ProjectTree.propTypes = {
	dispatch: PropTypes.func,
	editor: PropTypes.object,
	isActive: PropTypes.string,
	projectTree: PropTypes.object // eslint-disable-line
}

const mapStateToProps = (state) => {
	return {
		editor: state.editorReducer,
		projectTree: state.projectTreeReducer
	}
}

export default connect(mapStateToProps)(ProjectTree)
