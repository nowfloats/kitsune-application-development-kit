import ace from 'brace';

var oop = ace.acequire("ace/lib/oop");
var HtmlHighlightRules = ace.acequire("ace/mode/html_highlight_rules").HtmlHighlightRules;


var KitsuneHighlightRules = function() {
	this.$rules = new HtmlHighlightRules().getRules();

	this.$rules.kitsune_braces = [{
		token : "k-attribute.xml",
		regex : "\\[\\[",
		push : [
			{ token : "k-attribute.xml", regex: "\\]\\]", next: "pop" },
			{ include : "attr_reference" },
			{ defaultToken : "k-attribute.xml" }
		]
	}];
	this.$rules.kitsune_tags = [
		{ token : "k-tag.xml", regex : "k\\-\\w+" }
	];

	this.$rules.tag_stuff.unshift({ include: "kitsune_tags" });
	this.$rules.start.unshift({ include : "kitsune_braces" });
	this.$rules["string.attribute-value.xml"].push({ include : "kitsune_braces" });
	this.$rules["string.attribute-value.xml0"].push({ include : "kitsune_braces" });

	if (this.constructor === KitsuneHighlightRules)
		this.normalizeRules();

};

oop.inherits(KitsuneHighlightRules, HtmlHighlightRules);

exports.KitsuneHighlightRules = KitsuneHighlightRules;
