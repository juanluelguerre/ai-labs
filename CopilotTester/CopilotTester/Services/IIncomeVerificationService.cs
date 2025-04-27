namespace CopilotTester.Services;

public interface IIncomeVerificationService
{
    decimal GetMonthlyIncome(string customerId);
}
