import { mapGetters, mapActions } from 'vuex'
import dotsLoader from '../loaders/threeDots/ThreeDots.vue'
import helpers from '../mixin'

export default {
	name: 'deleteprojectmodal',

	components: {
		dotsLoader
	},

	mixins: [helpers],

	data() {
		return  {
			disableButton : false,
			buttonText : 'yes',
			buttonTextDeleting : 'deleting'
		}
	},

	computed : {
		...mapGetters({
			project : 'getDeleteProjectDetails'
		}),

		projectName(){
			return this.project.projectName.replace('https:','').replace('http:','').replace(/\//g,'');
		}

	},

	methods:{
		...mapActions([
			'toggleStatus',
			'deleteProject'
		]),

		closeDeleteProjectModal(e){
			this.toggleStatusHandler(e, ['overlay','deleteproject']);
		},

		keyUpHandler(event){
			if(event.keyCode === 27 ){
				this.closeDeleteProjectModal(event);
			}
		},

		addEventListenerTobodyToCloseOnEscKey (){
			let body = document.getElementsByTagName('body')[0];
			body.addEventListener('keyup',this.keyUpHandler);
		},

		removeEventListenerTobodyToCloseOnEscKey (){
			let body = document.getElementsByTagName('body')[0];
			body.removeEventListener('keyup',this.keyUpHandler);
		},

		removeProject(){
			this.disableButton = true;
			this.deleteProject();
		}
	},

	created() {
		this.addEventListenerTobodyToCloseOnEscKey();
	},

	destroyed() {
		this.removeEventListenerTobodyToCloseOnEscKey();
	}

}
