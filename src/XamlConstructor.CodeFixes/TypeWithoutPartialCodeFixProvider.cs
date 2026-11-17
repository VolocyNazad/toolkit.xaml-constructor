using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using XamlConstructor.Generator;

namespace XamlConstructor.CodeFixes;

/// <summary>
/// Offers the fix for <c>XCONS01</c>: adds the missing <c>partial</c> modifier to the declared type.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(TypeWithoutPartialCodeFixProvider)), Shared]
public sealed class TypeWithoutPartialCodeFixProvider : CodeFixProvider
{
    /// <summary>The diagnostics this provider can fix.</summary>
    public override ImmutableArray<string> FixableDiagnosticIds => [DiagnosticDescriptors.TypeWithoutPartialDiagnosticId];

    /// <summary>Returns the batch fixer, so the fix applies to every reported declaration at once.</summary>
    /// <returns>The fix-all provider for this code fix.</returns>
    public override FixAllProvider GetFixAllProvider()
    {
        return WellKnownFixAllProviders.BatchFixer;
    }

    /// <summary>Registers the <c>Make type partial</c> action for the reported declaration.</summary>
    /// <param name="context">The code-fix context that carries the document and the reported diagnostic.</param>
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        SyntaxNode? root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

        Diagnostic? diagnostic = context.Diagnostics[0];
        TextSpan diagnosticSpan = diagnostic.Location.SourceSpan;

        TypeDeclarationSyntax? declaration = root?.FindToken(diagnosticSpan.Start).Parent?.AncestorsAndSelf().OfType<TypeDeclarationSyntax>().First();
        if (declaration is null)
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Make type partial",
                createChangedDocument: cancellationToken => MakeTypePartialAsync(context.Document, declaration, cancellationToken),
                equivalenceKey: "Make type partial"),
            diagnostic);
    }

    private static async Task<Document> MakeTypePartialAsync(Document document, TypeDeclarationSyntax typeDeclarationSyntax, CancellationToken cancellationToken)
    {
        SyntaxTokenList newModifiers = typeDeclarationSyntax.Modifiers.Add(SyntaxFactory.Token(SyntaxKind.PartialKeyword));
        TypeDeclarationSyntax newDeclaration = typeDeclarationSyntax.WithModifiers(newModifiers);

        SyntaxNode? oldRoot = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException("Cannot fix code.");
        SyntaxNode newRoot = oldRoot.ReplaceNode(typeDeclarationSyntax, newDeclaration);
        return document.WithSyntaxRoot(newRoot);
    }
}
