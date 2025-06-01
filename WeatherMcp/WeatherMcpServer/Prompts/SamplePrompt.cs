using System.ComponentModel;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Server;

namespace WeatherMcpServer.Prompts;

[McpServerPromptType]
public class SamplePrompt
{
    [McpServerPrompt(Name = "GetWeatherHuelva"),
     Description(
         "Retrieve and provide a detailed weather report for the city of Huelva, including current conditions, temperature, and any relevant weather alerts.")]
    public static ChatMessage GetWeather()
    {
        //return "What's the weather in Huelva today?";  
        return new ChatMessage(
            ChatRole.User,
            "What's the weather in Huelva today?");
    }
}
