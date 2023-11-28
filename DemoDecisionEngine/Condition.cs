namespace DemoDecisionEngine
{
    public class Condition
    {
        public int IdCondition { get; set; }
        public string PropertyName { get; set; }
        public Operator Operator { get; set; }
        public string Value { get; set; }
        public LogicalOperator LogicalOperator { get; set; }
        public string MinValue { get; set; }
        public string MaxValue { get; set; }
        public string DataType { get; set; }
        public List<string> Values { get; set; }
        public int? IdParentCondition { get; set; }
        public List<Condition> AdditionalConditions { get; set; }
    }

    public enum Operator
    {
        Equals,
        GreaterThan,
        LessThan,
        GreaterThanOrEqualTo,
        LessThanOrEqualTo,
        In,
        Between
    }

    public enum LogicalOperator
    {
        OR,
        AND
    }
}
