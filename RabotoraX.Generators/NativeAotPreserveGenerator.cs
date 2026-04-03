using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace RabotoraX.Generators;

[Generator]
public sealed class NativeAotPreserveGenerator : IIncrementalGenerator
{
	private sealed record TypeCandidate(INamedTypeSymbol TypeSymbol, Location Location);
	
	private readonly static Version GeneratorVersion = typeof(NativeAotPreserveGenerator).Assembly.GetName().Version ?? new Version(1, 0, 0);
	private readonly static SymbolDisplayFormat TypeReferenceFormat = SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Included);
	private readonly static SymbolDisplayFormat TypeKeyFormat = SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted);
	
	private readonly static ImmutableArray<string> SerializablePrerequisites =
	[
		"RabotoraX.Core.Serialization.RBinarySerializableAttribute",
		"RabotoraX.Core.Serialization.RBinarySerializableAttribute`1",
		"System.SerializableAttribute"
	];
	
	private readonly static DiagnosticDescriptor DiagNonEmptyCtor = new("RATSG001",
		"Classes, structs, records and record structs marked with [RBinarySerializable] or [System.Serializable] must have a parameterless constructor",
		"Type '{0}' is marked with [RBinarySerializable] or [System.Serializable] but does not have a parameterless constructor. " +
		"Please add a parameterless constructor to this type for Native AOT Source Generation to work correctly.", "RabotoraX.Generators", DiagnosticSeverity.Error, true);

	private readonly static DiagnosticDescriptor DiagGenericMissingAttr = new("RATSG002",
		"Generic types must specify concrete types for Ahead-of-Time(AOT) generation",
		"Generic type '{0}' is marked as serializable but lacks generic [RBinarySerializable<T>] attributes. " +
		"Native AOT Source Generation requires concrete type information for generic types.",
		"RabotoraX.Generators", DiagnosticSeverity.Warning, true);
	
	private readonly static DiagnosticDescriptor DiagGenericArityMismatch = new("RATSG003",
		"Generic arguments mismatch",
		"The number of generic arguments in the attrbute does not match the generic type '{0}'. " +
		"Please ensure that the generic [RBinarySerializable<T>] attribute specifies the correct number of generic type parameters.",
		"RabotoraX.Generators", DiagnosticSeverity.Error, true);

	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		var classDeclarations = context.SyntaxProvider.CreateSyntaxProvider(
			predicate: static (s, _) => s is ClassDeclarationSyntax or StructDeclarationSyntax or RecordDeclarationSyntax,
			transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx))
			.Where(static m => m is not null);
		
		var compilationAndTypes = context.CompilationProvider.Combine(classDeclarations.Collect());
		
		context.RegisterSourceOutput(compilationAndTypes, static (spc, source) => Execute(source.Left, source.Right, spc));
	}

	private static TypeCandidate? GetSemanticTargetForGeneration(GeneratorSyntaxContext ctx)
	{
		if (ctx.Node is TypeDeclarationSyntax typeDeclaration and (ClassDeclarationSyntax or StructDeclarationSyntax or RecordDeclarationSyntax))
		{
			if (ctx.SemanticModel.GetDeclaredSymbol(typeDeclaration) is not INamedTypeSymbol typeSymbol) return null;
			return new TypeCandidate(typeSymbol, typeDeclaration.GetLocation());
		}
		return null;
	}

	private static void Execute(Compilation compilation, ImmutableArray<TypeCandidate?> types, SourceProductionContext ctx)
	{
		if (types.IsDefaultOrEmpty) return;

		var attrs = SerializablePrerequisites.Select(compilation.GetTypeByMetadataName)
			.Where(t => t is not null).ToImmutableArray();
		if (attrs.IsDefaultOrEmpty) return;
		
		var emitted = new Dictionary<string, (string TypeRef, bool IsAbstract)>(StringComparer.Ordinal); // "Namespace.TypeName" -> ("global::Namespace.TypeName", IsAbstract)

		foreach (var candidate in types)
		{
			if (candidate is null) continue;
			
			var typeSymbol = candidate.TypeSymbol;
			if (!IsTypeSerializable(typeSymbol, attrs)) continue;
			
			if (!typeSymbol.IsAbstract && !HasPublicParameterlessConstructor(typeSymbol))
			{
				ctx.ReportDiagnostic(Diagnostic.Create(DiagNonEmptyCtor, candidate.Location, typeSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)));
			}

			if (typeSymbol.IsGenericType)
			{
				GenerateCodeForGenericType(ctx, typeSymbol, emitted, candidate);
			}
			else
			{
				string typeRef = typeSymbol.ToDisplayString(TypeReferenceFormat);
				string typeKey = typeSymbol.ToDisplayString(TypeKeyFormat);
				if (!emitted.ContainsKey(typeKey))
				{
					emitted[typeKey] = (typeRef, typeSymbol.IsAbstract);
				}
			}
		}

		if (emitted.Count == 0) return;

		string rawAsmName = compilation.AssemblyName ?? "Unknown";
		string sanitizedNamespace = SantizeNamespace(rawAsmName);
		string fileSafeAsmName = sanitizedNamespace.Replace('.', '_');
		var sb = new StringBuilder();
		
		sb.AppendLine($$"""
		/// <auto-generated>
		/// This code was auto-generated by RabotoraX Source Generators. Do not modify this file manually.
		/// SourceGen Version: {{GeneratorVersion}}
		/// </auto-generated>
		using global::System;
		using global::System.Runtime.CompilerServices;
		using global::System.Diagnostics.CodeAnalysis;

		namespace {{sanitizedNamespace}}.Generated
		{
		    internal static class NativeAOTDeclarations
		    {
		""");

		// Add DynamicDependency for everything so the Trimmer keeps fields/properties safely
		foreach (var kvp in emitted)
		{
			sb.AppendLine($"        [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof({kvp.Value.TypeRef}))]"); // typeof(global::Namespace.TypeName) instead of typeof(Namespace.TypeName)
		}

		sb.AppendLine("""
		        [ModuleInitializer]
		        internal static void Initialize()
		        {
		""");

		foreach (var kvp in emitted)
		{
			string typeKey = kvp.Key;
			string typeRef = kvp.Value.TypeRef;
			// ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
			if (kvp.Value.IsAbstract) // IsAbstract
			{
				sb.AppendLine($"            global::RabotoraX.Core.Serialization.RTypeRegistry.Register(\"{typeKey}\", typeof({typeRef}), null);");
			}
			else
			{
				sb.AppendLine($"            global::RabotoraX.Core.Serialization.RTypeRegistry.Register(\"{typeKey}\", typeof({typeRef}), static () => new {typeRef}());");
			}
		}

		sb.AppendLine("""
		        }
		    }
		}
		""");
		
		ctx.AddSource($"NativeAotPreserve_{fileSafeAsmName}.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
	}

	private static void GenerateCodeForGenericType(SourceProductionContext ctx, INamedTypeSymbol typeSymbol, Dictionary<string, (string TypeRef, bool IsAbstract)> emitted, TypeCandidate candidate)
	{
		bool foundConcreteType = false;
		foreach (var attrData in typeSymbol.GetAttributes())
		{
			var attrClass = attrData.AttributeClass;
			if (attrClass is null || !attrClass.IsGenericType) continue;
					
			string attrName = attrClass.ConstructedFrom.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
			if (attrName is "global::RabotoraX.Core.Serialization.RBinarySerializableAttribute<T>")
			{
				if (attrClass.TypeArguments.Length == typeSymbol.TypeParameters.Length)
				{
					var constructedType = typeSymbol.Construct(attrClass.TypeArguments.ToArray());
					string typeRef = constructedType.ToDisplayString(TypeReferenceFormat);
					string typeKey = constructedType.ToDisplayString(TypeKeyFormat);

					if (!emitted.ContainsKey(typeKey))
					{
						emitted[typeKey] = (typeRef, constructedType.IsAbstract);
					}
					foundConcreteType = true;
				}
				else
				{
					ctx.ReportDiagnostic(Diagnostic.Create(DiagGenericArityMismatch, candidate.Location, typeSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)));
				}
			}
		}

		if (!foundConcreteType)
		{
			ctx.ReportDiagnostic(Diagnostic.Create(DiagGenericMissingAttr, candidate.Location, typeSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)));
		}
	}

	private static bool IsTypeSerializable(ITypeSymbol symbol, ImmutableArray<INamedTypeSymbol?> attrs)
	{
		var current = symbol;
		while (current is not null)
		{
			foreach (var attr in current.GetAttributes())
			{
				var attrClass = attr.AttributeClass;
				if (attrClass is null) continue;
				var originalDefinition = attrClass.OriginalDefinition;
				if (attrs.Any(a => SymbolEqualityComparer.Default.Equals(a, originalDefinition))) return true;
			}
			current = current.BaseType;
		}
		return false;
	}
	
	private static bool HasPublicParameterlessConstructor(INamedTypeSymbol typeSymbol)
	{
		if (typeSymbol.IsValueType) return true;
		foreach (var ctor in typeSymbol.Constructors)
		{
			if (ctor.Parameters.Length == 0)
			{
				if (ctor.DeclaredAccessibility == Accessibility.Public) return true;
				if (typeSymbol.IsAbstract && ctor.DeclaredAccessibility is Accessibility.Protected or Accessibility.ProtectedOrInternal) return true;
			}
		}
		return false;
	}

	[SuppressMessage("ReSharper", "ArrangeRedundantParentheses")]
	private static string SantizeNamespace(string text)
	{
		if (string.IsNullOrWhiteSpace(text)) return "Unknown";
		var sb = new StringBuilder(text.Length);
		foreach (char ch in text)
		{
			sb.Append((char.IsLetterOrDigit(ch) || ch == '.') ? ch : '_');
		}
		return sb.ToString();
	}
}