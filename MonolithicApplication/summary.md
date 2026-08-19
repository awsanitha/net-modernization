# Migration Summary: .NET Framework 4.7.2 → .NET 10

## Result
`dotnet build UnicornShopLegacy.sln` — **Build succeeded. 0 Error(s). 0 Warning(s).**  
`dotnet test UnicornShopLegacy.sln` — **Passed! Failed: 0, Passed: 29, Skipped: 0, Total: 29**

---

## Changes Made

### src/UnicornShopLegacy.csproj
- Converted from legacy MSBuild XML format to SDK-style (`Microsoft.NET.Sdk.Web`)
- Target framework: `net10.0`, `OutputType: Exe`
- Removed all legacy `<Reference>` and `<ItemGroup>` compile/content items (SDK auto-includes)
- Removed legacy imports (`Microsoft.WebApplication.targets`, `Microsoft.CSharp.targets`)
- Added EF Core 10 packages: `Microsoft.EntityFrameworkCore`, `Microsoft.EntityFrameworkCore.SqlServer`, `Microsoft.EntityFrameworkCore.Design`, `Microsoft.EntityFrameworkCore.Tools`
- Added `Microsoft.AspNetCore.Mvc.NewtonsoftJson` (JSON serialization compatibility)
- Added `Newtonsoft.Json 13.0.3`
- Set `GenerateAssemblyInfo=false` to avoid CS0579 duplicate attribute conflicts with `Properties/AssemblyInfo.cs`
- Excluded legacy files from compilation: `Global.asax.cs`, `App_Start/WebApiConfig.cs`, `UnicornShop.Designer.cs`, EDMX and T4 files, Web.config variants

### src/Program.cs (NEW)
- Replaced `Global.asax` + `WebApiConfig.cs` with ASP.NET Core minimal hosting model
- Configures CORS (allow any origin/header/method, preserving original open CORS policy)
- Registers `UnishopEntities` DbContext via DI with SQL Server provider
- Seeds database at startup via `DbPopulationHelper`
- Serves static files from `wwwroot` (SPA build output) with fallback to `index.html`
- Maps attribute-routed controllers

### src/appsettings.json (NEW)
- Migrated connection string from `Web.config` `<connectionStrings>` section
- SQL Server connection string updated from EF6 metadata format to standard EF Core format
- `TrustServerCertificate=true` added for compatibility

### src/UnicornShop.Context.cs
- Replaced EF6 auto-generated `DbContext` (using `System.Data.Entity`) with EF Core version
- Constructor updated to accept `DbContextOptions<UnishopEntities>` for DI
- Removed `UnintentionalCodeFirstException` — EF Core uses code-first by default
- Added `OnModelCreating` with explicit table name mappings (`basket`, `app_user`, `inventory`)
- Implements `IUnishopEntities`

### src/Classes/UnishopEntities.cs
- Updated `using System.Data.Entity` → `using Microsoft.EntityFrameworkCore`

### src/Classes/DbPopulationHelper.cs (NEW)
- Extracted DB seeding logic from `Global.asax.cs` into a standalone static helper
- Uses `context.Database.GetDbConnection().ConnectionString` for raw SQL access
- Guards seeding with try/catch so missing DB at startup (CI/CD, tests) is non-fatal

### src/Interfaces/IUnishopEntities.cs
- Replaced `using System.Data.Entity` + `using System.Data.Entity.Infrastructure` with `using Microsoft.EntityFrameworkCore`
- Removed `DbEntityEntry Entry(object entity)` (EF6-specific) — `SetModified` still present and calls `this.Entry(entity).State = EntityState.Modified` via EF Core
- `SaveChangesAsync` updated to include `CancellationToken` overload

### src/Controllers/UserController.cs
- `ApiController` → `ControllerBase` with `[ApiController]` and `[Route("api/[controller]")]`
- All `System.Web.Http.*` usings removed
- `IHttpActionResult` return types → `ActionResult<T>` / `IActionResult`
- `[ResponseType]` attributes removed (no equivalent needed; `ActionResult<T>` is self-documenting)
- `[EnableCors]` removed (CORS now configured globally in `Program.cs`)
- `new RNGCryptoServiceProvider()` → `RandomNumberGenerator.GetBytes(16)` (SYSLIB0023 fix)
- `new Rfc2898DeriveBytes(...)` constructor → `Rfc2898DeriveBytes.Pbkdf2(...)` static method (SYSLIB0060 fix)
- `[FromBody]` retained (still valid in ASP.NET Core)
- Removed `Dispose(bool)` override — `ControllerBase` does not expose this; DI manages `DbContext` lifetime
- Removed `using Microsoft.Ajax.Utilities` / `IsNullOrWhiteSpace` → `string.IsNullOrWhiteSpace()`

