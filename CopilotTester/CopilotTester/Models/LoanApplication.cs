using System.Text.Json.Serialization;

namespace CopilotTester.Models;

public record LoanApplication(
    string CustomerId,
    int Age,    decimal Amount,
    decimal MonthlyDebt,
    [property: JsonConverter(typeof(JsonStringEnumConverter))]
    EmploymentStatus EmploymentStatus,
    int LoanTermMonths,
    bool HasPreviousDefaults);
