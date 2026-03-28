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

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ComponentLifecycleCodeFixProvider)), Shared]
public class ComponentLifecycleCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds => [ComponentLifecycleAnalyzer.DiagnosticId];

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root == null) return;

        var diagnostic = context.Diagnostics.First();
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        var node = root.FindNode(diagnosticSpan);
        var constructorDecl = node.AncestorsAndSelf().OfType<ConstructorDeclarationSyntax>().FirstOrDefault();
        
        if (constructorDecl == null) return;

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Move initialization to OnAwake()",
                createChangedDocument: c => ReplaceConstructorWithOnAwakeAsync(context.Document, root, constructorDecl, c),
                equivalenceKey: nameof(ComponentLifecycleCodeFixProvider)),
            diagnostic);
    }

    // ReSharper disable once UnusedParameter.Local
    private static Task<Document> ReplaceConstructorWithOnAwakeAsync(Document document, SyntaxNode root, ConstructorDeclarationSyntax constructorDecl, CancellationToken cancellationToken)
    {
        var methodModifiers = SyntaxFactory.TokenList(
            SyntaxFactory.Token(SyntaxKind.PublicKeyword),
            SyntaxFactory.Token(SyntaxKind.OverrideKeyword));

        var returnType = SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword));

        var methodDecl = SyntaxFactory.MethodDeclaration(returnType, "OnAwake")
            .WithModifiers(methodModifiers)
            .WithLeadingTrivia(constructorDecl.GetLeadingTrivia())
            .WithTrailingTrivia(constructorDecl.GetTrailingTrivia());

        var baseCallStatement = SyntaxFactory.ExpressionStatement(
            SyntaxFactory.InvocationExpression(
                SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    SyntaxFactory.BaseExpression(),
                    SyntaxFactory.IdentifierName("OnAwake"))));

        if (constructorDecl.Body != null)
        {
            var newStatements = constructorDecl.Body.Statements.Insert(0, baseCallStatement);
            var newBody = SyntaxFactory.Block(newStatements);
            methodDecl = methodDecl.WithBody(newBody);
        }
        else if (constructorDecl.ExpressionBody != null)
        {
            var exprStatement = SyntaxFactory.ExpressionStatement(constructorDecl.ExpressionBody.Expression);
            var newBody = SyntaxFactory.Block(baseCallStatement, exprStatement);
            methodDecl = methodDecl.WithBody(newBody);
        }
        else
        {
            methodDecl = methodDecl.WithBody(SyntaxFactory.Block(baseCallStatement));
        }

        var newRoot = root.ReplaceNode(constructorDecl, methodDecl);
        return Task.FromResult(document.WithSyntaxRoot(newRoot));
    }
}