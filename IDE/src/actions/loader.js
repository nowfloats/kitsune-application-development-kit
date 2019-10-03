import { loader } from './actionTypes'

//action for initializing showing the loader
export function showLoading(loadingText) {
	return {
		type: loader.LOADER_SHOW,
		payload: { show: true , loadingText }
	}
}

//action for initializing hiding the loader
export function hideLoading() {
	return {
		type: loader.LOADER_HIDE,
		payload: { show: false }
	}
}
