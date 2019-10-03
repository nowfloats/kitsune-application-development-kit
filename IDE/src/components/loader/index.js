import React, { Component } from 'react';
import PropTypes from 'prop-types';
import { connect } from 'react-redux';

class GifLoading extends Component {
	constructor (props) {
		super(props);
	}

	render () {
		const { show, text } = this.props;
		return (
			<div className={show ? 'custom-loader' : 'custom-loader hide'} >
				<div className='loader-bars-container'>
					<div className='loader-bars'>
						<i />
						<i />
						<i />
						<i />
						<i />
						<i />
						<i />
						<i />
						<i />
						<i />
					</div>
				</div>
				<p>{text}</p>
			</div>
		);
	}
}

GifLoading.propTypes = {
	show: PropTypes.bool,
	text: PropTypes.string
};

const mapStateToProps = (state) => {
	return {
		show: state.loaderReducer.show,
		text: state.loaderReducer.loadingText
	};
};

export default connect(mapStateToProps)(GifLoading);
