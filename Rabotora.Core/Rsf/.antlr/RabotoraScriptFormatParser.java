// Generated from f:\C#\Misaka12456 (Misaka Castle)\Rabotora\Rabotora.Core\Rsf\RabotoraScriptFormat.g4 by ANTLR 4.8
import org.antlr.v4.runtime.atn.*;
import org.antlr.v4.runtime.dfa.DFA;
import org.antlr.v4.runtime.*;
import org.antlr.v4.runtime.misc.*;
import org.antlr.v4.runtime.tree.*;
import java.util.List;
import java.util.Iterator;
import java.util.ArrayList;

@SuppressWarnings({"all", "warnings", "unchecked", "unused", "cast"})
public class RabotoraScriptFormatParser extends Parser {
	static { RuntimeMetaData.checkVersion("4.8", RuntimeMetaData.VERSION); }

	protected static final DFA[] _decisionToDFA;
	protected static final PredictionContextCache _sharedContextCache =
		new PredictionContextCache();
	public static final int
		Whitespace=1, LineComment=2, Comment=3, WS=4, LParen=5, RParen=6, LBrace=7, 
		RBrace=8, QttMrk=9, QttMrk2=10, Comma=11, Semicolon=12, QuesMark=13, Equals=14, 
		ClassKeyWord=15, ActionKeyWord=16, PrivateKeyWord=17, PublicKeyWord=18, 
		InternalKeyWord=19, StaticKeyWord=20, Int=21, Float=22, Word=23, PlainWord=24, 
		DOT=25;
	public static final int
		RULE_funcWord = 0, RULE_plainString = 1, RULE_value = 2, RULE_values = 3, 
		RULE_cmd = 4, RULE_segment = 5, RULE_segmentBody = 6, RULE_typeDef = 7, 
		RULE_typeDefs = 8, RULE_classDef = 9, RULE_funcDef = 10, RULE_classSegment = 11, 
		RULE_funcSegment = 12, RULE_body = 13, RULE_script = 14;
	private static String[] makeRuleNames() {
		return new String[] {
			"funcWord", "plainString", "value", "values", "cmd", "segment", "segmentBody", 
			"typeDef", "typeDefs", "classDef", "funcDef", "classSegment", "funcSegment", 
			"body", "script"
		};
	}
	public static final String[] ruleNames = makeRuleNames();

