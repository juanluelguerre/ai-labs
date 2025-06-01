using System.ComponentModel;
using System.Net.Http.Json;
using ModelContextProtocol.Server;
using System.Globalization;
using WeatherMcpServer.Models;

namespace WeatherMcpServer.Tools;

[McpServerToolType]
public class WeatherTool
{
    private readonly HttpClient httpClient = new();

    [McpServerTool,
     Description("Provides current temperature and weather for a given city.")]
    public async Task<string> GetWeatherAsync(string city)
    {
        Console.WriteLine($"[WeatherTool] GetWeatherAsync invoked for city: {city}");

        try
        {
            var geoUrl =
                $"https://geocoding-api.open-meteo.com/v1/search?name={Uri.EscapeDataString(city)}&count=1";
            var geoResponse = await this.httpClient.GetFromJsonAsync<GeoResponse>(geoUrl);

            var (lat, lon) = this.GetFormatedCoordinates(geoResponse, city);

            var weatherUrl =
                $"https://api.open-meteo.com/v1/forecast?latitude={lat}&longitude={lon}&current_weather=true";
            var weatherResponse =
                await this.httpClient.GetFromJsonAsync<WeatherResponse>(weatherUrl);

            var weather = weatherResponse?.current_weather;

            return weather is null
                ? $" Could not retrieve weather data for {city}"
                : $"Current temperature in {city} is {weather.temperature}°C with wind speed {weather.windspeed} km/h.";
        }
        catch (Exception ex)
        {
            return $"Error fetching weather: {ex.Message}";
        }
    }

    private (string lat, string lon) GetFormatedCoordinates(GeoResponse? geoResponse, string city)
    {
        if (geoResponse?.results == null || geoResponse.results.Length == 0)
            throw new ArgumentException($"Could not find location for {city}");

        var (lat, lon) = (geoResponse.results[0].latitude, geoResponse.results[0].longitude);
        return (
            lat.ToString(CultureInfo.InvariantCulture),
            lon.ToString(CultureInfo.InvariantCulture)
        );
    }
}
