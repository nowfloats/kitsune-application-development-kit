import { customer } from '../actions/actionTypes';

const initialState = {
	websites: [],
	defaultCustomer: '',
	defaultCustomerDomain: ''
};

const publishReducer = (state = initialState, action) => {
	switch (action.type) {
		case customer.CUSTOMER_RECEIVED: //eslint-disable-line
			const defaultCustomer = action.payload.data.Websites[action.payload.data.Websites.length - 1];
			return {
				...state,
				websites: action.payload.data.Websites.map(website => ({
					className: 'data-options',
					value: website.WebsiteId,
					label: website.WebsiteDomain.toLowerCase(),
					createdOn: website.CreatedOn
				})),
				defaultCustomer: defaultCustomer.WebsiteId,
				defaultCustomerDomain: defaultCustomer.WebsiteDomain.toLowerCase()
			};
	}
	return state;
};

export default publishReducer;
