import _ from 'lodash';

// methods to attach events to all new created or cloned properties
const methodsForEditableFields = {
	onadd: function (fld) {
		baseMethod(fld);
		propertiesChanged(fld);
		addRemoveActionListener(fld);
	},
	onclone: function (fld) {
		onChangeMethod(); // change should be triggered if new element is cloned
		baseMethod(fld);
		addRemoveActionListener(fld);
		// console.log(toggleFormLock)
	}
}

const methodsForDisablingFields = {
	onadd : function (fld) {
		disableActionButtons(fld);
		disableFields(fld)
	}
}

const getOptions = (method) => {
	let opt = {
		typeUserEvents: {
			text: method,
			date: method,
			'checkbox-group': method,
			'radio-group': method,
			number: method,
			select: method
		}
	}
	return opt;
}

const disAllowEnterInFieldLabel = function(e) {
	if(e.keyCode == 13 || e.which == 13) {
		return false;
	}
}

const disableFields = (fld) => {
	let element = $(fld); //eslint-disable-line
	let inputWrappers = element.find('.input-wrap');
	_.forEach(inputWrappers, (inputWrapper) => {
		let children = $(inputWrapper).children(); //eslint-disable-line
		if(children.length > 0) {
			let input = children[0];
			if (input.tagName === "DIV") {
				input.contentEditable = false;
			} else {
				input.disabled = true;
			}
		}
	})
	// fakeEditButtonClick(0)
}

const disableActionButtons = (fld) => {
	let element = fld;
	if(element.hasChildNodes()) {
		let actionsButtons = element.firstElementChild;
		_.forEach(actionsButtons.children, (children)=> {
			if(children && children.type !== 'edit') {
				children.remove();
			}
		})
	}
	let reqWrap = $('.required-wrap').find('.input-wrap'); //eslint-disable-line
	reqWrap.toArray().map(elem => {
		if(!$(elem).hasClass('required-wrap-style')){ //eslint-disable-line
			$(elem).addClass('required-wrap-style') //eslint-disable-line
		}
	})
	$(fld).find('.frm-holder').addClass('disabled-fields') //eslint-disable-line
}

// attaches event to track if user removes a field
const addRemoveActionListener = (property) => {
	// add length count check
	let removeButton = $(property).find('.del-button.btn.icon-cancel.delete-confirm')[0]; //eslint-disable-line
	removeButton.addEventListener('click',onChangeMethod)
}
// the core on change method
let onChangeMethod = () => {}
/*
let tryF = () => {
	console.log('hahahaha')
}*/
//
let propertiesChanged = () => {}

let isNotEditable = null;
// adding events because some elements are div(editable)
// instead of input so need to attach event to observe change in it.
const addEventListnerToEditableTag = (element) => {
	element.on("DOMNodeInserted", onChangeMethod);
	element.on("DOMNodeRemoved", onChangeMethod);
	element.on("DOMCharacterDataModified", onChangeMethod);
}

const changeElementOrder = (parentElem, elem1, elem2) => {
	let formElements = $(`${parentElem}`); //eslint-disable-line
	let lastElement = formElements[formElements.length - 1];
	let requiredWrap = $(lastElement).children(`${elem1}`); //eslint-disable-line
	let labelWrap = $(lastElement).children(`${elem2}`); //eslint-disable-line
	$(requiredWrap).insertAfter($(labelWrap)) //eslint-disable-line
}

// attaching event to observe any change in property
const baseMethod = (ele) =>{
	attachEventsToOptions(ele);
	let listOfInputWraperrs = $(ele).find('.input-wrap'); //eslint-disable-line
	for (let i=0;i<listOfInputWraperrs.length;i++){
		// get list of all input elements
		let inputElements = $($(listOfInputWraperrs[i]).children()); //eslint-disable-line
		if (inputElements.length > 0) {
			let element = $(inputElements[0]); //eslint-disable-line
			// check if element id div or not and attach events accordingly
			if (element.prop('tagName').toLowerCase() === 'div') {
				addEventListnerToEditableTag(element)
			} else {
				eventAttacher(element,'change',onChangeMethod);
				eventAttacher(element,'input',onChangeMethod);
			}
		}
	}
	changeElementOrder('.form-elements', 'div.required-wrap', 'div.label-wrap');
	changeElementOrder('.field-actions', 'a[type=remove]', 'a[type=copy]');
	if(!isNotEditable) {
		let labelDivs = document.querySelectorAll('.fld-label')
		if(labelDivs.length) {
			let lastLabel = labelDivs[labelDivs.length - 1];
			lastLabel.onkeydown = disAllowEnterInFieldLabel;
			let reqWrap = $('.required-wrap').find('.input-wrap'); //eslint-disable-line
			reqWrap.toArray().map(elem => {
				if(!$(elem).hasClass('required-wrap-style')){ //eslint-disable-line
					$(elem).addClass('required-wrap-style') //eslint-disable-line
				}
			})
		}
		let maxLengthInputs = document.getElementsByName('maxlength')
		if(maxLengthInputs.length){
			maxLengthInputs[maxLengthInputs.length - 1].value = 255;
		}
		fakeEditButtonClick('a[type=edit]')
	}
}

const fakeEditButtonClick = (child) => {
	let formFields = document.querySelectorAll('.form-field')
	if(formFields.length) {
		formFields[formFields.length - 1].addEventListener('click', function(e) {
			let formElements = e.target.children;
			if(formElements.length) {
				let fieldActions = $($(formElements)[0]).children(`${child}`) //eslint-disable-line
				$(fieldActions).click(); //eslint-disable-line
			}
		})
	}
}

const attachEventsToOptions = (ele) => {
	let element = $(ele); //eslint-disable-line
	let elementType = element.attr('type');
	if( elementType === 'select' || elementType === 'checkbox-group' || elementType === 'radio-group') {

		let options = $(element.find('.form-group.field-options')); //eslint-disable-line
		if (options.length > 0) {
			let inputs = options.find('input');
			for(let i=0;i<inputs.length;i++) {
				eventAttacher(inputs[i],'change',onChangeMethod);
				eventAttacher(inputs[i],'input',onChangeMethod);
			}
			let btn = options.find('a');
			for(let i=0;i<btn.length;i++) {
				eventAttacher(btn[i],'click',onChangeMethod)
			}

		}

	}
}

const eventAttacher = (element, eventName, method)=>{
	let ele = $(element); //eslint-disable-line
	ele.on(eventName,method);
}

export const getFormOptions = (method, propertiesComparison, areFieldsDisabled) => {
	onChangeMethod = method;
	propertiesChanged = propertiesComparison;
	isNotEditable = areFieldsDisabled
	return areFieldsDisabled ? getOptions(methodsForDisablingFields) : getOptions(methodsForEditableFields);
	// return getOptions(methodsForEditableFields)
}
