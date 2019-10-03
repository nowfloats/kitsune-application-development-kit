import store from '../store'

export default {
	inserted: function() {
		let getWindowMatch = () => {
			return window.innerWidth <= 899;
		}

		let prevMatch = getWindowMatch()

		if(window.innerWidth <= 899) {
			store.dispatch('toggleOnResize', ['hamburger'])
		}

		window.addEventListener('resize', function () {
			if(!getWindowMatch() && prevMatch) {
				store.dispatch('toggleOnResize', ['hamburger'])
			}
			else if(getWindowMatch() && !prevMatch) {
				store.dispatch('toggleOnResize', ['hamburger'])
			}
			prevMatch = getWindowMatch()
		})
	}
}
