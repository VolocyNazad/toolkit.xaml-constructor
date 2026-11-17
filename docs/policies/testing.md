# Testing policy

Paths mentioned in this policy are relative to the repository root unless a Markdown link specifies otherwise.

Read when: adding tests, choosing a test kind or running a suite.

Tests are the evidence that a change works. The repository separates them by what they need
to run: a normal process, or a person in front of the product.

## Test kinds

| Kind | Project suffix | Runs in | May touch |
| --- | --- | --- | --- |
| Headless | `*.Tests` | a normal process, including a hosted runner | pure code, temporary files, in-memory state |
| Manual smoke | - | Visual Studio and a XAML designer, by a person | window, designer surface, file system |

The suffix is a contract: CI selects tests by it, and a project without `*.Tests` is not a
test project. Prefer the product prefix as well (`<Product>.Tests`), but the suffix is what
matters.

## What needs a test

- A bug fix comes with a regression test that fails without the fix.
- A behaviour change comes with a test of the new behaviour, not only proof that nothing
  crashes.
- The generator is tested through the generated code: drive it with `CSharpGeneratorDriver` over an
  in-memory compilation and assert the emitted source.
- The analyzer is tested through `CompilationWithAnalyzers`: assert the diagnostic id, its location
  and its severity.
- The code fix is tested through `AdhocWorkspace`: apply the fix to the reported document and assert
  the resulting text and that the diagnostic no longer appears.
- Every `XCONS` diagnostic has a test that triggers it and a test that shows the corrected
  declaration.

Documentation, formatting, dependency updates and purely visual changes do not need a test,
but the build must pass.

## Isolation and determinism

- Tests do not use the network and do not write into the repository; temporary data goes to a
  temporary directory.
- A test builds its own in-memory compilation. Do not reference the test project's own assembly from
  a compilation under test: assert against the source and the diagnostics instead.
- The parsed test source stays inside the C# syntax the pinned Microsoft.CodeAnalysis version
  supports; a test that needs a newer syntax is a signal to raise the pin, not to skip the test.
- No dialogs and no waiting for input in any test.
- A test does not depend on execution order, locale, registry state or the machine it runs on.

## Running the tests

Headless: `dotnet test tests/XamlConstructor.Tests/XamlConstructor.Tests.csproj --configuration Release`,
or the built executable, which is what CI may use.

## Manual smoke check

For a change that affects the generated code, add the analyzer package to a small WPF project, mark a
view-model with `[XamlConstructor]`, open it in the XAML designer and confirm that the designer
instantiates the type: the parameterless constructor exists and the readonly fields are assigned. A
change that only touches diagnostics, the code fix or documentation has no manual step.

## Flaky and skipped tests

A test that fails intermittently is fixed or quarantined with an issue and a date. A skipped
test has a reason and an issue; an unexplained skip is a defect.

## Coverage

Every behaviour a consumer of the generator can observe has a test. No numeric threshold
is enforced: a percentage that moves without a behaviour change is not a goal.

The workflow around tests - branches, commit messages and the changelog - is defined by the
[development policy](development.md).
