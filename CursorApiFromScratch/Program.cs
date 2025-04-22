using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        options.RoutePrefix = String.Empty;
    });
}

app.UseHttpsRedirection();

app.MapPost("/upload", async (IFormFileCollection files, ILogger<Program> logger) =>
    {
        try
        {
            if (files.Count == 0)
                return Results.BadRequest("No files uploaded");

            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
            Directory.CreateDirectory(uploadPath);

            var uploadedFiles = new List<string>();

            foreach (var file in files)
            {
                if (file.Length == 0)
                    continue;

                var fileName = Path.GetRandomFileName();
                var filePath = Path.Combine(uploadPath, fileName);

                await using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                uploadedFiles.Add(fileName);
            }

            if (uploadedFiles.Count == 0)
                return Results.BadRequest("All files are empty");

            return Results.Ok(
                $"Files uploaded successfully. Saved as: {String.Join(", ", uploadedFiles)}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error uploading files");
            return Results.BadRequest($"Error uploading files: {ex.Message}");
        }
    })
    .DisableAntiforgery()
    .Accepts<IFormFileCollection>("multipart/form-data")
    .WithName("UploadFiles")
    .WithOpenApi(operation => new OpenApiOperation(operation)
    {
        Description = "Uploads one or more files to the server",
        RequestBody = new OpenApiRequestBody
        {
            Content =
            {
                ["multipart/form-data"] = new OpenApiMediaType
                {
                    Schema = new OpenApiSchema
                    {
                        Type = "object",
                        Properties =
                        {
                            ["files"] = new OpenApiSchema
                            {
                                Type = "array",
                                Items = new OpenApiSchema
                                {
                                    Type = "string",
                                    Format = "binary"
                                }
                            }
                        },
                        Required = new HashSet<string> { "files" }
                    }
                }
            }
        }
    });

app.Run();
