using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace XamlConstructor.Generator;

/// <summary>
/// Syntax-level checks shared by <see cref="Analyzers.TypeWithoutPartialAnalyzer"/> and
/// <see cref="Generators.XamlConstructorGenerator"/>, so the rules for what a
/// <c>[XamlConstructor]</c>-attributed type must look like are defined once.
/// </summary>
internal static class TypeDeclarationAnalysis
{
    public static bool IsPartial(TypeDeclarationSyntax typeDeclaration)
    {
        return typeDeclaration.Modifiers.Any(SyntaxKind.PartialKeyword);
    }

    public static bool HasViewModelSuffix(TypeDeclarationSyntax typeDeclaration)
    {
        return typeDeclaration.Identifier.Text.EndsWith("ViewModel");
    }

    public static bool IsNestedType(TypeDeclarationSyntax typeDeclaration)
    {
        return typeDeclaration.Parent is TypeDeclarationSyntax;
    }

    public static bool HasParameterlessConstructor(TypeDeclarationSyntax typeDeclaration)
    {
        return typeDeclaration.Members
            .OfType<ConstructorDeclarationSyntax>()
            .Any(c => c.ParameterList.Parameters.Count == 0);
    }

    /// <summary>
    /// A type that declares instance constructors but none of them parameterless has no implicit
    /// parameterless constructor, so derived types need the generated one to call <c>base()</c>.
    /// </summary>
    /// <param name="typeDeclaration">The decorated type declaration to inspect.</param>
    /// <returns>
    /// <see langword="true"/> when the type needs a generated parameterless constructor; otherwise
    /// <see langword="false"/>.
    /// </returns>
    public static bool NeedsGeneratedParameterlessConstructor(TypeDeclarationSyntax typeDeclaration)
    {
        ConstructorDeclarationSyntax[] instanceConstructors =
        [
            .. typeDeclaration.Members
                .OfType<ConstructorDeclarationSyntax>()
                .Where(c => !c.Modifiers.Any(SyntaxKind.StaticKeyword)),
        ];

        return instanceConstructors.Length > 0
            && !instanceConstructors.Any(c => c.ParameterList.Parameters.Count == 0);
    }

    public static IEnumerable<FieldDeclarationSyntax> FindReadonlyFields(TypeDeclarationSyntax typeDeclaration)
    {
        return typeDeclaration.Members
            .OfType<FieldDeclarationSyntax>()
            .Where(f => f.Modifiers.Any(SyntaxKind.ReadOnlyKeyword)
                && (f.Modifiers.Any(SyntaxKind.PrivateKeyword) || f.Modifiers.Any(SyntaxKind.ProtectedKeyword))
                && !f.Declaration.Variables.Any(v => v.Initializer != null));
    }
}
