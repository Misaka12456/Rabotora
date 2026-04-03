using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace RabotoraX.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ComponentLifecycleAnalyzer : DiagnosticAnalyzer
{
	public const string DiagnosticId = "RAT0002";
	private const string TargetBaseClassName = "RabotoraX.Core.Component";

	private readonly static string[] ForbiddenMembers =
	[
		"RObject", "Layout", "GetComponent", "GetComponentInChildren", "StartCoroutine", "StopCoroutine"
	];
	
	private readonly static LocalizableString Title = "Do not access RObject-dependent members in constructor";
	private readonly static LocalizableString MessageFormat = "Accessing '{0}' in the constructor may cause unexpected behaviours because the Component is not fully initialized. Move this logic to OnAwake() or OnStart().";
	private readonly static LocalizableString Description = "Accessing RObject, Layout, or Find methods from constructors will result in a RabotoraException at runtime. These properties are only available after OnAwake has been called by the engine.";
	private const string Category = "Usage";
	
	private readonly static DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

	public override void Initialize(AnalysisContext context)
	{
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
		context.EnableConcurrentExecution();

		context.RegisterCompilationStartAction(compilationContext =>
		{
			var componentSymbol = compilationContext.Compilation.GetTypeByMetadataName(TargetBaseClassName);
			if (componentSymbol == null) return;

			// 注册对标识符和泛型名称的语法节点分析
			compilationContext.RegisterSyntaxNodeAction(syntaxContext => 
					AnalyzeNode(syntaxContext, componentSymbol), 
				SyntaxKind.IdentifierName, SyntaxKind.GenericName);
		});
	}

	private static void AnalyzeNode(SyntaxNodeAnalysisContext context, INamedTypeSymbol componentBaseSymbol)
	{
		var node = context.Node;
        
		string? name = node switch
		{
			IdentifierNameSyntax ins => ins.Identifier.Text,
			GenericNameSyntax gns => gns.Identifier.Text,
			_ => null
		};

		if (name == null || !ForbiddenMembers.Contains(name))
			return;

		var constructorDecl = node.Ancestors().OfType<ConstructorDeclarationSyntax>().FirstOrDefault();
		if (constructorDecl == null)
			return;

		var symbolInfo = context.SemanticModel.GetSymbolInfo(node);
		var symbol = symbolInfo.Symbol;
		if (symbol == null) return;

		if (!InheritsFrom(symbol.ContainingType, componentBaseSymbol))
			return;

		var diagnostic = Diagnostic.Create(Rule, node.GetLocation(), name);
		context.ReportDiagnostic(diagnostic);
	}

	private static bool InheritsFrom(ITypeSymbol type, INamedTypeSymbol baseType)
	{
		var current = type;
		while (current != null)
		{
			if (SymbolEqualityComparer.Default.Equals(current, baseType))
				return true;
			current = current.BaseType;
		}
		return false;
	}
}