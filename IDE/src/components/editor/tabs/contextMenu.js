import React, { Component } from 'react';
import PropTypes from 'prop-types';

class ContextMenu extends Component {
	render () {
		if (this.props.visibleCheck === true) {
			return (
				<div className='context-menu'>
					<ul>
						<li>Save and Preview</li>
						<li>Save All</li>
						<li>Close</li>
						<li>Close All</li>
						<li>Close All Tabs except this</li>
						<li>Create New File</li>
						<li>Open</li>
					</ul>
				</div>
			)
		} else {
			return (null)
		}
	}
}

ContextMenu.propTypes = {
	visibleCheck: PropTypes.bool.isRequired
}

export default ContextMenu

