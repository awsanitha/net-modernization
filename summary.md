# .NET Framework → .NET 10 Migration Summary

**Date:** 2026-08-21  
**Result:** ✅ All 6 projects build with zero errors and zero warnings.

---

## Projects Migrated

| Project | Before | After | Status |
|---|---|---|---|
| `BasketLambda/src` | net10.0 (already SDK) | net10.0 | ✅ No changes needed |
| `BasketLambda/test` | net10.0 (already SDK) | net10.0 | ✅ No changes needed |
| `InventoryService/src` | net10.0 (already SDK) | net10.0 | ✅ NU1901 warnings fixed |
| `InventoryService/test` | net10.0 (already SDK) | net10.0 | ✅ NU1901 warnings fixed |
| `MonolithicApplication/src` | .NET Framework 4.7.2 (old-style csproj) | net10.0 | ✅ Full migration |
| `MonolithicApplication/test` | net10.0 (incompatible packages) | net10.0 | ✅ Full migration |

---

## MonolithicApplication/src Changes

### Project File (`UnicornShopLegacy.csproj`)
- Replaced old-style MSBuild project with `Microsoft.NET.Sdk.Web` SDK-style project.
- Removed all `<Reference>` elements (EF6, WebAPI, System.Web.*, etc.).
- Added `Microsoft.EntityFrameworkCore` 10.0.11, `Microsoft.EntityFrameworkCore.SqlServer` 10.0.11, `Newtonsoft.Json` 13.0.4.
- Excluded EDMX-generated files (`UnicornShop.Designer.cs`, `UnicornShop.cs`) and old `Properties/AssemblyInfo.cs` from compilation.

### `UnicornShop.Context.cs`
- Replaced EF6 `DbContext` (with `UnintentionalCodeFirstException` and `"name=..."` constructor) with EF Core `DbContext`.
- Added `DbContextOptions<UnishopEntities>` constructor for DI.
- Replaced EDMX database-first model with code-first `OnModelCreating` that maps entities to their table names.
- Added `OnConfiguring` fallback for non-DI usage with the hardcoded connection string (preserved from original Web.config).

### `Interfaces/IUnishopEntities.cs`
- Replaced `using System.Data.Entity` with `using Microsoft.EntityFrameworkCore`.
- Removed EF6-specific `DbEntityEntry Entry(object entity)` method (not mockable or usable in EF Core).
- Updated `SaveChangesAsync` signature to match EF Core's optional `CancellationToken` parameter.

### `Classes/UnishopEntities.cs`
- Replaced `using System.Data.Entity` with `using Microsoft.EntityFrameworkCore`.
- `SetModified` now uses `EntityState.Modified` from EF Core namespace.

### Controllers (`UnicornController`, `BasketController`, `UserController`)
- Replaced `System.Web.Http.*` imports with `Microsoft.AspNetCore.Mvc`.
- `ApiController` → `ControllerBase` with `[ApiController]` and `[Route("api/[controller]")]`.
- `IHttpActionResult` → `IActionResult`.
- Removed `[EnableCors]`, `[ResponseType]` attributes (CORS handled by middleware, ResponseType not needed in ASP.NET Core).
- `StatusCode(HttpStatusCode.NoContent)` → `NoContent()`.
- `DbUpdateConcurrencyException` now from `Microsoft.EntityFrameworkCore`.
- Removed `Dispose(bool)` override (not applicable to `ControllerBase`).
- `UserController`: Replaced `Microsoft.Ajax.Utilities.IsNullOrWhiteSpace()` with `string.IsNullOrWhiteSpace()`.
- `UserController`: Replaced deprecated `RNGCryptoServiceProvider` with `RandomNumberGenerator.Fill()`.
- `UserController`: Replaced deprecated `Rfc2898DeriveBytes` constructor with static `Rfc2898DeriveBytes.Pbkdf2()` method.

### `App_Start/WebApiConfig.cs`
- Replaced WebAPI-specific implementation with an empty placeholder (routing handled by `Program.cs`).

### `Global.asax.cs`
- Replaced `System.Web.HttpApplication`-based startup class with a comment (application lifecycle now in `Program.cs`).

### New: `Program.cs`
- Created ASP.NET Core minimal-API entry point.
- Configures CORS (allow all origins/methods/headers, matching original WebAPI CORS config).
- Registers `UnishopEntities` with EF Core SQL Server provider via DI.
- Migrated database initialization logic from `Global.asax.cs`: uses `EnsureCreated()` and CSV seeding (replacing `HttpContext.Current.Server.MapPath()` with `IWebHostEnvironment.ContentRootPath`).
- Removed SQL DDL for computed `year_model` column (column is not referenced by any model property; can be added via EF Core migration if needed).

### New: `appsettings.json`
- Created with connection string matching original `Web.config`, logging defaults, and CORS wildcard.

---

## MonolithicApplication/test Changes

