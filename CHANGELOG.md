# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [1.4.1] - 2026-09-14

### Added

- Validate required repository files, navigation links, and Solution Items in CI.

### Fixed

- Correct the repository guide's test inventory, package-management comparison, and MinVer tag-prefix description.

### Changed

- Run the test suite through xUnit v3 and Microsoft.Testing.Platform in local builds, CI, and publishing.
- Clean package output and run tests before validating and publishing the NuGet package.
- Standardize GitHub Actions workflow filenames and display names by responsibility.
- Establish a shared EditorConfig baseline and use the repository-policy validator as the single structural CI check.
- Complete solution items for repository documents, configuration, workflows and maintenance scripts; document the shared layout.
- Standardize local and CI SDK selection on stable .NET 10.0 through global.json, restrict roll-forward to that major/minor line, and configure setup-dotnet to read the file.
- Show documentation in Visual Studio Solution Explorer under a `docs` solution folder with matching subfolders.
- Move development policies and repository guidance from `AGENTS.md` to `docs/policies/development.md` and `docs/repository.md`; keep required reading links in `AGENTS.md` and add README navigation.

## [1.4.0] - 2026-09-08

### Added

- XCONS03: warn when a `[XamlConstructor]`-attributed type has no private or
  protected readonly fields without an initializer, so the generated
  constructor would be empty.
- XCONS04: warn when a `[XamlConstructor]`-attributed type is a nested type.
  Previously the generator silently emitted a non-compiling
  `partial class Outer.Inner` declaration for these instead of refusing to
  generate.
- XCONS05: warn when a `[XamlConstructor]`-attributed type's name doesn't end
  with "ViewModel". Previously the generator silently skipped these with no
  diagnostic at all.
- XCONS06: warn when a `[XamlConstructor]`-attributed type already declares a
  parameterless constructor.

### Fixed

- Nested types no longer produce invalid generated code - XCONS04 now
  reports the problem and skips generation instead.

### Changed

- Set the Roslyn compatibility baseline to Microsoft.CodeAnalysis 4.14 for Visual Studio 2022 version 17.14.
