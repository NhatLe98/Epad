using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EPAD_Data.Extensions
{
    public static class DbMigrationHelpers
    {
        public static async Task EnsureSeedData(this Microsoft.AspNetCore.Hosting.IWebHost host)
        {
            try
            {
                Console.WriteLine("Migrate database to latest version");

                using var serviceScope = host.Services.CreateScope();
                var context = serviceScope.ServiceProvider.GetService<EPAD_Context>();
                await context.Database.MigrateAsync();
                Console.WriteLine(context.Database.GetConnectionString());
                Console.WriteLine("The database is already up to date");
                var mr = context.Database.GetAppliedMigrations().Last();

                await Task.Delay(1000);
                Console.WriteLine();
                Console.Write("Migration applied: ");
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine(mr);
                Console.WriteLine();
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error on seed db: " + ex.Message);
                throw ex;
            }
        }
    }
}