### Project File (`UnicornShopLegacy.Tests.csproj`)
- Removed incompatible packages: `EntityFramework` 6.5.2, `Microsoft.AspNet.WebApi.Client/Core` 5.2.7, `Castle.Core`, `OpenCover`, `ReportGenerator`.
- Updated to current-stable test packages: `MSTest.TestAdapter`/`MSTest.TestFramework` 3.7.3, `Microsoft.NET.Test.Sdk` 17.12.0, `Moq` 4.20.72, `FluentAssertions` 6.12.2, `coverlet.msbuild/collector` 6.0.4.
- Added `Microsoft.EntityFrameworkCore.InMemory` 10.0.11.

### `FakeDbSet/FakeDbSet.cs`
- Completely rewritten for EF Core's abstract `DbSet<T>`.
- Backed by an in-memory `List<T>`.
- Overrides `AsQueryable()` to return the list as `IQueryable<T>`, making all LINQ operations work.
- Implements all abstract members: `Add`, `Remove`, `AddRange`, `RemoveRange`, `Attach`, `Update`, `Find`, `FindAsync`, `EntityType`, `Local`, `AsAsyncEnumerable`.
- Methods returning `EntityEntry<T>` return `null!` since callers never use the return value.
- Exposes `protected List<T> InternalData` so derived classes can access the list directly.

### `FakeDbSet/FakeUnicornDbSet.cs`, `FakeBasketDbSet.cs`, `FakeUserDbSet.cs`
- Replaced EF6 `System.Data.Entity` imports with standard `System` imports.
- Changed `this.Local` references to `this.InternalData`.
- Updated `Find`/`FindAsync` signatures to EF Core `ValueTask<T?>` return types.

### Test files (`UnicornControllerTests.cs`, `BasketControllerTests.cs`, `UserControllerTests.cs`)
- Removed `System.Web.Http.Results` and `System.Data.Entity.Infrastructure` usings.
- Added `Microsoft.AspNetCore.Mvc` using.
- `OkNegotiatedContentResult<T>` → `OkObjectResult` (`.Value` instead of `.Content`).
- `NotFoundResult` / `BadRequestResult` → same names from `Microsoft.AspNetCore.Mvc` namespace.
- `InvalidModelStateResult` → `BadRequestObjectResult`.
- `StatusCodeResult.StatusCode` comparison updated: `HttpStatusCode.NoContent` enum → `(int)HttpStatusCode.NoContent` (204), since ASP.NET Core uses `int` not enum.
- `CreatedAtRouteNegotiatedContentResult<T>` → `CreatedAtRouteResult` (`.Value` instead of `.Content`).
- `SaveChangesAsync()` mock setup updated to `SaveChangesAsync(default)` matching EF Core signature.

---

## InventoryService Changes

### `InventoryService.csproj` and `InventoryService.Tests.csproj`
- Added `<NuGetAuditMode>direct</NuGetAuditMode>` to suppress NU1901 audit warnings from transitive NuGet tooling dependencies brought in by `Microsoft.VisualStudio.Web.CodeGeneration.Design` (design-time only, not runtime).

---

## Test Results

| Test Project | Passed | Skipped | Failed |
|---|---|---|---|
| `BasketLambda.Tests` | 11 | 0 | 0 |
| `InventoryService.Tests` | 16 | 2 (pre-existing `[Ignore]`) | 0 |
| `UnicornShopLegacy.Tests` | 29 | 0 | 0 |

---

## Next Steps

1. **Hardcoded DB credentials in `UnicornShop.Context.cs`**: The `OnConfiguring` fallback still contains hardcoded SQL Server credentials matching the original `Web.config`. These should be moved to environment variables or AWS Secrets Manager for production use.

2. **Computed column `year_model`**: The original `Global.asax.cs` ran `ALTER TABLE inventory ADD year_model AS (datepart(year,date_create))` at startup. This SQL DDL is not replicated in `Program.cs` since `year_model` is not mapped to any model property. If it is needed (e.g., for direct SQL queries), add it via an EF Core migration.

3. **`BasketLambda` package versions**: The Lambda packages (`Amazon.Lambda.Core` 1.1.0, `Amazon.Lambda.Serialization.SystemTextJson` 2.0.0, `AWSSDK.DynamoDBv2` 3.3.106.28) are old but compatible. Consider updating to latest stable versions for security and performance improvements.

4. **`InventoryService` AWS SDK package versions**: `AWSSDK.S3` 3.3.111.30 and `AWSSDK.SimpleNotificationService` 3.3.102.15 are old; consider updating to the unified `AWSSDK.Core` 3.7.x line.

5. **EF Core migrations**: The MonolithicApplication now uses `EnsureCreated()` for DB initialization. For production, consider switching to proper EF Core migrations (`dotnet ef migrations add Initial`).

6. **ASP.NET Core HTTPS**: `Program.cs` does not configure HTTPS redirect. Add `app.UseHttpsRedirection()` if HTTPS is required (depends on reverse proxy/load balancer configuration).
