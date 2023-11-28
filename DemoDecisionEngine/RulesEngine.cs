using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection.Metadata;

namespace DemoDecisionEngine
{
    public class RulesEngine
    {
        private readonly ConcurrentDictionary<string, Func<object, bool>> cache = new ConcurrentDictionary<string, Func<object, bool>>();

        public Func<object, bool> BuildRule<T>(List<Condition> rules)
        {
            string ruleKey = GetRuleKey(rules);

            return cache.GetOrAdd(ruleKey, key => CompileRule<T>(rules));
        }

        private string GetRuleKey(List<Condition> rules)
        {
            // Concatenate rule properties to form a unique key
            return string.Join(";", rules.Select(r => $"{r.PropertyName}-{r.Operator}-{r.IdCondition}"));
        }

        private Func<object, bool> CompileRule<T>(List<Condition> rules)
        {
            var parameter = Expression.Parameter(typeof(object), "x");
            UnaryExpression castedParameter = Expression.Convert(parameter, typeof(T));
            Expression expr = null;

            foreach (var rule in rules)
            {
                ValidateRule<T>(rule); // Validate each rule

                Expression binaryExpr = BuildExpressionForRule<T>(castedParameter, rule);

                if (rule.AdditionalConditions != null && rule.AdditionalConditions.Any())
                {
                    List<Expression> additionalExpressions
                    = rule.AdditionalConditions.Select(cond => BuildExpressionForRule<T>(castedParameter, cond)).ToList();

                    foreach (var additionalExpression in additionalExpressions)
                    {
                        binaryExpr = Expression.AndAlso(binaryExpr, additionalExpression);
                    }
                }

                if (expr == null)
                {
                    expr = binaryExpr;
                }
                else
                {
                    expr = rule.LogicalOperator == LogicalOperator.AND ?
                           Expression.AndAlso(expr, binaryExpr) :
                           Expression.OrElse(expr, binaryExpr);
                }
            }

            var lambda = Expression.Lambda<Func<object, bool>>(expr, parameter);
            return lambda.Compile();
        }

        private Expression BuildExpressionForRule<T>(Expression parameter, Condition rule)
        {
            var member = Expression.Property(parameter, rule.PropertyName);
            var targetType = member.Type;

            object value = ConvertToType(rule.Value, targetType);
            object minValue = ConvertToType(rule.MinValue, targetType);
            object maxValue = ConvertToType(rule.MaxValue, targetType);

            Expression left = null, right = null;

            if (minValue != null)
                left = Expression.GreaterThanOrEqual(member, Expression.Constant(minValue));

            if (maxValue != null)
                right = Expression.LessThanOrEqual(member, Expression.Constant(maxValue));

            if (left != null && right != null)
                return Expression.AndAlso(left, right);

            switch (rule.Operator)
            {
                case Operator.GreaterThan:
                    return Expression.GreaterThan(member, Expression.Constant(value));
                case Operator.GreaterThanOrEqualTo:
                    return Expression.GreaterThanOrEqual(member, Expression.Constant(value));
                case Operator.LessThan:
                    return Expression.LessThan(member, Expression.Constant(value));
                case Operator.LessThanOrEqualTo:
                    return Expression.LessThanOrEqual(member, Expression.Constant(value));
                case Operator.Equals:
                    return Expression.Equal(member, Expression.Constant(value));
                case Operator.In:
                    var values = rule.Values.ConvertAll(value => ConvertToType(value, targetType));
                    return BuildInExpression(member, values);
                case Operator.Between:
                    return BuildBetweenExpression(member, minValue, maxValue);
                    // Add other operators as needed
            }

            return left ?? right;
        }

        private Expression BuildInExpression(MemberExpression member, List<object> values)
        {
            // Logic for 'IN' condition
            var equalsExpressions = values.Select(value => Expression.Equal(member, Expression.Constant(value)));
            return equalsExpressions.Aggregate((current, next) => Expression.OrElse(current, next));
        }

        private Expression BuildBetweenExpression(MemberExpression member, object minValue, object maxValue)
        {
            // Logic for 'BETWEEN' condition
            var greaterThanMin = Expression.GreaterThanOrEqual(member, Expression.Constant(minValue));
            var lessThanMax = Expression.LessThanOrEqual(member, Expression.Constant(maxValue));
            return Expression.AndAlso(greaterThanMin, lessThanMax);
        }

        private void ValidateRule<T>(Condition rule)
        {
            if (string.IsNullOrEmpty(rule.PropertyName))
                throw new InvalidOperationException("Rule property name is required.");

            var propertyInfo = typeof(T).GetProperty(rule.PropertyName);
            if (propertyInfo == null)
                throw new InvalidOperationException($"Property {rule.PropertyName} not found in type {typeof(T).Name}.");

            // Add more validation as needed...
        }

        private object ConvertToType(string value, Type targetType)
        {
            try
            {
                if (string.IsNullOrEmpty(value))
                    return null;

                if (targetType == typeof(DateTime))
                    return DateTime.Parse(value);

                if (targetType.IsEnum)
                    return Enum.Parse(targetType, value);

                return Convert.ChangeType(value, targetType);
            }
            catch(Exception ex)
            {
                throw new InvalidOperationException("Error converting value", ex);
            }
        }
    }
}
