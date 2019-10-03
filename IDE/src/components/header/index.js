import React, { Component } from 'react';
import PropTypes from 'prop-types';
import { connect } from 'react-redux';
import HeaderNav from './header-nav/index';
import HeaderActions from './header-actions/index';
import { modalOpen } from '../../actions/modal';
import CreditScreen, { creditScreenLabel } from '../modals/credits/index';
import kitsuneLogo from '../../images/logo.svg';

class Header extends Component {

	constructor(props) {
		super(props);
	}

	render() {
		const { showCredits } = this.props;
		return (
			<div>
				<div className='header'>
					<a href='#' alt='kitsune logo' className='kitsune-logo' onClick={showCredits}>
						<img src={kitsuneLogo} alt='kitsune' />
					</a>
					<HeaderNav />
					<HeaderActions />
				</div>
			</div>
		);
	}
}

Header.propTypes = {
	showCredits: PropTypes.func
};

const mapDispatchToProps = (dispatch) => {
	return({
		showCredits: () => dispatch(modalOpen(<CreditScreen />, creditScreenLabel, null))
	});
};

export default connect(null, mapDispatchToProps)(Header);
