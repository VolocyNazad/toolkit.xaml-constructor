using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using XamlConstructor.Generator;
using XamlConstructor.Generator.Analyzers;
using Xunit;

namespace XamlConstructor.Tests;

public sealed class TypeWithoutPartialAnalyzerTests
{
    [Theory]
    [MemberData(nameof(DiagnosticTestCases.ForAnalyzer), MemberType = typeof(DiagnosticTestCases))]
    public async Task ReportsExactlyOneDiagnostic_ForEachSingleIssueScenario(string source, string expectedDiagnosticId)
    {
        ImmutableArray<Diagnostic> diagnostics = await GetAnalyzerDiagnosticsAsync(source);

        Diagnostic diagnostic = Assert.Single(diagnostics);
        Assert.Equal(expectedDiagnosticId, diagnostic.Id);
    }

    [Fact]
    public async Task DoesNotReportDiagnostic_WhenAttributedTypeIsPartial()
    {
        const string source = """
            namespace Test;

            [XamlConstructor]
            public partial class PartialViewModel
            {
                private readonly string _name;
            }
            """;

        ImmutableArray<Diagnostic> diagnostics = await GetAnalyzerDiagnosticsAsync(source);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task DoesNotReportDiagnostic_ForTypesWithoutTheAttribute()
    {
        const string source = """
            namespace Test;

            public class PlainType
            {
                private readonly string _name;
            }
            """;

        ImmutableArray<Diagnostic> diagnostics = await GetAnalyzerDiagnosticsAsync(source);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task DoesNotReportDiagnostic_WhenBaseTypeHasTheAttribute()
    {
        const string source = """
            namespace Test;

            [XamlConstructor]
            public partial class BaseViewModel
            {
                private readonly string _name;
            }

            [XamlConstructor]
            public partial class DerivedViewModel : BaseViewModel
            {
            }
            """;

        ImmutableArray<Diagnostic> diagnostics = await GetAnalyzerDiagnosticsAsync(source);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task DoesNotReportDiagnostic_WhenAnIndirectBaseTypeHasTheAttribute()
    {
        const string source = """
            namespace Test;

            [XamlConstructor]
            public partial class RootViewModel
            {
                private readonly string _name;
            }

            public class MiddleViewModel : RootViewModel
            {
            }

            [XamlConstructor]
            public partial class LeafViewModel : MiddleViewModel
            {
            }
            """;

        ImmutableArray<Diagnostic> diagnostics = await GetAnalyzerDiagnosticsAsync(source);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task DoesNotReportDiagnostic_WhenBaseTypeHasTheAttribute_AndTheTypeDeclaresParameterizedConstructors()
    {
        const string source = """
            namespace Test;

            [XamlConstructor]
            public partial class RootViewModel
            {
                protected readonly object _service;
            }

            [XamlConstructor]
            public partial class MiddleViewModel : RootViewModel
            {
                protected MiddleViewModel(object service)
                {
                }
            }

            [XamlConstructor]
            public partial class LeafViewModel : MiddleViewModel
            {
                private readonly string _name;

                public LeafViewModel(object service)
                    : base(service)
                {
                }
            }
            """;

        ImmutableArray<Diagnostic> diagnostics = await GetAnalyzerDiagnosticsAsync(source);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task ReportsXcons03_WhenBaseTypeDoesNotHaveTheAttribute()
    {
        const string source = """
            namespace Test;

            public class BaseViewModel
            {
                protected readonly string _name;
            }

            [XamlConstructor]
            public partial class DerivedViewModel : BaseViewModel
            {
            }
            """;

        ImmutableArray<Diagnostic> diagnostics = await GetAnalyzerDiagnosticsAsync(source);

        Diagnostic diagnostic = Assert.Single(diagnostics);
        Assert.Equal(DiagnosticDescriptors.NoPrivateReadonlyFieldsDiagnosticId, diagnostic.Id);
    }

    [Fact]
    public async Task ReportsAllApplicableDiagnostics_ForATypeWithSeveralIssuesAtOnce()
    {
        const string source = """
            namespace Test;

            [XamlConstructor]
            public class Broken
            {
                public Broken()
                {
                }
            }
            """;

        ImmutableArray<Diagnostic> diagnostics = await GetAnalyzerDiagnosticsAsync(source);

        IEnumerable<string> ids = diagnostics.Select(d => d.Id).Order();
        string[] expectedIds =
        [
            DiagnosticDescriptors.TypeWithoutPartialDiagnosticId,
            DiagnosticDescriptors.NoPrivateReadonlyFieldsDiagnosticId,
            DiagnosticDescriptors.TypeNameDoesNotEndWithViewModelDiagnosticId,
            DiagnosticDescriptors.ConflictingConstructorDiagnosticId,
        ];

        Assert.Equal(expectedIds.Order(), ids);
    }

    private static async Task<ImmutableArray<Diagnostic>> GetAnalyzerDiagnosticsAsync(string source)
    {
        CSharpCompilation compilation = CompilationFactory.CreateCompilation(
            "AnalyzerTests",
            source,
            CompilationFactory.MinimalAttributeSource);

        CompilationWithAnalyzers compilationWithAnalyzers = compilation.WithAnalyzers(
            [new TypeWithoutPartialAnalyzer()]);

        return await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();
    }
}
