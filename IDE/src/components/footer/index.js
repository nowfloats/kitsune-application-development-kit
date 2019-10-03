import React, { Component } from 'react';
import PropTypes from 'prop-types';
import { connect } from 'react-redux';
import EventLogs from './event-logs/index';
import Notifications from './notifications/index';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome'
import { faCheck, faBug } from '@fortawesome/free-solid-svg-icons'
import { footerCollapse } from '../../actions/footer';
import { config } from '../../config';

class Footer extends Component {
	constructor(props) {
		super(props);
		this.handleCollapse = this.handleCollapse.bind(this);
	}

	handleCollapse(element) {
		const { dispatch, footer } = this.props;
		const { isActive } = footer;
		dispatch(footerCollapse(element, isActive));
	}

	render() {
		const { EVENT_LOG, NOTIFICATION } = config.INTERNAL_SETTINGS.footerTabs;
		const { footer, style } = this.props;
		const { isActive, notification } = footer;
		let totalCount = notification.length >= 100 ? `99+` : notification.length;
		let errors = notification.filter((item) => {
			return (item.type === 'error' || item.type === 'kError');
		});

		return (
			<div>
				<footer style={style} className={isActive !== null ? 'active' : ''}>
					<section className={`footer-tabs-content ${isActive !== null ? 'active' : ''}`}>
						<EventLogs isActive={isActive} />
						<Notifications isActive={isActive} />
					</section>
					<div className='footer'>
						<div className='footer-tabs'>
							<div className={`footer-tab ${isActive === EVENT_LOG ? 'active' : ''}`}
								onClick={() => this.handleCollapse(EVENT_LOG)}>Event Logs</div>
							<div className={`footer-tab ${isActive === NOTIFICATION ? 'active' : ''}`}
								onClick={() => this.handleCollapse(NOTIFICATION)}>Notifications ({totalCount})</div>
						</div>

						<div className='footer-items'>
							<span className={`footer-item`}>
								{errors.length ? `${errors.length} errors` : 'no issues'}
								<span className={` ${errors.length ? 'has-error' : 'no-error'}`}>
									<FontAwesomeIcon icon={ errors.length ? faBug : faCheck} />
								</span>
							</span>
							<span className='footer-item'>UTF-8</span>
						</div>
					</div>
				</footer>
			</div>
		);
	}
}


Footer.propTypes = {
	dispatch: PropTypes.func,
	footer: PropTypes.object,
	style: PropTypes.object
};


const mapStateToProps = state => {
	return {
		footer: state.footerReducer
	}
};

export default connect(mapStateToProps)(Footer);
