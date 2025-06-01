using Microsoft.SemanticKernel;

namespace WeatherMcpServer;

public class FunctionInvocationLogger : IFunctionInvocationFilter
{
    public Task OnFunctionInvocationAsync(
        FunctionInvocationContext context, Func<FunctionInvocationContext, Task> next)
    {
        Console.WriteLine($"📡 Function called: {context.Function.Name}");
        return Task.CompletedTask;
    }
}
