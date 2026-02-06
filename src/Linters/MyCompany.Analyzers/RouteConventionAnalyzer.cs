using System.Collections.Immutable;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MyCompany.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RouteConventionAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "API001";
    private static readonly LocalizableString Title = "Route templates should be lowercase kebab-case";
    private static readonly LocalizableString MessageFormat = "Route '{0}' contains uppercase letters or non-kebab-case format";
    private static readonly LocalizableString Description = "REST API routes should follow lowercase kebab-case convention (e.g., 'get-users').";
    private const string Category = "Naming";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeAttribute, SyntaxKind.Attribute);
    }

    private void AnalyzeAttribute(SyntaxNodeAnalysisContext context)
    {
        var attributeSyntax = (AttributeSyntax)context.Node;

        // Simple check: looking for [Route("...")] or [Http*(...)]
        // In real world, resolve symbol to check if it really inherits from ASP.NET Core attributes
        var name = attributeSyntax.Name.ToString();
        
        if (!name.Contains("Route") && !name.StartsWith("Http"))
        {
            return;
        }

        if (attributeSyntax.ArgumentList?.Arguments.Count > 0)
        {
            var firstArg = attributeSyntax.ArgumentList.Arguments[0];
            if (firstArg.Expression is LiteralExpressionSyntax literal && 
                literal.IsKind(SyntaxKind.StringLiteralExpression))
            {
                var routeTemplate = literal.Token.ValueText;

                // Ignore params like {id}
                var cleanRoute = Regex.Replace(routeTemplate, @"\{.*?\}", ""); 

                if (HasUppercase(cleanRoute)) // Simple check, can be expanded to check for non-hyphen separators
                {
                    var diagnostic = Diagnostic.Create(Rule, literal.GetLocation(), routeTemplate);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }

    private static bool HasUppercase(string input)
    {
        return input.Any(char.IsUpper);
    }
}