### src/Controllers/BasketController.cs
- Same `ApiController` → `ControllerBase` migration as above
- HTTP verb attributes added: `[HttpGet]`, `[HttpGet("{id}")]`, `[HttpPut("{id}")]`, `[HttpPost]`, `[HttpDelete("{id}")]`
- `DbUpdateConcurrencyException` namespace → `Microsoft.EntityFrameworkCore`
- `EntityState` namespace → `Microsoft.EntityFrameworkCore`
- `this.StatusCode(HttpStatusCode.NoContent)` → `this.StatusCode((int)HttpStatusCode.NoContent)`
- Removed `Dispose(bool)` override

### src/Controllers/UnicornController.cs
- Same migration pattern as `BasketController`
- Removed `System.Data.Entity.Infrastructure` and `System.Data.Entity` usings

### src/basket.cs / src/inventory.cs / src/user.cs
- Added `#pragma warning disable CS8981` to suppress "type name only contains lower-cased ascii characters" warnings — these names match database column/table schema and cannot be changed

### test/UnicornShopLegacy.Tests.csproj
- Removed legacy packages: `EntityFramework 6.x`, `Microsoft.AspNet.WebApi.Client`, `Microsoft.AspNet.WebApi.Core`, `OpenCover`, `ReportGenerator`, `System.Runtime.CompilerServices.Unsafe`, `System.Configuration.ConfigurationManager`, `Castle.Core`, `System.ComponentModel.Annotations`
- Updated: `MSTest.TestAdapter/TestFramework` 2.x → 3.7.3, `Microsoft.NET.Test.Sdk` 16.x → 17.12.0, `Moq` 4.14 → 4.20.72, `FluentAssertions` 5.x → 6.12.2, `coverlet.msbuild` 2.x → 6.0.4
- Added: `Microsoft.AspNetCore.Mvc.Testing 10.0.0`, `Microsoft.EntityFrameworkCore 10.0.0`
- Set `GenerateAssemblyInfo=false`

### test/FakeDbSet/FakeDbSet.cs
- Removed EF6 `IDbSet<T>` inheritance (removed from EF Core)
- Reimplemented as `DbSet<T>` subclass implementing `IQueryable<T>` and `IAsyncEnumerable<T>`
- Implemented `EntityType` abstract property (required by EF Core 10)
- Added `AsyncEnumeratorWrapper<T>` helper for async enumeration support
- Used `new` keyword (not `override`) for `AddRange`, `RemoveRange`, `Local` — these are not virtual in EF Core's `DbSet<T>`
- Added `#nullable enable`

### test/FakeDbSet/FakeUnicornDbSet.cs / FakeUserDbSet.cs / FakeBasketDbSet.cs
- Removed EF6 `System.Data.Entity` imports
- Updated `Find`/`FindAsync` signatures to use `object?[]?` nullable parameters (EF Core 10)
- Added `#nullable enable`

### test/UserControllerTests.cs / BasketControllerTests.cs / UnicornControllerTests.cs
- Replaced `System.Web.Http.Results` result types with ASP.NET Core equivalents:
  - `CreatedAtRouteNegotiatedContentResult<T>` → `CreatedAtRouteResult` (value in `.Value`)
  - `OkNegotiatedContentResult<T>` → `OkObjectResult` (value in `.Value`)
  - `NotFoundResult` → `NotFoundResult` (same name, different namespace)
  - `BadRequestResult` → `BadRequestResult` (same name)
  - `StatusCodeResult` → `StatusCodeResult` (`.StatusCode` is `int`)
  - `InvalidModelStateResult` → `BadRequestObjectResult`
- `DbUpdateConcurrencyException` namespace → `Microsoft.EntityFrameworkCore`
- `IHttpActionResult` → `IActionResult` where applicable
- Removed `System.Web.Http` and `System.Data.Entity.Infrastructure` usings
- `SaveChangesAsync()` mock setup updated to `SaveChangesAsync(default)` to match the `CancellationToken` overload

---

## Next Steps

- **Database migrations**: The EF Core DbContext uses code-first mapping (`OnModelCreating`). If the production database was originally managed by EF6 database-first (EDMX), run `dotnet ef migrations add InitialCreate --project src` to create an initial EF Core migration that matches the existing schema, then use `dotnet ef database update` to apply it. The table names in `OnModelCreating` (`basket`, `app_user`, `inventory`) match the original SQL schema — verify these against the actual production DB.
- **`year_model` computed column**: The DB seeding code attempts to add a computed column `year_model AS (datepart(year, date_create))` to the `inventory` table. This was done via raw ADO.NET in the original `Global.asax`. In production, run this as a one-time migration script or add it as an EF Core raw SQL migration.
- **Static frontend files**: The original app served a React/Angular SPA from a `build/` directory under IIS. In ASP.NET Core, files must be in `wwwroot/`. Copy the frontend build output to `src/wwwroot/` or update the CI/CD pipeline accordingly.
- **CORS policy**: The original app used open CORS (`*`). Review whether this should be tightened for production.
- **Container/deployment**: `Web.config` IIS rewrite rules have been replaced by ASP.NET Core middleware (`MapFallbackToFile`). If deploying behind IIS, update `web.config` (auto-generated by `Microsoft.NET.Sdk.Web`) to use `aspNetCore` handler. For Linux/Docker, Kestrel handles routing directly.
