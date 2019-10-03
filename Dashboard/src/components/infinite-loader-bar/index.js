export default {
	name: 'infiniteloadingbar',

	props: {
		extraHeight: {
			type: Boolean,
			required : false,
			default : function(){
				return false
			}
		}
	}
}
