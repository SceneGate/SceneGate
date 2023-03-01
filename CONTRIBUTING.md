# Contributing guidelines

Thanks for taking the time to contribute! :sparkles:

In this document you will find all the information you need to make sure that
the projects continues to be consistent and with great quality!

## Reporting features and issues

### Issues

When reporting a problem, be as specific as possible. Ideally, you should
provide an small snippet of code that reproduces the issue.

Please fill the default template so we can have all the required information to
address the issue.

### Features

Features are requested and handled as GitHub _issues_.

If you want to ask for a new feature, first make sure it hasn't been reported
yet by using the search box in the issue tab. Make sure that the feature aligns
with the direction of the project.

**Do not ask for tools for games or translations**.

## Pull Request

Before starting a pull request, create an issue
[requesting the feature](#features) you would like to see and implement. If you
are fixing a bug, create also an issue to be able to track the problem.

In the issue or feature request specify that that you would like to work on it.
The team will reply as soon as possible to discuss the proposal. This guarantee
the Pull Request implementation match the direction the project is going.

In general, the process to create a pull request is:

1. Create an issue describing the bug or feature and state you would like to
   work on that.
2. The team will cheer you and/or discuss with you the issue.
3. Fork the project (if not done already).
4. Clone your forked project and create a git branch.
5. Make the necessary code changes in as many commits as you want. The commit
   message should follow this convention:

   ```plain
   :emoji: Short description #IssueID

   Long description if needed.
   ```

6. Create a pull request. After reviewing your changes and making any new
   commits if needed, the team will approve and merge it.

For a complete list of emoji description see
[this repository](https://github.com/slashsBin/styleguide-git-commit-message#suggested-emojis).

## Code Guidelines

The project includes a `.editorconfig` file that ensures the code style is
consistent. It is supported in any modern IDE.

In general, we follow the following standard guidelines with custom changes:

- [Mono Code Guidelines](https://raw.githubusercontent.com/mono/website/gh-pages/community/contributing/coding-guidelines.md).
- [Microsoft Framework Design Guidelines](https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/)
- [Microsoft C# Coding Convetions](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/inside-a-program/coding-conventions).

And as the
[mono team says](https://www.mono-project.com/community/contributing/coding-guidelines/#performance-and-readability):

- It is more important to be correct than to be fast.
- It is more important to be maintainable than to be fast.
- Fast code that is difficult to maintain is likely going to be looked down
  upon.

Make sure to follow these tips:

- :heavy_check_mark: **DO** write documentation for any public type and method.
- :heavy_check_mark: **DO** write a test for all the possible code branches of
  your methods. Use a TDD approach.
- :heavy_check_mark: **DO** seek for the maximum test coverage.
- :heavy_check_mark: **DO** clean compiler warning.
