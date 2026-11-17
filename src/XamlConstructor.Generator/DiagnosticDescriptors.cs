using Microsoft.CodeAnalysis;

namespace XamlConstructor.Generator;

/// <summary>
/// The diagnostic descriptors the generator and the analyzer report. Every rule has one stable
/// <c>XCONS</c> id, and the id links to the section of the package README that explains it.
/// </summary>
public static class DiagnosticDescriptors
{
    /// <summary>The type carries <c>[XamlConstructor]</c> but is not declared <c>partial</c>.</summary>
    public const string TypeWithoutPartialDiagnosticId = "XCONS01";

    /// <summary>The generator could not build the constructor for the type.</summary>
    public const string ConstructorCreationFailedDiagnosticId = "XCONS02";

    /// <summary>The type has no private or protected readonly field, so the constructor would be empty.</summary>
    public const string NoPrivateReadonlyFieldsDiagnosticId = "XCONS03";

    /// <summary>The type is nested, which the generator does not support.</summary>
    public const string NestedTypeNotSupportedDiagnosticId = "XCONS04";

    /// <summary>The type name does not end with <c>ViewModel</c>.</summary>
    public const string TypeNameDoesNotEndWithViewModelDiagnosticId = "XCONS05";

    /// <summary>The type already declares a parameterless constructor.</summary>
    public const string ConflictingConstructorDiagnosticId = "XCONS06";

    /// <summary>Reports a type that carries the attribute without the <c>partial</c> modifier.</summary>
    public static readonly DiagnosticDescriptor TypeWithoutPartialRule = CreateRule(
        TypeWithoutPartialDiagnosticId,
        "Couldn't generate constructor",
        $"Type decorated with {Source.AttributeFullName} must be also declared partial");

    /// <summary>Reports a type whose constructor the generator could not build.</summary>
    public static readonly DiagnosticDescriptor ConstructorCreationFailedRule = CreateRule(
        ConstructorCreationFailedDiagnosticId,
        "Constructor generation failed",
        "Failed to generate constructor for type '{0}': {1}",
        DiagnosticSeverity.Error);

    /// <summary>Reports a nested type, which the generator does not support.</summary>
    public static readonly DiagnosticDescriptor NestedTypeNotSupportedRule = CreateRule(
        NestedTypeNotSupportedDiagnosticId,
        "Couldn't generate constructor",
        $"Type decorated with {Source.AttributeFullName} must not be a nested type - nested types are not supported");

    /// <summary>Reports a type whose name does not end with <c>ViewModel</c>.</summary>
    public static readonly DiagnosticDescriptor TypeNameDoesNotEndWithViewModelRule = CreateRule(
        TypeNameDoesNotEndWithViewModelDiagnosticId,
        "Couldn't generate constructor",
        $"Type decorated with {Source.AttributeFullName} must have a name ending with \"ViewModel\" for the constructor to be generated");

    /// <summary>Reports a type that already declares a parameterless constructor.</summary>
    public static readonly DiagnosticDescriptor ConflictingConstructorRule = CreateRule(
        ConflictingConstructorDiagnosticId,
        "Couldn't generate constructor",
        $"Type decorated with {Source.AttributeFullName} already declares a parameterless constructor - remove it or remove the attribute");

    /// <summary>Reports a type with no private or protected readonly field without an initializer.</summary>
    public static readonly DiagnosticDescriptor NoPrivateReadonlyFieldsRule = CreateRule(
        NoPrivateReadonlyFieldsDiagnosticId,
        "Generated constructor has nothing to initialize",
        $"Type decorated with {Source.AttributeFullName} has no private or protected readonly fields without an initializer - the generated constructor will be empty");

    private static DiagnosticDescriptor CreateRule(string id, string title, string messageFormat, DiagnosticSeverity severity = DiagnosticSeverity.Warning)
    {
        return new DiagnosticDescriptor(
            id,
            title,
            messageFormat,
            "Usage",
            severity,
            true,
            null,
            $"https://github.com/VolocyNazad/toolkit.xaml-constructor?tab=readme-ov-file#{id.ToLowerInvariant()}",
            WellKnownDiagnosticTags.Build);
    }
}
