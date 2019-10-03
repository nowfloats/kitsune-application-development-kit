/*Herein lies all the common configuration*/
import firstLoginIcon from './images/login-icon.svg'
import editorBackground from './images/editor-background.png'
import ProjectTree from './components/sidebar/project-tree';
import WebForms from './components/sidebar/web-forms';
import SchemaKitsune from './components/sidebar/kitsune-schema/index'
import Shortcuts from './components/sidebar/shortcuts';
import { EDITOR_PLUGINS } from './components/editor/plugins';
const { NODE_ENV } = process.env;
const isProdEnv = (NODE_ENV === 'production' || NODE_ENV === 'DEV_PRODAPI');
export const isProdDeployed = NODE_ENV === 'production';
const domain = isProdEnv ? 'https://api2.kitsune.tools' : 'https://api2.kitsunedev.com';
export const httpDomainWithoutSSL = isProdEnv ? 'http://ide.kitsune.tools' : 'http://ide.kitsunedev.com';
const webactionDomain = 'https://webactions.kitsune.tools';
export const cookieDomain = isProdEnv ? '.kitsune.tools' : '.kitsunedev.com';
export const formBuilder = '/form-builder.min.js';
export const formRender = '/form-render.min.js';
export const riaComponentID = isProdEnv ? '5ab5190ba35c3b04e9817cb5' : '5ac5f6b0a96adf000174d3ef';
export const riaComponentName = isProdEnv ? 'reports' : 'ria-schema-dev';

const dataTypeOrder = [
	['string', 1],
	['number', 2],
	['image', 3],
	['datetime', 4],
	['link', 5],
	['boolean', 6],
	['k-string', 7],
	['array', 8]
];

