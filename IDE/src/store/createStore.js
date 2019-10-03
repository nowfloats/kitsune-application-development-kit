import { applyMiddleware, compose, createStore } from 'redux'
import thunk from 'redux-thunk'
import { browserHistory } from 'react-router'
import makeRootReducer from '../reducers/index'
import { updateLocation } from '../reducers/location'

export default (initialState = {}) => {
  // ======================================================
  // Middleware Configuration
  // ======================================================
	let middleware = [thunk]

  // ======================================================
  // Store Enhancers
  // ======================================================
	const enhancers = []

	let composeEnhancers = compose

	if (__DEV__) {
		const composeWithDevToolsExtension = window.__REDUX_DEVTOOLS_EXTENSION_COMPOSE__
		if (typeof composeWithDevToolsExtension === 'function') {
			composeEnhancers = composeWithDevToolsExtension
		}
	}

  // ======================================================
  // Store Instantiation and HMR Setup
  // ======================================================
	const store = createStore(
    makeRootReducer(),
    initialState,
    composeEnhancers(
      applyMiddleware(...middleware),
      ...enhancers
    )
  )
	store.asyncReducers = {}

  // To unsubscribe, invoke `store.unsubscribeHistory()` anytime
	store.unsubscribeHistory = browserHistory.listen(updateLocation(store))

	if (module.hot) {
		module.hot.accept('../reducers/index', () => {
			const reducers = require('../reducers/index').default
			store.replaceReducer(reducers(store.asyncReducers))
		})
	}

	return store
}
