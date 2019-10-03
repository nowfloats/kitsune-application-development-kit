import mapperFunctions from './mapper'
import axios from 'axios'
import helper from './helper'

const getHTML = (formData, webform) => {
	let { Name, ActionId } = webform;
	let htmlCode = '';
	let $markup = $('<div/>'); //eslint-disable-line
	$markup.formRender({ formData : formData });

	$.each(formData, (index, value) => { //eslint-disable-line
		const labelValue = value.label;
		$($markup).find('input').eq(index).attr('name', mapperFunctions.getPropertyName(labelValue)); //eslint-disable-line
		$($markup).find('label').eq(index).attr('name', mapperFunctions.getPropertyName(labelValue)); //eslint-disable-line

		if (value.type === 'select') {
			$($markup).find('select').attr('name', mapperFunctions.getPropertyName(labelValue)); //eslint-disable-line
		}
	});
	htmlCode = $markup[0].innerHTML;

	htmlCode = `
		<form id="${Name}">
			${htmlCode}
			${getButtonForForm()}
		</form>
		${getJavaScriptBaseCode(webform,true)}
		${getFormButtonForSubmission(Name, ActionId)}
	`;

	return htmlCode;
};

const getButtonForForm = () => {
	return `<button type="submit" id="wf-button" class="btn btn-default">submit</button>`;
};

const getFormButtonForSubmission = (webformName, webformId) => {
	return `
    <!-- javascript -->
		<script>
			let form_${webformId} = document.getElementById('${webformName}');
			form_${webformId}.onsubmit = function(e) {
				e.preventDefault();
				getFormDataAndSubmit_${webformId}('${webformName}');	
			}
		</script>
		`
};

const replaceChars = (name, flag) => flag ?
	name.replace(/&nbsp;/ig, ' ').replace(/<br>/ig, '').trim() :
	name.replace(/&nbsp;/ig, '').replace(/<br>/ig, '').trim();

const getJavaScriptBaseCode = ({ Name, Properties, ActionId }, isForHTML) => {
	Properties.forEach(property => {
		property.PropertyName = replaceChars(property.PropertyName);
		property.DisplayName = replaceChars(property.DisplayName, true);
	});
	let baseObject = helper.generateUserHelper(Properties, ActionId);
	let objectHelper = helper.getObjectHelper(Properties, Name);
	let userId = axios.defaults.headers.common['Authorization'];
	let extraFunctionsForHtml = '';
	if(isForHTML) {
		extraFunctionsForHtml = getFormProcessToObjectForHtml(Name, ActionId);

	}
	return `
		<script src="https://cdn.kitsune.tools/libs/webforms/v1/webforms.min.js"></script>
		<script>
		
			${baseObject};
			
			// this object contains the description of all the properties in object
			let _objectPropertiesDescription_${ActionId} = ${objectHelper}
			
			${extraFunctionsForHtml}
			
			function submit_webform_data_${ActionId}(data) {
				let _authenticationId = '${userId}'
				let _webformName = '${Name}'
				let _webformId = '${ActionId}'
	
				webforms.initialize(_webformName, _webformId, _authenticationId)
					.then(()=>{
						webforms.submit(data);
					})
					.catch(()=>{
						console.log('error')
					})
			}
		
		</script>
	`;
};

const getFormProcessToObjectForHtml = (webformName, webformId) => {
	return `
	
			function serializeArray(form) {
				var field, l, s = [];
				if (typeof form == 'object' && form.nodeName == "FORM") {
						var len = form.elements.length;
						for (var i=0; i<len; i++) {
								field = form.elements[i];
								if (field.name && !field.disabled && field.type != 'file' &&
								 field.type != 'reset' && field.type != 'submit' && field.type != 'button') {
										if (field.type == 'select-multiple') {
												l = form.elements[i].options.length; 
												for (j=0; j<l; j++) {
														if(field.options[j].selected)
																s[s.length] = { name: field.name, value: field.options[j].value };
												}
										} else if ((field.type != 'checkbox' && field.type != 'radio') || field.checked) {
												s[s.length] = { name: field.name, value: field.value };
										}
								}
						}
				}
				return s;
			}
	
			function getFormDataAndSubmit_${webformId}(formId){
	
				const objectifyForm = (formArray) => {
					let returnArray = {};
					for (let i = 0; i < formArray.length; i++) {
						returnArray[formArray[i]['name']] = formArray[i]['value'];
					}
					return returnArray;
				}
	
				let form = document.getElementById(formId);
				let serializedArray = serializeArray(form);
				let formData = objectifyForm(serializedArray);
				submit_webform_data_${webformId}(formData);
			}
	
	`
};

const codeGenerators = {
	getHTML,
	getJavaScriptBaseCode
};

export default codeGenerators;
