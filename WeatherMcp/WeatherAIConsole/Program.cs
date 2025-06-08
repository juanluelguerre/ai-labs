#pragma warning disable SKEXP0070
#pragma warning disable SKEXP0001

using System.Text;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Google;
using Microsoft.SemanticKernel.Connectors.Onnx;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using ModelContextProtocol.Client;
using WeatherAIConsole.Plugins;

try
{
    await using var mcpClient = await CreateMcpClient();
    var tools = await mcpClient.ListToolsAsync();

    PrintMcpTools(tools);

    // ONNX
    //const string modelPath =
    //    //@"C:\ai-models\phi-3\Phi-3-mini-4k-instruct-onnx\cpu_and_mobile\cpu-int4-awq-block-128";
    //    @"C:\ai-models\phi-4\cpu_and_mobile\cpu-int4-rtn-block-32-acc-level-4";

    var kernelBuilder = Kernel.CreateBuilder()
        //.AddOnnxRuntimeGenAIChatCompletion("phi-4", modelPath);
        .AddGoogleAIGeminiChatCompletion(
            "gemini-2.0-flash",
            Environment.GetEnvironmentVariable("GEMINI_API_KEY") ?? String.Empty);

    // MCP TOOLS
    kernelBuilder.Plugins.AddFromFunctions(
        "Tools", tools.Select(aiFunction => aiFunction.AsKernelFunction()));


    //kernelBuilder.Plugins.AddFromType<TimePlugin>();
    //kernelBuilder.Plugins.AddFromType<WeatherPlugin>();

    var kernel = kernelBuilder.Build();

    #region Direct plugin usages to test/debug them

    //var timePlugin = kernel.Plugins[nameof(TimePlugin)];
    //var currentTime = await kernel.InvokeAsync(timePlugin["GetCurrentTime"]);
    //Console.WriteLine($"[Plugin] Current time is >>: {currentTime}");
    //
    //var weatherPlugin = kernel.Plugins[nameof(WeatherPlugin)];
    //var currentWeather = await kernel.InvokeAsync(
    //    weatherPlugin["GetWeather"], new() { { "city", "Huelva" } });
    //Console.WriteLine($"[Plugin] Current weather is >>: {currentWeather}");

    #endregion

    #region ONNX

    //var executionSettings = new OnnxRuntimeGenAIPromptExecutionSettings
    //{
    //    Temperature = 0,
    //    FunctionChoiceBehavior =
    //        FunctionChoiceBehavior.Auto(
    //            options: new FunctionChoiceBehaviorOptions { RetainArgumentTypes = true, }),
    //    ToolCallBehavior = OnnxRuntimeGenAIToolCallBehavior.AutoInvokeKernelFunctions,
    //};

    #endregion

    var executionSettings = new GeminiPromptExecutionSettings
    {
        Temperature = 0,
        ToolCallBehavior = GeminiToolCallBehavior.AutoInvokeKernelFunctions,
    };

    // Option 1
    var chatService = kernel.Services.GetRequiredService<IChatCompletionService>();
    var chatMessages = new ChatHistory();

    while (true)
    {
        Console.Write("Prompt: ");
        chatMessages.AddUserMessage(Console.ReadLine() ?? String.Empty);

        var completion =
            chatService.GetStreamingChatMessageContentsAsync(
                chatMessages, executionSettings, kernel);

        var fullMessage = "> ";
        await foreach (var content in completion)
        {
            Console.Write(content.Content);
            fullMessage += content.Content ?? String.Empty;
        }

        chatMessages.AddAssistantMessage(fullMessage);
        Console.WriteLine();
    }

    // Option 2
    // const string prompt = "What's the weather in Huelva?";
    // var result = await kernel.InvokePromptAsync(prompt, new(executionSettings));
    // Console.WriteLine(result);

    // Console.Write("Press ENTER to finish...");
    // Console.ReadKey();
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

async Task<IMcpClient> CreateMcpClient()
{
    //const string mcpServerProjectPath =
    //    @"C:\code\elguerre\ai-labs\WeatherMcp\WeatherMcpServer\WeatherMcpServer.csproj";

    var mcpServerProjectPath = Path.GetFullPath(
        Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "..", "..",
            "WeatherMcpServer", "WeatherMcpServer.csproj"));

    var clientTransport = new StdioClientTransport(
        new StdioClientTransportOptions
        {
            Name = "Get-Weather",
            Command = "dotnet",
            Arguments =
            [
                "run", "--project", mcpServerProjectPath /*, "--no-build"*/
            ],
        });

    return await McpClientFactory.CreateAsync(clientTransport);
}

void PrintMcpTools(IList<McpClientTool> tools)
{
    foreach (var tool in tools)
    {
        Console.WriteLine($" - {tool.Name}: {tool.Description}");
    }
}
