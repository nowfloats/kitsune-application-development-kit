import helpers from '../mixin';
import foxImg from '../../assets/icons/fox.png';

export default {
	name: 'credits',

	mixins: [helpers],

	computed: {
		getCurrentYear() {
			return new Date().getFullYear();
		},

		getImage() {
			return foxImg;
		}
	}
};
