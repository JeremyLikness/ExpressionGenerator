using System;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace ExpressionGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            static void writeAndWait(string str)
            {
                Console.WriteLine(str);
                Console.ReadLine();
            }

            var jsonStr = File.ReadAllText("rules.json");
            var jsonDocument = JsonDocument.Parse(jsonStr);

            var jsonExpressionParser = new JsonExpressionParser();
            var predicate = jsonExpressionParser
                .ParsePredicateOf<Transaction>(jsonDocument);
            
            var transactionList = Transaction.GetList(1000);
            
            var filteredTransactions = transactionList.Where(predicate).ToList();
            
            writeAndWait($"Filtered to {filteredTransactions.Count} entities.");
            
            filteredTransactions.ForEach(Console.WriteLine);
        }
    }
}
