import React, { Component } from 'react'
import PropTypes from 'prop-types';
import Tab from '../editor/tabs/index'
import FormBuilder from './form-builder/index'
import { isWebFormOpen } from '../../actions/webForm'
import { connect } from 'react-redux'
import { checkFileChanged, fileClose } from "../../actions/editor";
import TabActions from '../editor/tabs/tabActions';

class WebForm extends Component {
	constructor(props) {
		super(props)
		this.closeWebForm = this.closeWebForm.bind(this)
	}

	closeWebForm() {
		const { dispatch } = this.props;
		dispatch(checkFileChanged(() => {
			dispatch(fileClose(0));
			dispatch(isWebFormOpen(false))
		}));
	}

	render() {
		const { webFormReducer, style, editor } = this.props;
		const { currentWebForm } = webFormReducer;
		return (
			<div style={style} className={webFormReducer.isOpen ? "web-form-container" : "hide"}>
				<div className="tabs">
					<Tab
						isActive={true}
						editor={editor}
						filename={currentWebForm.DisplayName ? currentWebForm.DisplayName : 'untitled'}
						removeTab={this.closeWebForm}
					/>
				</div>
				<TabActions editor={editor} webForm={webFormReducer} readOnly={false} />
				<FormBuilder />
			</div>
		);
	}
}

WebForm.propTypes = {
	dispatch: PropTypes.func,
	webFormReducer: PropTypes.shape({
		isOpen: PropTypes.bool
	}),
	style: PropTypes.object
}

const mapStateToProps = (state) => {
	const { activeTabs, visibleIndex } = state.editorReducer;
	return {
		webFormReducer: state.webFormReducer,
		editor: activeTabs[visibleIndex] || {},
		footer: state.footerReducer
	}
}

export default connect(mapStateToProps)(WebForm)
