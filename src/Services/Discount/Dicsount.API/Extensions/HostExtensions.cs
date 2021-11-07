using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Discount.API.Extensions
{
    public static class HostExtensions
    {
        public static IHost MigrateDatabase<TContext>(this IHost host, int? retyr = 0)
        {
            int retryForAvailabitity = retyr.Value;

            using (var scope = host.Services.CreateScope())
            {
                var service = scope.ServiceProvider;
                var configuration = service.GetRequiredService<IConfiguration>();
                var logger = service.GetRequiredService<ILogger<TContext>>();

                try
                {
                    logger.LogInformation("Migrating postgressql database");

                    using var connection = new NpgsqlConnection(configuration.GetValue<string>("DatabaseSettings:connectionString"));
                    connection.Open();

                    using var command = new NpgsqlCommand
                    {
                        Connection = connection
                    };
                    command.CommandText = "Drop table if exists coupon";
                    command.ExecuteNonQuery();

                    command.CommandText = @"CREATE TABLE Coupon (Id SERIAL PRIMARY KEY, ProductName VARCHAR(24) NOT NULL, Description TEXT, Amount INT)";
                    command.ExecuteNonQuery();

                    command.CommandText = @"INSERT INTO Coupon (ProductName,Description, Amount) Values ('IPhone X','IPhone Discount',150)";
                    command.ExecuteNonQuery();
                    command.CommandText = @"INSERT INTO Coupon (ProductName,Description, Amount) Values ('Samsung 10','Samung Discount',100)";
                    command.ExecuteNonQuery();

                    logger.LogInformation("Migrated to database");

                }
                catch (NpgsqlException ex)
                {
                    logger.LogError(ex, "an error occured while migrating");
                    if(retryForAvailabitity < 50)
                    {
                        retryForAvailabitity++;
                        Thread.Sleep(2000);
                        MigrateDatabase<TContext>(host, retryForAvailabitity);
                    }
                }

            }
            return host;
        }
    }
}
