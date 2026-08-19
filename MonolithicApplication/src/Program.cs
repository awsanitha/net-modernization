/*
 * Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
 * SPDX-License-Identifier: MIT-0
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UnicornShopLegacy;
using UnicornShopLegacy.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

builder.Services.AddDbContext<UnishopEntities>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("UnishopEntities")));

// Register context via interface for DI
builder.Services.AddScoped<IUnishopEntities>(sp =>
    sp.GetRequiredService<UnishopEntities>());

var app = builder.Build();

// Configure pipeline
app.UseCors();
app.UseRouting();
app.MapControllers();

// Populate DB if none
await PopulateDbIfNone(app);

app.Run();

static async Task PopulateDbIfNone(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<UnishopEntities>();

    await context.Database.EnsureCreatedAsync();

    if (!await context.inventorySet.AnyAsync())
    {
        var csvPath = Path.Combine(app.Environment.ContentRootPath, "unicorns.csv");
        if (File.Exists(csvPath))
        {
            var newUnicorns = new List<inventory>();
            foreach (var line in await File.ReadAllLinesAsync(csvPath))
            {
                var fields = line.Split(',');
                if (fields.Length >= 6)
                {
                    newUnicorns.Add(new inventory
                    {
                        unicorn_id = Guid.Parse(fields[0]),
                        name = fields[1],
                        description = fields[2],
                        price = Convert.ToDecimal(fields[3]),
                        image = fields[4],
                        date_create = Convert.ToDateTime(fields[5])
                    });
                }
            }

            context.inventorySet.AddRange(newUnicorns);
            await context.SaveChangesAsync();
        }
    }
}
