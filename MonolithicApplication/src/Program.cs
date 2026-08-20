/*
 * Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
 * SPDX-License-Identifier: MIT-0
 */

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using UnicornShopLegacy;
using UnicornShopLegacy.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers()
    .AddNewtonsoftJson();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddDbContext<UnishopEntities>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("UnishopEntities")));

builder.Services.AddScoped<IUnishopEntities, UnishopEntities>();

var app = builder.Build();

// Populate DB if none exists
using (var scope = app.Services.CreateScope())
{
    try
    {
        var context = scope.ServiceProvider.GetRequiredService<UnishopEntities>();
        PopulateDbIfNone(context, app.Environment.ContentRootPath);
    }
    catch
    {
        // DB may not be available at startup (e.g. test/dev environments) — continue
    }
}

// Rewrite rules (equivalent to Web.config system.webServer/rewrite rules)
var rewriteOptions = new RewriteOptions()
    .AddRewrite(@"^$", "/index.html", skipRemainingRules: false)
    .AddRewrite(@"^(.*[.].*)$", "/build/$1", skipRemainingRules: true)
    .AddRewrite(@"^(?!api/).*$", "/build/index.html", skipRemainingRules: false);
app.UseRewriter(rewriteOptions);

app.UseStaticFiles();
app.UseCors("AllowAll");
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "DefaultApi",
    pattern: "api/{controller}/{id?}");

app.MapControllers();

app.Run();

static void PopulateDbIfNone(UnishopEntities context, string contentRootPath)
{
    if (!context.Database.CanConnect())
    {
        context.Database.EnsureCreated();

        // Add computed column for year_model on inventory table
        try
        {
            var connString = context.Database.GetConnectionString();
            using var sqlCon = new SqlConnection(connString);
            using var cmd = new SqlCommand("ALTER TABLE inventory ADD year_model AS (datepart(year,date_create));", sqlCon);
            sqlCon.Open();
            cmd.ExecuteNonQuery();
        }
        catch
        {
            // Column may already exist — continue
        }

        var csvPath = Path.Combine(contentRootPath, "unicorns.csv");
        if (File.Exists(csvPath))
        {
            string[] readText = File.ReadAllLines(csvPath);
            var newUnicorns = new List<UnicornShopLegacy.inventory>();
            foreach (var line in readText)
            {
                string[] fields = line.Split(',');
                if (fields.Length >= 6)
                {
                    var unicorn = new UnicornShopLegacy.inventory
                    {
                        unicorn_id = Guid.Parse(fields[0]),
                        name = fields[1],
                        description = fields[2],
                        price = Convert.ToDecimal(fields[3]),
                        image = fields[4],
                        date_create = Convert.ToDateTime(fields[5])
                    };
                    newUnicorns.Add(unicorn);
                }
            }
            context.inventories.AddRange(newUnicorns);
            context.SaveChanges();
        }
    }
}
