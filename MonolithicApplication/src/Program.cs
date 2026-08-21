/*
 * Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
 * SPDX-License-Identifier: MIT-0
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this
 * software and associated documentation files (the "Software"), to deal in the Software
 * without restriction, including without limitation the rights to use, copy, modify,
 * merge, publish, distribute, sublicense, and/or sell copies of the Software, and to
 * permit persons to whom the Software is furnished to do so.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
 * INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A
 * PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
 * OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
 * SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using UnicornShopLegacy;
using UnicornShopLegacy.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Configure EF Core with SQL Server
var connectionString = builder.Configuration.GetConnectionString("UnishopEntities")
    ?? @"Data Source=db.unishop.local,1433\unishop;initial catalog=Unishop;User ID=admin;Password=dMdLgX6sZoXmOU2rnWTS;";

builder.Services.AddDbContext<UnishopEntities>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<IUnishopEntities, UnishopEntities>();

var app = builder.Build();

// Initialize database on startup
using (var scope = app.Services.CreateScope())
{
    try
    {
        var context = scope.ServiceProvider.GetRequiredService<UnishopEntities>();
        PopulateDbIfNone(context, app.Environment);
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"Database initialization error: {ex.Message}");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseCors();
app.UseRouting();
app.UseAuthorization();
app.MapControllers();

app.Run();

static void PopulateDbIfNone(UnishopEntities context, IWebHostEnvironment env)
{
    context.Database.EnsureCreated();

    if (context.inventories.Any())
    {
        return;
    }

    // Seed from CSV file
    var csvPath = Path.Combine(env.ContentRootPath, "unicorns.csv");
    if (!File.Exists(csvPath))
    {
        return;
    }

    var lines = File.ReadAllLines(csvPath);
    var newUnicorns = new System.Collections.Generic.List<inventory>();
    foreach (var line in lines)
    {
        var fields = line.Split(',');
        if (fields.Length < 6)
        {
            continue;
        }

        var unicorn = new inventory
        {
            unicorn_id = Guid.Parse(fields[0]),
            name = fields[1],
            description = fields[2],
            price = Convert.ToDecimal(fields[3]),
            image = fields[4],
            date_create = Convert.ToDateTime(fields[5]),
        };
        newUnicorns.Add(unicorn);
    }

    context.inventories.AddRange(newUnicorns);
    context.SaveChanges();
}
