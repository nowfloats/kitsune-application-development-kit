import { Component } from 'react';
import PropTypes from 'prop-types';
import { browserHistory } from 'react-router';
import { connect } from 'react-redux';
import { updateServerDetails } from '../actions/serverMaintenance';
import { config } from '../config';
import axios from 'axios';
// import { toastr } from 'react-redux-toastr';
class App extends Component {
	constructor(props) {
		super(props);
		this.state = {
			apiCheckDetails: null
		}
	}

	componentWillMount() {
		const { kitsuneApidowncheck } = config.API;
		axios.get(`${kitsuneApidowncheck}`)
		.then(response => {
			this.props.updateServerDetails(response.data);
			const { success, isDown } = response.data;
			if(success && isDown) {
				browserHistory.push(`/maintenance`);
			}
			this.setState({ apiCheckDetails: response });
		})
		.catch((error) => {
			//toastr.error('Unable to fetch api status');
		})

	}


	render() {
		return this.props.children ;
	}
}

App.propTypes = {
	children: PropTypes.object,
	updateServerDetails: PropTypes.func
};

const mapDispatchToProps = dispatch => {
	return ({
		updateServerDetails: (details) => dispatch(updateServerDetails(details)),
	})
}


export default connect(null, mapDispatchToProps)(App);
