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
using RabotoraX.Analyzers;

namespace RabotoraX.CodeFix;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(SerializationSetterCodeFixProvider)), Shared]
public sealed class SerializationSetterCodeFixProvider : CodeFixProvider
{
	public override ImmutableArray<string> FixableDiagnosticIds => [SerializationSetterAnalyzer.DiagnosticId];

	public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

	public override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
		if (root == null) return;
		
		var diagnostic = context.Diagnostics.First();
		var diagnosticSpan = diagnostic.Location.SourceSpan;

		var declProperty = root.FindNode(diagnosticSpan).FirstAncestorOrSelf<PropertyDeclarationSyntax>();
		if (declProperty == null) return;

		context.RegisterCodeFix(
			CodeAction.Create(
				title: "Add 'set;' accessor (setter) to this property",
				createChangedDocument: c => AddSetterAsync(context.Document, root, declProperty, c),
				equivalenceKey: nameof(SerializationSetterCodeFixProvider)),
			diagnostic);
	}

	private static Task<Document> AddSetterAsync(Document document, SyntaxNode root, PropertyDeclarationSyntax declProperty, CancellationToken ct)
	{
		ct.ThrowIfCancellationRequested();
		
		var setter = SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
			.WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
		
		PropertyDeclarationSyntax newDeclProperty;

		if (declProperty.AccessorList != null)
		{
			var newList = declProperty.AccessorList.AddAccessors(setter);
			newDeclProperty = declProperty.WithAccessorList(newList);
		}
		else
		{
			var getter = SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
				.WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
			
			var accessorList = SyntaxFactory.AccessorList(SyntaxFactory.List([getter, setter]));
			newDeclProperty = declProperty.WithExpressionBody(null)
				.WithSemicolonToken(default)
				.WithAccessorList(accessorList);
		}
		
		var newRoot = root.ReplaceNode(declProperty, newDeclProperty);
		return Task.FromResult(document.WithSyntaxRoot(newRoot));
	}
}