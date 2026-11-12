# Contributing

Before making a change, read the [development policy](docs/policies/development.md)
and the [repository guide](docs/repository.md). They define the repository-specific
stack, testing expectations and maintenance rules.

Branches follow the [branching policy](docs/policies/branching.md): `main` holds
the released state, `dev` integrates the next release, and every change arrives
through a short-lived `feature/`, `fix/`, `release/` or `hotfix/` branch and a
pull request.

Commit messages follow the [commit message policy](docs/policies/commits.md).

Tests follow the [testing policy](docs/policies/testing.md): headless `*.Tests`
projects run in CI, and a change with a manual smoke step is checked by hand.

Keep changes focused and update [CHANGELOG.md](CHANGELOG.md) under `Unreleased`
as the [changelog policy](docs/policies/changelog.md) requires. Run the build, the
tests and the policy validator before submitting a pull request:

```powershell
dotnet build --configuration Release
dotnet test tests/XamlConstructor.Tests/XamlConstructor.Tests.csproj --configuration Release
./scripts/Validate-Repository.ps1
```
