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
        Console.WriteLine($"{tool.Name}: {tool.Description}");
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

    var agent = new ChatCompletionAgent
    {
        Id = "WeatherAgent",
        Name = "WeatherAgent",
        Instructions =
            "You are a weather assistant. Your primary task is to provide current weather information. When asked for the weather (e.g., temperature, conditions) in a specific city, you MUST use the 'GetWeather-GetWeather' function to get this data. Do not attempt to answer weather queries without using this function.",
        Kernel = kernel,
        Arguments = new KernelArguments(
            new PromptExecutionSettings
            {
                FunctionChoiceBehavior =
                    FunctionChoiceBehavior.Auto()
            })
    };

    ChatMessageContent response =
        await agent.InvokeAsync("What's the weather in Madrid?").FirstAsync();
    //Console.WriteLine($"Response from WeatherAgent: \n{response.Content}");

    var content = response.Content?.ToString() ?? "";
    var match = System.Text.RegularExpressions.Regex.Match(
        content, @"GetWeather\(['""](?<city>[^'""]+)['""]\)");

    if (match.Success)
    {
        var city = match.Groups["city"].Value;
        Console.WriteLine($"[INFO] Ejecutando GetWeather para: {city}");

        // call the tool via the plugin functions
        var getWeatherFunction = kernel.Plugins["GetWeather"]["GetWeather"];
        var toolResult = await getWeatherFunction.InvokeAsync(
            kernel, new KernelArguments { ["city"] = city });
        Console.WriteLine($"[TOOL RESULT] {toolResult}");
    }
    else
    {
        Console.WriteLine("[INFO] No se detectó llamada a función en la respuesta.");
    }


    //--------------------------------------
    // My own chat completion service using OnnxRuntimeGenAI
    //--------------------------------------     
    //var chat = kernel.GetRequiredService<IChatCompletionService>();


    //var history = new ChatHistory();
    //history.AddUserMessage("What's the weather in Madrid?");

    //var response = new StringBuilder();
    //var result = chat.GetStreamingChatMessageContentsAsync(history);
    //await foreach (var message in result)
    //{
    //    response.Append(message.Content);
    //}

    //history.AddAssistantMessage(response.ToString());

    //Console.WriteLine(response.ToString());
    //Console.WriteLine();

    // -----------------
    //https://dev.to/stormhub/model-context-protocol-integration-with-microsoft-semantic-kernel-1cgb
    //----------------
    //var executionSettings = new PromptExecutionSettings
    //{
    //    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(
    //        options: new FunctionChoiceBehaviorOptions { RetainArgumentTypes = true }),
    //    ExtensionData = new Dictionary<string, object>
    //    {
    //        { "temperature", 0 }
    //    }
    //};

    //var chatCompletionService =
    //    kernel.Services.GetRequiredService<IChatCompletionService>();
    //var history = new ChatHistory();
    //history.AddUserMessage("What's the weather in Madrid?");
    //var messageContent = await chatCompletionService.GetChatMessageContentAsync(
    //    history,
    //    executionSettings,
    //    kernel);
    //Console.WriteLine(messageContent.Content);
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


//public sealed class OnnxRuntimeGenAIChatCompletionService : IChatCompletionService, IDisposable
//{
//    private readonly static OgaHandle s_ogaHandle = new();

//    static OnnxRuntimeGenAIChatCompletionService()
//        => AppDomain.CurrentDomain.ProcessExit += (sender, e) => s_ogaHandle.Dispose();

//    public IReadOnlyDictionary<string, object?> Attributes { get; }

//    public Task<IReadOnlyList<ChatMessageContent>> GetChatMessageContentsAsync(
//        ChatHistory chatHistory, PromptExecutionSettings? executionSettings = null,
//        Kernel? kernel = null, CancellationToken cancellationToken = new CancellationToken())
//    {
//        throw new NotImplementedException();
//    }

//    public IAsyncEnumerable<StreamingChatMessageContent> GetStreamingChatMessageContentsAsync(
//        ChatHistory chatHistory, PromptExecutionSettings? executionSettings = null,
//        Kernel? kernel = null, CancellationToken cancellationToken = new CancellationToken())
//    {
//        throw new NotImplementedException();
//    }

//    public void Dispose()
//    {
//        throw new NotImplementedException();
//    }
//}


//var content = response.Content?.ToString() ?? "";
//// Regex corregido: sin espacios en el nombre del grupo y sin espacios extra
//var match = System.Text.RegularExpressions.Regex.Match(
//    content, @"GetWeather\(['""](?<city>[^'""]+)['""]\)");

//if (match.Success)
//{
//    var city = match.Groups["city"].Value;
//    Console.WriteLine($"[INFO] Ejecutando GetWeather para: {city}");

//    // Call the tool via the plugin functions
//    var getWeatherFunction = kernel.Plugins["GetWeather"]["GetWeather"];
//    var toolResult = await getWeatherFunction.InvokeAsync(kernel, new() { ["city"] = city });
//    Console.WriteLine($"[TOOL RESULT] {toolResult}");
//}
//else
//{
//    Console.WriteLine("[INFO] No se detectó llamada a función en la respuesta.");
//}


//const string question = "What's the weather in Madrid (Spain)?";

//var history = new ChatHistory();
//history.AddUserMessage(question);

//var response = new StringBuilder();
//var result = agent.InvokeStreamingAsync(history);
//await foreach (var message in result)
//{
//    response.Append(message.Content);
//}

//history.AddAssistantMessage(response.ToString());

//Console.WriteLine(response.ToString());
//Console.WriteLine();

// Define a custom filter to log function invocations
