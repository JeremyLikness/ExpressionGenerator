using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;

namespace ExpressionGenerator
{
    public class JsonExpressionParser
    {
        private const string StringStr = "string";

        private readonly string BooleanStr = nameof(Boolean).ToLower();
        private readonly string Number = nameof(Number).ToLower();
        private readonly string In = nameof(In).ToLower();
        private readonly string And = nameof(And).ToLower();

        private readonly MethodInfo MethodContains = typeof(Enumerable).GetMethods(
                        BindingFlags.Static | BindingFlags.Public)
                        .Single(m => m.Name == nameof(Enumerable.Contains)
                            && m.GetParameters().Length == 2);

        private delegate Expression Binder(Expression left, Expression right);

        private Expression ParseTree<T>(
            JsonElement condition,
            ParameterExpression parm)
        {
            Expression left = null;
            var gate = condition.GetProperty(nameof(condition)).GetString();
            
            JsonElement rules = condition.GetProperty(nameof(rules));

            Binder binder = gate == And ? (Binder)Expression.And : Expression.Or;

            Expression bind(Expression left, Expression right) =>
                left == null ? right : binder(left, right);

            foreach (var rule in rules.EnumerateArray())
            {
                if (rule.TryGetProperty(nameof(condition), out JsonElement check))
                {
                    var right = ParseTree<T>(rule, parm);
                    left = bind(left, right);
                    continue;
                }
                
                string @operator = rule.GetProperty(nameof(@operator)).GetString();
                string type = rule.GetProperty(nameof(type)).GetString();
                string field = rule.GetProperty(nameof(field)).GetString();
                
                JsonElement value = rule.GetProperty(nameof(value));
                
                var property = Expression.Property(parm, field);
                
                if (@operator == In)
                {
                    var contains = MethodContains.MakeGenericMethod(typeof(string));
                    object val = value.EnumerateArray().Select(e => e.GetString())
                        .ToList();
                    var right = Expression.Call(
                        contains,
                        Expression.Constant(val),
                        property);
                    left = bind(left, right);
                }
                else
                {
                    object val = (type == StringStr || type == BooleanStr) ?
                        (object)value.GetString() : value.GetDecimal();
                    var toCompare = Expression.Constant(val);
                    var right = Expression.Equal(property, toCompare);
                    left = bind(left, right);
                }
            }

            return left;
        }

        public Func<T, bool> ParsePredicateOf<T>(JsonDocument doc)
        {
            var itemExpression = Expression.Parameter(typeof(T));
            var conditions = ParseTree<T>(doc.RootElement, itemExpression);
            if (conditions.CanReduce)
            {
                conditions = conditions.ReduceAndCheck();
            }

            Console.WriteLine(conditions.ToString());
            Console.ReadLine();

            var query = Expression.Lambda<Func<T, bool>>(conditions, itemExpression);
            return query.Compile();
        }
    }
}