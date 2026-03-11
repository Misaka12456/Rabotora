using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace RabotoraX.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class GraphicsApiSecurityAnalyzer : DiagnosticAnalyzer
{
	public const string DiagnosticId = "RAT0001";
	private const string TargetInterfaceName = "RabotoraX.Core.Graphics.INativeGraphicsAPI";

	private readonly static LocalizableString Title = "Do not access FramebufferSize directly";
	private readonly static LocalizableString MessageFormat = "Accessing FramebufferSize directly from {0} may cause thread deadlocks. Use GraphicsService.LatestWindowState to get window size instead.";
	private readonly static LocalizableString Description = "Accessing size properties from physical graphics API in multi-thread rendering architectures can lead to main thread lock races or deadlocks. " +
	                                                        "WindowStateSnapshot is designed to be synchronized with the main thread and can be safely accessed from any thread.";
	private const string Category = "Usage";

	private readonly static DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category,
		DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

	public override void Initialize(AnalysisContext context)
	{
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
		context.EnableConcurrentExecution();
		
		context.RegisterCompilationStartAction(compilationContext =>
		{
			var interfaceSymbol = compilationContext.Compilation.GetTypeByMetadataName(TargetInterfaceName);
			if (interfaceSymbol == null) return;

			compilationContext.RegisterSyntaxNodeAction(syntaxContext => 
					AnalyzeMemberAccess(syntaxContext, interfaceSymbol), 
				SyntaxKind.SimpleMemberAccessExpression);
		});
	}

	private void AnalyzeMemberAccess(SyntaxNodeAnalysisContext context, INamedTypeSymbol targetInterface)
	{
		var memberAccess = (MemberAccessExpressionSyntax)context.Node;
		
		if (memberAccess.Name.Identifier.Text != "FramebufferSize")
			return;

		var symbolInfo = context.SemanticModel.GetSymbolInfo(memberAccess);
		if (symbolInfo.Symbol is not IPropertySymbol propertySymbol) return;

		var containingType = propertySymbol.ContainingType;

		bool isProblematic = SymbolEqualityComparer.Default.Equals(containingType, targetInterface) ||
		                     containingType.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i, targetInterface));

		if (isProblematic)
		{
			var diagnostic = Diagnostic.Create(Rule, memberAccess.Name.GetLocation(), containingType.Name);
			context.ReportDiagnostic(diagnostic);
		}
	}
}