using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace WeatherAIConsole.Plugins;

public class TimePlugin
{
    [KernelFunction]
    [Description("Gets the current system time")]
    public string GetCurrentTime()
    {
        return DateTime.Now.ToString("HH:mm:ss");
    }

    [KernelFunction]
    [Description("Gets the current system date and time")]
    public string GetCurrentDateTime()
    {
        return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }
}
