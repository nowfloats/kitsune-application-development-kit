grammar KitsuneGrammer;

/*
 * Parser Rules
 */
 prog: expr+ ;
 
expr :  '-' expr																			#NegativeNumber
     | '(' expr ')'																			#Parens
	 | expr '.' op=('length'|'substr'|'replace' | 'urlencode' | 'distinct' | 'tolower' | 'toupper' | 'split' | 'indexof' | 'contains' | 'SubStr' | 'Length' | 'decode' | 'find' | 'tostring' | 'tonumber' ) '(' (expr ( ',' expr)* )* ')'				#PostfixUnaryWithArg
	 | expr '.' op=('offset' | 'currentpagenumber' | 'nextpage' | 'nextpage.url' | 'prevpage' | 'previouspage.url' | 'pagesize' | 'firstpage' | 'lastpage' )	#ViewProperties
	 | expr '.' op=('geturl' | 'setobject') '(' (expr ( ',' expr)* ) ')'					#PostfixUnaryView
	 | op=('length' | 'len') '(' expr ( ',' expr)*  ')'										#PrefixUnaryFunction
	 | PREFIXUNARY expr																		#PrefixUnary
	 | expr op=('*'|'/'|'%') expr															#MulDiv
     | expr op=('+'|'-') expr																#AddSub
	 | expr op=('=='|'!='|'>'|'<'|'>='|'<=') expr											#Conditional
	 | expr op=('&&'|'||') expr																#Conjunctional
     | OPERAND	( '.' OPERAND  )*															#Operand
     | expr '?' expr ':' expr																#Ternary
	 | expr ',' expr ',' expr ':' expr														#ForLoop
	 | expr 'in' expr																		#ForEachLoop
	 | expr (',' expr)* 'in' expr															#KObject
	 | ('[' expr (',' expr)* ']' | '['']')													#Array
	 | ('view'| 'View' )  '(' expr ')'														#ViewHandle 
	 | OPERAND  '.' expr																	#TransformedViewHandle
     ;

/*
 * Lexer Rules
INT : [0-9]+;
MUL : '*';
DIV : '/';
ADD : '+';
SUB : '-';
*/
BINARYOPERATOR : ([*/+-] | '==' | '!=' | '<' | '>' | '<=' | '>=' | '&&' | '||' | ',');
PREFIXUNARY : '!';
OPERAND : ([a-zA-Z_][a-zA-Z0-9_[\]]* | [0-9]+ | [0-9]*[.][0-9]+ | '\'' ~('\'')* '\'' );
WS
:	' ' -> channel(HIDDEN)
;
