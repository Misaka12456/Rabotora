﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     ANTLR Version: 4.6.6
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from F:\C#\Misaka12456 (Misaka Castle)\Rabotora\Rabotora.Core\RabotoraScriptFormat\RabotoraScriptFormat.g4 by ANTLR 4.6.6

// Unreachable code detected
#pragma warning disable 0162
// The variable '...' is assigned but its value is never used
#pragma warning disable 0219
// Missing XML comment for publicly visible type or member '...'
#pragma warning disable 1591
// Ambiguous reference in cref attribute
#pragma warning disable 419

namespace Rabotora.Core.RabotoraScriptFormat {
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using IToken = Antlr4.Runtime.IToken;

/// <summary>
/// This interface defines a complete generic visitor for a parse tree produced
/// by <see cref="RabotoraScriptFormatParser"/>.
/// </summary>
/// <typeparam name="Result">The return type of the visit operation.</typeparam>
[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.6.6")]
public interface IRabotoraScriptFormatVisitor<Result> : IParseTreeVisitor<Result> {
	/// <summary>
	/// Visit a parse tree produced by <see cref="RabotoraScriptFormatParser.plainString"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitPlainString([NotNull] RabotoraScriptFormatParser.PlainStringContext context);

	/// <summary>
	/// Visit a parse tree produced by <see cref="RabotoraScriptFormatParser.value"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitValue([NotNull] RabotoraScriptFormatParser.ValueContext context);

	/// <summary>
	/// Visit a parse tree produced by <see cref="RabotoraScriptFormatParser.values"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitValues([NotNull] RabotoraScriptFormatParser.ValuesContext context);

	/// <summary>
	/// Visit a parse tree produced by <see cref="RabotoraScriptFormatParser.cmd"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitCmd([NotNull] RabotoraScriptFormatParser.CmdContext context);

	/// <summary>
	/// Visit a parse tree produced by <see cref="RabotoraScriptFormatParser.segment"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitSegment([NotNull] RabotoraScriptFormatParser.SegmentContext context);

	/// <summary>
	/// Visit a parse tree produced by <see cref="RabotoraScriptFormatParser.segmentBody"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitSegmentBody([NotNull] RabotoraScriptFormatParser.SegmentBodyContext context);

	/// <summary>
	/// Visit a parse tree produced by <see cref="RabotoraScriptFormatParser.typeDef"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitTypeDef([NotNull] RabotoraScriptFormatParser.TypeDefContext context);

	/// <summary>
	/// Visit a parse tree produced by <see cref="RabotoraScriptFormatParser.typeDefs"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitTypeDefs([NotNull] RabotoraScriptFormatParser.TypeDefsContext context);

	/// <summary>
	/// Visit a parse tree produced by <see cref="RabotoraScriptFormatParser.classDef"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitClassDef([NotNull] RabotoraScriptFormatParser.ClassDefContext context);

	/// <summary>
	/// Visit a parse tree produced by <see cref="RabotoraScriptFormatParser.funcDef"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitFuncDef([NotNull] RabotoraScriptFormatParser.FuncDefContext context);

	/// <summary>
	/// Visit a parse tree produced by <see cref="RabotoraScriptFormatParser.classSegment"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitClassSegment([NotNull] RabotoraScriptFormatParser.ClassSegmentContext context);

	/// <summary>
	/// Visit a parse tree produced by <see cref="RabotoraScriptFormatParser.funcSegment"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitFuncSegment([NotNull] RabotoraScriptFormatParser.FuncSegmentContext context);

	/// <summary>
	/// Visit a parse tree produced by <see cref="RabotoraScriptFormatParser.body"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitBody([NotNull] RabotoraScriptFormatParser.BodyContext context);

	/// <summary>
	/// Visit a parse tree produced by <see cref="RabotoraScriptFormatParser.script"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitScript([NotNull] RabotoraScriptFormatParser.ScriptContext context);
}
} // namespace Rabotora.Core.RabotoraScriptFormat
