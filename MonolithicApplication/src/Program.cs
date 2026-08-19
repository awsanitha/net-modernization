/*
 * Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
 * SPDX-License-Identifier: MIT-0
 */

using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using UnicornShopLegacy;
using UnicornShopLegacy.Classes;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers()
    .AddNewtonsoftJson();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddDbContext<UnishopEntities>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("UnishopEntities") ?? string.Empty));

var app = builder.Build();

// Seed database if it does not yet contain data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<UnishopEntities>();
    DbPopulationHelper.PopulateIfNone(context, app.Environment);
}

app.UseCors();

// Serve static files from wwwroot (SPA build output)
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();

// Fall back to index.html for SPA routes (non-API, non-file requests)
app.MapFallbackToFile("index.html");

app.Run();
