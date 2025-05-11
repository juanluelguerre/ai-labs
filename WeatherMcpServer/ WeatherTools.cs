using System.ComponentModel;
using System.Net.Http.Json;
using ModelContextProtocol.Server;
using System.Globalization;

namespace WeatherMcpServer;

[McpServerToolType]
public static class WeatherTools
{
    private static readonly HttpClient httpClient = new();

    [McpServerTool, Description("Get the current temperature in a city.")]
    public static async Task<string> GetWeatherAsync(string city)
    {
        try
        {
            var geoUrl = $"https://geocoding-api.open-meteo.com/v1/search?name={Uri.EscapeDataString(city)}&count=1";
            var geoResponse = await httpClient.GetFromJsonAsync<GeoResponse>(geoUrl);

            var (lat, lon) = GetFormatedCoordinates(geoResponse, city);

            var weatherUrl = $"https://api.open-meteo.com/v1/forecast?latitude={lat}&longitude={lon}&current_weather=true";
            var weatherResponse = await httpClient.GetFromJsonAsync<WeatherResponse>(weatherUrl);

            var weather = weatherResponse?.current_weather;

            return weather is null
                ? $"Could not retrieve weather data for {city}"
                : $"Current temperature in {city} is {weather.temperature}°C with wind speed {weather.windspeed} km/h.";
        }
        catch (Exception ex)
        {
            return $"Error fetching weather: {ex.Message}";
        }
    }

    private static (string lat, string lon) GetFormatedCoordinates(GeoResponse? geoResponse, string city)
    {
        if (geoResponse?.results == null || geoResponse.results.Length == 0)
            throw new ArgumentException($"Could not find location for {city}");

        var (lat, lon) = (geoResponse.results[0].latitude, geoResponse.results[0].longitude);
        return (
            lat.ToString(CultureInfo.InvariantCulture),
            lon.ToString(CultureInfo.InvariantCulture)
        );
    }

    public class GeoResponse
    {
        public GeoResult[] results { get; set; }
    }

    public class GeoResult
    {
        public double latitude { get; set; }
        public double longitude { get; set; }
    }

    public class WeatherResponse
    {
        public CurrentWeather current_weather { get; set; }
    }

    public class CurrentWeather
    {
        public double temperature { get; set; }
        public double windspeed { get; set; }
    }
}