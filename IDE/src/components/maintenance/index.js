import React, { Component } from 'react';
import PropTypes from 'prop-types';
import { connect } from 'react-redux';
import kitsune from '../../images/kitsune.svg';
import { browserHistory } from 'react-router';

class MaintenancePage extends Component {
	constructor(props) {
		super(props);
	}

	componentWillReceiveProps() {
		const { isMaintenanceBreak, isApiDown } = this.props;
		if(!isMaintenanceBreak && !isApiDown) {
			browserHistory.push(`/`);
		}

	}

	render() {
		
		return (
			<div className='login-container'>
				<div className='login-header'>
					<img className='kitsune-login' src={kitsune} />
					<span className='info'>integrated development environment</span>
					<span className='version'>{'(version 2.6)'}</span>
				</div>
				<div className='login-footer'>
					<div className='ocean'>
						<div className='wave-1' />
						<div className='wave-2' />
						<div className='wave-3' />
						<div className='wave-4' />
					</div>
					{ this.props.detail ? (
						<div>
							<p className='terms text-center'>{this.props.detail.title}</p>
							<p className='terms'>{this.props.detail.description}</p>
						</div>
					) : null
				}
				</div>
			</div>
		);
	}
}

MaintenancePage.propTypes = {
	detail: PropTypes.object,
	isMaintenanceBreak: PropTypes.bool,
	isApiDown: PropTypes.bool
};

const mapStateToProps = state => {
	return {
		success: state.serverMaintenanceReducer.success,
		isMaintenanceBreak: state.serverMaintenanceReducer.isMaintenanceBreak,
		isApiDown: state.serverMaintenanceReducer.isApiDown,
		detail: state.serverMaintenanceReducer.Detail
	}
};


export default connect(mapStateToProps)(MaintenancePage);
