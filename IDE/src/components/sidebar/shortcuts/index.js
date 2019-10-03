import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { config } from "../../../config";

class Shortcuts extends Component {

	constructor(props) {
		super(props);
	}

	render() {
		const { isActive, variant } = this.props;
		let { SHORTCUTS } = config.INTERNAL_SETTINGS.sidebarItems[variant];
		return (
			<div className={isActive === SHORTCUTS ? '' : 'hide'}>
				{/*<p>Shortcuts</p>*/}
			</div>
		);
	}
}

Shortcuts.propTypes = {
	isActive: PropTypes.string
}

export default Shortcuts;
