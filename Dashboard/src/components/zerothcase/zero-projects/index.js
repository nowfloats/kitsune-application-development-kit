export default {
	name: 'zerothProjects',

	computed: {
		changeMessage() {
			return window.innerWidth < 599
		}
	},

	methods: {
		createNewProject(){
			let buttons = document.getElementsByClassName('create-button');
			if (buttons.length !=0 ){
				let createButton = buttons[0];
				createButton.click();
			}
		}
	}
}
