using Microsoft.EntityFrameworkCore;
using ExpressionGenerator;

namespace DatabaseTest
{
    public class TransactionContext : DbContext
    {
        public TransactionContext(DbContextOptions options)
            : base(options) { }

        public DbSet<Transaction> DbTransactions { get; set; }
    }
}
