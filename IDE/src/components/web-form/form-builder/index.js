import React, { Component } from 'react'
import PropTypes from 'prop-types';
import { showLoading, hideLoading } from '../../../actions/loader'
import { connect } from 'react-redux'
import { toastr } from 'react-redux-toastr'
import { config } from '../../../config'
import scriptLoader from 'react-async-script-loader'
import '../../../styles/third-party/jquery-ui.css'
import mapperFunction from './mapper'
import {
	setWebactionProperties, createWebform, setHelper,
	setIsEditable, initializeCurrentWebForm, setCanClone,
	webFormsFetching } from "../../../actions/webForm";
import _ from 'lodash';
import { getFormOptions } from './userEvents'
import { fileChanged } from "../../../actions/editor";
import helper from './helper'
import codeGenerators from '../../web-form/form-builder/formCodeGenerator'
import Clipboard from 'clipboard'
import { modalClose } from '../../../actions/modal'
import { modalOpen } from '../../../actions/modal'
import SaveWebForm, { saveWebFormLabel } from "../../modals/save-webForm/index";
import { formBuilder, formRender } from "../../../config";
import InfoIcon from "../../../images/info-icon.svg"

@scriptLoader(
	'https://cdnjs.cloudflare.com/ajax/libs/jquery/3.2.1/jquery.min.js',
	'https://cdnjs.cloudflare.com/ajax/libs/jqueryui/1.12.1/jquery-ui.min.js',
	formBuilder,
	formRender
)

class FormBuilder extends Component {
	constructor(props) {
		super(props);
		this.changed = this.changed.bind(this);
		this.saveWebform = this.saveWebform.bind(this);
		this.createOrUpdateForm = this.createOrUpdateForm.bind(this);
		this.insertHtmlIntoForm = this.insertHtmlIntoForm.bind(this);
		this.propertiesComparison = this.propertiesComparison.bind(this);
		this.isObjectEmpty = this.isObjectEmpty.bind(this);
		this.changeFieldName = this.changeFieldName.bind(this);
		this.checkForUniqueLabels = this.checkForUniqueLabels.bind(this);
		this.onSaveDisabled = this.onSaveDisabled.bind(this);
		this.onSaveEnabled = this.onSaveEnabled.bind(this);
		this.updateCurrentWebForm = this.updateCurrentWebForm.bind(this);
		this.getHtmlAndJsCode = this.getHtmlAndJsCode.bind(this);
		this.setButtonAttributes = this.setButtonAttributes.bind(this);
		this.closeModal = this.closeModal.bind(this);
		this.renderFormBuilder = this.renderFormBuilder.bind(this);
		this.cloneWebForm = this.cloneWebForm.bind(this);
		this.updateProperties = this.updateProperties.bind(this);
		this.removeUnwantedCharsFromLabels = this.removeUnwantedCharsFromLabels.bind(this);
		this.handleKeyBindings = this.handleKeyBindings.bind(this);
		this.addTooltip = this.addTooltip.bind(this);
		this.replaceChars = this.replaceChars.bind(this);

		this.helpers = {
			saveWebform: this.saveWebform,
			saveEnabled: this.onSaveEnabled,
			cloneWebForm: this.cloneWebForm,
			renderFormBuilder: this.renderFormBuilder
		};
		let { dispatch, webForm } = this.props;
		dispatch(setHelper(this.helpers));
		this.lock = false;
		this.clipboard = null;
		this.formBuilder = {};
		this.currentWebFromCopy = Object.assign({},webForm.currentWebForm)
		this.properties = []
		this.webForm = null;
		this.disableSave = false;
		this.generatedCode = {
			html: null,
			js: null
		}
	}

	setButtonAttributes() {
		let buttons = document.querySelectorAll('.getCode');
		_.forEach(buttons, (button) => {
			if (button.className.includes('js')) {
				button.setAttribute('data-clipboard-text',this.generatedCode.js);
			} else {
				button.setAttribute('data-clipboard-text',this.generatedCode.html);
			}
		})
	}

	updateCurrentWebForm(arr) {
		this.currentWebFromCopy.Properties = arr;
		this.properties = this.removeUnwantedCharsFromLabels(arr)
	}

	replaceChars (name, flag) {
		return flag ?
			name.replace(/&nbsp;/ig, ' ').replace(/<br>/ig, '').trim() :
			name.replace(/&nbsp;/ig, '').replace(/<br>/ig, '').trim();
	}

