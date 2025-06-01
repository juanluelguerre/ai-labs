using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WeatherMcpServer.Prompts;
using WeatherMcpServer.Resources;
using WeatherMcpServer.Tools;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.AddConsole();

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithPromptsFromAssembly()
    .WithToolsFromAssembly()
    .WithTools<WeatherTool>();
//.WithResources<SampleResource>()
//.WithPrompts<SamplePrompt>();

await builder.Build().RunAsync();
