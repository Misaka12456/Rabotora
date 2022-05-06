// Generated from f:\C#\Misaka12456 (Misaka Castle)\Rabotora\Rabotora.Core\RabotoraScriptFormat\RabotoraScriptFormat.g4 by ANTLR 4.8
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
		RBrace=8, QttMrk=9, QttMrk2=10, Comma=11, Semicolon=12, QuesMark=13, ClassKeyWord=14, 
		ActionKeyWord=15, PrivateKeyWord=16, PublicKeyWord=17, InternalKeyWord=18, 
		StaticKeyWord=19, Int=20, Float=21, Word=22, PlainWord=23, FuncWord=24;
	public static final int
		RULE_plainString = 0, RULE_value = 1, RULE_values = 2, RULE_cmd = 3, RULE_segment = 4, 
		RULE_segmentBody = 5, RULE_typeDef = 6, RULE_typeDefs = 7, RULE_classDef = 8, 
		RULE_funcDef = 9, RULE_classSegment = 10, RULE_funcSegment = 11, RULE_body = 12, 
		RULE_script = 13;
	private static String[] makeRuleNames() {
		return new String[] {
			"plainString", "value", "values", "cmd", "segment", "segmentBody", "typeDef", 
			"typeDefs", "classDef", "funcDef", "classSegment", "funcSegment", "body", 
			"script"
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
		enterRule(_localctx, 0, RULE_plainString);
		try {
			setState(34);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case QttMrk:
				enterOuterAlt(_localctx, 1);
				{
				{
				setState(28);
				match(QttMrk);
				setState(29);
				match(PlainWord);
				setState(30);
				match(QttMrk);
				}
				}
				break;
			case QttMrk2:
				enterOuterAlt(_localctx, 2);
				{
				{
				setState(31);
				match(QttMrk2);
				setState(32);
				match(PlainWord);
				setState(33);
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
		enterRule(_localctx, 2, RULE_value);
		try {
			setState(40);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case QttMrk:
			case QttMrk2:
				enterOuterAlt(_localctx, 1);
				{
				setState(36);
				plainString();
				}
				break;
			case Word:
				enterOuterAlt(_localctx, 2);
				{
				setState(37);
				match(Word);
				}
				break;
			case Int:
				enterOuterAlt(_localctx, 3);
				{
				setState(38);
				match(Int);
				}
				break;
			case Float:
				enterOuterAlt(_localctx, 4);
				{
				setState(39);
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
		enterRule(_localctx, 4, RULE_values);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(42);
			match(LParen);
			setState(51);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if ((((_la) & ~0x3f) == 0 && ((1L << _la) & ((1L << QttMrk) | (1L << QttMrk2) | (1L << Int) | (1L << Float) | (1L << Word))) != 0)) {
				{
				setState(43);
				value();
				setState(48);
				_errHandler.sync(this);
				_la = _input.LA(1);
				while (_la==Comma) {
					{
					{
					setState(44);
					match(Comma);
					setState(45);
					value();
					}
					}
					setState(50);
					_errHandler.sync(this);
					_la = _input.LA(1);
				}
				}
			}

			setState(53);
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
		public TerminalNode FuncWord() { return getToken(RabotoraScriptFormatParser.FuncWord, 0); }
		public ValuesContext values() {
			return getRuleContext(ValuesContext.class,0);
		}
		public TerminalNode Semicolon() { return getToken(RabotoraScriptFormatParser.Semicolon, 0); }
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
		enterRule(_localctx, 6, RULE_cmd);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(55);
			match(FuncWord);
			setState(56);
			values();
			setState(58);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==LBrace) {
				{
				setState(57);
				segment();
				}
			}

			setState(60);
			match(Semicolon);
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
		enterRule(_localctx, 8, RULE_segment);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(62);
			match(LBrace);
			setState(63);
			segmentBody();
			setState(64);
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
		enterRule(_localctx, 10, RULE_segmentBody);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(69);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==FuncWord) {
				{
				{
				setState(66);
				cmd();
				}
				}
				setState(71);
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
		enterRule(_localctx, 12, RULE_typeDef);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(72);
			match(Word);
			setState(74);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==QuesMark) {
				{
				setState(73);
				match(QuesMark);
				}
			}

			setState(76);
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
		enterRule(_localctx, 14, RULE_typeDefs);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(78);
			match(LParen);
			setState(87);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==Word) {
				{
				setState(79);
				typeDef();
				setState(84);
				_errHandler.sync(this);
				_la = _input.LA(1);
				while (_la==Comma) {
					{
					{
					setState(80);
					match(Comma);
					setState(81);
					typeDef();
					}
					}
					setState(86);
					_errHandler.sync(this);
					_la = _input.LA(1);
				}
				}
			}

			setState(89);
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
		enterRule(_localctx, 16, RULE_classDef);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(92);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if ((((_la) & ~0x3f) == 0 && ((1L << _la) & ((1L << PrivateKeyWord) | (1L << PublicKeyWord) | (1L << InternalKeyWord))) != 0)) {
				{
				setState(91);
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

			setState(95);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==StaticKeyWord) {
				{
				setState(94);
				match(StaticKeyWord);
				}
			}

			setState(97);
			match(ClassKeyWord);
			setState(98);
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
		enterRule(_localctx, 18, RULE_funcDef);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(101);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if ((((_la) & ~0x3f) == 0 && ((1L << _la) & ((1L << PrivateKeyWord) | (1L << PublicKeyWord) | (1L << InternalKeyWord))) != 0)) {
				{
				setState(100);
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

			setState(104);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==StaticKeyWord) {
				{
				setState(103);
				match(StaticKeyWord);
				}
			}

			setState(106);
			_la = _input.LA(1);
			if ( !(_la==ActionKeyWord || _la==Word) ) {
			_errHandler.recoverInline(this);
			}
			else {
				if ( _input.LA(1)==Token.EOF ) matchedEOF = true;
				_errHandler.reportMatch(this);
				consume();
			}
			setState(107);
			match(Word);
			setState(108);
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
		public SegmentContext segment() {
			return getRuleContext(SegmentContext.class,0);
		}
		public ClassSegmentContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_classSegment; }
	}

	public final ClassSegmentContext classSegment() throws RecognitionException {
		ClassSegmentContext _localctx = new ClassSegmentContext(_ctx, getState());
		enterRule(_localctx, 20, RULE_classSegment);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(110);
			classDef();
			setState(111);
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
		enterRule(_localctx, 22, RULE_funcSegment);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(113);
			funcDef();
			setState(114);
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
		enterRule(_localctx, 24, RULE_body);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(118); 
			_errHandler.sync(this);
			_la = _input.LA(1);
			do {
				{
				setState(118);
				_errHandler.sync(this);
				switch ( getInterpreter().adaptivePredict(_input,13,_ctx) ) {
				case 1:
					{
					setState(116);
					classSegment();
					}
					break;
				case 2:
					{
					setState(117);
					funcSegment();
					}
					break;
				}
				}
				setState(120); 
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
		enterRule(_localctx, 26, RULE_script);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(122);
			body();
			setState(123);
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
		"\3\u608b\ua72a\u8133\ub9ed\u417c\u3be7\u7786\u5964\3\32\u0080\4\2\t\2"+
		"\4\3\t\3\4\4\t\4\4\5\t\5\4\6\t\6\4\7\t\7\4\b\t\b\4\t\t\t\4\n\t\n\4\13"+
		"\t\13\4\f\t\f\4\r\t\r\4\16\t\16\4\17\t\17\3\2\3\2\3\2\3\2\3\2\3\2\5\2"+
		"%\n\2\3\3\3\3\3\3\3\3\5\3+\n\3\3\4\3\4\3\4\3\4\7\4\61\n\4\f\4\16\4\64"+
		"\13\4\5\4\66\n\4\3\4\3\4\3\5\3\5\3\5\5\5=\n\5\3\5\3\5\3\6\3\6\3\6\3\6"+
		"\3\7\7\7F\n\7\f\7\16\7I\13\7\3\b\3\b\5\bM\n\b\3\b\3\b\3\t\3\t\3\t\3\t"+
		"\7\tU\n\t\f\t\16\tX\13\t\5\tZ\n\t\3\t\3\t\3\n\5\n_\n\n\3\n\5\nb\n\n\3"+
		"\n\3\n\3\n\3\13\5\13h\n\13\3\13\5\13k\n\13\3\13\3\13\3\13\3\13\3\f\3\f"+
		"\3\f\3\r\3\r\3\r\3\16\3\16\6\16y\n\16\r\16\16\16z\3\17\3\17\3\17\3\17"+
		"\2\2\20\2\4\6\b\n\f\16\20\22\24\26\30\32\34\2\4\3\2\22\24\4\2\21\21\30"+
		"\30\2\u0082\2$\3\2\2\2\4*\3\2\2\2\6,\3\2\2\2\b9\3\2\2\2\n@\3\2\2\2\fG"+
		"\3\2\2\2\16J\3\2\2\2\20P\3\2\2\2\22^\3\2\2\2\24g\3\2\2\2\26p\3\2\2\2\30"+
		"s\3\2\2\2\32x\3\2\2\2\34|\3\2\2\2\36\37\7\13\2\2\37 \7\31\2\2 %\7\13\2"+
		"\2!\"\7\f\2\2\"#\7\31\2\2#%\7\f\2\2$\36\3\2\2\2$!\3\2\2\2%\3\3\2\2\2&"+
		"+\5\2\2\2\'+\7\30\2\2(+\7\26\2\2)+\7\27\2\2*&\3\2\2\2*\'\3\2\2\2*(\3\2"+
		"\2\2*)\3\2\2\2+\5\3\2\2\2,\65\7\7\2\2-\62\5\4\3\2./\7\r\2\2/\61\5\4\3"+
		"\2\60.\3\2\2\2\61\64\3\2\2\2\62\60\3\2\2\2\62\63\3\2\2\2\63\66\3\2\2\2"+
		"\64\62\3\2\2\2\65-\3\2\2\2\65\66\3\2\2\2\66\67\3\2\2\2\678\7\b\2\28\7"+
		"\3\2\2\29:\7\32\2\2:<\5\6\4\2;=\5\n\6\2<;\3\2\2\2<=\3\2\2\2=>\3\2\2\2"+
		">?\7\16\2\2?\t\3\2\2\2@A\7\t\2\2AB\5\f\7\2BC\7\n\2\2C\13\3\2\2\2DF\5\b"+
		"\5\2ED\3\2\2\2FI\3\2\2\2GE\3\2\2\2GH\3\2\2\2H\r\3\2\2\2IG\3\2\2\2JL\7"+
		"\30\2\2KM\7\17\2\2LK\3\2\2\2LM\3\2\2\2MN\3\2\2\2NO\7\30\2\2O\17\3\2\2"+
		"\2PY\7\7\2\2QV\5\16\b\2RS\7\r\2\2SU\5\16\b\2TR\3\2\2\2UX\3\2\2\2VT\3\2"+
		"\2\2VW\3\2\2\2WZ\3\2\2\2XV\3\2\2\2YQ\3\2\2\2YZ\3\2\2\2Z[\3\2\2\2[\\\7"+
		"\b\2\2\\\21\3\2\2\2]_\t\2\2\2^]\3\2\2\2^_\3\2\2\2_a\3\2\2\2`b\7\25\2\2"+
		"a`\3\2\2\2ab\3\2\2\2bc\3\2\2\2cd\7\20\2\2de\7\30\2\2e\23\3\2\2\2fh\t\2"+
		"\2\2gf\3\2\2\2gh\3\2\2\2hj\3\2\2\2ik\7\25\2\2ji\3\2\2\2jk\3\2\2\2kl\3"+
		"\2\2\2lm\t\3\2\2mn\7\30\2\2no\5\20\t\2o\25\3\2\2\2pq\5\22\n\2qr\5\n\6"+
		"\2r\27\3\2\2\2st\5\24\13\2tu\5\n\6\2u\31\3\2\2\2vy\5\26\f\2wy\5\30\r\2"+
		"xv\3\2\2\2xw\3\2\2\2yz\3\2\2\2zx\3\2\2\2z{\3\2\2\2{\33\3\2\2\2|}\5\32"+
		"\16\2}~\7\2\2\3~\35\3\2\2\2\21$*\62\65<GLVY^agjxz";
	public static final ATN _ATN =
		new ATNDeserializer().deserialize(_serializedATN.toCharArray());
	static {
		_decisionToDFA = new DFA[_ATN.getNumberOfDecisions()];
		for (int i = 0; i < _ATN.getNumberOfDecisions(); i++) {
			_decisionToDFA[i] = new DFA(_ATN.getDecisionState(i), i);
		}
	}
}