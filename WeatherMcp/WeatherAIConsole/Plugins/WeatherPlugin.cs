using System.ComponentModel;
using Microsoft.SemanticKernel;
using Weather.Common;

namespace WeatherAIConsole.Plugins;

public class WeatherPlugin
{
    private readonly HttpClient httpClient = new();

    [KernelFunction,
     Description("Provides current temperature and weather for a given city.")]
    public async Task<string> GetWeather(string city)
    {
        return await WeatherService.GetWeather(city);
    }
}
