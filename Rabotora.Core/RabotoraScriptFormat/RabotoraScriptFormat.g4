grammar RabotoraScriptFormat;

Whitespace: [{White_Space}] -> skip;
LineComment: '//' .*? '\r'? '\n' -> skip;
Comment: '/*' .*? '*/' -> skip;
WS: (' ' | '\t' | '\r' | '\n') -> skip;

LParen: '(';
RParen: ')';
LBrace: '{';
RBrace: '}';
QttMrk: '\'';
QttMrk2: '"';
Comma: ',';
Semicolon: ';';
QuesMark: '?';
ClassKeyWord: 'class';
ActionKeyWord: 'void';
PrivateKeyWord: 'private';
PublicKeyWord: 'public';
InternalKeyWord: 'internal';
StaticKeyWord: 'static';

fragment ALPHABET: [a-zA-Z];
fragment UNDERLINE: '_';
fragment DIGITSTART: [1-9];
fragment ZERO: '0';
fragment DIGIT: DIGITSTART | ZERO;
fragment DOT: '.';
fragment NEGATIVE: '-';
Int: NEGATIVE? (ZERO | DIGITSTART DIGIT*);
Float: Int DOT DIGIT+;
Word: UNDERLINE? ALPHABET (ALPHABET | DIGIT)+;
PlainWord: UNDERLINE? ALPHABET (ALPHABET | DIGIT)+ (ALPHABET | DIGIT | Whitespace)+ (ALPHABET | DIGIT);
FuncWord: Word (DOT Word)*;

plainString: (QttMrk PlainWord QttMrk) | (QttMrk2 PlainWord QttMrk2);
value: plainString | Word | Int | Float;
values: LParen (value (Comma value)*)? RParen;
cmd: FuncWord values segment? Semicolon;
segment: LBrace segmentBody RBrace;
segmentBody: cmd*;
typeDef: Word QuesMark? Word;
typeDefs: LParen (typeDef (Comma typeDef)*)? RParen;
classDef: (PublicKeyWord | PrivateKeyWord | InternalKeyWord)? StaticKeyWord? ClassKeyWord Word;
funcDef: (PublicKeyWord | PrivateKeyWord | InternalKeyWord)? StaticKeyWord? (Word | ActionKeyWord) Word typeDefs;
classSegment: classDef segment;
funcSegment: funcDef segment;
body: (classSegment | funcSegment)+;
script: body EOF;