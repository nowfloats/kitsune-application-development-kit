import React, { Component } from 'react';
import PropTypes from 'prop-types';
import { connect } from 'react-redux';
import { schemaFetchDetails } from '../../../actions/schema';
import { isWebFormOpen } from "../../../actions/webForm";
import { checkFileChanged, editorClear } from '../../../actions/editor';
import { config } from "../../../config";
import SchemaIcon from "../../../images/language-white.svg";
import { ContextMenuProvider } from 'kt-contexify';
import { modalOpen } from '../../../actions/modal';
import CreateSchema, { createSchemaLable } from '../../modals/create-schema/index';

class SchemaKitsune extends Component {

	constructor(props) {
		super(props);
		this.state = {
			activeSchemaId: null
		};
		this.openSchema = this.openSchema.bind(this);

	}

	openSchema(schemaID, readOnly) {
		const { dispatch, isOpen } =  this.props;
		dispatch(checkFileChanged(() => {
			dispatch(editorClear());
			isOpen ? dispatch(isWebFormOpen(false)) : '';
			dispatch(schemaFetchDetails(schemaID, readOnly));
			this.setState({ activeSchemaId: schemaID });
		}));
	}

	newSchema() {
		const { dispatch, isOpen } = this.props;
		dispatch(checkFileChanged(() => {
			isOpen ? dispatch(isWebFormOpen(false)) : ''
			dispatch(modalOpen(<CreateSchema />, createSchemaLable, null));
		}));
	}

	render() {
		const { isActive, schemaCreator, mappedSchemaId, variant } = this.props;
		const { KITSUNE_SCHEMA } = config.INTERNAL_SETTINGS.sidebarItems[variant];
		const { schemas, systemSchemas } = schemaCreator;
		const { language } = config.INTERNAL_SETTINGS.contextMenus;
		let mappedSchemaObj = schemas.filter((iterator) => {
			return iterator.SchemaId === mappedSchemaId
		});

		const schemasList = mappedSchemaObj.map((iterator, index) => {
			const { EntityName, SchemaId } = iterator;
			return (
				<ContextMenuProvider id={language} node={Object.assign({}, iterator)} key={index}>
					<li onClick={()=>{this.openSchema(SchemaId, false)}} 
						className={this.state.activeSchemaId === SchemaId ? "slct-lng-bkg": ""} >
						<img src={SchemaIcon} />
						<span>{EntityName}</span>
					</li>
				</ContextMenuProvider>
			);
		});

		const systemSchemasList = systemSchemas.map((iterator, index) => {
			const { EntityName, SchemaId } = iterator;
			return (
				<ContextMenuProvider id={language} node={Object.assign({}, iterator)} key={index}>
					<li onClick={()=>{this.openSchema(SchemaId, true)}}>
						<img src={SchemaIcon} />
						<span>{EntityName}</span>
					</li>
				</ContextMenuProvider>
			);
		});

		return (
			<div className={isActive === KITSUNE_SCHEMA.key ? '' : 'hide'}>
				<div className={!schemaCreator.isFetching ? 'hide schema-list-loader' : 'schema-list-loader'} />
				<div className={schemaCreator.isFetching ? 'hide' : 'schema-sidebar'}>
					{schemasList.length ?
						<div>
							<ul className='schema-list'>
								<h2 className='title'>user-defined data model{schemasList.length > 1 ? 's' : ''}</h2>
								{schemasList}
							</ul>
							{systemSchemasList.length ?
								<ul className='system-schema-list'>
									<h2 className='title'>system-defined data model{systemSchemasList.length > 1 ? 's' : ''}</h2>
									{systemSchemasList}
								</ul> :
								null
							}
						</div> :
						<div className='schema-list-intro'>
							<span className='schema-logo'>
								[  ]
							</span>
							<h2 className='intro-title'>kitsune data model</h2>
							<p className='intro-text'>
								create a modular data language and integrate it on HTML.
								let's start by creating a base class and adding datatypes.</p>
							<button  className='new-schema btn' onClick={()=>{this.newSchema()}} >create a data model</button>
						</div>
					}
				</div>
			</div>
		);
	}
}

SchemaKitsune.propTypes = {
	isActive: PropTypes.string,
	dispatch: PropTypes.func,
	schemaCreator: PropTypes.object,
	isOpen: PropTypes.bool,
	mappedSchemaId: PropTypes.string
}

const mapStateToProps = (state) => {
	const { activeTabs, visibleIndex } = state.editorReducer;
	const editor = activeTabs[visibleIndex];
	return {
		schemaCreator: state.schemaCreatorReducer,
		fileChanged: editor ? editor.fileChanged : false,
		isOpen: state.webFormReducer.isOpen,
		mappedSchemaId: state.projectTreeReducer.data.SchemaId
	}
}

export default connect(mapStateToProps)(SchemaKitsune)
