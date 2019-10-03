import migrationHeader from '../header/stage0header/stage0header.vue';
import migrationBody from '../body/errorbody/errorbody.vue';

export default {
	name : 'migrationError',

	components:{
		migrationHeader,
		migrationBody
	},

	methods:{

		minimizeProject(){
			this.$emit('minimize');
		}

	}
}
