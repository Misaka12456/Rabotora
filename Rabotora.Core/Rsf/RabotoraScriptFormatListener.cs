﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     ANTLR Version: 4.6.6
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from F:\C#\Misaka12456 (Misaka Castle)\Rabotora\Rabotora.Core\Rsf\RabotoraScriptFormat.g4 by ANTLR 4.6.6

// Unreachable code detected
#pragma warning disable 0162
// The variable '...' is assigned but its value is never used
#pragma warning disable 0219
// Missing XML comment for publicly visible type or member '...'
#pragma warning disable 1591
// Ambiguous reference in cref attribute
#pragma warning disable 419

namespace Rabotora.Core.Rsf {
using Antlr4.Runtime.Misc;
using IParseTreeListener = Antlr4.Runtime.Tree.IParseTreeListener;
using IToken = Antlr4.Runtime.IToken;

/// <summary>
/// This interface defines a complete listener for a parse tree produced by
/// <see cref="RabotoraScriptFormatParser"/>.
/// </summary>
[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.6.6")]
[System.CLSCompliant(false)]
public interface IRabotoraScriptFormatListener : IParseTreeListener {
	/// <summary>
	/// Enter a parse tree produced by <see cref="RabotoraScriptFormatParser.funcWord"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterFuncWord([NotNull] RabotoraScriptFormatParser.FuncWordContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="RabotoraScriptFormatParser.funcWord"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitFuncWord([NotNull] RabotoraScriptFormatParser.FuncWordContext context);

	/// <summary>
	/// Enter a parse tree produced by <see cref="RabotoraScriptFormatParser.plainString"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterPlainString([NotNull] RabotoraScriptFormatParser.PlainStringContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="RabotoraScriptFormatParser.plainString"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitPlainString([NotNull] RabotoraScriptFormatParser.PlainStringContext context);

	/// <summary>
	/// Enter a parse tree produced by <see cref="RabotoraScriptFormatParser.value"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterValue([NotNull] RabotoraScriptFormatParser.ValueContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="RabotoraScriptFormatParser.value"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitValue([NotNull] RabotoraScriptFormatParser.ValueContext context);

	/// <summary>
	/// Enter a parse tree produced by <see cref="RabotoraScriptFormatParser.values"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterValues([NotNull] RabotoraScriptFormatParser.ValuesContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="RabotoraScriptFormatParser.values"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitValues([NotNull] RabotoraScriptFormatParser.ValuesContext context);

	/// <summary>
	/// Enter a parse tree produced by <see cref="RabotoraScriptFormatParser.cmd"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterCmd([NotNull] RabotoraScriptFormatParser.CmdContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="RabotoraScriptFormatParser.cmd"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitCmd([NotNull] RabotoraScriptFormatParser.CmdContext context);

	/// <summary>
	/// Enter a parse tree produced by <see cref="RabotoraScriptFormatParser.segment"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterSegment([NotNull] RabotoraScriptFormatParser.SegmentContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="RabotoraScriptFormatParser.segment"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitSegment([NotNull] RabotoraScriptFormatParser.SegmentContext context);

	/// <summary>
	/// Enter a parse tree produced by <see cref="RabotoraScriptFormatParser.segmentBody"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterSegmentBody([NotNull] RabotoraScriptFormatParser.SegmentBodyContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="RabotoraScriptFormatParser.segmentBody"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitSegmentBody([NotNull] RabotoraScriptFormatParser.SegmentBodyContext context);

	/// <summary>
	/// Enter a parse tree produced by <see cref="RabotoraScriptFormatParser.typeDef"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterTypeDef([NotNull] RabotoraScriptFormatParser.TypeDefContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="RabotoraScriptFormatParser.typeDef"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitTypeDef([NotNull] RabotoraScriptFormatParser.TypeDefContext context);

	/// <summary>
	/// Enter a parse tree produced by <see cref="RabotoraScriptFormatParser.typeDefs"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterTypeDefs([NotNull] RabotoraScriptFormatParser.TypeDefsContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="RabotoraScriptFormatParser.typeDefs"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitTypeDefs([NotNull] RabotoraScriptFormatParser.TypeDefsContext context);

	/// <summary>
	/// Enter a parse tree produced by <see cref="RabotoraScriptFormatParser.classDef"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterClassDef([NotNull] RabotoraScriptFormatParser.ClassDefContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="RabotoraScriptFormatParser.classDef"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitClassDef([NotNull] RabotoraScriptFormatParser.ClassDefContext context);

	/// <summary>
	/// Enter a parse tree produced by <see cref="RabotoraScriptFormatParser.funcDef"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterFuncDef([NotNull] RabotoraScriptFormatParser.FuncDefContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="RabotoraScriptFormatParser.funcDef"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitFuncDef([NotNull] RabotoraScriptFormatParser.FuncDefContext context);

	/// <summary>
	/// Enter a parse tree produced by <see cref="RabotoraScriptFormatParser.classSegment"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterClassSegment([NotNull] RabotoraScriptFormatParser.ClassSegmentContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="RabotoraScriptFormatParser.classSegment"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitClassSegment([NotNull] RabotoraScriptFormatParser.ClassSegmentContext context);

	/// <summary>
	/// Enter a parse tree produced by <see cref="RabotoraScriptFormatParser.funcSegment"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterFuncSegment([NotNull] RabotoraScriptFormatParser.FuncSegmentContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="RabotoraScriptFormatParser.funcSegment"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitFuncSegment([NotNull] RabotoraScriptFormatParser.FuncSegmentContext context);

	/// <summary>
	/// Enter a parse tree produced by <see cref="RabotoraScriptFormatParser.body"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterBody([NotNull] RabotoraScriptFormatParser.BodyContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="RabotoraScriptFormatParser.body"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitBody([NotNull] RabotoraScriptFormatParser.BodyContext context);

	/// <summary>
	/// Enter a parse tree produced by <see cref="RabotoraScriptFormatParser.script"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterScript([NotNull] RabotoraScriptFormatParser.ScriptContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="RabotoraScriptFormatParser.script"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitScript([NotNull] RabotoraScriptFormatParser.ScriptContext context);
}
} // namespace Rabotora.Core.Rsf