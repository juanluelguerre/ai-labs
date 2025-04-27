namespace CopilotTester.Services;

public class DummyCreditScoreService : ICreditScoreService
{
    public int GetScore(string customerId) => 700;
}
