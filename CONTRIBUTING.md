# Contributing to this project

## How to check your contributions

This project uses pre-commit to perform first line validation of code,
please ensure that you have pre-commit installed, enabled and have run it on your contribution
prior to requesting a merge into this project

See pre-commit install guide <https://pre-commit.com/#install>

## IDEs

The default IDE for development on this project is VSCode/VSCodium,
please do not submit any other IDE specific files to the project
(it is recommended to block them via updating the .gitignore file)

## Formatting Standards

The following are project standards and will not be changed,
(this is for our own sanity, we don't want to be battling against a continual stream
of X formatting standard is better type requests/comments)

All indents (C#, yaml etc.) are set to 2 spaces
All lines (for any file, excluding License files) must not exceed 120 characters in length

## Files that are not open to contributions

The following files are used to enforce project standards and are not open to contributions,
please do not ask for/suggest changes to them.

- .pre-commit-config.yaml
- .yamllint.yaml
- .csharpierrc.yaml
- .mdformat.toml
