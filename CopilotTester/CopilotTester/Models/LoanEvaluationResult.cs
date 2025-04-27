namespace CopilotTester.Models;

public record LoanEvaluationResult(bool Approved, string Reason)
{
    public static LoanEvaluationResult CreateApproved() => new(true, "Approved");
    public static LoanEvaluationResult Denied(string reason) => new(false, reason);
}
