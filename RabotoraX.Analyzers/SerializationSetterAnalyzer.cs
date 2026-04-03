using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace RabotoraX.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class SerializationSetterAnalyzer : DiagnosticAnalyzer
{
	public const string DiagnosticId = "RAT0004";
	private const string TargetAttributeName = "RabotoraX.Core.Serialization.RSerializableFieldAttribute";
	private readonly static LocalizableString Title = "Serializable property must have a setter";
	private readonly static LocalizableString MessageFormat = "The property '{0}' is marked for backing field serialization but lacks a setter. Data will not be included during serialization and deserialization.";
	private readonly static LocalizableString Description = "Properties marked with [field: RSerializableField] must have a setter because the underlying backing field of a getter-only property is read-only, " +
	                                                        "which cannot be serialized or deserialized by the RBinaryFormatter. " +
	                                                        "To fix this issue, add a setter to the property or remove the [field: RSerializableField] attribute if you intend to not serialize this property.";
	private const string Category = "Usage";
	private readonly static DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category,
		DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);
	
	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];
	
	public override void Initialize(AnalysisContext context)
	{
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
		context.EnableConcurrentExecution();
		
		context.RegisterCompilationStartAction(ctxCompilation =>
		{
			var attributesSymbol = ctxCompilation.Compilation.GetTypeByMetadataName(TargetAttributeName);
			if (attributesSymbol == null) return;

			ctxCompilation.RegisterSyntaxNodeAction(ctxSyntax => AnalyzeProperty(ctxSyntax, attributesSymbol), SyntaxKind.PropertyDeclaration);
		});
	}

	private static void AnalyzeProperty(SyntaxNodeAnalysisContext context, INamedTypeSymbol attributeSymbol)
	{
		var declProperty = (PropertyDeclarationSyntax)context.Node;

		bool hasFieldAttribute = declProperty.AttributeLists.Any(list =>
			list.Target?.Identifier.IsKind(SyntaxKind.FieldKeyword) == true &&
			list.Attributes.Any(attr => IsTargetAttribute(context, attr, attributeSymbol)));
		
		if (!hasFieldAttribute) return;

		if (context.SemanticModel.GetDeclaredSymbol(declProperty) is {SetMethod: null})
		{
			var diagnostic = Diagnostic.Create(Rule, declProperty.Identifier.GetLocation(), declProperty.Identifier.Text);
			context.ReportDiagnostic(diagnostic);
		}
	}

	private static bool IsTargetAttribute(SyntaxNodeAnalysisContext context, AttributeSyntax attr, INamedTypeSymbol targetSymbol)
	{
		var symbolInfo = context.SemanticModel.GetSymbolInfo(attr);
		return SymbolEqualityComparer.Default.Equals(symbolInfo.Symbol?.ContainingType, targetSymbol);
	}
}