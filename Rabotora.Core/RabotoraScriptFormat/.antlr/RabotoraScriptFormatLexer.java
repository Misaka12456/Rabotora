// Generated from f:\C#\Misaka12456 (Misaka Castle)\Rabotora\Rabotora.Core\RabotoraScriptFormat\RabotoraScriptFormat.g4 by ANTLR 4.8
import org.antlr.v4.runtime.Lexer;
import org.antlr.v4.runtime.CharStream;
import org.antlr.v4.runtime.Token;
import org.antlr.v4.runtime.TokenStream;
import org.antlr.v4.runtime.*;
import org.antlr.v4.runtime.atn.*;
import org.antlr.v4.runtime.dfa.DFA;
import org.antlr.v4.runtime.misc.*;

@SuppressWarnings({"all", "warnings", "unchecked", "unused", "cast"})
public class RabotoraScriptFormatLexer extends Lexer {
	static { RuntimeMetaData.checkVersion("4.8", RuntimeMetaData.VERSION); }

	protected static final DFA[] _decisionToDFA;
	protected static final PredictionContextCache _sharedContextCache =
		new PredictionContextCache();
	public static final int
		Whitespace=1, LineComment=2, Comment=3, WS=4, LParen=5, RParen=6, LBrace=7, 
		RBrace=8, QttMrk=9, QttMrk2=10, Comma=11, Semicolon=12, QuesMark=13, ClassKeyWord=14, 
		ActionKeyWord=15, PrivateKeyWord=16, PublicKeyWord=17, InternalKeyWord=18, 
		StaticKeyWord=19, Int=20, Float=21, Word=22, PlainWord=23, FuncWord=24;
	public static String[] channelNames = {
		"DEFAULT_TOKEN_CHANNEL", "HIDDEN"
	};

	public static String[] modeNames = {
		"DEFAULT_MODE"
	};

	private static String[] makeRuleNames() {
		return new String[] {
			"Whitespace", "LineComment", "Comment", "WS", "LParen", "RParen", "LBrace", 
			"RBrace", "QttMrk", "QttMrk2", "Comma", "Semicolon", "QuesMark", "ClassKeyWord", 
			"ActionKeyWord", "PrivateKeyWord", "PublicKeyWord", "InternalKeyWord", 
			"StaticKeyWord", "ALPHABET", "UNDERLINE", "DIGITSTART", "ZERO", "DIGIT", 
			"DOT", "NEGATIVE", "Int", "Float", "Word", "PlainWord", "FuncWord"
		};
	}
	public static final String[] ruleNames = makeRuleNames();

	private static String[] makeLiteralNames() {
		return new String[] {
			null, null, null, null, null, "'('", "')'", "'{'", "'}'", "'''", "'\"'", 
			"','", "';'", "'?'", "'class'", "'void'", "'private'", "'public'", "'internal'", 
			"'static'"
		};
	}
	private static final String[] _LITERAL_NAMES = makeLiteralNames();
	private static String[] makeSymbolicNames() {
		return new String[] {
			null, "Whitespace", "LineComment", "Comment", "WS", "LParen", "RParen", 
			"LBrace", "RBrace", "QttMrk", "QttMrk2", "Comma", "Semicolon", "QuesMark", 
			"ClassKeyWord", "ActionKeyWord", "PrivateKeyWord", "PublicKeyWord", "InternalKeyWord", 
			"StaticKeyWord", "Int", "Float", "Word", "PlainWord", "FuncWord"
		};
	}
	private static final String[] _SYMBOLIC_NAMES = makeSymbolicNames();
	public static final Vocabulary VOCABULARY = new VocabularyImpl(_LITERAL_NAMES, _SYMBOLIC_NAMES);

	/**
	 * @deprecated Use {@link #VOCABULARY} instead.
	 */
	@Deprecated
	public static final String[] tokenNames;
	static {
		tokenNames = new String[_SYMBOLIC_NAMES.length];
		for (int i = 0; i < tokenNames.length; i++) {
			tokenNames[i] = VOCABULARY.getLiteralName(i);
			if (tokenNames[i] == null) {
				tokenNames[i] = VOCABULARY.getSymbolicName(i);
			}

			if (tokenNames[i] == null) {
				tokenNames[i] = "<INVALID>";
			}
		}
	}

	@Override
	@Deprecated
	public String[] getTokenNames() {
		return tokenNames;
	}

	@Override

	public Vocabulary getVocabulary() {
		return VOCABULARY;
	}


	public RabotoraScriptFormatLexer(CharStream input) {
		super(input);
		_interp = new LexerATNSimulator(this,_ATN,_decisionToDFA,_sharedContextCache);
	}

