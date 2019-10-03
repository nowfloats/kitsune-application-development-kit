import { Component } from 'react';
import { connect } from 'react-redux';
import PropTypes from 'prop-types';
import {
	getUserToken,
	initializeUser,
	setRedirectUrl
} from "../../actions/login";
import { browserHistory } from 'react-router';

class IsLoggedIn extends Component {
	constructor(props) {
		super(props);
		const {
			loggedIn,
			setRedirect,
			initUser,
		} = props;

		if(!loggedIn) {
			setRedirect(browserHistory.getCurrentLocation().pathname);
			browserHistory.replace("/login");
		} else {
			initUser();
		}
	}

	render() {
		const { loggedIn, children } = this.props;
		return loggedIn ? children : null;
	}
}

IsLoggedIn.propTypes = {
	loggedIn: PropTypes.bool,
	children: PropTypes.object,
	setRedirect: PropTypes.func,
	initUser: PropTypes.func
};

const mapDispatchToProps = dispatch => ({
	initUser: () => dispatch(getUserToken()).then(() => dispatch(initializeUser())),
	setRedirect: path => dispatch(setRedirectUrl(path))
});

const mapStateToProps = state => ({
	loggedIn: state.login.loggedIn
});

export default connect(mapStateToProps, mapDispatchToProps)(IsLoggedIn);
