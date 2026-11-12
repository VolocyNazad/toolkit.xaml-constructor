using Microsoft.CodeAnalysis;
using XamlConstructor.Generator.Extensions;

namespace XamlConstructor.Generator;

/// <summary>
/// Symbol-level checks shared by <see cref="Analyzers.TypeWithoutPartialAnalyzer"/> and
/// <see cref="Generators.XamlConstructorGenerator"/>, so the rules that depend on the type
/// hierarchy are defined once.
/// </summary>
internal static class InheritanceAnalysis
{
    /// <summary>
    /// Returns <see langword="true"/> when a base type already carries <see cref="Source.AttributeFullName"/>.
    /// </summary>
    /// <param name="symbol">The decorated type to inspect.</param>
    /// <returns>
    /// <see langword="true"/> when the derived type reuses the base type constructor and must not
    /// generate its own; otherwise <see langword="false"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="symbol"/> is <see langword="null"/>.</exception>
    public static bool HasXamlConstructorBaseType(INamedTypeSymbol symbol)
    {
        _ = symbol ?? throw new ArgumentNullException(nameof(symbol));

        for (INamedTypeSymbol? baseType = symbol.BaseType; baseType is not null; baseType = baseType.BaseType)
        {
            if (baseType.HasAttribute(Source.AttributeFullName))
            {
                return true;
            }
        }

        return false;
    }
}