	removeUnwantedCharsFromLabels (arr) {
		let newArray = arr.map(({ label, DisplayName }) => {
			let property = label ? label : DisplayName;
			if(property) {
				return property.replace(/&nbsp;/ig, ' ').replace(/<br>/ig, '').trim();
			}
		});
		return newArray
	}

	updateProperties(arr) {
		this.properties = this.removeUnwantedCharsFromLabels(arr)
	}

	/**
	 * checks for duplicate labels in the list
	 * @param list => array of all the labels
	 * @returns {Array}
	 */
	checkForUniqueLabels(list) {
		let countNumber = list.reduce((newArr, label) => {//counts the number of repetitions of each label
			label in newArr ? newArr[label.trim()]++ : newArr[label.trim()]=1
			return newArr
		},{})
		let dupLabels = [];
		for(let label in countNumber) {
			if(countNumber[label] > 1){
				dupLabels.push(label)
			}
		}
		return dupLabels //returns all duplicate labels
	}

	/**
	 * executes if this.disabled is true
	 * stops saving and marks red borders for all the duplicate labels
	 * raises an error toastr notifying presence of duplicate labels
	 * @param dupLabels => array of duplicate labels
	 * @param list => array of all the labels
	 */

	onSaveDisabled(dupLabels, list) {
		const { dispatch } = this.props;
		let indexOfDuplicateLabels = [];
		let indexOfUniqueLabels = []
		dupLabels.map(duplicateLabel => {
			list.map((eachLabel, index) => {		//returns index for duplicate labels
				// and undefined for rest
				return eachLabel === duplicateLabel ? indexOfDuplicateLabels.push(index)
					: indexOfUniqueLabels.push(index)
			})
		})
		let listOfFieldsObject = $($('.form-builder-container').find('ul')[0]).find("li"); //eslint-disable-line
		//jquery form builder object
		indexOfDuplicateLabels.map(lab => {
			$(listOfFieldsObject[lab]).hasClass('duplicate-label') ? null //eslint-disable-line
				: $(listOfFieldsObject[lab]).addClass('duplicate-label')	//eslint-disable-line
			//sets duplicate borders to red
		})
		indexOfUniqueLabels.map(lab => {
			$(listOfFieldsObject[lab]).hasClass('duplicate-label') //eslint-disable-line
				? $(listOfFieldsObject[lab]).removeClass('duplicate-label') //eslint-disable-line
				: ''
		})
		dispatch(hideLoading())
		toastr.error('all labels must be unique') // raises error toastr for duplicate labels
	}

	/**
	 * is executed if this.disabled is false
	 * if new webform, then opens modal for asking name of webform and then saves
	 * else directly saves
	 * @param isNewWebForm => boolean value which checks if webform is new or not
	 */
	onSaveEnabled(callback) {
		const { getData } = this.formBuilder.actions;
		const { dispatch } = this.props;
		dispatch(webFormsFetching(true))
		let fbData = getData();
		let webActionProperties = mapperFunction.getWebActionProperties(fbData); //mapped to webAction
		// Object to be send via api
		//this.currentWebFromCopy.Properties = webActionProperties
		this.updateCurrentWebForm(webActionProperties);
		webActionProperties.forEach( property => {
			property.DisplayName = this.replaceChars(property.DisplayName, true);
			property.PropertyName = this.replaceChars(property.PropertyName);
		});
		if(fbData.length) {
			this.updateProperties(fbData)
		}
		dispatch(setWebactionProperties(webActionProperties)); //sets properties in current webform in state
		let data = JSON.stringify(fbData);
		data = data.split(`"`).join(`'`);
		dispatch(createWebform(true, data)).then(() => {
			if(callback !== null) {
				callback();
			}
		});
		this.getHtmlAndJsCode();
		dispatch(setIsEditable(false))
		if(this.props.webForm.canClone) {
			dispatch(setCanClone(false))
		}
	}

	closeModal() {
		const { dispatch } = this.props;
		dispatch(modalClose());
	}

	/**
	 * saves the current form
	 * **/
	saveWebform(callback = null) {
		let { fileChanged: isModified, webForm, dispatch } = this.props;
		let { currentWebForm } = webForm;
		let isNewWebForm = currentWebForm.ActionId == undefined;
		if (isModified || (isNewWebForm && isModified)) {
			let formData = this.formBuilder.actions.getData();
			let arrayOfLabels = formData.map(({ label }) =>
				label.replace(/&nbsp;/ig, ' ').replace(/<br>/ig, '').trim());
			let duplicateLabelsArray = this.checkForUniqueLabels(arrayOfLabels)
			this.disableSave = duplicateLabelsArray.length !== 0
			this.disableSave ? this.onSaveDisabled(duplicateLabelsArray, arrayOfLabels) :
				$($('.ui-sortable')[0]).children().length !== 0 ? //eslint-disable-line
					dispatch(modalOpen(<SaveWebForm />, saveWebFormLabel, callback)) :
					toastr.info('please add fields to save')
		}
	}

