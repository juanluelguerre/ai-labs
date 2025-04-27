namespace CopilotTester.Services;

public class DummyIncomeVerificationService : IIncomeVerificationService
{
    public decimal GetMonthlyIncome(string customerId) => 5000;
}
