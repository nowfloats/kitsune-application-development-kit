import { mapGetters } from "vuex";

export default {
	name: 'zerothChatbots',

	computed: {
		...mapGetters([
			'isAnaAccountPresent'
		]),
		changeMessage: () => window.innerWidth < 599
	},

	methods: {
		createNewProject(){
			const buttons = document.getElementsByClassName('create-button');
			if (buttons.length !== 0 ) {
				const createButton = buttons[0];
				createButton.click();
			}
		}
	}
}