	/**
	 * compares properties in copy of current webform and formbuilder properties
	 * formbuilder is set in the constructor
	 * @param fld => newly added element in form builder
	 */
	propertiesComparison(fld) {
		const { getData } = this.formBuilder.actions;
		const { dispatch, webForm } = this.props;
		let { fileChanged: isModified } = this.props;
		let formData = getData();
		let formBuilderLabels = this.removeUnwantedCharsFromLabels(formData, 'label')
		let regexToFindNewLineChar = /\r?\n|\r/;
		let newFieldLabel = fld.childNodes[1].innerText.trim();
		newFieldLabel = regexToFindNewLineChar.test(newFieldLabel) ?
			newFieldLabel.replace(regexToFindNewLineChar, '') : newFieldLabel;
		let isExistingProperty = helper.comparer(this.properties, formBuilderLabels, newFieldLabel);
		if(!isExistingProperty && !isModified) {
			dispatch(fileChanged(true));
		}
		if(!isExistingProperty && !this.lock) {
			this.lock = true;
			let listOfFieldsObject = $($('.form-builder-container').find('ul')[0]).children("li"); //eslint-disable-line
			let currentIndex = listOfFieldsObject.index(fld) //finds the index of the currently added field
			// so that name change function can be called for that position
			//this.currentWebFromCopy.Properties = formBuilderProperties
			//as formbuilder will re-render the
			// form in changeFieldName, so to avoid infinite loop, both are set the same so that the next time it
			// will be perceived as existing property
			if(!webForm.isEditable) {
				this.changeFieldName(fld, webForm.jsonWebForm, currentIndex);
			}
			else {
				this.changeFieldName(fld, formData, currentIndex)
			}
		}
		if(!isExistingProperty && this.lock){
			this.lock = false;
		}
	}

	/**
	 * checks if object is empty or not
	 * @param obj => target object
	 * @returns {boolean} if empty returns true
	 */
	isObjectEmpty(obj) {
		for(let key in obj) {
			if(obj.hasOwnProperty(key))
				return false;
		}
		return true;
	}

	/**
	 * adds a number at the end of the name
	 * number is calculated based on the similar label having a number
	 * so if we have 'number 2' as a label, so the new label 'number' will be renamed to 'number 3'
	 * @param arr => array of all labels
	 * @param name => duplicate name that has to be changed
	 * @param count => count which is appended at the end of the name to make it unique
	 * @returns {*}
	 */
	changeName(arr, name, count=1) {
		const newName = `${name} ${count}`
		if(arr.indexOf(newName) < 0)
			return newName //exit condition
		else
			return this.changeName(arr, name, ++count) //recursion that keeps on counting the number to be appended
	}

	/**
	 * counts the occurrence of the current label in the list of labels
	 * @param arr => array of all the labels
	 * @param value => label to be counted
	 */
	countInArray (arr, value) {
		return arr.reduce((n, x) => n + (x === value), 0)
	}

	/**
	 * changes the duplicate labels to unique labels by appending a number to it
	 * @param fld => currently added field in form builder
	 * @param builderData => current form builder fields as an array
	 * @param currentIndex => the index of the label to be renamed
	 */
	changeFieldName(fld, builderData, currentIndex) {
		let formBuilderData = builderData.slice(0);
		let filteredArray = formBuilderData.filter(data => {
			return data.type == fld.type
		})
		let listOfLabels = filteredArray.map(object => object.label)
		let currentLabel = fld.children[1].textContent;
		let labelCount = this.countInArray(listOfLabels, currentLabel)
		if(labelCount > 1){
			currentLabel = this.changeName(listOfLabels, currentLabel)
			formBuilderData[currentIndex].label = currentLabel;
			formBuilderData = formBuilderData.filter(obj => !this.isObjectEmpty(obj)) //removes empty object from the array
			this.updateCurrentWebForm(mapperFunction.getWebFormToWebactionWithEnum(formBuilderData))
			this.formBuilder.actions.setData(JSON.stringify(formBuilderData))
		}
	}

