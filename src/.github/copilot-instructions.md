# Copilot Instructions for Cannery Crucible (Universal Library Subset)

## Coding Conventions (Universal, Unopinionated Library)
- **C# code:** PascalCase for types, methods, and properties. Use _camelCase for private fields, camelCase for locals.
- **Never use underscores in test or method names.**
- Use C# 12, async/await, and nullable enabled.
- Generate XML docs for all public APIs.
- Use only Microsoft analyzers and .NET built-in attributes for validation.
- Never introduce non-Microsoft or non-Goodtocode packages.
- Never break dependency rules (Domain → nothing, Application → Domain, etc.).
- All files and folders should use PascalCase for C# and lowercase-hyphens for config/docs.

## Domain Project Standards
- Domain Entities and Value Objects always inherit from SecuredVersionedEntity, SecuredEntity, or DomainEntity base classes as appropriate.
- Domain Entities and Value Objects must always contain static factory method Create().
- Domain Entities and Value Objects must always maintain invariant state via state change methods (e.g. UpdateName(), ChangeStatus()), not via public setters or constructors.

## Tests Project Standards
- **Never use underscores in test or method names.**
- Use PascalCase for all test method names.

## General
- All public APIs must have XML documentation comments.
- All code must compile with warnings as errors (treat warnings as errors).
- Use explicit access modifiers for all types and members.
- Prefer expression-bodied members for simple properties and methods.

## Domain Entities & Value Objects
- Do not expose setters on public properties; use private setters or backing fields.
- All state changes must be performed via explicit methods (not property setters).
- Use immutable value objects wherever possible.
- Static factory methods should be named Create.

## Testing
- Use [TestMethod] for all test methods.
- Test method names must be descriptive and use PascalCase (no underscores).
- Each test should assert a single behavior or invariant.
- Prefer using Goodtocode.Assertion for assertions, not raw Assert unless required.

## Naming & Structure
- Namespace structure must match folder structure.
- File names must match the main type they contain.
- Use PascalCase for all C# folders and files.
- Use lowercase-hyphens for markdown, YAML, and config files.

## Dependency & Package Management
- Do not add any NuGet packages except Microsoft or Goodtocode packages.
- Do not use reflection or dynamic code generation unless absolutely necessary and documented.

## Documentation
- Update relevant documentation files (e.g., README.md, SECURITY_BY_DESIGN.md) when making changes that affect usage or security.

## Never Do
- Never introduce non-Microsoft or non-Goodtocode packages.
- Never break dependency rules.
- Never write code that bypasses analyzers or disables warnings globally.

## References
- [docs/SECURITY_BY_DESIGN.md](../docs/SECURITY_BY_DESIGN.md): Security By Design principles and practices
- [README.md](../README.md): Project overview and getting started
