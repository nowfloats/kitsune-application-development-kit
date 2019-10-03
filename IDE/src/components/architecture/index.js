import React, { Component } from 'react';
import PropTypes from 'prop-types';
import { existingAppDeploymentType } from '../../actions/actionTypes'
import { connect } from 'react-redux';
import vis from 'vis';
import * as networkData from './network';
import dottedLine from '../../images/dotted-line.svg';
import solidLine from '../../images/solid-line.svg';
import hideSidePaneIcon from '../../images/hide-side-pane.svg';


class Architecture extends Component {
	constructor(props) {
		super(props);

		this.state = {
			selectedTab: 'aws',
			tabs: {
				aws: 'AWS',
				pingan: 'PingAn Cloud',
				aliCloud: 'Alibaba Cloud'
			},
			showsidepane: true
		};
		this.componentCount = '--';
		this.componentDetails=[];
		this.network = null;
		this.draw = this.draw.bind(this);
	}

	switchTab(index) {
		this.setState({ selectedTab: index }, function () {
			this.network.destroy();
			this.draw();
		}.bind(this));
	}

	draw() {
		const { selectedTab } = this.state;
		const isSchemaMapped = this.props.schemaId != null ? this.props.schemaId.length > 0 : false;

		// create a network
		let data = {};
		if (isSchemaMapped) {
			var newNodesArray = JSON.parse(JSON.stringify(networkData[selectedTab].dynamicWebsite.nodes));
			var newEdgesArray = JSON.parse(JSON.stringify(networkData[selectedTab].dynamicWebsite.edges));
			var temp_componentCount = networkData[selectedTab].dynamicWebsite.componentCount;
			if (this.props.isReportsEnabled) {
				newNodesArray = newNodesArray.concat(networkData[selectedTab].reportsComponent.nodes);
				newEdgesArray = newEdgesArray.concat(networkData[selectedTab].reportsComponent.edges);
				temp_componentCount = temp_componentCount + networkData[selectedTab].reportsComponent.componentCount;
			}
			if(this.props.isCallTrackerEnabled){
				newNodesArray = newNodesArray.concat(networkData[selectedTab].callTrackerComponent.nodes);
				newEdgesArray = newEdgesArray.concat(networkData[selectedTab].callTrackerComponent.edges);
				temp_componentCount = temp_componentCount + networkData[selectedTab].callTrackerComponent.componentCount;
			}
			if(this.props.isPGEnabled){
				newNodesArray = newNodesArray.concat(networkData[selectedTab].pgComponent.nodes);
				newEdgesArray = newEdgesArray.concat(networkData[selectedTab].pgComponent.edges);
				temp_componentCount = temp_componentCount + networkData[selectedTab].pgComponent.componentCount;
			}
			if(this.props.existingAppDetails && 
				this.props.existingAppDetails.deploymentConfig.type === existingAppDeploymentType.CONTAINERISE){
					newNodesArray = newNodesArray.concat(networkData[selectedTab].containerisedAppComponent.nodes);
					newEdgesArray = newEdgesArray.concat(networkData[selectedTab].containerisedAppComponent.edges);
					temp_componentCount = temp_componentCount + networkData[selectedTab].containerisedAppComponent.componentCount;
			}
			data = {
				nodes: newNodesArray,
				edges: newEdgesArray
			};
			this.componentCount = temp_componentCount;
		} else {
			var newNodesArray = JSON.parse(JSON.stringify(networkData[selectedTab].staticWebsite.nodes));
			var newEdgesArray = JSON.parse(JSON.stringify(networkData[selectedTab].staticWebsite.edges));
			var temp_componentCount = networkData[selectedTab].staticWebsite.componentCount;
			if(this.props.existingAppDetails && 
				this.props.existingAppDetails.deploymentConfig.type === existingAppDeploymentType.CONTAINERISE){
					newNodesArray = newNodesArray.concat(networkData[selectedTab].containerisedAppComponent.nodes);
					newEdgesArray = newEdgesArray.concat(networkData[selectedTab].containerisedAppComponent.edges);
					temp_componentCount = temp_componentCount + networkData[selectedTab].containerisedAppComponent.componentCount;
			}
			data = {
				nodes: newNodesArray,
				edges: newEdgesArray
			};
			this.componentCount = temp_componentCount;
		}
		this.componentDetails =newNodesArray;
		this.setState();
		this.network = new vis.Network(this.canvas, data, networkData.NETWORK_OPTIONS);
		// alert(this.componentDetails[0].label);
		// this.network.on('click', (properties)=>{
		// 	var ids = properties.nodes;
			
		// });
	}

