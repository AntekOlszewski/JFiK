grammar yyz;

program: functionDefinition* line* EOF;
print: expression PRINT_OPERATOR;
read: IDENTIFIER READ_OPERATOR;
line: statement | ifBlock | whileBlock | print | read;
statement: (assignment | functionCall) ';';
ifBlock: IF '(' expression ')' block;
whileBlock: WHILE '(' expression ')' block;
functionDefinition: IDENTIFIER '(' (IDENTIFIER (',' IDENTIFIER)*)? ')' '=' functionBody;
functionBody: '{' line* (RETURN expression)? '}';
block: '{' line* '}';
assignment: GLOBAL? (CONST | TYPED)? IDENTIFIER '=' expression;
functionCall: IDENTIFIER '(' (expression (',' expression)*)? ')';
expression
    : constant                                              #constantExpression   
    | IDENTIFIER                                            #identifierExpression
    | functionCall                                          #functionCallExpression
    | '(' expression ')'                                    #bracketExpression
    | expression multiplicativeOperation expression         #multiplicativeExpression
    | expression additiveOperation expression               #additiveExpression
    | expression compareOperation expression                #compareExpression
    | negationOperation expression                          #negationExpression
    | expression booleanOperation expression                #booleanExpression
    ;

multiplicativeOperation: MULTIPLICATIVE_OPERATOR;
additiveOperation: ADDITIVE_OPERATOR;
compareOperation: COMPARE_OPERATOR;
booleanOperation: BOOL_OPERATOR;
negationOperation: NEGATION_OPERATOR;

constant: INTEGER | DOUBLE | STRING | BOOL | NULL;
COMMENT: '//' ~('\r' | '\n')* NL -> skip;
MULTILINE_COMMENT: '/*' .* '*/';
NL: '\r'? '\n' | '\r';
WS: [ \t\r\n]+ -> skip;
IF: 'if';
ELSE: 'else';
WHILE: 'while';

PRINT_OPERATOR: '>>';
READ_OPERATOR: '<<';
MULTIPLICATIVE_OPERATOR: '*' | '/';
ADDITIVE_OPERATOR: '+' | '-';
COMPARE_OPERATOR: '==' | '!=' | '>' | '>=' | '<' | '<=';
NEGATION_OPERATOR: 'NEG';
BOOL_OPERATOR: 'AND' | 'OR' | 'XOR';


BOOL: 'true' | 'false';
INTEGER: '-'?[0-9]+;
DOUBLE: '-'?[0-9]+'.'[0-9]+;
STRING: ('"' ~'"'* '"') | ('\'' ~'\''* '\'');
NULL: 'null';
CONST: 'const';
TYPED: 'typed';
GLOBAL: 'global';


RETURN: 'return';
IDENTIFIER: [a-zA-Z][a-zA-Z0-9]*;