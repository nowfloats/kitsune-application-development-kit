import ace from 'brace';
import KitsuneCompletions from './kitsuneCompletions';
import _ from 'lodash';

ace.define("ace/mode/kitsune",["require","exports","module","ace/lib/oop","ace/mode/html","ace/mode/text",
	"ace/mode/javascript","ace/mode/css","ace/mode/kitsune_highlight_rules","ace/mode/behaviour/xml",
	"ace/mode/folding/html","ace/mode/html_completions","ace/worker/worker_client"],
	function(acequire, exports, module) {
		"use strict";

		var oop = acequire("ace/lib/oop");
		var lang = acequire("ace/lib/lang");
		var TextMode = acequire("ace/mode/text").Mode;
		var JavaScriptMode = acequire("ace/mode/javascript").Mode;
		var CssMode = acequire("ace/mode/css").Mode;
		var KitsuneHighlightRules = require("./kitsuneHighlightRules").KitsuneHighlightRules;
		var XmlBehaviour = acequire("ace/mode/behaviour/xml").XmlBehaviour;
		var HtmlFoldMode = acequire("ace/mode/folding/html").FoldMode;
		var WorkerClient = acequire("ace/worker/worker_client").WorkerClient;
		var voidElements = ["area", "base", "br", "col", "embed", "hr", "img", "input", "keygen", "link", "meta",
			"menuitem", "param", "source", "track", "wbr"];
		var optionalEndTags = ["li", "dt", "dd", "p", "rt", "rp", "optgroup", "option", "colgroup", "td", "th"];

		var Mode = function(options) {
			this.fragmentContext = options && options.fragmentContext;
			this.HighlightRules = KitsuneHighlightRules;
			this.$behaviour = new XmlBehaviour();
			this.$completer = new KitsuneCompletions();

			this.createModeDelegates({
				"js-": JavaScriptMode,
				"css-": CssMode
			});

			this.foldingRules = new HtmlFoldMode(this.voidElements, lang.arrayToMap(optionalEndTags));
		};
		oop.inherits(Mode, TextMode);

		(function() {

			this.blockComment = { start: "<!--", end: "-->" };

			this.voidElements = lang.arrayToMap(voidElements);

			this.getNextLineIndent = function(state, line, tab) {
				return this.$getIndent(line);
			};

			this.checkOutdent = function(state, line, input) {
				return false;
			};

			this.getCompletions = function(state, session, pos, prefix) {
				return this.$completer.getCompletions(state, session, pos, prefix);
			};

			this.createWorker = function(session) {
				if (this.constructor != Mode)
					return;
				var worker = new WorkerClient(["ace"], require("brace/worker/html"), "Worker");
				worker.attachToDocument(session.getDocument());

				if (this.fragmentContext)
					worker.call("setOptions", [{ context: this.fragmentContext }]);

				worker.on("error", function(e) {
					var filteredAnnotations = _.reject(e.data, ({ text }) => (text === 'Named entity expected. Got none.' ||
					text === 'Unexpected end tag (head). Ignored.' || text === 'Unexpected start tag (body).'));
					session.setAnnotations(filteredAnnotations);
				});

				worker.on("terminate", function() {
					session.clearAnnotations();
				});

				return worker;
			};

			this.$id = "ace/mode/kitsune";
		}).call(Mode.prototype);

		exports.Mode = Mode;
	});
