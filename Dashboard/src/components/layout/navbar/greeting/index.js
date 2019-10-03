import { mapGetters } from "vuex"

export default {
	name: 'greeting',

	props : ['name'],

	data() {
		return {
			greet : this.getGreetMessage()
		}
	},

	computed: {
		...mapGetters({
			componentStatus: 'componentStatus'
		})
	},

	methods: {
		getGreetMessage: function () {
			let hour = new Date().getHours();
			return hour < 12 && hour > 6 ? 'morning' :
				hour >=12 && hour < 17 ? 'afternoon' : 'evening'
		}
	},

	mounted() {
		let greet = document.querySelectorAll('.greeting.greet-animation');
		setTimeout(() => [...greet].map(ele => ele.classList.remove('greet-animation')), 5300)
	}
}
