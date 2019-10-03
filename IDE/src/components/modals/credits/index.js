import React, { Component } from 'react'
import foxImage from '../../../images/fox.png'

export const creditScreenLabel = 'Credit Screen'

class CreditScreen extends Component {
	constructor(props){
		super(props)
	}

	render() {
		return (
			<div className='credits-container k-modal-medium'>
				<div className='credits-header'>
					<h1>kitsun<span>e</span></h1>
					<p className='info'>integrated development environment</p>
					<p className='release'>v2.9</p><br />
					<p className='info'><a href='https://www.getkitsune.com' target='_blank'>https://www.getkitsue.com</a></p>
				</div>
				<div className='credits-footer'>
					<div className='copyright'>
						<p>&copy; 2019 team kitsune all right reserved</p>
					</div>
					<img src={foxImage} alt='kitsune fox' />
				</div>
			</div>
		);
	}
}

export default CreditScreen;
