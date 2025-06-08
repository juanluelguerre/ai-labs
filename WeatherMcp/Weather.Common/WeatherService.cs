using System.Globalization;
using System.Net.Http.Json;
using Weather.Common.Models;

namespace Weather.Common;

public static class WeatherService
{
    public static async Task<string> GetWeather(string city)
    {
        try
        {
            var geoUrl =
                $"https://geocoding-api.open-meteo.com/v1/search?name={Uri.EscapeDataString(city)}&count=1";
            var geoResponse = await new HttpClient().GetFromJsonAsync<GeoResponse>(geoUrl);
            var (lat, lon) = GetFormattedCoordinates(geoResponse, city);
            var weatherUrl =
                $"https://api.open-meteo.com/v1/forecast?latitude={lat}&longitude={lon}&current_weather=true";
            var weatherResponse =
                await new HttpClient().GetFromJsonAsync<WeatherResponse>(weatherUrl);
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

    private static (string lat, string lon) GetFormattedCoordinates(
        GeoResponse? geoResponse, string city)
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
