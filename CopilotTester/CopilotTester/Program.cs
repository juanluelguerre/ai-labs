using CopilotTester.Models;
using CopilotTester.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddScoped<ILoanEvaluationService, LoanEvaluationService>();
builder.Services.AddScoped<ICreditScoreService, DummyCreditScoreService>();
builder.Services.AddScoped<IIncomeVerificationService, DummyIncomeVerificationService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Loan Evaluation API V1");
    });
}

app.UseHttpsRedirection();


app.MapPost(
    "/evaluate", (LoanApplication application, ILoanEvaluationService service) =>
    {
        var result = service.Evaluate(application);
        return Results.Ok(result);
    });

app.Run();
