using System;
using System.Collections.Generic;

namespace ExpressionGenerator
{
    public class Transaction
    {
        public int Id { get; set; }
        public string Category { get; set; }
        public string TransactionType { get; set; }
        public string PaymentMode { get; set; }
        public decimal Amount { get; set; }

        public static string GetOne(Random random, string[] list)
            => list[(int)Math.Floor(random.NextDouble() * list.Length)];
     
        public static string[] Categories = new[]
        {
            "Clothing",
            "Kitchen",
            "Electronics",
            "Landscaping",
            "Home and Garden"
        };

        public static string[] TransactionTypes = new[]
        {
            "payment",
            "income"
        };

        public static string[] PaymentModes = new[]
        {
            "Cash",
            "Check",
            "Credit"
        };

        public static decimal Amounts(Random random)
        {
            if (random.NextDouble() < 0.5)
            {
                return 10;
            }
            return (decimal)(random.NextDouble() * 1000);
        }

        public static List<Transaction> GetList(int count)
        {
            var random = new Random();
            var id = 1;
            var result = new List<Transaction>
            {
                new Transaction
                {
                    Id = id++,
                    Amount = 10,
                    Category = "Clothing",
                    PaymentMode = "Cash",
                    TransactionType = "income"
                }
            };
            while (--count > 0)
            {
                var newTransaction = new Transaction
                {
                    Id = id++,
                    Amount = Amounts(random),
                    Category = GetOne(random, Categories),
                    PaymentMode = GetOne(random, PaymentModes),
                    TransactionType = GetOne(random, TransactionTypes)
                };
                result.Add(newTransaction);
            }
            return result;
        }

        public override string ToString()
        {
            var amount = Amount.ToString("c");
            return $"Id:{Id} Category:{Category} Mode:{PaymentMode} Type:{TransactionType} Amount:{amount}";
        }
    }
}
