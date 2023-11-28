using DemoDecisionEngine;

var rules = new List<Condition>
{
    new Condition { PropertyName = "Balance", Operator = Operator.GreaterThan, Value = "100", DataType = "decimal", LogicalOperator = LogicalOperator.AND },
    new Condition { PropertyName = "Balance", Operator = Operator.LessThan, Value = "10000", DataType = "decimal", LogicalOperator = LogicalOperator.AND },
    new Condition { PropertyName = "IsActive", Operator = Operator.Equals, Value = "True", DataType = "bool", LogicalOperator = LogicalOperator.AND },
    new Condition { PropertyName = "AccountType", Operator = Operator.In, Value = "Savings", Values = new List<string> { "Savings", "Checking" }, DataType = "AccountType", LogicalOperator = LogicalOperator.AND },
    new Condition { PropertyName = "TransactionCount", Operator = Operator.GreaterThanOrEqualTo, Value = "5", DataType = "int", LogicalOperator = LogicalOperator.AND },
    new Condition { PropertyName = "CreditScore", Operator = Operator.GreaterThanOrEqualTo, Value = "700", DataType = "decimal", LogicalOperator = LogicalOperator.AND },
    new Condition { PropertyName = "LastTransactionAmount", Operator = Operator.LessThanOrEqualTo, Value = "5000", DataType = "decimal", LogicalOperator = LogicalOperator.AND },
    new Condition { PropertyName = "AccountAgeInYears", Operator = Operator.GreaterThanOrEqualTo, Value = "1", DataType = "int", LogicalOperator = LogicalOperator.AND },
    new Condition { PropertyName = "NumberOfOverdrafts", Operator = Operator.LessThanOrEqualTo, Value = "3" , DataType = "int", LogicalOperator = LogicalOperator.AND},
    new Condition { PropertyName = "AverageMonthlyDeposit", Operator = Operator.Between, MinValue = "500", MaxValue = "3000", DataType = "decimal", LogicalOperator = LogicalOperator.AND },
    new Condition { PropertyName = "CreationDate", Operator = Operator.GreaterThan, Value = "2010-01-01", DataType = "DateTime", LogicalOperator = LogicalOperator.AND },
    new Condition { PropertyName = "TransactionCount", Operator = Operator.LessThanOrEqualTo, Value = "100", DataType = "int", LogicalOperator = LogicalOperator.AND },
    new Condition { PropertyName = "AccountAgeInYears", Operator = Operator.GreaterThanOrEqualTo, Value = "3", DataType = "int", LogicalOperator = LogicalOperator.AND },
    new Condition { PropertyName = "Balance", Operator = Operator.LessThan, Value = "1000", LogicalOperator = LogicalOperator.AND, DataType = "decimal", AdditionalConditions = new List<Condition>{ new Condition() { PropertyName = "CreditScore", Value = "600", Operator = Operator.LessThan } } },
    new Condition
    {
        IdCondition = 1,
        PropertyName = "AverageMonthlyDeposit",
        Operator = Operator.GreaterThan,
        Value = "1000",
        DataType = "decimal",
        AdditionalConditions = new List<Condition>
        {
            new Condition
            {
                IdParentCondition = 1,
                PropertyName = "CreationDate",
                MaxValue = "11/28/2023", // Indicates last 6 months
                MinValue = "5/28/2023", // Indicates last 6 months
                DataType = "DateTime"
            }
        }
    },
    //new Condition { PropertyName = "Balance", Operator = Operator.GreaterThan, Value = "500", LogicalOperator = LogicalOperator.AND, DataType = "decimal", Condition = "AccountType == 'Savings'" },
    //new Condition { PropertyName = "TransactionCount", Operator = Operator.GreaterThanOrEqualTo, Value = "10", LogicalOperator = LogicalOperator.AND, DataType = "int", Condition = "AccountType == 'Checking'" },
    //new Condition { PropertyName = "NumberOfOverdrafts", Operator = Operator.Equals, Value = "0", LogicalOperator = LogicalOperator.AND, DataType = "int", Condition = "CreditScore < 650" }
};

var ruleEngine = new RulesEngine();
var compiledRule = ruleEngine.BuildRule<Account>(rules);
var account = new Account
{
    Balance = 950, // Must be greater than $100 and less than $10,000. Also, less than $1,000 if CreditScore < 600.
    IsActive = true, // Account must be active.
    AccountType = AccountType.Savings, // Either "Savings" or "Checking".
    TransactionCount = 50, // At least 5 transactions per month but not exceeding 100.
    CreditScore = 700, // Must be greater than or equal to 700.
    LastTransactionAmount = 500, // Should not be more than $5,000. Also, less than half of the current balance (950/2 = 475).
    CreationDate = new DateTime(2011, 1, 1), // After January 1, 2010.
    AccountAgeInYears = 3, // At least 1 year old, and active for at least 3 years.
    NumberOfOverdrafts = 0, // Should not have more than 3 overdrafts. Zero for the last two years.
    AverageMonthlyDeposit = 1500, // Between $500 and $3,000. Over the last six months, it should be greater than $1,000.
    // Additional properties can be added as needed.
};
bool result = compiledRule(account);
Console.WriteLine($"Rule evaluation result: {result}");



public class Account
{
    public decimal Balance { get; set; }
    public DateTime CreationDate { get; set; }
    public bool IsActive { get; set; }
    public AccountType AccountType { get; set; }
    public int TransactionCount { get; set; }
    public int CreditScore { get; set; }
    public decimal LastTransactionAmount { get; set; }
    public int AccountAgeInYears { get; set; }
    public int NumberOfOverdrafts { get; set; }
    public decimal AverageMonthlyDeposit { get; set; }
}

public enum AccountType
{ 
    Savings, 
    Checking
}