	/**
	 * executed when ever user changes anything in opened form
	 * **/
	changed(fld) {
		const { fileChanged: isModified } = this.props;
		const { dispatch } = this.props;
		if(!isModified) {
			dispatch(fileChanged(true))
		}
	}

	getHtmlAndJsCode() {
		const { currentWebForm } = this.props.webForm;
		const { getData } = this.formBuilder.actions;
		this.generatedCode = {
			html: codeGenerators.getHTML(getData(), currentWebForm),
			js: codeGenerators.getJavaScriptBaseCode(currentWebForm)
		}
		this.setButtonAttributes();
	}

	renderFormBuilder(formBuilderOptions, isEditable, currentWebForm, renderFlag) {
		let el = $(this.el); //eslint-disable-line
		let builder = el //eslint-disable-line
			.formBuilder({ ...formBuilderOptions, onSave: this.saveWebform,
				...getFormOptions(this.changed, this.propertiesComparison, !isEditable) });//eslint-disable-line no-undef
		builder.promise.then((fb) => {
			if(!isEditable) {
				let elements = document.querySelectorAll('.cb-wrap');
				if(elements.length) {
					let [ element ] = elements;
					element.hidden = true;
				}
			}
			this.formBuilder = Object.assign({}, fb);
			const { setData } = this.formBuilder.actions;
			const { webForm, dispatch } = this.props;
			let { jsonWebForm, canClone } = webForm
			if(!isEditable || canClone) {
				this.updateProperties(jsonWebForm)
				setData(JSON.stringify(jsonWebForm));
			}
			else {
				let webFormProperties = mapperFunction.getWebformObject(currentWebForm);
				this.updateProperties(webFormProperties)
				setData(JSON.stringify(webFormProperties));
			}
			if(renderFlag) {
				if(!isEditable) {
					el.formRender({
						dataType: 'json',
						formData: this.formBuilder.actions.getData()
					});
					let renderedInputs = $('.rendered-form input') //eslint-disable-line
					renderedInputs.prop('disabled', true);
				}
			}
			this.getHtmlAndJsCode();
			this.insertHtmlIntoForm();
			const { isFetching } = webForm
			if(!isFetching) {
				dispatch(hideLoading())
			}
		});
		builder.promise.catch(() => {
			toastr.error('webform error','error in rendering form');
		})
	}

	/**
	 * if form builder is initialized:
	 * 		update form with new form
	 * else:
	 * 		create new instance and initialize with webform
	 * **/
	createOrUpdateForm(webForm) {
		const { dispatch } = this.props;
		const { formBuilderOptions } = config.INTERNAL_SETTINGS;
		const { currentWebForm, isEditable, jsonWebForm, isFetching } = webForm;
		let el = $(this.el); //eslint-disable-line
		if (Object.keys(this.formBuilder).length === 0) {
			this.renderFormBuilder(formBuilderOptions, isEditable, currentWebForm, true);
		}
		else {
			const { setData } = this.formBuilder.actions;
			this.updateCurrentWebForm(currentWebForm.Properties)
			if(!isEditable) {
				if(jsonWebForm.length) {
					this.updateProperties(jsonWebForm)
				}
				setData(JSON.stringify(jsonWebForm));
				el.formRender({
					dataType: 'json',
					formData: this.formBuilder.actions.getData()
				});
				let renderedInputs = $('.rendered-form input') //eslint-disable-line
				renderedInputs.prop('disabled', true);
				if(!isFetching) {
					dispatch(hideLoading())
				}
			}
			else {
				let webFormProperties = mapperFunction.getWebformObject(currentWebForm);
				setData(JSON.stringify(webFormProperties));
			}
			this.getHtmlAndJsCode();
		}
	}

	cloneWebForm() {
		const { dispatch, webForm } = this.props;
		// dispatch(isWebFormOpen(false))
		const { formBuilderOptions } = config.INTERNAL_SETTINGS;
		const { currentWebForm } = webForm;
		dispatch(setIsEditable(true));
		dispatch(initializeCurrentWebForm(true));
		let el = $(this.el); //eslint-disable-line
		el.empty();
		this.renderFormBuilder(formBuilderOptions, true, currentWebForm, false);
	}