	private static String[] makeLiteralNames() {
		return new String[] {
			null, null, null, null, null, "'('", "')'", "'{'", "'}'", "'''", "'\"'", 
			"','", "';'", "'?'", "'='", "'class'", "'void'", "'private'", "'public'", 
			"'internal'", "'static'"
		};
	}
	private static final String[] _LITERAL_NAMES = makeLiteralNames();
	private static String[] makeSymbolicNames() {
		return new String[] {
			null, "Whitespace", "LineComment", "Comment", "WS", "LParen", "RParen", 
			"LBrace", "RBrace", "QttMrk", "QttMrk2", "Comma", "Semicolon", "QuesMark", 
			"Equals", "ClassKeyWord", "ActionKeyWord", "PrivateKeyWord", "PublicKeyWord", 
			"InternalKeyWord", "StaticKeyWord", "Int", "Float", "Word", "PlainWord", 
			"DOT"
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

	@Override
	public String getGrammarFileName() { return "RabotoraScriptFormat.g4"; }

	@Override
	public String[] getRuleNames() { return ruleNames; }

	@Override
	public String getSerializedATN() { return _serializedATN; }

	@Override
	public ATN getATN() { return _ATN; }

	public RabotoraScriptFormatParser(TokenStream input) {
		super(input);
		_interp = new ParserATNSimulator(this,_ATN,_decisionToDFA,_sharedContextCache);
	}

	public static class FuncWordContext extends ParserRuleContext {
		public List<TerminalNode> Word() { return getTokens(RabotoraScriptFormatParser.Word); }
		public TerminalNode Word(int i) {
			return getToken(RabotoraScriptFormatParser.Word, i);
		}
		public List<TerminalNode> DOT() { return getTokens(RabotoraScriptFormatParser.DOT); }
		public TerminalNode DOT(int i) {
			return getToken(RabotoraScriptFormatParser.DOT, i);
		}
		public FuncWordContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_funcWord; }
	}

	public final FuncWordContext funcWord() throws RecognitionException {
		FuncWordContext _localctx = new FuncWordContext(_ctx, getState());
		enterRule(_localctx, 0, RULE_funcWord);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(30);
			match(Word);
			setState(35);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==DOT) {
				{
				{
				setState(31);
				match(DOT);
				setState(32);
				match(Word);
				}
				}
				setState(37);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class PlainStringContext extends ParserRuleContext {
		public List<TerminalNode> QttMrk() { return getTokens(RabotoraScriptFormatParser.QttMrk); }
		public TerminalNode QttMrk(int i) {
			return getToken(RabotoraScriptFormatParser.QttMrk, i);
		}
		public TerminalNode PlainWord() { return getToken(RabotoraScriptFormatParser.PlainWord, 0); }
		public List<TerminalNode> QttMrk2() { return getTokens(RabotoraScriptFormatParser.QttMrk2); }
		public TerminalNode QttMrk2(int i) {
			return getToken(RabotoraScriptFormatParser.QttMrk2, i);
		}
		public PlainStringContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_plainString; }
	}

	public final PlainStringContext plainString() throws RecognitionException {
		PlainStringContext _localctx = new PlainStringContext(_ctx, getState());
		enterRule(_localctx, 2, RULE_plainString);
		try {
			setState(44);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case QttMrk:
				enterOuterAlt(_localctx, 1);
				{
				{
				setState(38);
				match(QttMrk);
				setState(39);
				match(PlainWord);
				setState(40);
				match(QttMrk);
				}
				}
				break;
			case QttMrk2:
				enterOuterAlt(_localctx, 2);
				{
				{
				setState(41);
				match(QttMrk2);
				setState(42);
				match(PlainWord);
				setState(43);
				match(QttMrk2);
				}
				}
				break;
			default:
				throw new NoViableAltException(this);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class ValueContext extends ParserRuleContext {
		public PlainStringContext plainString() {
			return getRuleContext(PlainStringContext.class,0);
		}
		public TerminalNode Word() { return getToken(RabotoraScriptFormatParser.Word, 0); }
		public TerminalNode Int() { return getToken(RabotoraScriptFormatParser.Int, 0); }
		public TerminalNode Float() { return getToken(RabotoraScriptFormatParser.Float, 0); }
		public ValueContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_value; }
	}

	public final ValueContext value() throws RecognitionException {
		ValueContext _localctx = new ValueContext(_ctx, getState());
		enterRule(_localctx, 4, RULE_value);
		try {
			setState(50);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case QttMrk:
			case QttMrk2:
				enterOuterAlt(_localctx, 1);
				{
				setState(46);
				plainString();
				}
				break;
			case Word:
				enterOuterAlt(_localctx, 2);
				{
				setState(47);
				match(Word);
				}
				break;
			case Int:
				enterOuterAlt(_localctx, 3);
				{
				setState(48);
				match(Int);
				}
				break;
			case Float:
				enterOuterAlt(_localctx, 4);
				{
				setState(49);
				match(Float);
				}
				break;
			default:
				throw new NoViableAltException(this);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class ValuesContext extends ParserRuleContext {
		public TerminalNode LParen() { return getToken(RabotoraScriptFormatParser.LParen, 0); }
		public TerminalNode RParen() { return getToken(RabotoraScriptFormatParser.RParen, 0); }
		public List<ValueContext> value() {
			return getRuleContexts(ValueContext.class);
		}
		public ValueContext value(int i) {
			return getRuleContext(ValueContext.class,i);
		}
		public List<TerminalNode> Comma() { return getTokens(RabotoraScriptFormatParser.Comma); }
		public TerminalNode Comma(int i) {
			return getToken(RabotoraScriptFormatParser.Comma, i);
		}
		public ValuesContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_values; }
	}

	public final ValuesContext values() throws RecognitionException {
		ValuesContext _localctx = new ValuesContext(_ctx, getState());
		enterRule(_localctx, 6, RULE_values);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(52);
			match(LParen);
			setState(61);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if ((((_la) & ~0x3f) == 0 && ((1L << _la) & ((1L << QttMrk) | (1L << QttMrk2) | (1L << Int) | (1L << Float) | (1L << Word))) != 0)) {
				{
				setState(53);
				value();
				setState(58);
				_errHandler.sync(this);
				_la = _input.LA(1);
				while (_la==Comma) {
					{
					{
					setState(54);
					match(Comma);
					setState(55);
					value();
					}
					}
					setState(60);
					_errHandler.sync(this);
					_la = _input.LA(1);
				}
				}
			}

			setState(63);
			match(RParen);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class CmdContext extends ParserRuleContext {
		public FuncWordContext funcWord() {
			return getRuleContext(FuncWordContext.class,0);
		}
		public ValuesContext values() {
			return getRuleContext(ValuesContext.class,0);
		}
		public TerminalNode Semicolon() { return getToken(RabotoraScriptFormatParser.Semicolon, 0); }
		public List<TerminalNode> Word() { return getTokens(RabotoraScriptFormatParser.Word); }
		public TerminalNode Word(int i) {
			return getToken(RabotoraScriptFormatParser.Word, i);
		}
		public TerminalNode Equals() { return getToken(RabotoraScriptFormatParser.Equals, 0); }
		public ValueContext value() {
			return getRuleContext(ValueContext.class,0);
		}
		public PlainStringContext plainString() {
			return getRuleContext(PlainStringContext.class,0);
		}
		public SegmentContext segment() {
			return getRuleContext(SegmentContext.class,0);
		}
		public CmdContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_cmd; }
	}

	public final CmdContext cmd() throws RecognitionException {
		CmdContext _localctx = new CmdContext(_ctx, getState());
		enterRule(_localctx, 8, RULE_cmd);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(83);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,7,_ctx) ) {
			case 1:
				{
				setState(65);
				funcWord();
				setState(66);
				values();
				setState(68);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==LBrace) {
					{
					setState(67);
					segment();
					}
				}

				setState(70);
				match(Semicolon);
				}
				break;
			case 2:
				{
				setState(72);
				match(Word);
				setState(73);
				match(Equals);
				setState(76);
				_errHandler.sync(this);
				switch (_input.LA(1)) {
				case Word:
					{
					setState(74);
					match(Word);
					}
					break;
				case QttMrk:
				case QttMrk2:
					{
					setState(75);
					plainString();
					}
					break;
				default:
					throw new NoViableAltException(this);
				}
				setState(78);
				match(Semicolon);
				}
				break;
			case 3:
				{
				setState(79);
				funcWord();
				setState(80);
				value();
				setState(81);
				match(Semicolon);
				}
				break;
			}
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class SegmentContext extends ParserRuleContext {
		public TerminalNode LBrace() { return getToken(RabotoraScriptFormatParser.LBrace, 0); }
		public SegmentBodyContext segmentBody() {
			return getRuleContext(SegmentBodyContext.class,0);
		}
		public TerminalNode RBrace() { return getToken(RabotoraScriptFormatParser.RBrace, 0); }
		public SegmentContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_segment; }
	}

