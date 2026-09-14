# Repository guide

Paths in this document are relative to the repository root.
Read and follow the [development policy](policies/development.md) alongside this guide.

## About

Source for `XamlConstructor` (NuGet package `XamlConstructor`): a C#
incremental source generator that emits a parameterless "XAML Design
Mode" constructor for any class or struct decorated with
`[XamlConstructor]` and declared `partial`, assigning every private/
protected readonly field to `null!`. Intended for WPF/XAML view-models
that need a design-time-only constructor (so the XAML designer can
instantiate them) without hand-writing and maintaining one. Despite
living alongside the `toolkit.revit.*` family, it has **no dependency on
the Revit API or WPF** - it's a plain Roslyn analyzer/generator package,
usable in any C# project.

Standard Roslyn analyzer/generator layout: the generator and its
companion "type must be partial" analyzer + code fix live in separate
`netstandard2.0` projects, and are packaged together into a single
`analyzers/dotnet/cs` NuGet package by a fourth, code-free packaging
project.

## Repository structure

```
.
├── src/
│   ├── XamlConstructor/                 packaging project only (no source files) -
│   │                                    packs XamlConstructor.Generator.dll and
│   │                                    XamlConstructor.CodeFixes.dll as analyzers
│   │                                    and publishes the NuGet package
│   ├── XamlConstructor.Generator/       the incremental generator (XCONS01/XCONS02
│   │   │                                diagnostics) + the "type without partial" analyzer
│   │   ├── Analyzers/
│   │   ├── Extensions/
│   │   └── Generators/
│   └── XamlConstructor.CodeFixes/       code fix provider for the XCONS01 diagnostic
│                                        (adds the missing `partial` modifier)
└── tests/
    └── XamlConstructor.Tests/           tests for the generator, analyzer and
                                         code fix provider
```

## Tech stack

- Roslyn incremental generator (`IIncrementalGenerator`) + `DiagnosticAnalyzer` +
  `CodeFixProvider`, all targeting `netstandard2.0` (required for analyzers/generators)
- Central package management (`Directory.Packages.props`,
  `ManagePackageVersionsCentrally=true`) **is** used here; repositories that do
  not use central package management pin versions per-`<PackageReference>`.
- Repo-wide global analyzers pinned in `Directory.Packages.props`:
  Roslynator.Analyzers, SonarAnalyzer.CSharp
- MinVer with the `v` tag prefix, as configured by `MinVerTagPrefix` in
  `Directory.Build.props`.
- Tests target `net10.0` and use xUnit v3 through Microsoft.Testing.Platform directly against the raw Roslyn APIs
  (`CSharpCompilation`, `CompilationWithAnalyzers`, `CSharpGeneratorDriver`,
  `AdhocWorkspace`) rather than the `Microsoft.CodeAnalysis.CSharp.{Analyzer,
  CodeFix,SourceGenerators}.Testing` helper packages - those were referenced
  early on but never actually used in test code, so they were dropped
- `Microsoft.CodeAnalysis` / `.CSharp` / `.CSharp.Workspaces` are exact-pinned
  (`[4.14.0]`, not a floor) in `Directory.Packages.props` - this is the
  compiler version the analyzer/generator/code-fix assemblies are built
  against, and it also governs which C# syntax test sources may use when
  parsed directly via `CSharpSyntaxTree`/`CSharpCompilation` at test time.
  `4.14.0` matches the Roslyn version shipped with the latest Visual Studio
  2022 release line, so the analyzer stays loadable by any current VS 2022
  install. Bumping past the VS-2022-compatible range (e.g. into the `5.x`
  series, which targets newer hosts) is a deliberate compatibility trade-off,
  not a routine "latest version" update - if it's ever done, call it out in
  the README/release notes so consumers on older hosts aren't surprised

## Notes

- `src/Xaml.Generator/` (a stray, unreferenced `net10.0` scaffold project with
  no source files, left over from an early restructuring) was removed - it
  wasn't part of the `.slnx` and duplicated no functionality that
  `XamlConstructor.Generator` doesn't already cover.

## Documentation layout

- `AGENTS.md` links to the required repository guidance.
- `docs/policies/development.md` contains the development policy.
- `docs/repository.md` describes the project, repository structure, and technology stack.

The root solution exposes the documentation files under a `docs` solution folder in Visual Studio, preserving their subfolder structure. When adding documentation files, also add them as solution items; solution folders do not automatically include new files.

The root `global.json` selects stable .NET SDK 10.0 (minimum `10.0.103`, `rollForward: latestFeature`). CI and publishing install the SDK from this file. Additional SDK installations may provide older test runtimes. See the [SDK selection policy](policies/development.md#net-sdk-selection).

## Solution items

The root solution exposes repository-level documents and configuration under `solutionItems`, GitHub files and maintenance scripts in matching subfolders, and documentation under `docs/`. The list is explicit, not a filesystem glob; keep links up to date when files change. See the [solution items policy](policies/development.md#solution-items).
## Repository validation

`scripts/Validate-Repository.ps1` enforces the required repository documents,
their navigation links, and complete, valid Solution Items. The
`.github/workflows/repository-policy.yml` workflow runs it for pushes and pull
requests. See the [development policy](policies/development.md#repository-validation).

## Formatting

The root `.editorconfig` defines the portable formatting baseline. Existing repositories may add stricter C# or analyzer-specific settings. See the [development policy](policies/development.md#formatting-baseline).

## Versioning and release tags

The package uses MinVer 8 with stable tags in the `vMAJOR.MINOR.PATCH` format. The tag without its `v` prefix is the NuGet package version. Manual publishing accepts exactly one matching tag at HEAD and verifies that exactly the `XamlConstructor` package has the expected version before pushing to NuGet.
