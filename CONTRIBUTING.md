# Contributing

## Development Workflow

All Jira User Stories must be implemented using the strict TDD cycle:

1. **Red** — create failing unit tests before implementing business logic.
2. **Green** — implement the minimum code required to pass the tests.
3. **Refactor** — improve design, naming, validation, and error handling without breaking passing tests.

## Testing Standards

- Place unit tests in `Library.Tests`.
- Use xUnit as the test framework.
- Use Moq for dependency mocking.
- Use FluentAssertions for assertions.
- Follow the Arrange–Act–Assert pattern.
- Cover every acceptance criterion from the Jira User Story.

## Architecture Standards

- Keep domain models and abstractions in `Library.Core`.
- Keep business services in `Library.BLL`.
- Keep persistence concerns in `Library.DAL`.
- Keep API concerns in `Library.API`.
- Services must depend on repository interfaces rather than concrete persistence implementations.

## Language Standards

- Write source code, code comments, and commit messages in English.
- Write explanations and documentation in Ukrainian.