	public final SegmentContext segment() throws RecognitionException {
		SegmentContext _localctx = new SegmentContext(_ctx, getState());
		enterRule(_localctx, 10, RULE_segment);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(85);
			match(LBrace);
			setState(86);
			segmentBody();
			setState(87);
			match(RBrace);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class SegmentBodyContext extends ParserRuleContext {
		public List<CmdContext> cmd() {
			return getRuleContexts(CmdContext.class);
		}
		public CmdContext cmd(int i) {
			return getRuleContext(CmdContext.class,i);
		}
		public SegmentBodyContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_segmentBody; }
	}

	public final SegmentBodyContext segmentBody() throws RecognitionException {
		SegmentBodyContext _localctx = new SegmentBodyContext(_ctx, getState());
		enterRule(_localctx, 12, RULE_segmentBody);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(92);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==Word) {
				{
				{
				setState(89);
				cmd();
				}
				}
				setState(94);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class TypeDefContext extends ParserRuleContext {
		public List<TerminalNode> Word() { return getTokens(RabotoraScriptFormatParser.Word); }
		public TerminalNode Word(int i) {
			return getToken(RabotoraScriptFormatParser.Word, i);
		}
		public TerminalNode QuesMark() { return getToken(RabotoraScriptFormatParser.QuesMark, 0); }
		public TypeDefContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_typeDef; }
	}

	public final TypeDefContext typeDef() throws RecognitionException {
		TypeDefContext _localctx = new TypeDefContext(_ctx, getState());
		enterRule(_localctx, 14, RULE_typeDef);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(95);
			match(Word);
			setState(97);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==QuesMark) {
				{
				setState(96);
				match(QuesMark);
				}
			}

