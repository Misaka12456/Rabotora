using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace RabotoraX.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class RaTweenAwaitAnalyzer : DiagnosticAnalyzer
{
	public const string DiagnosticId = "RAT0003";
	private const string TargetBaseTypeName = "RabotoraX.Core.Tweening.RaTweener"; // RaTweener (base) <- RaTweenCore<T> (the common return type of all tweening methods)
	
	private readonly static LocalizableString Title = "Directly awaiting a RaTweener lacks semantic clarity";
	private readonly static LocalizableString MessageFormat = "Consider using AsyncWaitForCompletion() instead of directly awaiting the RaTweener for better semantic clarity";
	private readonly static LocalizableString Description = "Directly awaiting a tween object does not explicitly express the intent of waiting for its completion. " +
	                                                        "Use the explicit AsyncWaitForCompletion() method instead.";
	private const string Category = "Readability";
	
	private readonly static DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

	public override void Initialize(AnalysisContext context)
	{
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
		context.EnableConcurrentExecution();

		context.RegisterCompilationStartAction(compilationContext =>
		{
			var tweenerSymbol = compilationContext.Compilation.GetTypeByMetadataName(TargetBaseTypeName);
			if (tweenerSymbol == null) return;

			compilationContext.RegisterSyntaxNodeAction(syntaxContext => AnalyzeAwaitExpression(syntaxContext, tweenerSymbol), SyntaxKind.AwaitExpression);
		});
	}
	
	private static void AnalyzeAwaitExpression(SyntaxNodeAnalysisContext context, INamedTypeSymbol tweenerBaseSymbol)
	{
		var awaitExpr = (AwaitExpressionSyntax)context.Node;
		var awaitedExpr = awaitExpr.Expression;

		var typeInfo = context.SemanticModel.GetTypeInfo(awaitedExpr);
		var typeSymbol = typeInfo.Type ?? typeInfo.ConvertedType;

		if (typeSymbol == null) return;

		if (InheritsFrom(typeSymbol, tweenerBaseSymbol))
		{
			var diagnostic = Diagnostic.Create(Rule, awaitedExpr.GetLocation(), typeSymbol.Name);
			context.ReportDiagnostic(diagnostic);
		}
	}
	
	private static bool InheritsFrom(ITypeSymbol type, INamedTypeSymbol baseType)
	{
		var current = type;
		while (current != null)
		{
			if (SymbolEqualityComparer.Default.Equals(current, baseType))
			{
				return true;
			}
			current = current.BaseType;
		}
		return false;
	}
}