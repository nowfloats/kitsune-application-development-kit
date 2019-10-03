import ace from 'brace';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import TabClose from '../../../images/close.svg';
import FileChangeIcon from '../../../images/edited-file.svg';
import ContextMenu from './contextMenu';
import EditorTabIcon from './editorIcon';

class Tab extends Component {
	constructor(props) {
		super(props)
		this.state = {
			visible: true,
			contextVisible: false
		}
		this.handleOptionsButtonClick = this.handleOptionsButtonClick.bind(this);
	}

	handleOptionsButtonClick() {
		this.setState({ visible: true })
	}

	render() {
		const { id, editor, removeTab, isActive, onClick } = this.props;
		const { fileChanged, type } = editor;
		let tabCloseIcon = fileChanged ? FileChangeIcon : TabClose;

		return (
			this.state.visible === true ? (
				<div id={id} className='tab'>
					<div className={`tab-box ${isActive ? '' : 'inactive'}`}>
						<EditorTabIcon onClick={onClick} fileType={type} />
						<span title={this.props.filename} className='file-name' onClick={onClick}>{this.props.filename}</span>
						<div className={`icon-close${fileChanged !== true ? ' edited' : ''}`}
							aria-hidden onClick={removeTab}>
							<img src={tabCloseIcon} />
						</div>
					</div>
					<ContextMenu visibleCheck={this.state.contextVisible} />
				</div>
			) : null
		)
	}
}

Tab.propTypes = {
	dispatch: PropTypes.func,
	filename: PropTypes.string,
	removeTab: PropTypes.func,
	editor: PropTypes.shape({
		fileChanged: PropTypes.bool,
		helper: PropTypes.object,
		type: PropTypes.string
	}),
	webForm: PropTypes.shape({
		isOpen: PropTypes.bool,
		helper: PropTypes.object,
		isEditable: PropTypes.bool
	}),
	schema: PropTypes.shape({
		isOpen: PropTypes.bool,
		helper: PropTypes.object
	}),
	readOnly: PropTypes.bool,
	isActive: PropTypes.bool,
	onClick: PropTypes.func
}

Tab.defaultProps = {
	onClick: () => { }
}

const mapStateToProps = (state) => {
	return {
		webForm: state.webFormReducer,
		schema: state.schemaCreatorReducer
	}
}

export default connect(mapStateToProps)(Tab)

