using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using XamlConstructor.Generator.Extensions;

namespace XamlConstructor.Generator.Analyzers;

/// <summary>
/// Reports the declarations that <c>[XamlConstructor]</c> cannot be applied to, so the failure appears
/// as a diagnostic on the declaration instead of a compile error in generated code.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class TypeWithoutPartialAnalyzer : DiagnosticAnalyzer
{
    /// <summary>The rules this analyzer can report.</summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
        DiagnosticDescriptors.TypeWithoutPartialRule,
        DiagnosticDescriptors.NestedTypeNotSupportedRule,
        DiagnosticDescriptors.TypeNameDoesNotEndWithViewModelRule,
        DiagnosticDescriptors.ConflictingConstructorRule,
        DiagnosticDescriptors.NoPrivateReadonlyFieldsRule);

    /// <summary>Registers the named-type action that inspects attributed declarations.</summary>
    /// <param name="context">The analysis context the analyzer registers its actions on.</param>
    /// <exception cref="ArgumentNullException"><paramref name="context"/> is <see langword="null"/>.</exception>
    public override void Initialize(AnalysisContext context)
    {
        _ = context ?? throw new ArgumentNullException(nameof(context));

        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
    }

    private static void AnalyzeSymbol(SymbolAnalysisContext context)
    {
        INamedTypeSymbol symbol = (INamedTypeSymbol)context.Symbol;

        if (symbol.DeclaringSyntaxReferences[0].GetSyntax(context.CancellationToken) is not TypeDeclarationSyntax typeDeclarationSyntax
            || !symbol.HasAttribute(Source.AttributeFullName))
        {
            return;
        }

        List<FieldDeclarationSyntax> readonlyFields = [.. TypeDeclarationAnalysis.FindReadonlyFields(typeDeclarationSyntax)];
        bool hasXamlConstructorBaseType = InheritanceAnalysis.HasXamlConstructorBaseType(symbol);

        if (hasXamlConstructorBaseType
            && readonlyFields.Count == 0
            && !TypeDeclarationAnalysis.NeedsGeneratedParameterlessConstructor(typeDeclarationSyntax))
        {
            return;
        }

        Location location = typeDeclarationSyntax.Identifier.GetLocation();

        if (!TypeDeclarationAnalysis.IsPartial(typeDeclarationSyntax))
        {
            Diagnostic diagnostic = Diagnostic.Create(DiagnosticDescriptors.TypeWithoutPartialRule, location);
            context.ReportDiagnostic(diagnostic);
        }

        if (!TypeDeclarationAnalysis.HasViewModelSuffix(typeDeclarationSyntax))
        {
            Diagnostic diagnostic = Diagnostic.Create(DiagnosticDescriptors.TypeNameDoesNotEndWithViewModelRule, location);
            context.ReportDiagnostic(diagnostic);
        }

        if (TypeDeclarationAnalysis.IsNestedType(typeDeclarationSyntax))
        {
            Diagnostic diagnostic = Diagnostic.Create(DiagnosticDescriptors.NestedTypeNotSupportedRule, location);
            context.ReportDiagnostic(diagnostic);
        }

        if (TypeDeclarationAnalysis.HasParameterlessConstructor(typeDeclarationSyntax))
        {
            Diagnostic diagnostic = Diagnostic.Create(DiagnosticDescriptors.ConflictingConstructorRule, location);
            context.ReportDiagnostic(diagnostic);
        }

        if (readonlyFields.Count == 0 && !hasXamlConstructorBaseType)
        {
            Diagnostic diagnostic = Diagnostic.Create(DiagnosticDescriptors.NoPrivateReadonlyFieldsRule, location);
            context.ReportDiagnostic(diagnostic);
        }
    }
}
