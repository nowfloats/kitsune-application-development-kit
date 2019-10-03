import React, { Component } from 'react';
import PropTypes from 'prop-types';
import { config } from '../../../config'
import { connect } from 'react-redux'
import { hideLoading, showLoading } from '../../../actions/loader';
import { checkFileChanged, fileSourceFetch } from '../../../actions/editor';
import { append, footerCollapse } from "../../../actions/footer";
import MinimizeIcon from '../../../images/minimize.svg';
import InfoIcon from '../../../images/info-icon.svg';
import _ from 'lodash';
import { multipleUpload, resetFailed, updateFiles } from "../../../actions/upload";
import Architecture from "../../architecture";
import CloseIcon from '../../../images/close-thin.svg';

const FailedFiles = props => {
	const { files, count, handleRetry } = props;
	if (files.length) {
		return (
			<p>{files.length} out of {count} files failed.
				<span className='retry-button' onClick={handleRetry}>retry</span>
			</p>
		);
	}
	return null;
};

const ArchitectureContainer = ({ close }) => (
	<div className='architecture-container'>
		<img src={CloseIcon} className='closeIcon' alt='close' onClick={() => close()} />
		<Architecture />
	</div>
);


class EventLogs extends Component {
	constructor(props) {
		super(props);
		this.handleCollapse = this.handleCollapse.bind(this);
		this.openErrorFile = this.openErrorFile.bind(this);
		this.handleRetry = this.handleRetry.bind(this);
		this.closeArchitecturePopup = this.closeArchitecturePopup.bind(this);

		this.state = {
			showArchitecture: false
		}
	}

	closeArchitecturePopup() {
		this.setState({ showArchitecture: false })
	}

	handleCollapse() {
		const { isActive, collapseFooter } = this.props;
		collapseFooter(isActive);
	}

	handleRetry() {
		const { failedFiles, failedPaths, retryUpload } = this.props;
		retryUpload(failedFiles, failedPaths);
	}

	componentDidUpdate({ footerLog }) {
		if (footerLog !== this.props.footerLog) {
			this.logs.scrollTo(0, this.logs.scrollHeight);
		}
	}

	openErrorFile(path) {
		const { openFile, filePath, isFileFetching } = this.props;
		openFile(path, filePath, isFileFetching);
	}

