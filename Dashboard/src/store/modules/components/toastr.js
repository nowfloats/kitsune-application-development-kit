import Enum from 'enum';
import iziToast from 'izitoast';
import * as types from '../../mutation-types';


const state = {
	toasterTypes : new Enum({
		ERROR : 'error',
		INFO : 'info',
		SUCCESS : 'success',
		WARNING : 'warning',
	}),
	defaultToaster: {
		title: 'Hey',
		message: 'What would you like to add?',
		theme : 'dark',
		progressBar: false,
		layout: 2,
		timeout: 5000,
		close: true,
		pauseOnHover: true,
		position : 'bottomRight',
		image: '../../assets/imagesheader/resized.png',
		imageWidth : 70,
		transitionIn: 'fadeInLeft',
		transitionOut: 'fadeOutRight',
	}
};

// getters
const getters = {
	toasterType: state => state.toasterTypes,
	toasterEventName : state => state.toasterEventName
};

// actions
const actions = {

	addToaster({ dispatch,commit },payload){
		commit(types.setTitleAndMessageForToastr,payload);
		dispatch('addToasterToLayout',payload);
	},

	addToasterToLayout({ commit },payload){

		switch (payload.type.value){
		case state.toasterTypes.INFO.value:
			iziToast.info(state.defaultToaster);
			break;
		case state.toasterTypes.SUCCESS.value:
			iziToast.success(state.defaultToaster);
			break;
		case state.toasterTypes.ERROR.value:
			iziToast.error(state.defaultToaster);
			break;
		case state.toasterTypes.WARNING.value:
			iziToast.warning(state.defaultToaster);
			break;
		default:
			// todo handle error
			console.log('oob');
			break;
		}

	}

};

// mutations
const mutations = {

	[types.setTitleAndMessageForToastr] (state,payload){
		state.defaultToaster.title = payload.title;
		state.defaultToaster.message = payload.message;
		state.defaultToaster.class = `iziToast-${payload.type}`;
	}

};

export default {
	state,
	getters,
	actions,
	mutations
}
