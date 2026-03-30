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

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RaTweenAwaitCodeFixProvider)), Shared]
public class RaTweenAwaitCodeFixProvider : CodeFixProvider
{
    private const string RequiredNamespace = "RabotoraX.Core.Threading.Tasks"; // Namespace where RTaskExtensions.AsyncWaitForCompletion is defined.

    public override ImmutableArray<string> FixableDiagnosticIds => [RaTweenAwaitAnalyzer.DiagnosticId];

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root == null) return;

        var diagnostic = context.Diagnostics.First();
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        var expressionToFix = root.FindNode(diagnosticSpan, getInnermostNodeForTie: true).FirstAncestorOrSelf<ExpressionSyntax>();
        
        if (expressionToFix == null) return;

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Append .AsyncWaitForCompletion()",
                createChangedDocument: c => FixAsync(context.Document, root, expressionToFix, c),
                equivalenceKey: nameof(RaTweenAwaitCodeFixProvider)),
            diagnostic);
    }

    private static Task<Document> FixAsync(Document document, SyntaxNode root, ExpressionSyntax originalExpr, CancellationToken cancellationToken)
    {
        // <originalExpr>.AsyncWaitForCompletion
        var memberAccess = SyntaxFactory.MemberAccessExpression(
            SyntaxKind.SimpleMemberAccessExpression,
            originalExpr.WithoutTrailingTrivia(),
            SyntaxFactory.IdentifierName("AsyncWaitForCompletion"));

        // <originalExpr>.AsyncWaitForCompletion()
        var invocation = SyntaxFactory.InvocationExpression(memberAccess)
            .WithArgumentList(SyntaxFactory.ArgumentList())
            .WithTrailingTrivia(originalExpr.GetTrailingTrivia());
        
        var newRoot = root.ReplaceNode(originalExpr, invocation);

        var compilationUnit = (CompilationUnitSyntax)newRoot;
        var requiredUsingName = SyntaxFactory.ParseName(RequiredNamespace);

        bool usingExists = compilationUnit.Usings
            .Any(u => u.Name?.ToString() == RequiredNamespace);

        if (!usingExists)
        {
            var newUsing = SyntaxFactory.UsingDirective(requiredUsingName).WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed);
            
            // using RabotoraX.Core.Threading.Tasks; -- to ensure RTaskExtensions.AsyncWaitForCompletion is in scope for the code fix.
            newRoot = compilationUnit.AddUsings(newUsing);
        }

        return Task.FromResult(document.WithSyntaxRoot(newRoot));
    }
}