	render() {
		const {
			isActive,
			footerLog,
			failedFiles
		} = this.props;
		const { EVENT_LOG } = config.INTERNAL_SETTINGS.footerTabs;
		const logs = footerLog.map((item, index) => {
			switch (item.type) {
				case 'info':
				case 'error':
				case 'progress':
					return <p key={index}>{item.text}</p>;
				case 'view-arch':
					return (
						<p
							key={index}
							className={item.class}
							onClick={() => this.setState({ showArchitecture: true })}
						>
							{item.text}
						</p>
					);
				case 'error-urgent':
					return <p key={index}>
						in case of repeated build failures, please reach out to Chirag Makwana <a href='tel:+919099932030'>
							+91 909 993 2030</a> or Ronak Samantray <a href='tel:+917702302286'>+91 770 230 2286</a>
					</p>;
				case 'progress-with-bold':
					return (
						<p key={index}>
							<span>{item.textPreBold}</span>
							<span className='font-heavy'>{item.boldText}</span>
							<span>{item.textPostBold}</span>
						</p>
					);
				case 'link':
					return (
						<a key={index} href={item.href} target='_blank'>
							<span>{item.text}</span>
							<i className='fas fa-external-link-alt' />
						</a>
					);
				case 'text-and-link':
					return (
						<p key={index}>{item.text} <a href={item.href} target='_blank'>{item.hrefText}</a></p>
					);
				case 'project':
					return item.errors.map((iterator, index) => (
						<div key={index}>
							<p className='project-errors'>
								<img src={InfoIcon} />
								{iterator.text}
							</p>
						</div>
					));
				case 'file':
					return (
						<p key={index}>
							<i className={`${item.icon}`} />
							<span className='view-file'
								onClick={() => this.openErrorFile(item.path)}
								title='click to view file'>{item.path}</span>
							{item.text}
						</p>
					);
				case 'heading':
					return <h3 key={index}>{item.message}</h3>;
				case 'message':
					return (
						<p key={index}>
							{item.message}{item.success !== null ?
								<span className={item.success ? 'complete' : 'failed'}>
									{item.success ? 'complete' : 'failed'}</span> : null}
						</p>
					);
				case 'status':
					return (
						<div key={index}>
							<p>successfully uploaded {item.count - failedFiles.length} out of {item.count} files</p>
							<FailedFiles files={failedFiles} count={item.count} handleRetry={this.handleRetry} />
						</div>
					)
				case 'application_upload_status':
					return (
						<div key={index}>
							<p>successfully uploaded.
						kitsune compiler might take few hours to containerise your {item.appType} application.
						you will be notified by email once the container is live.</p>
							<FailedFiles files={failedFiles} count={item.count} handleRetry={this.handleRetry} />
						</div>
					)
			}
		});
		return (
			<div className={isActive === EVENT_LOG
				? 'content-container-eventlog active-eventlog'
				: 'content-container-eventlog'}>
				<header onClick={this.handleCollapse} title='click to collapse'>
					<h4>Event Logs</h4>
					<img src={MinimizeIcon} />
				</header>
				<section className='content logs'>
					<div className='log-list' ref={(node) => {
						this.logs = node
					}}>{logs}</div>
				</section>
				{this.state.showArchitecture && <ArchitectureContainer close={this.closeArchitecturePopup} />}
			</div>
		);
	}
}

EventLogs.propTypes = {
	isActive: PropTypes.string,
	footerLog: PropTypes.array,
	collapseFooter: PropTypes.func,
	retryUpload: PropTypes.func,
	openFile: PropTypes.func,
	filePath: PropTypes.string,
	isFileFetching: PropTypes.bool,
	failedFiles: PropTypes.array,
	failedPaths: PropTypes.array
};

FailedFiles.propTypes = {
	files: PropTypes.array,
	count: PropTypes.number,
	handleRetry: PropTypes.func
};

const mapDispatchToProps = dispatch => {
	const { text: UPLOAD_FILE } = config.INTERNAL_SETTINGS.loadingText.UPLOAD_FILE;
	const { text: OPEN_FILE } = config.INTERNAL_SETTINGS.loadingText.OPEN_FILE;
	return ({
		collapseFooter: current => dispatch(footerCollapse(current, current)),
		retryUpload: (failedFiles, failedPaths) => {
			dispatch(updateFiles(failedFiles, failedPaths));
			dispatch(showLoading(UPLOAD_FILE));
			dispatch(resetFailed());
			dispatch(multipleUpload())
				.then(({ projectId, count }) => {
					dispatch(append({
						type: 'status',
						count
					}));
					dispatch(hideLoading());
				})
		},
		openFile: (pathToOpen, currentPath, isFileFetching) => {
			const fileName = _.last(_.split(pathToOpen, '/'));
			dispatch(checkFileChanged(() => {
				if (currentPath !== pathToOpen && !isFileFetching) {
					dispatch(showLoading(OPEN_FILE));
					dispatch(fileSourceFetch(fileName, pathToOpen, false));
				}
			}));
		}
	});
};

ArchitectureContainer.propTypes = {
	close: PropTypes.func
};

const mapStateToProps = state => {
	const { activeTabs, visibleIndex } = state.editorReducer;
	const editor = activeTabs[visibleIndex];
	return {
		filePath: editor ? editor.path : null,
		isFileFetching: editor ? editor.isFetching : false,
		footerLog: state.footerReducer.log,
		failedFiles: state.uploadReducer.failed.files,
		failedPaths: state.uploadReducer.failed.paths
	}
};

export default connect(mapStateToProps, mapDispatchToProps)(EventLogs);
