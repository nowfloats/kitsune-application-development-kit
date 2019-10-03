import React, { Component } from 'react';
import PropTypes from 'prop-types';
import { bindActionCreators } from 'redux';
import { connect } from 'react-redux';
import Dropzone from 'kt-dropzone';
import FlipMove from 'react-flip-move';
import CloseIcon from '../../../images/close-thin.svg';
import IssueIcon from '../../../images/bug.svg';
import { modalClose, reportBug } from '../../../actions/modal';

export const MODAL_LABEL = 'report a bug';

const PreviewImage = ({ selectedFiles, onFileRemoved }) => (
	<FlipMove typeName='ul'>
		{selectedFiles.map((image, index) =>
			<li key={index}>
				<div className='image-container'>
					<img src={image.preview} />
				</div>
				<span onClick={() => onFileRemoved(index)} className='fas fa-times-circle' />
			</li>
		)}
	</FlipMove>
);

class ReportBug extends Component {
	constructor(props) {
		super(props);

		this.state = {
			description: '',
			selectedFiles: []
		};

		this.onFilesSelected = this.onFilesSelected.bind(this);
		this.onFileRemoved = this.onFileRemoved.bind(this);
		this.onSubmit = this.onSubmit.bind(this);
	}

	onFilesSelected(files) {
		this.setState({
			selectedFiles: [...this.state.selectedFiles, ...files]
		});
	}

	onFileRemoved(id) {
		this.setState({
			selectedFiles: this.state.selectedFiles.filter((_, index) => index !== id)
		});
	}

	onSubmit() {
		this.props.actions.reportBug(this.state);
	}

	render() {
		const { selectedFiles } = this.state;
		const { actions } = this.props;

		return (
			<div className='report-bug-modal'>
				<div className='k-modal-head'>
					<div>
						<img src={IssueIcon} className='modal-header-icon' />
						<h1 className='modal-header-title'>report an issue</h1>
					</div>
					<img src={CloseIcon} className='closeIcon' onClick={() => actions.modalClose()} />
				</div>
				<div className='k-modal-content k-modal-small'>
					<div className='k-modal-container'>
						<textarea
							rows='5'
							autoFocus
							className='k-modal-input'
							placeholder='write the details about bug here...'
							onChange={(event) => this.setState({ description: event.target.value })}
						/>
					</div>
					<div className='attachments'>
						<PreviewImage selectedFiles={selectedFiles} onFileRemoved={this.onFileRemoved} />
						<Dropzone accept='image/jpeg, image/png' className='file-dropper' onDrop={this.onFilesSelected}>
							<span className='fas fa-paperclip' />
						</Dropzone>
					</div>
				</div>
				<div className='k-modal-footer'>
					<button onClick={this.onSubmit}>SUBMIT</button>
				</div>
			</div>
		);
	}
}

const mapDispatchToProps = dispatch => ({
	actions: bindActionCreators({ modalClose, reportBug }, dispatch)
});

ReportBug.propTypes = {
	actions: PropTypes.object
};

PreviewImage.propTypes = {
	selectedFiles: PropTypes.array,
	onFileRemoved: PropTypes.func
};

export default connect(null, mapDispatchToProps)(ReportBug);
