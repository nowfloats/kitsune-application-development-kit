import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { checkFileChanged, editorClear } from '../../actions/editor';
import { config } from "../../config";

class SideBar extends Component {
	constructor(props) {
		super(props);
		const { defaultKey } = config.INTERNAL_SETTINGS.sidebarItems[props.variant];
		this.state = {
			isActive: defaultKey
		};
		this.handleCollapse = this.handleCollapse.bind(this);
	}

	handleCollapse(element) {
		const { isActive } = this.state;
		const { hasAnyFileChanged, clearEditors } = this.props;

		if (element !== isActive) {
			hasAnyFileChanged(() => {
				clearEditors();
				this.setState({ isActive: element })
			})
		}
	}

	renderSiderbarContents(keyMap, isActive) {
		const icons = [];
		const views = [];
		const { variant } = this.props;

		let index = 0;
		for (let [key, config] of Object.entries(keyMap)) {
			// Do not process the default key
			if (key === 'defaultKey') continue;
			// Or any value which is not an object
			if (typeof config !== 'object') continue;

			// If supplied a display text, it needs an icon
			if (!!config.display) {
				icons.push(
					<li key={`${variant}-sidebar-icon-${index}`} className={isActive === key ? 'active' : ''}
						onClick={() =>
							this.handleCollapse(key)} >
						<button className={`button ${config.buttonClass || ''}`} />
						<p>{config.display}</p>
					</li>
				);
			}

			// If supplied a component, it needs a view
			if (!!config.component) {
				const ItemComponent = config.component;
				views.push(
					<ItemComponent key={`${this.props.variant}-sidebar-comp-${index}`} isActive={isActive} variant={variant} />
				)
			}

			index++;
		}

		return [icons, views];
	}

	render() {
		const { style, className, variant } = this.props;
		const variantKeyMap = config.INTERNAL_SETTINGS.sidebarItems[variant];
		const { isActive } = this.state;

		const sidebarStyle = {
			...style,
			minWidth: variant === 'right' && !isActive ? 0 : style.minWidth || 0
		}

		const [icons, views] = this.renderSiderbarContents(variantKeyMap, isActive);
		const navBar = (
			<ul className={`sidebar-nav ${variant} ${!!isActive ? 'has-active' : ''}`}>
				{icons}
			</ul>
		);
		const activeView = isActive === '' ? null : (
			<div className={`sidebar-tree ${variant}`}>
				{views}
			</div>
		);

		return (
			<aside style={sidebarStyle} className={`${className} ${variant}`} >
				{
					variant === 'left' ? navBar : activeView
				}
				{
					variant === 'left' ? activeView : navBar
				}
			</aside>
		)
	}
}

SideBar.propTypes = {
	className: PropTypes.string,
	style: PropTypes.object,
	variant: PropTypes.oneOf(['left', 'right'])
};

SideBar.defaultProps = {
	variant: 'left'
};

const mapDispatchToProps = dispatch => ({
	hasAnyFileChanged: callback => dispatch(checkFileChanged(callback)),
	clearEditors: () => dispatch(editorClear())
})

export default connect(null, mapDispatchToProps)(SideBar);