	insertHtmlIntoForm() {
		const { webForm } = this.props;
		const { isEditable } = webForm;
		if (!$('.form-builder-header').length) { //eslint-disable-line
			this.clipboard = new Clipboard('.getCode');
			this.clipboard.on('success', (e) => {
				toastr.success('code succesfully copied')
			})
			this.clipboard.on('error', (e) => {
				toastr.error('code copy error')
			})
			const buildArea = `<div class="form-builder-header">build area
			</div>`;
			const elements = `<div class="form-builder-header">elements
			</div>`;
			$('.stage-wrap.pull-left').prepend(buildArea) //eslint-disable-line
			$('.cb-wrap.pull-right').prepend(elements) //eslint-disable-line
			this.setButtonAttributes();
		}
		if (isEditable) {
			this.addTooltip();
		}
	}

	addTooltip() {
		let elemLabels = $('.input-control span'); //eslint-disable-line
		for(let i = 0;i<elemLabels.length;i++) {
			$(elemLabels[i]) //eslint-disable-line
				.append(`<img src=${InfoIcon} class='infoIconWF' 
				title='creates a ${elemLabels[i].innerHTML} field' />`);
		}
	}

	componentWillReceiveProps({ isScriptLoaded, isScriptLoadSucceed }) {
		const { dispatch, isScriptLoaded: prevScriptLoaded } = this.props;
		if (isScriptLoaded && !prevScriptLoaded) {
			dispatch(hideLoading())
			if (!isScriptLoadSucceed) {
				toastr.error('something went wrong');
			}
		}
	}

	componentWillMount() {
		const { dispatch } = this.props;
		const { text: OPEN_WEBFORM } = config.INTERNAL_SETTINGS.loadingText.OPEN_WEBFORM
		dispatch(showLoading(OPEN_WEBFORM));
		//testing local shelveset
	}

	componentDidMount() {
		const { isScriptLoaded, webForm } = this.props;
		document.addEventListener('keydown', this.handleKeyBindings, false);
		if (isScriptLoaded) {
			this.createOrUpdateForm(webForm);
		}
	}

	componentDidUpdate(prevProps) {
		const { webForm, isScriptLoaded: nextScript } = this.props;
		const { jsonWebForm: nextJson, isEditable: nextEditable, currentWebForm: nextCurrentWf } = webForm;
		const { isScriptLoaded } = prevProps
		const { jsonWebForm, isEditable, currentWebForm } = prevProps.webForm;
		const isJsonModified = nextJson !== jsonWebForm;
		const isEditableModified = nextEditable !== isEditable;
		const isScriptModified = isScriptLoaded !== nextScript;
		// const isOpenModified = isOpen !== nextIsOpen;
		if((isScriptLoaded && (isJsonModified || isEditableModified)) ||
			(isScriptModified && !isJsonModified)) {
			this.createOrUpdateForm(webForm);
		}
		if(currentWebForm.ActionId !== nextCurrentWf.ActionId){
			this.getHtmlAndJsCode();
		}
	}

	handleKeyBindings(e) {
		if((e.ctrlKey || e.metaKey) && e.keyCode === 83) {
			e.preventDefault();
			this.saveWebform();
		}
	}

	componentWillUnmount() {
		document.removeEventListener('keydown', this.handleKeyBindings, false);
	}

	render() {
		let { style, footer, webForm } = this.props;
		const { isEditable } = webForm;
		const note = isEditable ? '' : 'note: this is just a preview of the webform';
		style = { ...style, height: footer.isActive !== null ? `calc(100vh - 296px)` : `calc(100vh - 144px)` }
		return (
			<div className='form-builder-container' style={style}>
				<div>
					<p className='note'>{note}</p>
					<div ref={el => this.el = el} />
				</div>
			</div>
		)
	}
}

FormBuilder.propTypes = {
	isScriptLoaded: PropTypes.bool,
	isScriptLoadSucceed: PropTypes.func,
	dispatch: PropTypes.func,
	webForm: PropTypes.shape({
		currentWebForm: PropTypes.object,
		isOpen: PropTypes.bool,
		isEditable: PropTypes.bool,
		jsonWebForm: PropTypes.array,
		canClone: PropTypes.bool,
		isFetching: PropTypes.bool
	}),
	fileChanged: PropTypes.bool,
	style: PropTypes.object,
	footer: PropTypes.object
}

const mapStateToProps = (state) => {
	const { activeTabs, visibleIndex } = state.editorReducer;
	const editor = activeTabs[visibleIndex];
	return {
		webForm : state.webFormReducer,
		fileChanged: editor ? editor.fileChanged : false,
		footer: state.footerReducer
	}
}

export default connect(mapStateToProps)(FormBuilder)
