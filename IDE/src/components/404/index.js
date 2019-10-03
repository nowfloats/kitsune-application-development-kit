import React, { Component } from 'react';
import kitsuneFox from '../../images/404.svg';

class FourOhFour extends Component {
	constructor(props) {
		super(props)
	}

	render() {
		return (
			<section className='page-not-found'>
				<div className='not-found-container'>
					<img src={kitsuneFox} alt='image not found' />
					<h2>unknown forces have stopped our noble work</h2>
					<p>our speedy fox is off looking for what's gone wrong. it'll be fixed in no time.</p>
					<button onClick={() => window.location.reload()}>retry</button>
				</div>
				<div className='ocean'>
					<div className='wave-1' />
					<div className='wave-2' />
					<div className='wave-3' />
					<div className='wave-4' />
				</div>
			</section>
		)
	}
}

export default FourOhFour
