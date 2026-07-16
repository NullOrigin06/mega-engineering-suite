# Contributing to MEGA Engineering Suite

First off, thank you for considering contributing to MEGA Engineering Suite. 

## Workflow

1. Ensure any changes are discussed in an issue before beginning work.
2. Fork the repository and create your branch from `develop`.
3. If you've added code that should be tested, add tests.
4. Ensure the test suite passes.
5. Make sure your code lints.
6. Issue that pull request!

## Branching Strategy

- **`main`**: Stable production release. Do not commit directly to `main`.
- **`develop`**: The primary integration branch. All feature branches should target `develop`.
- **Feature Branches**: Prefix with `feature/` (e.g., `feature/flange-module`).
- **Bug Fixes**: Prefix with `fix/` (e.g., `fix/com-leak`).

## Code Style

- We follow standard C# conventions. 
- Avoid obsolete/temporary comments (e.g. `// Stage 8 fix`). Comments should explain *why*, not *what*.
- Remove unused `using` directives before committing.
- Ensure all COM objects are properly released using deterministic `ReleaseComObject` patterns as established in the `PipelineOrchestrator`.

## Pull Request Process

1. Update the `README.md` with details of changes to the interface, this includes new environment variables, exposed ports, useful file locations and container parameters.
2. Update the `CHANGELOG.md` with your changes.
3. The PR will be merged once you have the sign-off of at least one other developer.
