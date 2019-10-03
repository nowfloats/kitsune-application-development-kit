import React, { Component } from 'react';
import _ from 'lodash';
import PropTypes from 'prop-types';
import BaseGroup from './group';
import { config } from "../../../../config";
import { SortPropertyByGroupName } from "../../../../helpers/schema-creator";

class SchemaBase extends Component {
	constructor (props) {
		super(props);
		this.renderGroups = this.renderGroups.bind(this);
		this.addNodeHandler = this.addNodeHandler.bind(this);
		this.mapStateOfNode = this.mapStateOfNode.bind(this);
	}

	mapStateOfNode(name) {
		const { base } = config.INTERNAL_SETTINGS.schemaConfig;
		return base[name] ? base[name] : 'visible';
	}

	renderGroups(classes) {
		const { helpers, readOnly } = this.props;
		return classes.map((Class, i) => {
			const { ClassType, Name, PropertyList } = Class;
			if(ClassType === 1) {
				const groupedPropertyList = _.chain(PropertyList)
					.reject(({ Name }) => {
						const stateOfNode = this.mapStateOfNode(Name);
						return stateOfNode === 'hidden';
					})
					.groupBy('GroupName')
					.reduce((result, value, key) => {
						const target = key && key !== 'null' ? key : 'unnamed';
						result[target] ? result[target].push(...value) :
							result[target] = value;
						return result;
					}, {})
					.toPairs()
					.value();
				const unnamedIndex = _.findIndex(groupedPropertyList, ([ name ]) => name === 'unnamed');
				let sortedGroups;
				if(unnamedIndex !==  -1) {
					const unnamedGroup = groupedPropertyList[unnamedIndex];
					const groupsWithoutUnnamed = unnamedIndex === 0 ? _.slice(groupedPropertyList, 1) :
						(unnamedIndex === groupedPropertyList.length -1 ?
								_.slice(groupedPropertyList, 0, groupedPropertyList.length - 1) :
						[
							..._.slice(groupedPropertyList, 0, unnamedIndex),
							..._.slice(groupedPropertyList, unnamedIndex+1)
						]
						);
					sortedGroups = [
						...groupsWithoutUnnamed.sort(SortPropertyByGroupName),
						unnamedGroup
					];
				} else {
					sortedGroups = groupedPropertyList.sort(SortPropertyByGroupName)
				}
				return (
					<li key={i}>
						<p>{Name}</p>
						<ul>
							{
								_.map(sortedGroups, ([ groupName, groupProps ], index) =>
									<BaseGroup parent={Name}
										key={index}
										groupProps={groupProps}
										groupName={groupName}
										helpers={helpers}
										readOnly={readOnly} />)
							}
							{
								!readOnly ?
									<li className='add-node'>
										<p ref={addNode => this.addNode = addNode} onClick={e => this.addNodeHandler(e, Name)}>+</p>
									</li> :
									null
							}
						</ul>
					</li>
				);
			} else {
				return null;
			}
		});
	}

	addNodeHandler(e, parent) {
		const { openMetaDefinition } = this.props.helpers;
		openMetaDefinition({
			parent,
			ref: this.addNode,
			event: e,
			props: null,
			newDefinition: null
		});
	}

	render() {
		const { baseClasses } = this.props;
		return (
			<div className='base-definition'>
				<ul>
					{this.renderGroups(baseClasses)}
				</ul>
			</div>
		);
	}
}

SchemaBase.propTypes = {
	baseClasses: PropTypes.array,
	helpers: PropTypes.object,
	readOnly: PropTypes.bool
};

export default SchemaBase;
