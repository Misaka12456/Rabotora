using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using RabotoraX.Analyzers;

namespace RabotoraX.CodeFix;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(SerializationAttributeTargetCodeFixProvider)), Shared]
public sealed class SerializationAttributeTargetCodeFixProvider : CodeFixProvider
{
	public override ImmutableArray<string> FixableDiagnosticIds => [SerializationAttributeTargetAnalyzer.DiagnosticId];
	public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;
	
	
	public override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
		var diagnostic = context.Diagnostics.First();
		var diagnosticSpan = diagnostic.Location.SourceSpan;
		
		var attribute = root?.FindToken(diagnosticSpan.Start).Parent?.AncestorsAndSelf().OfType<AttributeSyntax>().FirstOrDefault();
		if (attribute == null) return;

		context.RegisterCodeFix(
			CodeAction.Create(
				title: "Add 'field:' target specifier to the backing field of the auto-property",
				createChangedDocument: ct => FixAttributeTargetAsync(context.Document, attribute, ct),
				equivalenceKey: "AddRSerializableFieldTarget"),
			diagnostic);
	}

	private static async Task<Document> FixAttributeTargetAsync(Document document, AttributeSyntax attribute, CancellationToken ct)
	{
		var root = await document.GetSyntaxRootAsync(ct).ConfigureAwait(false);
		if (root == null) return document;
		
		var oldAttributeList = (AttributeListSyntax)attribute.Parent!;
		var declProperty = oldAttributeList.FirstAncestorOrSelf<PropertyDeclarationSyntax>();
		if (declProperty == null) return document;

		var fieldTarget = SyntaxFactory.AttributeTargetSpecifier(SyntaxFactory.Token(SyntaxKind.FieldKeyword)).WithTrailingTrivia(SyntaxFactory.Space);
		if (oldAttributeList.Attributes.Count == 1)
		{
			var newAttrList = oldAttributeList.WithTarget(fieldTarget);
			var newRoot = root.ReplaceNode(oldAttributeList, newAttrList);
			return document.WithSyntaxRoot(newRoot);
		}
		else
		{
			var newOldAttrList = oldAttributeList.RemoveNode(attribute, SyntaxRemoveOptions.KeepNoTrivia);
			
			var newAttrList = SyntaxFactory.AttributeList(fieldTarget, SyntaxFactory.SingletonSeparatedList(attribute.WithoutTrivia()))
				.WithLeadingTrivia(oldAttributeList.GetLeadingTrivia())
				.WithAdditionalAnnotations(Formatter.Annotation);

			var newDeclProperty = declProperty.ReplaceNode(oldAttributeList, newOldAttrList!);
			int listIndex = declProperty.AttributeLists.IndexOf(oldAttributeList);
			
			newDeclProperty = newDeclProperty.WithAttributeLists(newDeclProperty.AttributeLists.Insert(listIndex, newAttrList));
			
			var newRoot = root.ReplaceNode(declProperty, newDeclProperty);
			return document.WithSyntaxRoot(newRoot);
		}
	}
}