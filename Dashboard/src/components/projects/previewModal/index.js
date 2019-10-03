import { mapGetters, mapActions } from 'vuex'
import dotsLoader from '../../loaders/threeDots/ThreeDots.vue'
import helpers from '../../mixin'
import Multiselect from 'vue-multiselect'

export default {
	name: 'PreviewProjectModal',

	components: {
		dotsLoader,
		Multiselect
	},

	mixins: [helpers],

	data() {
		return {
			customer: '',
			previewLink: '',
			disableButton: true
		}
	},

	computed : {
		...mapGetters({
			previewProject : 'getPreviewProjectDetails'
		}),

		getCustomerList(){
			return this.previewProject.customerList
		}

	},

	methods:{
		...mapActions([
			'toggleStatus'
		]),

		closePreviewProjectModal(e){
			this.toggleStatusHandler(e, ['overlay','previewProject']);
		},

		generatePreviewLink() {
			if(this.customer != null || this.customer != undefined) {
				this.disableButton = false;
				this.previewLink = `http://${this.customer.CustomerId}.demo.getkitsune.com`
			}
		},

		previewProjectInNewTab(e) {
			window.open(this.previewLink);
			this.closePreviewProjectModal(e)
		},

		keyUpHandler(event){
			if(event.keyCode === 27 ){
				this.closePreviewProjectModal(event);
			}
		},

		addEventListenerTobodyToCloseOnEscKey (){
			let body = document.getElementsByTagName('body')[0];
			body.addEventListener('keyup',this.keyUpHandler);
		},

		removeEventListenerTobodyToCloseOnEscKey (){
			let body = document.getElementsByTagName('body')[0];
			body.removeEventListener('keyup',this.keyUpHandler);
		}
	},

	created() {
		this.addEventListenerTobodyToCloseOnEscKey();
	},

	destroyed() {
		this.removeEventListenerTobodyToCloseOnEscKey();
	}

}
