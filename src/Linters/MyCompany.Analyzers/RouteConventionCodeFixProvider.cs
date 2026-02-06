using System.Collections.Immutable;
using System.Composition;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MyCompany.Analyzers;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RouteConventionCodeFixProvider)), Shared]
public class RouteConventionCodeFixProvider : CodeFixProvider
{
    public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(RouteConventionAnalyzer.DiagnosticId);

    public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        var diagnostic = context.Diagnostics.First();
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        // Find the string literal token
        var literalExpr = root?.FindToken(diagnosticSpan.Start).Parent as LiteralExpressionSyntax;
        if (literalExpr == null) return;

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Convert to kebab-case",
                createChangedDocument: c => FixRouteAsync(context.Document, literalExpr, c),
                equivalenceKey: nameof(RouteConventionCodeFixProvider)),
            diagnostic);
    }

    private async Task<Document> FixRouteAsync(Document document, LiteralExpressionSyntax literalExpr, CancellationToken cancellationToken)
    {
        var originalRoute = literalExpr.Token.ValueText;
        
        // Very naive conversion:
        // 1. "GetUsers" -> "get-users" 
        // 2. "UserProfile" -> "user-profile"
        // 3. Keep {id} intact
        
        string fixedRoute = ToKebabCase(originalRoute);

        var newLiteral = SyntaxFactory.LiteralExpression(
            SyntaxKind.StringLiteralExpression,
            SyntaxFactory.Literal(fixedRoute));

        var root = await document.GetSyntaxRootAsync(cancellationToken);
        if (root == null) return document;

        var newRoot = root.ReplaceNode(literalExpr, newLiteral);
        return document.WithSyntaxRoot(newRoot);
    }

    private string ToKebabCase(string input)
    {
        // Keep parameters like {id} safe from conversion
        return Regex.Replace(input, @"(?<!^)([A-Z])", "-$1").ToLower();
    }
}
