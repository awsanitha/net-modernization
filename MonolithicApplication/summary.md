# Migration Summary: .NET Framework 4.7.2 → .NET 10

## Status: ✅ COMPLETE

`dotnet build UnicornShopLegacy.sln` — **0 errors, 0 warnings**  
`dotnet test UnicornShopLegacy.sln` — **29 passed, 0 failed**

---

## Changes Made

### Project Files

**`src/UnicornShopLegacy.csproj`** — Replaced old-style SDK with `Microsoft.NET.Sdk.Web`, targeting `net10.0`. Removed all legacy package references (`EntityFramework 6.x`, `Microsoft.AspNet.WebApi.*`, `WebGrease`, `Antlr`, `Microsoft.Web.Infrastructure`, etc.). Added:
- `Microsoft.EntityFrameworkCore` 10.0.0
- `Microsoft.EntityFrameworkCore.SqlServer` 10.0.0
- `Microsoft.EntityFrameworkCore.Design` 10.0.0
- `Newtonsoft.Json` 13.0.4

**`test/UnicornShopLegacy.Tests.csproj`** — Updated to SDK-style, `net10.0`. Removed `EntityFramework 6`, `Microsoft.AspNet.WebApi.*`, `OpenCover`, `ReportGenerator`. Updated test packages to MSTest v3 (`MSTest.TestAdapter` 3.6.0, `MSTest.TestFramework` 3.6.0, `Microsoft.NET.Test.Sdk` 17.11.1, `Moq` 4.20.72, `FluentAssertions` 8.4.0, `coverlet.msbuild` 6.0.4).

### Startup / Configuration

**`src/Program.cs`** (new) — Replaced `Global.asax` and `App_Start/WebApiConfig.cs` with ASP.NET Core top-level minimal startup. Registers DI, CORS, EF Core DbContext, and maps controllers. Includes the original DB population logic ported to async with EF Core.

**`src/appsettings.json`** / **`src/appsettings.Development.json`** (new) — Connection string migrated from `Web.config` `<connectionStrings>`. EF6 metadata connection string replaced with plain ADO.NET connection string for EF Core.

**`src/Global.asax.cs`** — Replaced with a placeholder comment (startup is in Program.cs).

**`src/App_Start/WebApiConfig.cs`** — Replaced with a placeholder comment (routing is in Program.cs).

### Entity Framework 6 → EF Core 10

**`src/UnicornShop.Context.cs`** — Replaced `DbContext` inheriting from EF6's `System.Data.Entity.DbContext` with EF Core `Microsoft.EntityFrameworkCore.DbContext`. Changed constructor to accept `DbContextOptions<UnishopEntities>`. Added `OnModelCreating` with explicit entity configuration. Renamed DbSet properties to `basketSet`/`userSet`/`inventorySet` to avoid naming conflicts with the `IEntitySet<T>` wrappers exposed on the interface.

