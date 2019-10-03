/* eslint-disable max-len */
import ace from 'brace';

ace.define("ace/theme/kitsuneTheme",["require","exports","module","ace/lib/dom"], function(acequire, exports, module) {

	exports.isDark = true;
	exports.cssClass = "kitsuneTheme";
	exports.cssText = '.kitsuneTheme .ace_gutter {\
	background: #2F3129;\
	color: #8F908A\
	}\
	.kitsuneTheme .ace_print-margin {\
	width: 1px;\
	background: #555651\
	}\
	.kitsuneTheme {\
	background-color: #272822;\
	color: #F8F8F2\
	}\
	.kitsuneTheme .ace_cursor {\
	color: #F8F8F0\
	}\
	.kitsuneTheme .ace_marker-layer .ace_selection {\
	background: #49483E\
	}\
	.kitsuneTheme.ace_multiselect .ace_selection.ace_start {\
	box-shadow: 0 0 3px 0px #272822;\
	}\
	.kitsuneTheme .ace_marker-layer .ace_step {\
	background: rgb(102, 82, 0)\
	}\
	.kitsuneTheme .ace_marker-layer .ace_bracket {\
	margin: -1px 0 0 -1px;\
	border: 1px solid #49483E\
	}\
	.kitsuneTheme .ace_marker-layer .ace_active-line {\
	background: #202020\
	}\
	.kitsuneTheme .ace_gutter-active-line {\
	background-color: #272727\
	}\
	.kitsuneTheme .ace_marker-layer .ace_selected-word {\
	border: 1px solid #49483E\
	}\
	.kitsuneTheme .ace_invisible {\
	color: #52524d\
	}\
	.kitsuneTheme .ace_entity.ace_name.ace_tag,\
	.kitsuneTheme .ace_keyword,\
	.kitsuneTheme .ace_meta.ace_tag,\
	.kitsuneTheme .ace_storage {\
	color: #F92672\
	}\
	.kitsuneTheme .ace_punctuation,\
	.kitsuneTheme .ace_punctuation.ace_tag {\
	color: #fff\
	}\
	.kitsuneTheme .ace_constant.ace_character,\
	.kitsuneTheme .ace_constant.ace_language,\
	.kitsuneTheme .ace_constant.ace_numeric,\
	.kitsuneTheme .ace_constant.ace_other {\
	color: #AE81FF\
	}\
	.kitsuneTheme .ace_invalid {\
	color: #F8F8F0;\
	background-color: #F92672\
	}\
	.kitsuneTheme .ace_invalid.ace_deprecated {\
	color: #F8F8F0;\
	background-color: #AE81FF\
	}\
	.kitsuneTheme .ace_support.ace_constant,\
	.kitsuneTheme .ace_support.ace_function {\
	color: #66D9EF\
	}\
	.kitsuneTheme .ace_fold {\
	background-color: #A6E22E;\
	border-color: #F8F8F2\
	}\
	.kitsuneTheme .ace_storage.ace_type,\
	.kitsuneTheme .ace_support.ace_class,\
	.kitsuneTheme .ace_support.ace_type {\
	font-style: italic;\
	color: #66D9EF\
	}\
	.kitsuneTheme .ace_entity.ace_name.ace_function,\
	.kitsuneTheme .ace_entity.ace_other,\
	.kitsuneTheme .ace_entity.ace_other.ace_attribute-name,\
	.kitsuneTheme .ace_variable {\
	color: #A6E22E\
	}\
	.kitsuneTheme .ace_variable.ace_parameter {\
	font-style: italic;\
	color: #FD971F\
	}\
	.kitsuneTheme .ace_string {\
	color: #E6DB74\
	}\
	.kitsuneTheme .ace_comment {\
	color: #75715E\
	}\
	.kitsuneTheme .ace_k-attribute{\
	font-style: bold;\
	color: #F06428\
	}\
	.kitsuneTheme .ace_k-tag{\
	font-style: bold;\
	color: #6fb4ff\
	}\
	.kitsuneTheme .ace_indent-guide {\
	background: url(data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAACCAYAAACZgbYnAAAAEklEQVQImWPQ0FD0ZXBzd/wPAAjVAoxeSgNeAAAAAElFTkSuQmCC) right repeat-y\
	}';

	var dom = acequire("../lib/dom");
	dom.importCssString(exports.cssText, exports.cssClass);
});
