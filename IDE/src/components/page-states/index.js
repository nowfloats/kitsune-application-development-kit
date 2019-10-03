import React, { Component } from 'react'
import PropTypes from 'prop-types';
import { connect } from 'react-redux'
import { config } from '../../config'
import firstLoginArrow from '../../images/login-arrow.svg'

class PageStates extends Component {
	constructor(props) {
		super(props)
	}

	render() {
		const { style, pageState: pageState, schemaCreator: schemaCreator, webFormReducer: webForm,
			className, editor: editor } = this.props;
		const { pageState: currentPageState } = pageState;
		const { pageStates } = config.INTERNAL_SETTINGS;
		const { FILE_NONE } = pageStates;
		const { activeTabs } = editor;
		const { userName } = this.props.login;


		return (
			<div style={(currentPageState === FILE_NONE.name && activeTabs.length <= 0) ? style : {}}
				className={currentPageState === FILE_NONE.name && (activeTabs.length > 0 || schemaCreator.isOpen ||
					webForm.isOpen) ? 'hide' : `${currentPageState} ${className}`}>
				<div className={currentPageState === FILE_NONE.name ? 'k-indicator hide' : 'k-indicator'} >
					<img src={firstLoginArrow} alt='placeholder' />
				</div>
				<div className='k-message'>
					<img src={pageStates[currentPageState].icon} alt={pageStates[currentPageState].iconAlt} />
					<h1>{pageStates[currentPageState].heading}</h1>
					<p>
						{pageStates[currentPageState].para}
						<span className={currentPageState === FILE_NONE.name ? '' : 'hide'}>{userName}.</	span>
					</p>
				</div>
			</div>
		);
	}
}

PageStates.propTypes = {
	pageState: PropTypes.shape({
		pageState: PropTypes.string
	}),
	editor: PropTypes.shape({
		isOpened: PropTypes.bool
	}),
	style: PropTypes.object,
	className: PropTypes.string,
	schemaCreator: PropTypes.shape({
		isOpen: PropTypes.bool
	}),
	webFormReducer: PropTypes.shape({
		isOpen: PropTypes.bool
	}),
	login: PropTypes.object
}

const mapStateToProps = (state) => {
	return {
		pageState: state.pageStateReducer,
		editor: state.editorReducer,
		schemaCreator: state.schemaCreatorReducer,
		webFormReducer: state.webFormReducer,
		login: state.login
	}
}

export default connect(mapStateToProps)(PageStates)
