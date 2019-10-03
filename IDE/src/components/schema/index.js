import React, { Component } from 'react';
import PropTypes from 'prop-types';
import Tab from '../editor/tabs/index';
import { resetSchema, saveSchema, setHelper } from '../../actions/schema';
import SchemaCreator from './schema-creator/index';
import { connect } from 'react-redux';
import { checkFileChanged, fileClose } from "../../actions/editor";
import { TabActions } from '../editor/tabs/tabActions';

class Schema extends Component {
	constructor(props) {
		super(props);
		this.closeSchema = this.closeSchema.bind(this);
		this.saveSchemaTrigger = this.saveSchemaTrigger.bind(this);
		this.handleKeyBindings = this.handleKeyBindings.bind(this);
		this.helper = {
			saveSchema: this.saveSchemaTrigger
		};
		this.props.dispatch(setHelper(this.helper))
	}

	componentWillReceiveProps({ schemaCreator }) {
		const { isOpen: nextIsOpen } = schemaCreator;
		const { isOpen } = this.props.schemaCreator;
		if (!isOpen && nextIsOpen) {
			document.addEventListener('keydown', this.handleKeyBindings, false);
		} else if (isOpen && !nextIsOpen) {
			document.removeEventListener('keydown', this.handleKeyBindings, false);
		}
	}

	handleKeyBindings(e) {
		if ((e.ctrlKey || e.metaKey) && e.keyCode === 83) {
			e.preventDefault();
			this.saveSchemaTrigger();
		}
	}

	closeSchema() {
		const { dispatch } = this.props;
		dispatch(checkFileChanged(() => {
			dispatch(fileClose(0));
			dispatch(resetSchema());
		}));
	}

	saveSchemaTrigger() {
		const { dispatch, schemaCreator } = this.props;
		const { schemaDetails, readOnly } = schemaCreator;
		if (!readOnly) {
			dispatch(saveSchema(schemaDetails));
		}
	}

	render() {
		const { schemaCreator, style, footerResizerStyle, editor } = this.props;
		const { schemaDetails, isOpen, readOnly } = schemaCreator;
		const { EntityName } = schemaDetails;

		if (isOpen && editor) {
			return (
				<div style={style} className='schema-container'>
					<div className="tabs">
						<Tab isActive={true} filename={EntityName} editor={editor} removeTab={this.closeSchema} readOnly={readOnly} />
					</div>
					<TabActions editor={editor} schema={schemaCreator} readOnly={readOnly} />
					<SchemaCreator parentWidth={style} parentHeight={footerResizerStyle} readOnly={readOnly} />
				</div>
			);
		}
		return null;
	}
}

Schema.propTypes = {
	schemaCreator: PropTypes.shape({
		isOpen: PropTypes.bool,
		schemaDetails: PropTypes.object
	}),
	dispatch: PropTypes.func,
	style: PropTypes.object,
	footerResizerStyle: PropTypes.object
};

const mapStateToProps = (state) => {
	const { activeTabs, visibleIndex } = state.editorReducer;
	return {
		schemaCreator: state.schemaCreatorReducer,
		editor: activeTabs[visibleIndex] || {}
	}
};

export default connect(mapStateToProps)(Schema);
