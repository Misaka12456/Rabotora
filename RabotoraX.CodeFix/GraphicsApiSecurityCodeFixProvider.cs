using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Simplification;
using RabotoraX.Analyzers;

namespace RabotoraX.CodeFix;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GraphicsApiSecurityCodeFixProvider)), Shared]
public class GraphicsApiSecurityCodeFixProvider : CodeFixProvider
{
    private const string TargetReplacement = "RabotoraX.Core.Render.GraphicsService.LatestWindowState";

    public override ImmutableArray<string> FixableDiagnosticIds => [GraphicsApiSecurityAnalyzer.DiagnosticId];

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root == null) return;

        var diagnostic = context.Diagnostics.First();
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        var memberAccess = root.FindNode(diagnosticSpan).FirstAncestorOrSelf<MemberAccessExpressionSyntax>();
        if (memberAccess == null) return;

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Use GraphicsService.LatestWindowState",
                createChangedDocument: c => ReplaceAndSimplifyAsync(context.Document, memberAccess, c),
                equivalenceKey: nameof(GraphicsApiSecurityCodeFixProvider)),
            diagnostic);
    }

    private static async Task<Document> ReplaceAndSimplifyAsync(Document document, MemberAccessExpressionSyntax memberAccess, CancellationToken cancellationToken)
    {
        var replacementSyntax = SyntaxFactory.ParseName(TargetReplacement)
            .WithAdditionalAnnotations(Simplifier.Annotation) 
            .WithTriviaFrom(memberAccess);

        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root == null) return document;

        // 2. 替换旧节点
        var newRoot = root.ReplaceNode(memberAccess, replacementSyntax);
        var newDocument = document.WithSyntaxRoot(newRoot);
        return await Simplifier.ReduceAsync(newDocument, cancellationToken: cancellationToken).ConfigureAwait(false);
    }
}