**`src/Classes/UnishopEntities.cs`** — Updated partial class to implement the new `IUnishopEntities` interface using `DbSetWrapper<T>` wrappers. Added explicit `SaveChangesAsync()` forwarding to `DbContext.SaveChangesAsync()` (required because EF Core's `SaveChangesAsync` has a default `CancellationToken` parameter).

**`src/DbSetWrapper.cs`** (new) — Wraps EF Core's `DbSet<T>` to implement the `IEntitySet<T>` testability interface.

**`src/UnicornShop.Designer.cs`**, **`src/UnicornShop.cs`** — Retained as-is (empty auto-generated files, no impact).

### Interface / API

**`src/Interfaces/IUnishopEntities.cs`** — Replaced `System.Data.Entity` types with EF Core equivalents:
- `DbEntityEntry` → `EntityEntry` (from `Microsoft.EntityFrameworkCore.ChangeTracking`)
- `DbSet<T>` → `IEntitySet<T>` (custom testable abstraction, see below)
- Added `IEntitySet<T>` interface definition to enable in-memory fakes in tests without depending on EF Core's concrete `DbSet<T>`

### Controllers

All three controllers (`BasketController`, `UnicornController`, `UserController`) migrated from Web API 2 to ASP.NET Core:
- `ApiController` → `ControllerBase`
- `IHttpActionResult` → `IActionResult`
- `System.Web.Http` → `Microsoft.AspNetCore.Mvc`
- `[EnableCors]` attribute removed (CORS configured globally in Program.cs)
- `[ResponseType]` attribute removed (not needed in ASP.NET Core)
- `DbEntityEntry`/`EntityState` from `System.Data.Entity` → `Microsoft.EntityFrameworkCore`
- Added explicit HTTP verb attributes (`[HttpGet]`, `[HttpPost]`, `[HttpPut]`, `[HttpDelete]`) and route templates
- Removed `Dispose(bool)` overrides (ASP.NET Core's `ControllerBase` has no virtual `Dispose(bool)`)
- `HttpStatusCode.NoContent` → `StatusCode((int)HttpStatusCode.NoContent)`

**`UserController`** additionally: replaced obsolete `RNGCryptoServiceProvider` with `RandomNumberGenerator.Fill(salt)` and replaced deprecated `Rfc2898DeriveBytes` constructor with `Rfc2898DeriveBytes.Pbkdf2()` static method.

Removed unused import `using Microsoft.Ajax.Utilities` (no equivalent needed) and `using System.Security.Policy` (removed).

### Entities

**`src/user.cs`**, **`src/basket.cs`**, **`src/inventory.cs`** — Added nullable annotations (`string?`, `decimal?`, `DateTime?`) to match `Nullable enable` semantics. Retained all property names and types.

### Tests

**`test/FakeDbSet/FakeDbSet.cs`** — Replaced EF6's `DbSet<T>` + `IDbSet<T>` inheritance with a pure in-memory `IEntitySet<T>` implementation. No more dependency on `System.Data.Entity`. Uses a `List<T>` backed `IQueryable<T>`.

**`test/FakeDbSet/FakeUserDbSet.cs`**, **`FakeBasketDbSet.cs`**, **`FakeUnicornDbSet.cs`** — Updated to override `FindAsync` returning `Task<T?>` instead of `Task<T>`.

**`test/UserControllerTests.cs`**, **`test/BasketControllerTests.cs`**, **`test/UnicornControllerTests.cs`** — Updated result type assertions:
- `System.Web.Http` / `System.Web.Http.Results` imports removed
- `OkNegotiatedContentResult<T>` → `OkObjectResult` (content in `.Value` cast to `T`)
- `CreatedAtRouteNegotiatedContentResult<T>` → `CreatedAtRouteResult` (content in `.Value` cast to `T`)
- `NotFoundResult` → `Microsoft.AspNetCore.Mvc.NotFoundResult`
- `BadRequestResult` → `Microsoft.AspNetCore.Mvc.BadRequestResult`
- `InvalidModelStateResult` → `BadRequestObjectResult`
- `StatusCodeResult.StatusCode` — same type, different namespace
- Mock setup for `SaveChangesAsync()` updated to `Task.FromResult(0)` (returns `Task<int>`)

**`src/Properties/AssemblyInfo.cs`** / **`test/Properties/AssemblyInfo.cs`** — Stripped duplicate assembly attributes that conflict with SDK auto-generation. Retained only `Guid` and `ComVisible`.

---

## Architectural Decisions

1. **`IEntitySet<T>` abstraction**: Rather than trying to subclass EF Core's `DbSet<T>` (which requires a live `DbContext` and has internal/protected constructors), a lightweight `IEntitySet<T>` interface was introduced. The production code uses `DbSetWrapper<T>`, which delegates to the real EF Core `DbSet<T>`. Tests use `FakeDbSet<T>` backed by a `List<T>`. This preserves all existing test logic and mock setups.

2. **Dependency injection for controllers**: Removed parameterless constructors from `BasketController`, `UnicornController`, and `UserController` — DI provides the `IUnishopEntities` context. This follows ASP.NET Core best practice.

3. **CORS**: Moved from per-controller `[EnableCors]` attribute to a global policy registered in `Program.cs` with `AllowAnyOrigin/Header/Method`, matching the original permissive CORS setup.

---

## Next Steps

- **Database**: The EF6 EDMX/database-first model (`UnicornShop.edmx`, `UnicornShop.Context.tt`, `UnicornShop.tt`) is no longer used. The application now uses EF Core Code-First with table names mapped in `OnModelCreating`. If the existing SQL Server database schema doesn't match EF Core's conventions, adjust the `ToTable()`/`HasKey()` mappings accordingly or run `dotnet ef migrations add InitialCreate` against the actual DB.
- **SQL column `year_model`**: The legacy `Global.asax.cs` ran a raw SQL `ALTER TABLE inventory ADD year_model AS (datepart(year,date_create))` computed column addition. This is not replicated in the new `Program.cs` since EF Core `EnsureCreatedAsync()` creates tables from the model (no computed columns by default). If the `year_model` column is required, add it via an EF Core migration or `HasComputedColumnSql()` in `OnModelCreating`.
- **Static frontend files**: The legacy `Web.config` had IIS rewrite rules pointing to `/build/` for SPA assets. In ASP.NET Core this is handled via `app.UseStaticFiles()` + `app.MapFallbackToFile("build/index.html")`. If serving the SPA through this project, add those lines to `Program.cs`.
- **Production secrets**: The connection string in `appsettings.json` contains a plain-text password. For production, move credentials to environment variables or a secrets manager.
- **FluentAssertions version**: `FluentAssertions` 8.x introduced breaking changes to the API. If assertion failures appear, review the FluentAssertions changelog for the specific assertion methods used.
