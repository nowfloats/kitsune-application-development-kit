import React, { Component } from 'react';
import PropTypes from 'prop-types';
import { config } from '../../../config'
import ace from 'brace'
import { connect } from 'react-redux'
import { footerCollapse } from "../../../actions/footer";
import MinimizeIcon from '../../../images/minimize.svg'
import ErrorIcon from '../../../images/error.svg'
import WarningIcon from '../../../images/warning.svg'
import MessageIcon from '../../../images/message.svg'
import kErrorIcon from '../../../images/k-error.svg'

function gotoLine(row, column) {
	let editor = ace.edit('kitsune-editor')
	editor.gotoLine(row, column, true)
	editor.focus()
}

class Notifications extends Component {

	constructor(props) {
		super(props);
		this.handleCollapse = this.handleCollapse.bind(this);
	}

	handleCollapse() {
		const { isActive, dispatch } = this.props;
		dispatch(footerCollapse(isActive, isActive));
	}

	render() {
		const { isActive, footer } = this.props;
		const { notification } = footer;
		const { NOTIFICATION } = config.INTERNAL_SETTINGS.footerTabs;
		let kitsuneErrorCount, warningCount, errorCount, messageCount;
		kitsuneErrorCount = warningCount = errorCount = messageCount = 0;
		const notifications = notification.map((item, index) => {
			let logItemImage = null;
			const { type, row, column, text } = item;
			switch(type) {
			case 'warning':
				logItemImage = <img src={WarningIcon} />
				warningCount++;
				break;
			case 'error':
				logItemImage = <img src={ErrorIcon} />
				errorCount++;
				break;
			case 'info':
				logItemImage = <img src={MessageIcon} />
				messageCount++;
				break;
			case 'kError':
				logItemImage = <img src={kErrorIcon} />
				kitsuneErrorCount++;
				break;
			}

			return (
				<li onClick={() => gotoLine(row+1, column+1)} title='go to line' key={index}>
					<div className='line-number'>{row+1}:{column+1}</div>
					{logItemImage}
					<span>{text}</span>
				</li>
			)
		})
		return (
			<div className={isActive=== NOTIFICATION 
			? 'content-container-notification active-notification' 
			: 'content-container-notification'}>
				<header onClick={this.handleCollapse} title='click to collapse'>
					<h4>Notifications</h4>
					<img src={MinimizeIcon} />
				</header>
				<ul className='icon-tray'>
					<li>
						<img src={kErrorIcon} />
						<span>{kitsuneErrorCount}</span> &nbsp; kitsune errors
					</li>
					<li>
						<img src={ErrorIcon} />
						<span>{errorCount}</span> &nbsp; errors
					</li>
					<li>
						<img src={WarningIcon} />
						<span>{warningCount}</span> &nbsp; warnings
					</li>
					<li>
						<img src={MessageIcon} />
						<span>{messageCount}</span> &nbsp; messages
					</li>
				</ul>
				<section className='content notifications'>
					<ul className='notification-list'>
						{notifications}
					</ul>
				</section>
			</div>
		);
	}
}

Notifications.propTypes = {
	isActive: PropTypes.string,
	footer: PropTypes.object,
	dispatch: PropTypes.func,
}

const mapStateToProps = (state) => {
	return {
		footer: state.footerReducer
	}
}

export default connect(mapStateToProps)(Notifications)
