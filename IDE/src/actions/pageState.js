import { pages } from './actionTypes'

export function pageStateUpdate(pageState) {
	return {
		type: pages.PAGESTATE_UPDATE,
		payload: { pageState: pageState }
	}
}
