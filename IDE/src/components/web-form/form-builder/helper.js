import { config } from '../../../config'

const predefinedProps = [
	['websiteid', 1],
	['createdon', 1],
	['updatedon', 1],
];

const predefinedPropsMap = new Map(predefinedProps);

const comparer = (currentProperties,formBuilderProps, newProperty) => {
	const { regexToRemoveSpace } = config.regex;
	//removing discrimination between space and non-breaking space
	let newProps = currentProperties.map(val => val.replace(regexToRemoveSpace, '-'));
	let newFormProps = formBuilderProps.reduce((acc, val) => {
		if(val !== undefined) {
			//removing discrimination between space and non-breaking space
			acc.push(val.replace(regexToRemoveSpace, '-'));
		}
		return acc;
	}, []);

	//removing discrimination between space and non-breaking space
	let reformedProp = newProperty.replace(regexToRemoveSpace, '-');
	if(newProps && newProperty) {
		return newProps.indexOf(reformedProp) < 0 ? false
			: countNumberOfLabel(newProps, reformedProp) === countNumberOfLabel(newFormProps, reformedProp);
	}
	return false;
};

const countNumberOfLabel = (arr, currentLabel) => {
	let count = 0;
	arr.forEach(val => val === currentLabel ? count++ : count);
	return count;
};

const generateWebformObjectForUser = listOfProperties => listOfProperties !== undefined ?
	JSON.stringify(listOfProperties.reduce((acc, { PropertyName }) => {
		//to prevent pre-defined webform properties to be added into webform object
		if(predefinedPropsMap.get(PropertyName.toLowerCase()) === undefined) {
			acc[PropertyName] = PropertyName;
		}
		return acc;
	}, {}), null, 4) : JSON.stringify({}, null, 4);

const generateUserHelper = (webformProperties, webformId) => {
	let userObject = generateWebformObjectForUser(webformProperties);
	return `
		// to submit data to the please use following object with exact properties name : 
		let webformObject_${webformId} =  ${userObject}
	`;
};

const getObjectHelper =  (properties) => properties !== undefined ?
	JSON.stringify(
		properties.reduce((acc, { PropertyName, IsRequired, DataType }) => {
			if(predefinedPropsMap.get(PropertyName.toLowerCase()) === undefined) {
				acc[PropertyName] = {
					IsRequired : IsRequired,
					DataType : DataType
				};
			}
			return acc;
		}, {}), null, 4) : JSON.stringify({}, null, 4);

const helper = {
	comparer,
	generateUserHelper,
	getObjectHelper
};

export default helper;
