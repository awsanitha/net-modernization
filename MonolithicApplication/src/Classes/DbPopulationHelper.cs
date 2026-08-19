/*
 * Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
 * SPDX-License-Identifier: MIT-0
 */

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace UnicornShopLegacy.Classes
{
    public static class DbPopulationHelper
    {
        public static void PopulateIfNone(UnishopEntities context, IWebHostEnvironment environment)
        {
            try
            {
                if (!context.Database.CanConnect())
                {
                    context.Database.EnsureCreated();

                    // Add a computed column to the inventory table via raw SQL if it doesn't exist
                    var connStr = context.Database.GetDbConnection().ConnectionString;
                    using var sqlCon = new SqlConnection(connStr);
                    sqlCon.Open();
                    using var cmd = sqlCon.CreateCommand();
                    cmd.CommandText = "IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = N'year_model' AND Object_ID = Object_ID(N'inventory')) " +
                                      "ALTER TABLE inventory ADD year_model AS (datepart(year, date_create));";
                    cmd.ExecuteNonQuery();

                    var csvPath = Path.Combine(environment.ContentRootPath, "unicorns.csv");
                    if (File.Exists(csvPath))
                    {
                        string[] readText = File.ReadAllLines(csvPath);
                        var newUnicorns = new List<inventory>();
                        foreach (var line in readText)
                        {
                            string[] fields = line.Split(',');
                            var unicorn = new inventory
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

                        context.inventories.AddRange(newUnicorns);
                        context.SaveChanges();
                    }
                }
            }
            catch (Exception)
            {
                // If database is not available at startup (e.g., in test/dev without DB), skip seeding
            }
        }
    }
}
