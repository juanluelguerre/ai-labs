using CopilotTester.Models;

namespace CopilotTester.Services;

public class LoanEvaluationService(
    ICreditScoreService creditScoreService,
    IIncomeVerificationService incomeService)
    : ILoanEvaluationService
{
    public LoanEvaluationResult Evaluate(LoanApplication application)
    {
        if (application == null)
            throw new ArgumentNullException(nameof(application));

        if (application.Amount <= 0)
            return LoanEvaluationResult.Denied("Invalid loan amount.");

        if (application.LoanTermMonths is < 6 or > 84)
            return LoanEvaluationResult.Denied("Loan term must be between 6 and 84 months.");

        if (application.Age is < 18 or > 70)
            return LoanEvaluationResult.Denied("Applicant must be between 18 and 70 years old.");

        if (application.EmploymentStatus is not
            (EmploymentStatus.Employed or EmploymentStatus.SelfEmployed))
            return LoanEvaluationResult.Denied("Unacceptable employment status.");

        if (application.HasPreviousDefaults)
            return LoanEvaluationResult.Denied("Applicant has a history of defaults.");

        var creditScore = creditScoreService.GetScore(application.CustomerId);
        if (creditScore < 600)
            return LoanEvaluationResult.Denied("Low credit score.");

        var income = incomeService.GetMonthlyIncome(application.CustomerId);
        if (income <= 0)
            return LoanEvaluationResult.Denied("Income could not be verified.");

        var monthlyInstallment = application.Amount / application.LoanTermMonths;
        var debtToIncomeRatio = (application.MonthlyDebt + monthlyInstallment) / income;

        if (debtToIncomeRatio > 0.4m)
            return LoanEvaluationResult.Denied("High debt-to-income ratio.");

        if (application.Amount > income * 12)
            return LoanEvaluationResult.Denied("Loan amount exceeds annual income.");

        return LoanEvaluationResult.CreateApproved();
    }
}
