using Microsoft.EntityFrameworkCore;
using ExpressionGenerator;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using System;
using System.Text.Json;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace DatabaseTest
{
    class Program
    {
        public static readonly ILoggerFactory loggerFactory =
            LoggerFactory.Create(builder =>
            {
                builder.AddFilter("Microsoft.EntityFrameworkCore.Database.Command",
                    LogLevel.Information)
                .AddConsole();
            });

        static async Task Main(string[] args)
        {
            Console.WriteLine("Creating and seeding database...");
            var options = new DbContextOptionsBuilder<TransactionContext>()
                .UseSqlite("Data Source=transactions.db").Options;
            using (var context = new TransactionContext(options))
            {
                await context.Database.EnsureDeletedAsync();
                await context.Database.EnsureCreatedAsync();
                context.DbTransactions.AddRange(Transaction.GetList(1000));
                await context.SaveChangesAsync();
            }

            var optionsWithLog = new DbContextOptionsBuilder<TransactionContext>()
                .UseSqlite("Data Source=transactions.db")
                .UseLoggerFactory(loggerFactory).Options;

            using (var context = new TransactionContext(optionsWithLog))
            {
                var count = await context.DbTransactions.CountAsync();
                Console.WriteLine($"Verified insert count: {count}.");
                Console.WriteLine("Parsing expression...");
                var parser = new JsonExpressionParser();
                var predicate = parser.ParseExpressionOf<Transaction>(
                    JsonDocument.Parse(
                        await File.ReadAllTextAsync("databaseRules.json")));
                Console.WriteLine("Retrieving from database...");
                var query = context.DbTransactions.Where(predicate)
                    .OrderBy(t => t.Id);
                var results = await query.ToListAsync();
                Console.WriteLine($"Retrieved {results.Count}");
                Console.WriteLine("Sample:");
                Console.WriteLine(results[0]);
            }
        }
    }
}
