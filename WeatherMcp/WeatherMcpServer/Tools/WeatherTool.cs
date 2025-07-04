using System.ComponentModel;
using ModelContextProtocol.Server;
using Weather.Common;

namespace WeatherMcpServer.Tools;

[McpServerToolType]
public class WeatherTool
{
    [McpServerTool,
     Description("Provides current temperature and weather for a given city.")]
    public async Task<string> GetWeather(string city)
    {
        return await WeatherService.GetWeather(city);
    }
}
