---
name: entity-framework
description: 'Use for Entity Framework Core tasks such as DbContext setup, entity relationships, migrations, dotnet ef commands, seeding, and fixing model-to-database mapping issues.'
argument-hint: 'migration | DbContext | relationships | seeding | dotnet ef'
user-invocable: true
disable-model-invocation: false
---

# Entity Framework Core

Use this skill when working on EF Core database setup, model configuration, migrations, or data access issues.

## Project Context

- ASP.NET Core MVC application
- Database provider: SQL Server LocalDB
- Main DbContext: AppDbContext
- Connection string key: DefaultConnection
- Models are stored in the Models folder

## When to Use
- Create or update `DbContext` classes
- Add or fix entity relationships and foreign keys
- Generate or apply migrations with `dotnet ef`
- Seed initial data
- Diagnose mapping, cascade delete, or model binding issues

## Procedure
1. Inspect the current entity classes and `DbContext`.
2. Confirm the target database provider and connection string.
3. Make the smallest model or configuration change needed.
4. Run `dotnet build` to verify the project still compiles.
5. Run the relevant `dotnet ef` command:
   - `dotnet ef migrations add <Name>`
   - `dotnet ef database update`
   - `dotnet ef migrations remove` when cleaning up an invalid migration
6. If a migration fails, inspect cascade delete paths, nullability, and navigation properties.

## Common Checks
- Ensure entity names, folder names, and `DbSet` names are consistent.
- Keep required foreign keys non-nullable when the relationship is mandatory.
- Use `virtual` navigation properties only when lazy loading or proxies are intended.
- If SQL Server reports multiple cascade paths, configure one relationship with `DeleteBehavior.NoAction`.

## References
- Use the project `DbContext` and model classes.
- Use `dotnet ef` from the project root when creating or applying migrations.

## Migration Workflow

Always:
1. Run `dotnet build`
2. Create migration
3. Apply migration
4. Run the application and verify data loading