	componentDidMount() {
		this.draw()
	}	

	toggleSidePane(){
		const currentState = this.state.showsidepane;
		this.setState({ showsidepane : !currentState });
	}
	
	render() {
		const { selectedTab } = this.state;
		const componentCountClassName = this.componentCount > 0 ? '' : 'no-show';
		
		return (
			<div className='build-architecture'>
				<div className='tabs-container'>
					<h2>your application's cloud-native architecture<br/>
					<span className={`${componentCountClassName}`}>[{this.componentCount} components used on {selectedTab}]</span>
					</h2>
					<ul className='tabs'>
						{
							Object.keys(this.state.tabs).map((tab) => {
								let className = `tabs__item ${selectedTab === tab ? 'tabs__item--active' : ''}`;
								return (
									<li key={tab} className={className} onClick={() => this.switchTab(tab)}>
										{this.state.tabs[tab]}
									</li>
								)
							})
						}
					</ul>
				</div>
				<div className='architecture-content'>
					<div className='canvas' ref={canvas => this.canvas = canvas} />
					<img src={hideSidePaneIcon} className={this.state.showsidepane?'reopenSidePane hide':'reopenSidePane show'} onClick={()=> this.toggleSidePane()} />
					<div className={this.state.showsidepane?'side-pane':'side-pane-collapse'}>
						<h2 className={this.state.showsidepane?'side-pane-header':'side-pane-header hide'}>know your components <img src={hideSidePaneIcon} className='closeSidePane' onClick={()=> this.toggleSidePane()} /></h2>
						{
							Object.keys(this.componentDetails).map((index)=>{
								return(
									<div className={this.state.showsidepane?'component-container':'component-container hide'}>
										<div className='component-image'>
											<img src={this.componentDetails[index].image} />
										</div>
										<div className='component-content'>
											<p className='component-label'>{this.componentDetails[index].label}</p>
											<p className='component-description'>{this.componentDetails[index].description}</p>
										</div>
									</div>
								)
							})
						}
					</div>
				</div>
				<div className='architecture-footer'>
					<div className='solid-line'>
						<img src={solidLine} />
						<br />
						user interaction flow
					</div>
					<div className='dotted-line'>
						<img src={dottedLine} />
						<br />
						application management flow
					</div>
				</div>
				
			</div>
		)
	}
}

Architecture.propTypes = {
	schemaId: PropTypes.string,
	isReportsEnabled: PropTypes.bool,
	isCallTrackerEnabled: PropTypes.bool,
	isCSRFEnabled: PropTypes.bool,
	PGname: PropTypes.string,
	isPGEnabled: PropTypes.bool,
	existingAppDetails: PropTypes.object
};

const mapStateToProps = (state) => {
	return {
		schemaId: state.projectTreeReducer.data.SchemaId,
		isReportsEnabled: state.projectTreeReducer.components[0].isMapped,
		isPGEnabled: state.projectTreeReducer.gateways.instamojo || state.projectTreeReducer.gateways.paytm ||
					state.projectTreeReducer.gateways.payu || state.projectTreeReducer.gateways.stripe || 
					state.projectTreeReducer.gateways.aamarpay || state.projectTreeReducer.gateways.paddle,
		PGname: (state.projectTreeReducer.gateways.instamojo? "| Instamojo. " : "") + 
				(state.projectTreeReducer.gateways.paytm? "PayTM. " : "") + 
				(state.projectTreeReducer.gateways.payu? "PayU. " : "") + 
				(state.projectTreeReducer.gateways.stripe? "Stripe. " : "") +  
				(state.projectTreeReducer.gateways.aamarpay? "Aamarpay. " : "") + 
				(state.projectTreeReducer.gateways.paddle? "Paddle. " : ""),
		isCallTrackerEnabled: state.projectTreeReducer.components[3].isMapped,
		isCSRFEnabled: state.projectTreeReducer.components[4].isMapped,
		existingAppDetails: state.projectTreeReducer.existingAppDetails
	};
};

export default connect(mapStateToProps)(Architecture);

// export default Architecture