			setState(99);
			match(Word);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class TypeDefsContext extends ParserRuleContext {
		public TerminalNode LParen() { return getToken(RabotoraScriptFormatParser.LParen, 0); }
		public TerminalNode RParen() { return getToken(RabotoraScriptFormatParser.RParen, 0); }
		public List<TypeDefContext> typeDef() {
			return getRuleContexts(TypeDefContext.class);
		}
		public TypeDefContext typeDef(int i) {
			return getRuleContext(TypeDefContext.class,i);
		}
		public List<TerminalNode> Comma() { return getTokens(RabotoraScriptFormatParser.Comma); }
		public TerminalNode Comma(int i) {
			return getToken(RabotoraScriptFormatParser.Comma, i);
		}
		public TypeDefsContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_typeDefs; }
	}

	public final TypeDefsContext typeDefs() throws RecognitionException {
		TypeDefsContext _localctx = new TypeDefsContext(_ctx, getState());
		enterRule(_localctx, 16, RULE_typeDefs);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(101);
			match(LParen);
			setState(110);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==Word) {
				{
				setState(102);
				typeDef();
				setState(107);
				_errHandler.sync(this);
				_la = _input.LA(1);
				while (_la==Comma) {
					{
					{
					setState(103);
					match(Comma);
					setState(104);
					typeDef();
					}
					}
					setState(109);
					_errHandler.sync(this);
					_la = _input.LA(1);
				}
				}
			}

			setState(112);
			match(RParen);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class ClassDefContext extends ParserRuleContext {
		public TerminalNode ClassKeyWord() { return getToken(RabotoraScriptFormatParser.ClassKeyWord, 0); }
		public TerminalNode Word() { return getToken(RabotoraScriptFormatParser.Word, 0); }
		public TerminalNode StaticKeyWord() { return getToken(RabotoraScriptFormatParser.StaticKeyWord, 0); }
		public TerminalNode PublicKeyWord() { return getToken(RabotoraScriptFormatParser.PublicKeyWord, 0); }
		public TerminalNode PrivateKeyWord() { return getToken(RabotoraScriptFormatParser.PrivateKeyWord, 0); }
		public TerminalNode InternalKeyWord() { return getToken(RabotoraScriptFormatParser.InternalKeyWord, 0); }
		public ClassDefContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_classDef; }
	}

	public final ClassDefContext classDef() throws RecognitionException {
		ClassDefContext _localctx = new ClassDefContext(_ctx, getState());
		enterRule(_localctx, 18, RULE_classDef);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(115);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if ((((_la) & ~0x3f) == 0 && ((1L << _la) & ((1L << PrivateKeyWord) | (1L << PublicKeyWord) | (1L << InternalKeyWord))) != 0)) {
				{
				setState(114);
				_la = _input.LA(1);
				if ( !((((_la) & ~0x3f) == 0 && ((1L << _la) & ((1L << PrivateKeyWord) | (1L << PublicKeyWord) | (1L << InternalKeyWord))) != 0)) ) {
				_errHandler.recoverInline(this);
				}
				else {
					if ( _input.LA(1)==Token.EOF ) matchedEOF = true;
					_errHandler.reportMatch(this);
					consume();
				}
				}
			}

			setState(118);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==StaticKeyWord) {
				{
				setState(117);
				match(StaticKeyWord);
				}
			}

			setState(120);
			match(ClassKeyWord);
			setState(121);
			match(Word);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class FuncDefContext extends ParserRuleContext {
		public List<TerminalNode> Word() { return getTokens(RabotoraScriptFormatParser.Word); }
		public TerminalNode Word(int i) {
			return getToken(RabotoraScriptFormatParser.Word, i);
		}
		public TypeDefsContext typeDefs() {
			return getRuleContext(TypeDefsContext.class,0);
		}
		public TerminalNode ActionKeyWord() { return getToken(RabotoraScriptFormatParser.ActionKeyWord, 0); }
		public TerminalNode StaticKeyWord() { return getToken(RabotoraScriptFormatParser.StaticKeyWord, 0); }
		public TerminalNode PublicKeyWord() { return getToken(RabotoraScriptFormatParser.PublicKeyWord, 0); }
		public TerminalNode PrivateKeyWord() { return getToken(RabotoraScriptFormatParser.PrivateKeyWord, 0); }
		public TerminalNode InternalKeyWord() { return getToken(RabotoraScriptFormatParser.InternalKeyWord, 0); }
		public FuncDefContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_funcDef; }
	}

	public final FuncDefContext funcDef() throws RecognitionException {
		FuncDefContext _localctx = new FuncDefContext(_ctx, getState());
		enterRule(_localctx, 20, RULE_funcDef);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(124);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if ((((_la) & ~0x3f) == 0 && ((1L << _la) & ((1L << PrivateKeyWord) | (1L << PublicKeyWord) | (1L << InternalKeyWord))) != 0)) {
				{
				setState(123);
				_la = _input.LA(1);
				if ( !((((_la) & ~0x3f) == 0 && ((1L << _la) & ((1L << PrivateKeyWord) | (1L << PublicKeyWord) | (1L << InternalKeyWord))) != 0)) ) {
				_errHandler.recoverInline(this);
				}
				else {
					if ( _input.LA(1)==Token.EOF ) matchedEOF = true;
					_errHandler.reportMatch(this);
					consume();
				}
				}
			}

			setState(127);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==StaticKeyWord) {
				{
				setState(126);
				match(StaticKeyWord);
				}
			}

			setState(129);
			_la = _input.LA(1);
			if ( !(_la==ActionKeyWord || _la==Word) ) {
			_errHandler.recoverInline(this);
			}
			else {
				if ( _input.LA(1)==Token.EOF ) matchedEOF = true;
				_errHandler.reportMatch(this);
				consume();
			}
			setState(130);
			match(Word);
			setState(131);
			typeDefs();
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class ClassSegmentContext extends ParserRuleContext {
		public ClassDefContext classDef() {
			return getRuleContext(ClassDefContext.class,0);
		}
		public TerminalNode LBrace() { return getToken(RabotoraScriptFormatParser.LBrace, 0); }
		public TerminalNode RBrace() { return getToken(RabotoraScriptFormatParser.RBrace, 0); }
		public List<FuncSegmentContext> funcSegment() {
			return getRuleContexts(FuncSegmentContext.class);
		}
		public FuncSegmentContext funcSegment(int i) {
			return getRuleContext(FuncSegmentContext.class,i);
		}
		public ClassSegmentContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_classSegment; }
	}

	public final ClassSegmentContext classSegment() throws RecognitionException {
		ClassSegmentContext _localctx = new ClassSegmentContext(_ctx, getState());
		enterRule(_localctx, 22, RULE_classSegment);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(133);
			classDef();
			setState(134);
			match(LBrace);
			setState(138);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & ((1L << ActionKeyWord) | (1L << PrivateKeyWord) | (1L << PublicKeyWord) | (1L << InternalKeyWord) | (1L << StaticKeyWord) | (1L << Word))) != 0)) {
				{
				{
				setState(135);
				funcSegment();
				}
				}
				setState(140);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(141);
			match(RBrace);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class FuncSegmentContext extends ParserRuleContext {
		public FuncDefContext funcDef() {
			return getRuleContext(FuncDefContext.class,0);
		}
		public SegmentContext segment() {
			return getRuleContext(SegmentContext.class,0);
		}
		public FuncSegmentContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_funcSegment; }
	}

	public final FuncSegmentContext funcSegment() throws RecognitionException {
		FuncSegmentContext _localctx = new FuncSegmentContext(_ctx, getState());
		enterRule(_localctx, 24, RULE_funcSegment);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(143);
			funcDef();
			setState(144);
			segment();
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class BodyContext extends ParserRuleContext {
		public List<ClassSegmentContext> classSegment() {
			return getRuleContexts(ClassSegmentContext.class);
		}
		public ClassSegmentContext classSegment(int i) {
			return getRuleContext(ClassSegmentContext.class,i);
		}
		public List<FuncSegmentContext> funcSegment() {
			return getRuleContexts(FuncSegmentContext.class);
		}
		public FuncSegmentContext funcSegment(int i) {
			return getRuleContext(FuncSegmentContext.class,i);
		}
		public BodyContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_body; }
	}

	public final BodyContext body() throws RecognitionException {
		BodyContext _localctx = new BodyContext(_ctx, getState());
		enterRule(_localctx, 26, RULE_body);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(148); 
			_errHandler.sync(this);
			_la = _input.LA(1);
			do {
				{
				setState(148);
				_errHandler.sync(this);
				switch ( getInterpreter().adaptivePredict(_input,17,_ctx) ) {
				case 1:
					{
					setState(146);
					classSegment();
					}
					break;
				case 2:
					{
					setState(147);
					funcSegment();
					}
					break;
				}
				}
				setState(150); 
				_errHandler.sync(this);
				_la = _input.LA(1);
			} while ( (((_la) & ~0x3f) == 0 && ((1L << _la) & ((1L << ClassKeyWord) | (1L << ActionKeyWord) | (1L << PrivateKeyWord) | (1L << PublicKeyWord) | (1L << InternalKeyWord) | (1L << StaticKeyWord) | (1L << Word))) != 0) );
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class ScriptContext extends ParserRuleContext {
		public BodyContext body() {
			return getRuleContext(BodyContext.class,0);
		}
		public TerminalNode EOF() { return getToken(RabotoraScriptFormatParser.EOF, 0); }
		public ScriptContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_script; }
	}

	public final ScriptContext script() throws RecognitionException {
		ScriptContext _localctx = new ScriptContext(_ctx, getState());
		enterRule(_localctx, 28, RULE_script);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(152);
			body();
			setState(153);
			match(EOF);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static final String _serializedATN =
		"\3\u608b\ua72a\u8133\ub9ed\u417c\u3be7\u7786\u5964\3\33\u009e\4\2\t\2"+
		"\4\3\t\3\4\4\t\4\4\5\t\5\4\6\t\6\4\7\t\7\4\b\t\b\4\t\t\t\4\n\t\n\4\13"+
		"\t\13\4\f\t\f\4\r\t\r\4\16\t\16\4\17\t\17\4\20\t\20\3\2\3\2\3\2\7\2$\n"+
		"\2\f\2\16\2\'\13\2\3\3\3\3\3\3\3\3\3\3\3\3\5\3/\n\3\3\4\3\4\3\4\3\4\5"+
		"\4\65\n\4\3\5\3\5\3\5\3\5\7\5;\n\5\f\5\16\5>\13\5\5\5@\n\5\3\5\3\5\3\6"+
		"\3\6\3\6\5\6G\n\6\3\6\3\6\3\6\3\6\3\6\3\6\5\6O\n\6\3\6\3\6\3\6\3\6\3\6"+
		"\5\6V\n\6\3\7\3\7\3\7\3\7\3\b\7\b]\n\b\f\b\16\b`\13\b\3\t\3\t\5\td\n\t"+
		"\3\t\3\t\3\n\3\n\3\n\3\n\7\nl\n\n\f\n\16\no\13\n\5\nq\n\n\3\n\3\n\3\13"+
		"\5\13v\n\13\3\13\5\13y\n\13\3\13\3\13\3\13\3\f\5\f\177\n\f\3\f\5\f\u0082"+
		"\n\f\3\f\3\f\3\f\3\f\3\r\3\r\3\r\7\r\u008b\n\r\f\r\16\r\u008e\13\r\3\r"+
		"\3\r\3\16\3\16\3\16\3\17\3\17\6\17\u0097\n\17\r\17\16\17\u0098\3\20\3"+
		"\20\3\20\3\20\2\2\21\2\4\6\b\n\f\16\20\22\24\26\30\32\34\36\2\4\3\2\23"+
		"\25\4\2\22\22\31\31\2\u00a4\2 \3\2\2\2\4.\3\2\2\2\6\64\3\2\2\2\b\66\3"+
		"\2\2\2\nU\3\2\2\2\fW\3\2\2\2\16^\3\2\2\2\20a\3\2\2\2\22g\3\2\2\2\24u\3"+
		"\2\2\2\26~\3\2\2\2\30\u0087\3\2\2\2\32\u0091\3\2\2\2\34\u0096\3\2\2\2"+
		"\36\u009a\3\2\2\2 %\7\31\2\2!\"\7\33\2\2\"$\7\31\2\2#!\3\2\2\2$\'\3\2"+
		"\2\2%#\3\2\2\2%&\3\2\2\2&\3\3\2\2\2\'%\3\2\2\2()\7\13\2\2)*\7\32\2\2*"+
		"/\7\13\2\2+,\7\f\2\2,-\7\32\2\2-/\7\f\2\2.(\3\2\2\2.+\3\2\2\2/\5\3\2\2"+
		"\2\60\65\5\4\3\2\61\65\7\31\2\2\62\65\7\27\2\2\63\65\7\30\2\2\64\60\3"+
		"\2\2\2\64\61\3\2\2\2\64\62\3\2\2\2\64\63\3\2\2\2\65\7\3\2\2\2\66?\7\7"+
		"\2\2\67<\5\6\4\289\7\r\2\29;\5\6\4\2:8\3\2\2\2;>\3\2\2\2<:\3\2\2\2<=\3"+
		"\2\2\2=@\3\2\2\2><\3\2\2\2?\67\3\2\2\2?@\3\2\2\2@A\3\2\2\2AB\7\b\2\2B"+
		"\t\3\2\2\2CD\5\2\2\2DF\5\b\5\2EG\5\f\7\2FE\3\2\2\2FG\3\2\2\2GH\3\2\2\2"+
		"HI\7\16\2\2IV\3\2\2\2JK\7\31\2\2KN\7\20\2\2LO\7\31\2\2MO\5\4\3\2NL\3\2"+
		"\2\2NM\3\2\2\2OP\3\2\2\2PV\7\16\2\2QR\5\2\2\2RS\5\6\4\2ST\7\16\2\2TV\3"+
		"\2\2\2UC\3\2\2\2UJ\3\2\2\2UQ\3\2\2\2V\13\3\2\2\2WX\7\t\2\2XY\5\16\b\2"+
		"YZ\7\n\2\2Z\r\3\2\2\2[]\5\n\6\2\\[\3\2\2\2]`\3\2\2\2^\\\3\2\2\2^_\3\2"+
		"\2\2_\17\3\2\2\2`^\3\2\2\2ac\7\31\2\2bd\7\17\2\2cb\3\2\2\2cd\3\2\2\2d"+
		"e\3\2\2\2ef\7\31\2\2f\21\3\2\2\2gp\7\7\2\2hm\5\20\t\2ij\7\r\2\2jl\5\20"+
		"\t\2ki\3\2\2\2lo\3\2\2\2mk\3\2\2\2mn\3\2\2\2nq\3\2\2\2om\3\2\2\2ph\3\2"+
		"\2\2pq\3\2\2\2qr\3\2\2\2rs\7\b\2\2s\23\3\2\2\2tv\t\2\2\2ut\3\2\2\2uv\3"+
		"\2\2\2vx\3\2\2\2wy\7\26\2\2xw\3\2\2\2xy\3\2\2\2yz\3\2\2\2z{\7\21\2\2{"+
		"|\7\31\2\2|\25\3\2\2\2}\177\t\2\2\2~}\3\2\2\2~\177\3\2\2\2\177\u0081\3"+
		"\2\2\2\u0080\u0082\7\26\2\2\u0081\u0080\3\2\2\2\u0081\u0082\3\2\2\2\u0082"+
		"\u0083\3\2\2\2\u0083\u0084\t\3\2\2\u0084\u0085\7\31\2\2\u0085\u0086\5"+
		"\22\n\2\u0086\27\3\2\2\2\u0087\u0088\5\24\13\2\u0088\u008c\7\t\2\2\u0089"+
		"\u008b\5\32\16\2\u008a\u0089\3\2\2\2\u008b\u008e\3\2\2\2\u008c\u008a\3"+
		"\2\2\2\u008c\u008d\3\2\2\2\u008d\u008f\3\2\2\2\u008e\u008c\3\2\2\2\u008f"+
		"\u0090\7\n\2\2\u0090\31\3\2\2\2\u0091\u0092\5\26\f\2\u0092\u0093\5\f\7"+
		"\2\u0093\33\3\2\2\2\u0094\u0097\5\30\r\2\u0095\u0097\5\32\16\2\u0096\u0094"+
		"\3\2\2\2\u0096\u0095\3\2\2\2\u0097\u0098\3\2\2\2\u0098\u0096\3\2\2\2\u0098"+
		"\u0099\3\2\2\2\u0099\35\3\2\2\2\u009a\u009b\5\34\17\2\u009b\u009c\7\2"+
		"\2\3\u009c\37\3\2\2\2\25%.\64<?FNU^cmpux~\u0081\u008c\u0096\u0098";
	public static final ATN _ATN =
		new ATNDeserializer().deserialize(_serializedATN.toCharArray());
	static {
		_decisionToDFA = new DFA[_ATN.getNumberOfDecisions()];
		for (int i = 0; i < _ATN.getNumberOfDecisions(); i++) {
			_decisionToDFA[i] = new DFA(_ATN.getDecisionState(i), i);
		}
	}
}