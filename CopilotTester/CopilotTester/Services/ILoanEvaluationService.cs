using CopilotTester.Models;

namespace CopilotTester.Services;

public interface ILoanEvaluationService
{
    LoanEvaluationResult Evaluate(LoanApplication application);
}
