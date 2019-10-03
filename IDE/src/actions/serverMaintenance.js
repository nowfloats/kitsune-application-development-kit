import { serverMaintenance } from './actionTypes';


export const updateServerDetails = details => {
	return {
		type: serverMaintenance.UPDATE_SERVER_DETAILS,
		payload: details
	}
};