	@Override
	public String getGrammarFileName() { return "RabotoraScriptFormat.g4"; }

	@Override
	public String[] getRuleNames() { return ruleNames; }

	@Override
	public String getSerializedATN() { return _serializedATN; }

	@Override
	public String[] getChannelNames() { return channelNames; }

	@Override
	public String[] getModeNames() { return modeNames; }

	@Override
	public ATN getATN() { return _ATN; }

	public static final String _serializedATN =
		"\3\u608b\ua72a\u8133\ub9ed\u417c\u3be7\u7786\u5964\2\32\u00ef\b\1\4\2"+
		"\t\2\4\3\t\3\4\4\t\4\4\5\t\5\4\6\t\6\4\7\t\7\4\b\t\b\4\t\t\t\4\n\t\n\4"+
		"\13\t\13\4\f\t\f\4\r\t\r\4\16\t\16\4\17\t\17\4\20\t\20\4\21\t\21\4\22"+
		"\t\22\4\23\t\23\4\24\t\24\4\25\t\25\4\26\t\26\4\27\t\27\4\30\t\30\4\31"+
		"\t\31\4\32\t\32\4\33\t\33\4\34\t\34\4\35\t\35\4\36\t\36\4\37\t\37\4 \t"+
		" \3\2\3\2\3\2\3\2\3\3\3\3\3\3\3\3\7\3J\n\3\f\3\16\3M\13\3\3\3\5\3P\n\3"+
		"\3\3\3\3\3\3\3\3\3\4\3\4\3\4\3\4\7\4Z\n\4\f\4\16\4]\13\4\3\4\3\4\3\4\3"+
		"\4\3\4\3\5\3\5\3\5\3\5\3\6\3\6\3\7\3\7\3\b\3\b\3\t\3\t\3\n\3\n\3\13\3"+
		"\13\3\f\3\f\3\r\3\r\3\16\3\16\3\17\3\17\3\17\3\17\3\17\3\17\3\20\3\20"+
		"\3\20\3\20\3\20\3\21\3\21\3\21\3\21\3\21\3\21\3\21\3\21\3\22\3\22\3\22"+
		"\3\22\3\22\3\22\3\22\3\23\3\23\3\23\3\23\3\23\3\23\3\23\3\23\3\23\3\24"+
		"\3\24\3\24\3\24\3\24\3\24\3\24\3\25\3\25\3\26\3\26\3\27\3\27\3\30\3\30"+
		"\3\31\3\31\5\31\u00ae\n\31\3\32\3\32\3\33\3\33\3\34\5\34\u00b5\n\34\3"+
		"\34\3\34\3\34\7\34\u00ba\n\34\f\34\16\34\u00bd\13\34\5\34\u00bf\n\34\3"+
		"\35\3\35\3\35\6\35\u00c4\n\35\r\35\16\35\u00c5\3\36\5\36\u00c9\n\36\3"+
		"\36\3\36\3\36\6\36\u00ce\n\36\r\36\16\36\u00cf\3\37\5\37\u00d3\n\37\3"+
		"\37\3\37\3\37\6\37\u00d8\n\37\r\37\16\37\u00d9\3\37\3\37\3\37\6\37\u00df"+
		"\n\37\r\37\16\37\u00e0\3\37\3\37\5\37\u00e5\n\37\3 \3 \3 \3 \7 \u00eb"+
		"\n \f \16 \u00ee\13 \4K[\2!\3\3\5\4\7\5\t\6\13\7\r\b\17\t\21\n\23\13\25"+
		"\f\27\r\31\16\33\17\35\20\37\21!\22#\23%\24\'\25)\2+\2-\2/\2\61\2\63\2"+
		"\65\2\67\269\27;\30=\31?\32\3\2\6\f\2\13\17\"\"\u0087\u0087\u00a2\u00a2"+
		"\u1682\u1682\u2002\u200c\u202a\u202b\u2031\u2031\u2061\u2061\u3002\u3002"+
		"\5\2\13\f\17\17\"\"\4\2C\\c|\3\2\63;\2\u00fa\2\3\3\2\2\2\2\5\3\2\2\2\2"+
		"\7\3\2\2\2\2\t\3\2\2\2\2\13\3\2\2\2\2\r\3\2\2\2\2\17\3\2\2\2\2\21\3\2"+
		"\2\2\2\23\3\2\2\2\2\25\3\2\2\2\2\27\3\2\2\2\2\31\3\2\2\2\2\33\3\2\2\2"+
		"\2\35\3\2\2\2\2\37\3\2\2\2\2!\3\2\2\2\2#\3\2\2\2\2%\3\2\2\2\2\'\3\2\2"+
		"\2\2\67\3\2\2\2\29\3\2\2\2\2;\3\2\2\2\2=\3\2\2\2\2?\3\2\2\2\3A\3\2\2\2"+
		"\5E\3\2\2\2\7U\3\2\2\2\tc\3\2\2\2\13g\3\2\2\2\ri\3\2\2\2\17k\3\2\2\2\21"+
		"m\3\2\2\2\23o\3\2\2\2\25q\3\2\2\2\27s\3\2\2\2\31u\3\2\2\2\33w\3\2\2\2"+
		"\35y\3\2\2\2\37\177\3\2\2\2!\u0084\3\2\2\2#\u008c\3\2\2\2%\u0093\3\2\2"+
		"\2\'\u009c\3\2\2\2)\u00a3\3\2\2\2+\u00a5\3\2\2\2-\u00a7\3\2\2\2/\u00a9"+
		"\3\2\2\2\61\u00ad\3\2\2\2\63\u00af\3\2\2\2\65\u00b1\3\2\2\2\67\u00b4\3"+
		"\2\2\29\u00c0\3\2\2\2;\u00c8\3\2\2\2=\u00d2\3\2\2\2?\u00e6\3\2\2\2AB\t"+
		"\2\2\2BC\3\2\2\2CD\b\2\2\2D\4\3\2\2\2EF\7\61\2\2FG\7\61\2\2GK\3\2\2\2"+
		"HJ\13\2\2\2IH\3\2\2\2JM\3\2\2\2KL\3\2\2\2KI\3\2\2\2LO\3\2\2\2MK\3\2\2"+
		"\2NP\7\17\2\2ON\3\2\2\2OP\3\2\2\2PQ\3\2\2\2QR\7\f\2\2RS\3\2\2\2ST\b\3"+
		"\2\2T\6\3\2\2\2UV\7\61\2\2VW\7,\2\2W[\3\2\2\2XZ\13\2\2\2YX\3\2\2\2Z]\3"+
		"\2\2\2[\\\3\2\2\2[Y\3\2\2\2\\^\3\2\2\2][\3\2\2\2^_\7,\2\2_`\7\61\2\2`"+
		"a\3\2\2\2ab\b\4\2\2b\b\3\2\2\2cd\t\3\2\2de\3\2\2\2ef\b\5\2\2f\n\3\2\2"+
		"\2gh\7*\2\2h\f\3\2\2\2ij\7+\2\2j\16\3\2\2\2kl\7}\2\2l\20\3\2\2\2mn\7\177"+
		"\2\2n\22\3\2\2\2op\7)\2\2p\24\3\2\2\2qr\7$\2\2r\26\3\2\2\2st\7.\2\2t\30"+
		"\3\2\2\2uv\7=\2\2v\32\3\2\2\2wx\7A\2\2x\34\3\2\2\2yz\7e\2\2z{\7n\2\2{"+
		"|\7c\2\2|}\7u\2\2}~\7u\2\2~\36\3\2\2\2\177\u0080\7x\2\2\u0080\u0081\7"+
		"q\2\2\u0081\u0082\7k\2\2\u0082\u0083\7f\2\2\u0083 \3\2\2\2\u0084\u0085"+
		"\7r\2\2\u0085\u0086\7t\2\2\u0086\u0087\7k\2\2\u0087\u0088\7x\2\2\u0088"+
		"\u0089\7c\2\2\u0089\u008a\7v\2\2\u008a\u008b\7g\2\2\u008b\"\3\2\2\2\u008c"+
		"\u008d\7r\2\2\u008d\u008e\7w\2\2\u008e\u008f\7d\2\2\u008f\u0090\7n\2\2"+
		"\u0090\u0091\7k\2\2\u0091\u0092\7e\2\2\u0092$\3\2\2\2\u0093\u0094\7k\2"+
		"\2\u0094\u0095\7p\2\2\u0095\u0096\7v\2\2\u0096\u0097\7g\2\2\u0097\u0098"+
		"\7t\2\2\u0098\u0099\7p\2\2\u0099\u009a\7c\2\2\u009a\u009b\7n\2\2\u009b"+
		"&\3\2\2\2\u009c\u009d\7u\2\2\u009d\u009e\7v\2\2\u009e\u009f\7c\2\2\u009f"+
		"\u00a0\7v\2\2\u00a0\u00a1\7k\2\2\u00a1\u00a2\7e\2\2\u00a2(\3\2\2\2\u00a3"+
		"\u00a4\t\4\2\2\u00a4*\3\2\2\2\u00a5\u00a6\7a\2\2\u00a6,\3\2\2\2\u00a7"+
		"\u00a8\t\5\2\2\u00a8.\3\2\2\2\u00a9\u00aa\7\62\2\2\u00aa\60\3\2\2\2\u00ab"+
		"\u00ae\5-\27\2\u00ac\u00ae\5/\30\2\u00ad\u00ab\3\2\2\2\u00ad\u00ac\3\2"+
		"\2\2\u00ae\62\3\2\2\2\u00af\u00b0\7\60\2\2\u00b0\64\3\2\2\2\u00b1\u00b2"+
		"\7/\2\2\u00b2\66\3\2\2\2\u00b3\u00b5\5\65\33\2\u00b4\u00b3\3\2\2\2\u00b4"+
		"\u00b5\3\2\2\2\u00b5\u00be\3\2\2\2\u00b6\u00bf\5/\30\2\u00b7\u00bb\5-"+
		"\27\2\u00b8\u00ba\5\61\31\2\u00b9\u00b8\3\2\2\2\u00ba\u00bd\3\2\2\2\u00bb"+
		"\u00b9\3\2\2\2\u00bb\u00bc\3\2\2\2\u00bc\u00bf\3\2\2\2\u00bd\u00bb\3\2"+
		"\2\2\u00be\u00b6\3\2\2\2\u00be\u00b7\3\2\2\2\u00bf8\3\2\2\2\u00c0\u00c1"+
		"\5\67\34\2\u00c1\u00c3\5\63\32\2\u00c2\u00c4\5\61\31\2\u00c3\u00c2\3\2"+
		"\2\2\u00c4\u00c5\3\2\2\2\u00c5\u00c3\3\2\2\2\u00c5\u00c6\3\2\2\2\u00c6"+
		":\3\2\2\2\u00c7\u00c9\5+\26\2\u00c8\u00c7\3\2\2\2\u00c8\u00c9\3\2\2\2"+
		"\u00c9\u00ca\3\2\2\2\u00ca\u00cd\5)\25\2\u00cb\u00ce\5)\25\2\u00cc\u00ce"+
		"\5\61\31\2\u00cd\u00cb\3\2\2\2\u00cd\u00cc\3\2\2\2\u00ce\u00cf\3\2\2\2"+
		"\u00cf\u00cd\3\2\2\2\u00cf\u00d0\3\2\2\2\u00d0<\3\2\2\2\u00d1\u00d3\5"+
		"+\26\2\u00d2\u00d1\3\2\2\2\u00d2\u00d3\3\2\2\2\u00d3\u00d4\3\2\2\2\u00d4"+
		"\u00d7\5)\25\2\u00d5\u00d8\5)\25\2\u00d6\u00d8\5\61\31\2\u00d7\u00d5\3"+
		"\2\2\2\u00d7\u00d6\3\2\2\2\u00d8\u00d9\3\2\2\2\u00d9\u00d7\3\2\2\2\u00d9"+
		"\u00da\3\2\2\2\u00da\u00de\3\2\2\2\u00db\u00df\5)\25\2\u00dc\u00df\5\61"+
		"\31\2\u00dd\u00df\5\3\2\2\u00de\u00db\3\2\2\2\u00de\u00dc\3\2\2\2\u00de"+
		"\u00dd\3\2\2\2\u00df\u00e0\3\2\2\2\u00e0\u00de\3\2\2\2\u00e0\u00e1\3\2"+
		"\2\2\u00e1\u00e4\3\2\2\2\u00e2\u00e5\5)\25\2\u00e3\u00e5\5\61\31\2\u00e4"+
		"\u00e2\3\2\2\2\u00e4\u00e3\3\2\2\2\u00e5>\3\2\2\2\u00e6\u00ec\5;\36\2"+
		"\u00e7\u00e8\5\63\32\2\u00e8\u00e9\5;\36\2\u00e9\u00eb\3\2\2\2\u00ea\u00e7"+
		"\3\2\2\2\u00eb\u00ee\3\2\2\2\u00ec\u00ea\3\2\2\2\u00ec\u00ed\3\2\2\2\u00ed"+
		"@\3\2\2\2\u00ee\u00ec\3\2\2\2\25\2KO[\u00ad\u00b4\u00bb\u00be\u00c5\u00c8"+
		"\u00cd\u00cf\u00d2\u00d7\u00d9\u00de\u00e0\u00e4\u00ec\3\b\2\2";
	public static final ATN _ATN =
		new ATNDeserializer().deserialize(_serializedATN.toCharArray());
	static {
		_decisionToDFA = new DFA[_ATN.getNumberOfDecisions()];
		for (int i = 0; i < _ATN.getNumberOfDecisions(); i++) {
			_decisionToDFA[i] = new DFA(_ATN.getDecisionState(i), i);
		}
	}
}