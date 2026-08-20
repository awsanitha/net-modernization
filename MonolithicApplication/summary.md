# Migration Summary: .NET Framework 4.7.2 → .NET 10

## Migration Status: ✅ COMPLETE

Both projects build with **0 errors, 0 warnings**. All **29 unit tests pass**.

---

## Changes Made

### `src/UnicornShopLegacy.csproj`
- Replaced legacy verbose MSBuild XML with SDK-style project (`Microsoft.NET.Sdk.Web`)
- Target framework: `net10.0`
- Output type: `Exe` (ASP.NET Core web app)
- Removed all legacy `<Reference>` elements (EF6, System.Web.Http, WebGrease, Antlr, etc.)
- Added: `Microsoft.EntityFrameworkCore` + `Microsoft.EntityFrameworkCore.SqlServer` 10.0.0
- Added: `Microsoft.EntityFrameworkCore.Design` 10.0.0
- Added: `Newtonsoft.Json` 13.0.3 + `Microsoft.AspNetCore.Mvc.NewtonsoftJson` 10.0.0
- Suppressed `CS8981` (lowercase type names are intentional — match DB schema)
- Excluded all legacy auto-generated EF6 T4 files and `Global.asax`/`Web.config` content

### New Files (src)
- `src/Program.cs` — replaces `Global.asax.cs` and `App_Start/WebApiConfig.cs`; ASP.NET Core startup with CORS, EF Core DI, routing, URL rewrite rules
- `src/appsettings.json` + `src/appsettings.Development.json` — replaces `Web.config`
- `src/Models/basket.cs`, `src/Models/user.cs`, `src/Models/inventory.cs` — clean EF Core entity models (replaces EF6 T4-generated files)
- `src/Data/UnishopContext.cs` — EF Core `DbContext` with `DbContextOptions<T>` constructor pattern; replaces EF6 auto-generated `UnicornShop.Context.cs`

### Updated Files (src)
- `src/Interfaces/IUnishopEntities.cs` — replaced `DbEntityEntry` (EF6) with `EntityEntry` (EF Core); updated `SaveChangesAsync()` signature
- `src/Classes/UnishopEntities.cs` — EF Core `EntityState.Modified`; explicit `IUnishopEntities.SaveChangesAsync()` wrapper
- `src/Controllers/UserController.cs` — migrated from `ApiController`/`IHttpActionResult` to `ControllerBase`/`IActionResult`; replaced obsolete `RNGCryptoServiceProvider` with `RandomNumberGenerator.Create()`; replaced obsolete `Rfc2898DeriveBytes` constructor with `Rfc2898DeriveBytes.Pbkdf2` static method (SYSLIB0060 resolved)
- `src/Controllers/BasketController.cs` — migrated from `ApiController` to `ControllerBase`; added HTTP verb attributes
- `src/Controllers/UnicornController.cs` — migrated from `ApiController` to `ControllerBase`; added HTTP verb attributes

### `test/UnicornShopLegacy.Tests.csproj`
- Updated all packages to modern versions compatible with net10.0
- Removed legacy packages: `EntityFramework` 6, `Microsoft.AspNet.WebApi.*`, `Castle.Core`, `OpenCover`, `ReportGenerator`, `System.ComponentModel.Annotations`, `System.Runtime.CompilerServices.Unsafe`, `System.Configuration.ConfigurationManager`
- Added: `Microsoft.EntityFrameworkCore` 10.0.0, `coverlet.msbuild` 6.0.4
- Updated: `MSTest` 3.6.4, `Moq` 4.20.72, `FluentAssertions` 6.12.0, `Microsoft.NET.Test.Sdk` 17.12.0
- Excluded legacy `Properties/AssemblyInfo.cs` (SDK-generated)

### Updated Files (test)
- `test/FakeDbSet/FakeDbSet.cs` — replaced EF6 `IDbSet<T>` base with EF Core `DbSet<T>`; implemented abstract `EntityType` property; corrected `AddRange`/`RemoveRange` return types to `void` (EF Core 10 signature); added `IAsyncEnumerable<T>` support
- `test/FakeDbSet/FakeUserDbSet.cs` — updated to EF Core `FindAsync(object?[]?, CancellationToken)` signature
- `test/FakeDbSet/FakeUnicornDbSet.cs` — updated to EF Core `FindAsync` signatures
- `test/FakeDbSet/FakeBasketDbSet.cs` — updated to EF Core `FindAsync` signatures
- `test/UserControllerTests.cs` — replaced `System.Web.Http.Results` types with ASP.NET Core: `CreatedAtRouteNegotiatedContentResult<T>` → `CreatedAtRouteResult`, `OkNegotiatedContentResult<T>` → `OkObjectResult`, `NotFoundResult`/`BadRequestResult`/`StatusCodeResult` same names new namespace
- `test/BasketControllerTests.cs` — same result type migration; `InvalidModelStateResult` → `BadRequestObjectResult`
- `test/UnicornControllerTests.cs` — same result type migration; removed `System.Web.Http.*` and `System.Data.Entity.Infrastructure` imports

---

## Architecture After Migration

- **Framework**: ASP.NET Core (`Microsoft.NET.Sdk.Web`) on net10.0
- **Data access**: EF Core 10 with SQL Server provider; `DbContextOptions<T>` injection pattern
- **API**: ASP.NET Core Web API (`[ApiController]`, `ControllerBase`, attribute routing)
- **CORS**: Built-in `AddCors`/`UseCors` with "AllowAll" policy
- **Configuration**: `appsettings.json` / `appsettings.Development.json`
- **Startup**: `Program.cs` (minimal hosting model)
- **Cryptography**: `RandomNumberGenerator.Create()` + `Rfc2898DeriveBytes.Pbkdf2` static API
- **Tests**: MSTest 3.x + Moq + FluentAssertions; in-memory `FakeDbSet<T>` based on EF Core `DbSet<T>`

---

## Next Steps

- The `src/UnicornShop.edmx` and related T4 template files remain in the repo as artifacts but are excluded from compilation. They can be safely deleted from the filesystem once the team has confirmed the migration.
- The legacy `src/Global.asax`, `src/Global.asax.cs`, `src/Web.config`, `src/App_Start/WebApiConfig.cs`, and `src/packages.config` files remain on disk but are excluded from the build. They can be deleted.
- The `Properties/AssemblyInfo.cs` in the test project remains on disk but is excluded; can be deleted.
- The DB population logic in `Program.cs` uses `CanConnect()` to guard seeding — this requires a reachable SQL Server at startup. In environments without a DB at startup (CI, dev), this is already wrapped in a try/catch and will silently skip seeding.
- Consider creating EF Core migrations (`dotnet ef migrations add Initial`) to manage the schema going forward instead of the EF6 database-first EDMX approach.
- The `year_model` computed column (added via raw SQL in `Populate_DB_If_None`) is not mapped in the EF Core model since it was a computed column added post-schema-create. If it's needed in queries, add it to the `inventory` entity and configure it as a computed column in `OnModelCreating`.
