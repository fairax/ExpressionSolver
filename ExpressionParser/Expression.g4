grammar Expression;

/*
 * Parser Rules
 */

logicalVariable              :  A | B | C;
							 
simpleLogicalExpression      : logicalVariable | LPAREN logicalExpression RPAREN;
							 
logicalNotExpression         : NOT simpleLogicalExpression;
						     
unaryLogicalExpression       : logicalNotExpression | simpleLogicalExpression;
					         
logicalAndExpression         : unaryLogicalExpression AND (unaryLogicalExpression | logicalAndExpression);
					         
logicalOrExpression          : unaryLogicalExpression OR logicalExpression;
					         
logicalExpression            : unaryLogicalExpression | logicalAndExpression | logicalOrExpression;
					         
caseExpression               : H EQ M | H EQ P | H EQ T;
					         
caseConditionExpression      : logicalExpression ARROW caseExpression;
						     
calculationVariable          : D | E | F;

decimalCalculationExpression : DECIMAL;
							
simpleCalculationExpression	 : decimalCalculationExpression | calculationVariable | LPAREN calculationExpression RPAREN;
					    	 
unaryMinusExpression         : MINUS simpleCalculationExpression;
					         
unaryPlusExpression          : PLUS simpleCalculationExpression;
						     
unaryCalculationExpression   : simpleCalculationExpression | unaryMinusExpression | unaryPlusExpression;
					         
summationExpression          : priorityCalculationExpression PLUS calculationExpression;
					         
substractionExpression       : priorityCalculationExpression MINUS calculationExpression;
						     
multiplicationExpression     : unaryCalculationExpression MULT priorityCalculationExpression;
						     
divisionExpression           : unaryCalculationExpression DIV  priorityCalculationExpression;

priorityCalculationExpression: unaryCalculationExpression | multiplicationExpression | divisionExpression;
							 
calculationExpression        : summationExpression |  substractionExpression | priorityCalculationExpression;
							 
caseActionExpression         : caseExpression ARROW K EQ calculationExpression;
							 
input                        : (caseConditionExpression | caseActionExpression) EOF;

/*
 * Lexer Rules
 */
DECIMAL      :  ('0'..'9')+  { Text.Length <= 28 }? ('.' ('0'..'9')+)?;
A            : 'A' ;
B            : 'B' ;
C            : 'C' ;
D            : 'D' ;
E            : 'E' ;
F            : 'F' ;
H            : 'H' ;
K            : 'K' ;
M            : 'M' ;
P            : 'P' ;
T            : 'T' ;
AND          : '&' '&';
OR           : '|' '|';
NOT          : '!' ;
EQ           : '=' ;
LPAREN       : '(';
RPAREN       : ')';
PLUS         : '+';
MINUS        : '-';
MULT         : '*';
DIV          : '/';
POINT        : '.';
ARROW        : '=' '>';
WHITESPACE   : [ \n\t\r]+ -> skip ;
ErrChar      : .;