export const config = {
	API: {
		webActionDomain: webactionDomain,
		projectList: `${domain}/api/ide/List`,
		project: `${domain}/api/ide`,
		projectAPI: `${domain}/api/project`,
		projectEdit: `${domain}/api/Project/v1/Project`,
		websiteData: `${domain}/language/v1/{schema}/get-data?website=`,
		schemaList: `${domain}/language/v1/get-schemas`,
		schemaDetails: `${domain}/language/v1`, // {schemaID}
		mapSchema: `${domain}/language/v1/MapSchema`,
		newOrUpdateSchema: `${domain}/language/v1/CreateOrUpdateLanguageEntity`,
		themeAssetUpload: `${domain}/ide`,
		build: `${domain}/api/Project/v1/Build`,
		buildTotal: `${domain}/api/Project/v1/ProjectDetailForBuild`,
		customers: `${domain}/api/website/v1?projectId=`,
		kAdminLogin: `${domain}/api/Website/v1/KdaminLogin?websiteId=`,
		publish: `${domain}/api/Project/v1/Publish`, // {project_id}/MakeProjectLive/{developer_email}
		userToken: `${domain}/api/developer/v1/getuserid?useremail=`, //{userEmail}
		createUser: `${domain}/api/Developer/v1/CreateUser`,
		userDetails: `${domain}/api/Developer/v1/UserProfile?useremail=`, //{userEmail}
		intellisense: `${domain}/language/v1/`, //{projectID}/get-intellisense
		createWebform: `${webactionDomain}/api/v1/CreateOrUpdate`,
		webFormsList: `${webactionDomain}/api/v1/List?limit=100&Type=webform`,
		addJsonOfWebForm: `https://api2.kitsune.tools/api/project/webform/{webActionId}/add-data`,
		updateJsonOfWebForm: `https://api2.kitsune.tools/api/project/webform/{webActionId}/update-data`,
		getJsonOfWebForm: `https://api2.kitsune.tools/api/project/webform/{webActionId}/get-data`,
		userIdForDevKitsune: '5959ec985d643701d48ee8ab',
		toggleEnableRiaApp: `${domain}/api/Project/v1/UpdateProjectComponents?projectId={projectId}
		&componentId={componentId}&enable={enable}`,
		listOfRiaApps: `${domain}/api/Project/v1/ListAvailableKApps`,
		bugImages: `${webactionDomain}/api/v1/bugs/upload-file`,
		sendMail: `${domain}/api/Internal/v1/SendEmail?configType=1`,
		reportBug: `${webactionDomain}/api/v1/bugs/add-data`,
		kitsuneApidowncheck: "https://status.api.kitsune.tools/kitsuneapi"
	},
	regex: {
		regexToRemoveSpace: /\s/ig,
		regexToCheckStartSchemaAndWebForm: /^([a-zA-Z\_])$/,
		regexToCheckSchemaAndWebForm: /[a-zA-Z0-9\s\_]+/g
	},
	INTERNAL_SETTINGS: {
		extensionsIconMap: new Map(),
		extensionMap: new Map(),
		dataTypeOrderMap: new Map(dataTypeOrder),
		SchemaType: [
			{
				className: 'data-options',
				value: 0,
				label: 'string'
			},
			{
				className: 'data-options',
				value: 1,
				label: 'array'
			},
			{
				className: 'data-options',
				value: 2,
				label: 'number'
			},
			{
				className: 'data-options',
				value: 3,
				label: 'boolean'
			},
			{
				className: 'data-options',
				value: 4,
				label: 'date'
			},
			{
				className: 'data-options',
				value: 5,
				label: 'object'
			}
		],
		mapSchemaType: {
			0: 'string',
			1: 'array',
			2: 'number',
			3: 'boolean',
			4: 'datetime',
			5: 'object',
			6: 'function',
			7: 'k-string'
		},
		mapDataType: {
			0: 'custom',
			1: 'base',
			2: 'kitsune',
			3: 'primitive'
		},
		mapPropertyType: {
			0: 'string',
			1: 'array',
			2: 'number',
			3: 'boolean',
		},
		responsivePreview: {
			desktop: {
				name: 'desktop',
				width: 1440,
				height: 900
			},
			tablet: {
				name: 'tablet',
				width: 768,
				height: 1024
			},
			phone: {
				name: 'phone',
				width: 320,
				height: 568
			},
			none: {
				name: 'none',
				width: '100%',
				height: 'calc(100vh - 45px)'
			}
		},
		dataTypes: {
			STRING: 'STRING',
			NUMBER: 'NUMBER',
			BOOLEAN: 'BOOLEAN',
			DATETIME: 'DATETIME',
			FUNCTION: 'FUNCTION',
			KSTRING: 'KSTRING'
		},
		advancedSchemaProperties: {
			Default: {
				type: 'k-default',
				label: 'default value',
				schemaTypes: ['string', 'number', 'array', 'datetime', 'boolean', 'kstring']
			},
			ValueRange: {
				type: 'k-default',
				range: true,
				label: 'value range',
				schemaTypes: ['number', 'datetime']
			},
			ArrayLength: {
				type: 'number',
				range: true,
				label: 'array length',
				schemaTypes: ['array']
			},
			TextLength: {
				type: 'number',
				range: true,
				label: 'string length',
				schemaTypes: ['string']
			},
			FixedList: {
				type: 'array',
				label: 'fixed values',
				creatable: true,
				schemaTypes: ['array'],
				options: []
			},
			AdminReadOnly: {
				type: 'boolean',
				label: 'admin read-only',
				schemaTypes: [],
				value: false
			},
			IsUnique: {
				type: 'boolean',
				label: 'is unique',
				schemaTypes: [],
				value: false
			}
		},
		pageStates: {
			PROJECT_NONE: {
				name: 'PROJECT_NONE',
				icon: firstLoginIcon,
				iconAlt: `placeholder`,
				heading: `kitsune's development environment`,
				para: `try opening a project or start a project.`
			},
			PROJECT_CLOSED: {
				name: 'PROJECT_CLOSED',
				icon: firstLoginIcon,
				iconAlt: `placeholder`,
				heading: `open a project!`,
				para: `open an existing project or create a new one.`
			},
			FILE_NONE: {
				name: 'FILE_NONE',
				icon: editorBackground,
				iconAlt: `placeholder`,
				heading: `almost there!`,
				para: `build your cloud native project, `
			}
		},
		riaJSON: {
			notifications: [{
				type: "PERIODIC_PERFORMANCE_REPORT",
				period: 7,
				email: {
					body: "periodic_performance_report.html"
				}
			}],
			settings: {
				email: {
					host: "",
					port: 0,
					password: "",
					username: "",
					ssl_enabled: true
				}
			}
		},
		kitsuneSettingsEncoded: `ew0KICAgICJwYXltZW50cyI6IHsNCiAgICAgICAgInByZXZpZXciOiBbDQogICAgICAgICAgICB7DQogICAgICAgI
		CAgICAgICAgImRvbWFpbiI6ICJleGFtcGxlLmNvbSIsDQogICAgICAgICAgICAgICAgImdhdGV3YXkiOiAiaW5zdGFtb2pvIiwNCiAgICAgICAgICA
		gICAgICAic2FsdCI6ICJTQUxUIiwNCiAgICAgICAgICAgICAgICAiYXBpX2tleSI6ICJBUElfS0VZIiwNCiAgICAgICAgICAgICAgICAiYXBpX3NlY
		3JldCI6ICJBUElfU0VDUkVUIiwNCiAgICAgICAgICAgICAgICAicmVkaXJlY3RfcGF0aCI6ICIvdHJhbnNhY3Rpb25fc3RhdHVzLmh0bWwiLA0KICA
		gICAgICAgICAgICAgICJhcGlfdXJsIjogImh0dHBzOi8vdGVzdC5pbnN0YW1vam8uY29tL2FwaS8xLjEiDQogICAgICAgICAgICB9DQogICAgICAgI
		F0sDQogICAgICAgICJsaXZlIjogWw0KICAgICAgICAgICAgew0KICAgICAgICAgICAgICAgICJkb21haW4iOiAiZXhhbXBsZS5jb20iLA0KICAgICA
		gICAgICAgICAgICJnYXRld2F5IjogImluc3RhbW9qbyIsDQogICAgICAgICAgICAgICAgInNhbHQiOiAiU0FMVCIsDQogICAgICAgICAgICAgICAgI
		mFwaV9rZXkiOiAiQVBJX0tFWSIsDQogICAgICAgICAgICAgICAgImFwaV9zZWNyZXQiOiAiQVBJX1NFQ1JFVCIsDQogICAgICAgICAgICAgICAgInJ
		lZGlyZWN0X3BhdGgiOiAiL3RyYW5zYWN0aW9uX3N0YXR1cy5odG1sIg0KICAgICAgICAgICAgfQ0KICAgICAgICBdDQogICAgfQ0KfQ==`,
		paymentSettings: {
			instamojo: {
				preview: {
					domain: "example.com",
					gateway: "instamojo",
					salt: "SALT",
					api_key: "API_KEY",
					api_secret: "API_SECRET",
					redirect_path: "/transaction_status",
					api_url: "https://test.instamojo.com/api/1.1"
				},
				live: {
					domain: "example.com",
					gateway: "instamojo",
					salt: "SALT",
					api_key: "API_KEY",
					api_secret: "API_SECRET",
					redirect_path: "/transaction_status"
				}
			},
			payu: {
				preview: {
					domain: "example.com",
					gateway: "payu",
					api_key: "API_KEY",
					salt: "SALT",
					api_secret: "",
					redirect_path: "/transaction_status",
					api_url: "https://secure.payu.in",
					response_webhook: "https://yyhloe6kfg.execute-api.ap-south-1.amazonaws.com/prod/payment_response_proxy/" +
						"http://bin.prayashm.com/qu0mllqu"
				},
				live: {
					domain: "example.com",
					gateway: "payu",
					api_key: "API_KEY",
					salt: "SALT",
					api_secret: "",
					redirect_path: "/transaction_status",
					api_url: "https://secure.payu.in"
				}
			},
			paytm: {
				preview: {
					domain: "example.com",
					gateway: "paytm",
					api_secret: "Merchant_ID",
					api_key: "Account_Secret_Key",
					redirect_path: "/transaction_status",
					api_url: "https://pguat.paytm.com",
					payment_request_endpoint: "/oltp-web/processTransaction",
					transaction_status_endpoint: "/oltp/HANDLER_INTERNAL/getTxnStatus?JsonData="
				},
				live: {
					domain: "*",
					gateway: "paytm",
					api_secret: "API_SECRET",
					api_key: "API_KEY",
					redirect_path: "/transaction_status",
					api_url: "https://pguat.paytm.com",
					payment_request_endpoint: "/oltp-web/processTransaction",
					transaction_status_endpoint: "/oltp/HANDLER_INTERNAL/getTxnStatus?JsonData=",
					response_webhook: "https://yyhloe6kfg.execute-api.ap-south-1.amazonaws.com/prod/payment_response_proxy/" +
						"http://bin.prayashm.com/qu0mllqu"
				}
			},
			aamarpay: {
				preview: {
					domain: "example.com",
					gateway: "aamarpay",
					api_secret: "signature_key",
					api_key: "store_id",
					redirect_path: "/transaction_status",
					api_url: "https://sandbox.aamarpay.com"
				},
				live: {
					domain: "*",
					gateway: "aamarpay",
					api_secret: "signature_key",
					api_key: "store_id",
					redirect_path: "/transaction_status",
					api_url: "https://secure.aamarpay.com"
				}
			}
		},
		promptMessages: {
			LOG_OUT: {
				name: 'LOG_OUT',
				heading: 'logout',
				text: `are you sure you want to logout? if yes, you will be redirected to our home page`
			},
			CLOSE_PROJECT: {
				name: 'CLOSE_PROJECT',
				heading: 'close project',
				text: `are you sure you want to close the project? you will lose all your unsaved changes.`
			},
			SAVE_PROJECT: {
				name: 'SAVE_PROJECT',
				heading: 'save project',
				text: `do you want to save the changes you've made? your changes will be lost if you don't save them.`
			},
			SAVE_LANGUAGE: {
				name: 'SAVE_LANGUAGE',
				heading: 'save data model',
				text: `do you want to save the changes you've made? your changes will be lost if you don't save them.`
			},
			SAVE_WEBFORM: {
				name: 'SAVE_WEBFORM',
				heading: 'save webForm',
				text: `do you want to save the changes you've made? your changes will be lost if you don't save them.`
			},
			DELETE_FILE: {
				name: 'DELETE_FILE',
				heading: 'delete ',
				text: `are you sure you want to delete this file? the file will be permanently deleted from this project`
			},
			DELETE_FOLDER: {
				name: 'DELETE_FOLDER',
				heading: 'delete ',
				text: `are you sure you want to delete this folder? the folder will be permanently deleted from this project`
			},
			DELETE_PROPERTY: {
				name: 'DELETE_PROPERTY',
				heading: 'delete ',
				text: `are you sure you want to delete this property? any websites using the property will permanently lose the associated data`
			},
			PUBLISH_ALL: {
				name: 'PUBLISH_ALL',
				heading: 'publish to all',
				text: 'are you sure you want to publish the project for all your customers?'
			}
		},
		loadingText: {
			GENERAL_LOADING: {
				text: 'loading...'
			},
			LOAD_PROJECT: {
				text: 'loading  project...'
			},
			FETCH_PROJECTS: {
				text: 'fetching projects...'
			},
			PUBLISH_PROJECT: {
				text: 'publishing project...'
			},
			PUBLISH_ALL: {
				text: 'publishing to all...'
			},
			CREATE_PROJECT: {
				text: 'creating project...'
			},
			BUILD_PROJECT: {
				text: 'building project...'
			},
			SAVE_FILE: {
				text: 'saving file...'
			},
			UPLOAD_APPLICATION_ZIP_FILE: {
				text: 'uploading application zip file...'
			},
			UPLOAD_FILE: {
				text: 'uploading file...'
			},
			OPEN_FILE: {
				text: 'opening file...'
			},
			FETCH_LANGUAGE: {
				text: 'fetching data model...'
			},
			CREATE_LANGUAGE: {
				text: 'creating data model...'
			},
			OPEN_LANGUAGE: {
				text: 'opening data model...'
			},
			SAVE_LANGUAGE: {
				text: 'saving data model...'
			},
			OPEN_WEBFORM: {
				text: 'opening webform...'
			},
			SAVE_WEBFORM: {
				text: 'saving webform...'
			},
			STARTING_UPLOAD: {
				text: `starting your upload...`
			},
			USER_DETAILS: {
				text: 'fetching your details...'
			},
			FETCHING_DATATYPES: {
				text: 'fetching datatypes...'
			},
			RENAMING_FILE: {
				text: 'renaming file...'
			}
		},
		keepOnPage: function (e) {
			let message = `warning!\n\nnavigating away from the IDE, you might lose your un-saved changes. Please save before you exit.`;
			e.returnValue = message;
			return message;
		},
		sidebarItems: {
			right: {
			},
			left: {
				defaultKey: 'PROJECT_EXPLORER',
				PROJECT_EXPLORER: {
					key: 'PROJECT_EXPLORER',
					display: 'projects',
					buttonClass: 'project-explorer',
					component: ProjectTree
				},
				KITSUNE_SCHEMA: {
					key: 'KITSUNE_SCHEMA',
					display: 'data models',
					buttonClass: 'language-editor',
					component: SchemaKitsune
				},
				WEB_FORMS: {
					key: 'WEB_FORMS',
					display: 'webforms',
					buttonClass: 'webform-builder',
					component: WebForms
				},
				SHORTCUTS: {
					key: 'SHORTCUTS',
					component: Shortcuts
				}
			}
		},
		editorPlugins: {
			application: {
				types: [EDITOR_PLUGINS.JSON_EDITOR]
			},
			json: {
				types: [EDITOR_PLUGINS.JSON_EDITOR]
			},
			kitsune: {
				types: [EDITOR_PLUGINS.SCHEMA_VIEWER]
			}
		},
		footerTabs: {
			EVENT_LOG: 'EVENT_LOG',
			NOTIFICATION: 'NOTIFICATION'
		},
		actionPanelItems: {
			EDIT_TAB: 'EDIT_TAB',
			ADD_TAB: 'ADD_TAB'
		},
		widths: {
			sidebar: {
				width: 320,
				minwidth: 250
			},
			editor: {
				minwidth: 330
			},
			resizer: {
				width: 8
			}
		},
		heights: {
			header: {
				height: 45
			},
			playground: {
				minheight: 300
			},
			footer: {
				minheight: 150,
				height: 179,
				collapsedHeight: 27
			},
			resizer: {
				height: 8
			}
		},
		contextMenus: {
			project: 'menu_project',
			folder: 'menu_folder',
			pages: 'menu_pages',
			assets: 'menu_assets',
			language: 'menu_language',
			component: 'component_pages',
			webForm: 'module_webForm'
		},
		schemaConfig: {
			global: {
				"_kid": "hidden",
				"k_referenceid": "hidden",
				"createdon": "hidden",
				"updatedon": "hidden",
				"isarchived": "hidden"
			},
			base: {
				"userid": "hidden",
				"schemaid": "hidden",
				"websiteid": "hidden",
				"rootaliasurl": "hidden",
				"createdon": "hidden",
				"updatedon": "hidden",
				"isarchived": "hidden",
				"_kid": "hidden",
				"k_referenceid": "hidden"
			}
		},
		formBuilderOptions: {
			disableFields: [
				'autocomplete',
				'button',
				'file',
				'header',
				'hidden',
				'paragraph',
				'textarea',
				'phone'
			],
			dataType: 'json',
			disabledAttrs: [
				'access',
				'description',
				'inline',
				'multiple',
				'other',
				'placeholder',
				'rows',
				'step',
				'toggle',
				'value',
				'min',
				'className',
				'name'
			],
			i18n: {
				langs: ['en-US'],
				preloaded: {
					'en-US': {
						label: 'Display Name',
						required: 'required',
						subtype: 'type',
						dateField: 'date',
						addOption: 'add option',
						allFieldsRemoved: 'all fields were removed.',
						allowMultipleFiles: 'allow users to upload multiple files',
						autocomplete: 'autocomplete',
						button: 'button',
						cannotBeEmpty: 'this field cannot be empty',
						checkbox: 'checkbox',
						checkboxes: 'checkboxes',
						className: 'class',
						clearAllMessage: 'are you sure you want to clear all fields?',
						clear: 'reset',
						close: 'close',
						content: 'content',
						copy: 'copy to clipboard',
						copyButton: '&#43;',
						description: 'help text',
						descriptionField: 'description',
						devMode: 'developer mode',
						editNames: 'edit names',
						editorTitle: 'form elements',
						editXML: 'edit xmL',
						enableOther: 'enable &quot;Other&quot;',
						enableOtherMsg: 'let users to enter an unlisted option',
						fieldNonEditable: 'this field cannot be edited.',
						fieldRemoveWarning: 'are you sure you want to remove this field?',
						fileUpload: 'file upload',
						formUpdated: 'form updated',
						header: 'header',
						hidden: 'hidden input',
						inline: 'inline',
						inlineDesc: 'display {type} inline',
						labelEmpty: 'field Label cannot be empty',
						limitRole: 'limit access to one or more of the following roles:',
						mandatory: 'mandatory',
						maxlength: 'max length',
						minOptionMessage: 'this field requires a minimum of 2 options',
						minSelectionRequired: 'minimum {min} selections required',
						multipleFiles: 'multiple files',
						name: 'name',
						no: 'no',
						noFieldsToClear: 'there are no fields to clear',
						number: 'number',
						off: 'off',
						on: 'on',
						option: 'option',
						options: 'options',
						optional: 'optional',
						optionLabelPlaceholder: 'label',
						optionValuePlaceholder: 'value',
						optionEmpty: 'option value required',
						other: 'other',
						paragraph: 'paragraph',
						placeholder: 'Prop Name',
						'placeholder.value': 'value',
						'placeholder.label': 'label',
						'placeholder.text': '',
						'placeholder.textarea': '',
						'placeholder.email': 'enter you email',
						'placeholder.placeholder': '',
						'placeholder.className': 'space separated classes',
						'placeholder.password': 'enter your password',
						preview: 'preview',
						radioGroup: 'radio',
						radio: 'radio',
						removeOption: 'remove option',
						remove: '&#215',
						richText: 'rich Text editor',
						roles: 'access',
						rows: 'rows',
						save: 'save',
						selectOptions: 'options',
						select: 'select',
						selectColor: 'select color',
						selectionsMessage: 'allow Multiple selections',
						size: 'size',
						'size.xs': 'extra small',
						'size.sm': 'small',
						'size.m': 'default',
						'size.lg': 'large',
						style: 'style',
						'styles.btn.default': 'default',
						'styles.btn.danger': 'danger',
						'styles.btn.info': 'info',
						'styles.btn.primary': 'primary',
						'styles.btn.success': 'success',
						'styles.btn.warning': 'warning',
						text: 'text',
						textArea: 'text area',
						toggle: 'toggle',
						warning: 'warning!',
						value: 'value',
						viewJSON: '{  }',
						viewXML: '&lt;/&gt;',
						yes: 'Yes',
						checkboxGroup: 'checkbox',
						getStarted: 'start building this webform',
						copyButtonTooltip: 'duplicate',
						removeMessage: 'remove element',
						hide: 'edit'
					}
				}
			},
			showActionButtons: false,
			inputSets: [{
				label: 'phone',
				name: 'phone',
				icon: '<i class="fas fa-mobile fa-2x"></i>',
				fields: [{
					type: 'number',
					label: 'phone',
					className: 'form-control',
				}]
			}],
			controlOrder: [
				'text',
				'phone',
				'number',
				'date'
			],
		},
		buildErrors: {
			START_FAILED: {
				type: 'START_FAILED',
				message: 'failed to start build. please try again after some time.'
			},
			POLL_TIMEOUT: {
				type: 'POLL_TIMEOUT',
				message: 'your build request has timed out. please try again after some time.'
			},
			RETRY_TIMEOUT: {
				type: 'RETRY_TIMEOUT',
				message: 'something went wrong. please refresh in a while and try again.'
			},
			FAILED: {
				type: 'FAILED',
				message: 'build process has failed. please check event logs for errors.'
			}
		}
	},
	DATA: {
		newSchema: {
			"UserId": "",
			"Entity": {
				"EntityName": "SchemaName",
				"Type": 0,
				"Classes": [
					{
						"Name": "SchemaName",
						"Schemas": null,
						"Description": "This is the description of schema",
						"Id": "1",
						"IsCustom": false,
						"ClassType": 1,
						"PropertyList": [
						]
					}
				]
			}
		}
	}
};
//set extension mapper
config.INTERNAL_SETTINGS.extensionMap.set('html', 'kitsune');
config.INTERNAL_SETTINGS.extensionMap.set('zip', 'application');
config.INTERNAL_SETTINGS.extensionMap.set('css', 'css');
config.INTERNAL_SETTINGS.extensionMap.set('js', 'javascript');
config.INTERNAL_SETTINGS.extensionMap.set('scss', 'scss');
config.INTERNAL_SETTINGS.extensionMap.set('sass', 'sass');
config.INTERNAL_SETTINGS.extensionMap.set('png', 'image');
config.INTERNAL_SETTINGS.extensionMap.set('jpg', 'image');
config.INTERNAL_SETTINGS.extensionMap.set('jpeg', 'image');
config.INTERNAL_SETTINGS.extensionMap.set('gif', 'image');
config.INTERNAL_SETTINGS.extensionMap.set('webp', 'image');
config.INTERNAL_SETTINGS.extensionMap.set('bmp', 'image');
config.INTERNAL_SETTINGS.extensionMap.set('tiff', 'image');
config.INTERNAL_SETTINGS.extensionMap.set('ico', 'image');
config.INTERNAL_SETTINGS.extensionMap.set('svg', 'svg');
config.INTERNAL_SETTINGS.extensionMap.set('handlebars', 'javascript');
config.INTERNAL_SETTINGS.extensionMap.set('json', 'json');
config.INTERNAL_SETTINGS.extensionMap.set('less', 'less');
config.INTERNAL_SETTINGS.extensionMap.set('md', 'markdown');
config.INTERNAL_SETTINGS.extensionMap.set('xml', 'xml');
config.INTERNAL_SETTINGS.extensionMap.set('otf', 'font');
config.INTERNAL_SETTINGS.extensionMap.set('woff2', 'font');
config.INTERNAL_SETTINGS.extensionMap.set('ttf', 'font');
config.INTERNAL_SETTINGS.extensionMap.set('woff', 'font');
config.INTERNAL_SETTINGS.extensionMap.set('eot', 'font');
config.INTERNAL_SETTINGS.extensionMap.set('pdf', 'pdf');
//set icon mapper
config.INTERNAL_SETTINGS.extensionsIconMap.set('folder', 'fas fa-folder');
config.INTERNAL_SETTINGS.extensionsIconMap.set('folderOpen', 'fas fa-folder-open');
config.INTERNAL_SETTINGS.extensionsIconMap.set('css', 'fab fa-css3');
config.INTERNAL_SETTINGS.extensionsIconMap.set('kitsune', 'fas fa-code');
config.INTERNAL_SETTINGS.extensionsIconMap.set('application', 'fab fa-docker');
config.INTERNAL_SETTINGS.extensionsIconMap.set('javascript', 'fab fa-js-square');
config.INTERNAL_SETTINGS.extensionsIconMap.set('scss', 'fab fa-css3-alt');
config.INTERNAL_SETTINGS.extensionsIconMap.set('sass', 'fab fa-sass');
config.INTERNAL_SETTINGS.extensionsIconMap.set('image', 'fas fa-image');
config.INTERNAL_SETTINGS.extensionsIconMap.set('svg', 'fas fa-file-image');
config.INTERNAL_SETTINGS.extensionsIconMap.set('json', 'fas fa-cogs');
config.INTERNAL_SETTINGS.extensionsIconMap.set('less', 'fab fa-less');
config.INTERNAL_SETTINGS.extensionsIconMap.set('md', 'fas fa-file');
config.INTERNAL_SETTINGS.extensionsIconMap.set('xml', 'fas fa-code');
config.INTERNAL_SETTINGS.extensionsIconMap.set('font', 'fas fa-font');
config.INTERNAL_SETTINGS.extensionsIconMap.set('pdf', 'fas fa-file-pdf');