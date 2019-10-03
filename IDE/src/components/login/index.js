import React, { Component } from 'react';
import PropTypes from 'prop-types';
import kitsune from '../../images/kitsune.svg';
import { connect } from 'react-redux';
import { logIn } from "../../actions/login";
import { browserHistory } from "react-router";
import GifLoading from '../loader/index';
import { showLoading, hideLoading } from '../../actions/loader';

class Login extends Component {
	constructor(props) {
		super(props);

		this.state = {
			hidden: ''
		};
		this.login = this.login.bind(this);
		this.responseGoogle = this.responseGoogle.bind(this);
	}

	componentDidMount() {
		const { dispatch, location, redirectUrl } = this.props;

		if (/access_token|id_token|error/.test(location.hash)) {
			const { handleAuthentication, getProfile } = this.props.auth;

			this.setState({ ...this.state, hidden: 'hide' });
			dispatch(showLoading('Logging in'));
			handleAuthentication().then(() => {
				getProfile((err, profile) => {
					dispatch(logIn(profile, profile.picture));
					this.setState({ ...this.state, hidden: '' })
					dispatch(hideLoading());
					browserHistory.replace(redirectUrl);
				});
			});
		}
	}

	responseGoogle(response) {
		const { imageUrl } = response.profileObj;
		const { dispatch, redirectUrl } = this.props;
		dispatch(logIn(response, imageUrl));
		browserHistory.replace(redirectUrl);
	}

	login() {
		this.props.auth.login()
	}

	render() {
		return (
			<div className='login-container'>
				<GifLoading />
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
					<button
						className={`login-button ${this.state.hidden}`}
						onClick={this.login}>
						sign-in to continue
					</button>
					<p className='terms'>by continuing you agree to our terms and conditions</p>
				</div>
			</div>
		);
	}
}

Login.propTypes = {
	dispatch: PropTypes.func,
	redirectUrl: PropTypes.string,
	auth: PropTypes.object,
	location: PropTypes.object
};

const mapStateToProps = state => ({
	redirectUrl: state.login.redirectUrl,
	auth: state.login.auth
});

export default connect(mapStateToProps)(Login);
