import { serverMaintenance } from '../actions/actionTypes';

const initialState = {
	success: null,
	isDown: null,
	isMaintenanceBreak: null,
	isApiDown: null,
	Detail: {
		start: "2018-07-12 16:10:55.982000",
		end: "2018-07-12 21:10:55.982000",
		title: "Title1",
		description: "Description1"

	}
};
   

const serverMaintenanceReducer = (state = initialState, action) => {
	switch (action.type) {
	case serverMaintenance.UPDATE_SERVER_DETAILS:
		return { ...state,  ...action.payload };
	}
	return state;
};

export default  serverMaintenanceReducer;