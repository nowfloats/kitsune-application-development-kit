let devRules = {
	"indent"               : ["error", "tab"],
	"key-spacing"          : 0,
	"max-len"              : [2, 120, 2],
	"object-curly-spacing" : [2, "always"],
};

let prodRules = Object.assign(devRules, { "no-console": 0,  "no-debugger": 0 });
let rules = process.env.NODE_ENV === 'production' ? prodRules : devRules;

module.exports = {
	//TODO: Add vuejs eslint plugin
	root: true,
	parser: 'babel-eslint',
	parserOptions: {
		sourceType: 'module'
	},
	env: {
		browser: true,
	},
	// https://github.com/feross/standard/blob/master/RULES.md#javascript-standard-style
	extends: [
		"standard",
		"eslint:recommended"
	],
	// required to lint *.vue files
	plugins: [
		'html'
	],
	// add your custom rules here
	'rules': rules
}
