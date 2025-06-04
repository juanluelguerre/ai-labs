#pragma warning disable SKEXP0070
#pragma warning disable SKEXP0001

using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ML.OnnxRuntimeGenAI;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using ModelContextProtocol.Client;
using WeatherMcpServer;

const string mcpServerProjectPath =
    @"C:\code\elguerre\ai-labs\WeatherMcp\WeatherMcpServer\WeatherMcpServer.csproj";

try
{
    var config = new ConfigurationBuilder()
        .Build();

    Console.WriteLine("Client Weather Console started !");

    // ----------------- MCP Server Setup ----------------- 

    await using var mcpClient = await McpClientFactory.CreateAsync(
        new StdioClientTransport(
            new StdioClientTransportOptions
            {
                Name = "GetWeather",
                Command = "dotnet",
                Arguments =
                [
                    "run", "--project", mcpServerProjectPath
                ],
            }));

    var tools = await mcpClient.ListToolsAsync();
    foreach (var tool in tools)
    {
        Console.WriteLine($" - {tool.Name}: {tool.Description}");
    }

    // ----------------- Semantic Kernel with the MCP Tools -----------------

    const string modelPath =
        @"C:\ai-models\phi-3\Phi-3-mini-4k-instruct-onnx\cpu_and_mobile\cpu-int4-awq-block-128";
    //@"C:\ai-models\phi-4\cpu_and_mobile\cpu-int4-rtn-block-32-acc-level-4";

    var builder = Kernel.CreateBuilder()
        .AddOnnxRuntimeGenAIChatCompletion("phi-3", modelPath);
    builder.Plugins.AddFromFunctions(
        pluginName: "GetWeather",
        functions: tools.Select(aiFunction => aiFunction.AsKernelFunction()));

    var kernel = builder.Build();

    kernel.FunctionInvocationFilters.Add(new FunctionInvocationLogger());

    var executionSettings = new PromptExecutionSettings
    {
        FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(
            options: new FunctionChoiceBehaviorOptions { RetainArgumentTypes = true }),
        ExtensionData = new Dictionary<string, object>
        {
            { "temperature", 0 }
        }
    };

    // ----------------- Chat Completion  -----------------
    var chat =
        kernel.Services.GetRequiredService<IChatCompletionService>();
    var history = new ChatHistory();
    history.AddUserMessage("What's the weather in Madrid?");
    var messageContent = await chat.GetChatMessageContentAsync(
        history,
        executionSettings,
        kernel);
    Console.WriteLine(messageContent.Content);
}
catch (Exception ex)
{
    Console.WriteLine($"[ERROR] An error occurred: {ex.Message}");
    Console.WriteLine(ex.StackTrace);
}
finally
{
    GC.Collect();
    GC.WaitForPendingFinalizers();
    GC.Collect();
}

Console.WriteLine();
