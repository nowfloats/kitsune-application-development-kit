import React, { Component } from 'react';
import PropTypes from 'prop-types';
import Modal from 'react-modal';
import GifLoading from '../loader/index';
import ReduxToastr from 'react-redux-toastr';
import Context from '../context-menu/index';
import { connect } from 'react-redux';
import { modalClose } from "../../actions/modal";
import { isWebFormOpen } from "../../actions/webForm";

class ThirdParty extends Component {
	constructor (props) {
		super(props);
		this.closeModal = this.closeModal.bind(this);
	}

	closeModal() {
		const { dispatch, canClone } = this.props;
		dispatch(modalClose());

		if(canClone) {
			dispatch(isWebFormOpen(false))
		}
	}

	render() {
		const { modal } = this.props;
		return (
			<div>
				<GifLoading />
				<Modal
					portalClassName='k-modalPortal'
					isOpen={modal.show}
					onRequestClose={this.closeModal}
					contentLabel={modal.label} >
					{modal.html}
				</Modal>
				<ReduxToastr
					timeOut={4000}
					newestOnTop={false}
					preventDuplicates
					position='bottom-right'
					transitionIn='fadeIn'
					transitionOut='fadeOut' />
				<Context animation='zoomIn' theme='light' />
			</div>
		)
	}
}

ThirdParty.propTypes = {
	modal: PropTypes.object,
	dispatch: PropTypes.func,
	canClone: PropTypes.bool
};

const mapStateToProps = (state) => {
	return {
		modal: state.modalReducer,
		canClone: state.webFormReducer.canClone
	};
};

export default connect(mapStateToProps)(ThirdParty);
