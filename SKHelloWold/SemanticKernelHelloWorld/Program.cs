using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Drawing;
using System.Text;

const string modelPath =
    @"C:\phi-3\models\Phi-3-mini-4k-instruct-onnx\cpu_and_mobile\cpu-int4-awq-block-128";

const string culture = "en-us";
const string currency = "eur";

var products = new List<Product>
{
    new Product("MacBook Pro", "Electronics", "MBP2023", Color.Silver, 2399.99),
    new Product("ErgoChair 2", "Furniture", "ERGO2", Color.Gray, 349.99),
    new Product("Nike Air Max", "Footwear", "AIRMAX2023", Color.White, 149.99),
    new Product("Samsung Galaxy S21", "Electronics", "S21", Color.Pink, 799.99),
    new Product("Sony WH-1000XM4", "Electronics", "WH1000XM4", Color.Black, 349.99),
    new Product("IKEA Malm Desk", "Furniture", "MALM001", Color.Brown, 199.99),
};

// create kernel
var builder = Kernel.CreateBuilder();
builder.AddOnnxRuntimeGenAIChatCompletion(modelPath: modelPath);
var kernel = builder.Build();

// create chat
var chat = kernel.GetRequiredService<IChatCompletionService>();

foreach (var p in products)
{
    Console.WriteLine($"Generating description for product: {p.Name} ...");

    var question = GetQuestion(p);

    var history = new ChatHistory();
    history.AddUserMessage(question);

    var response = new StringBuilder();
    var result = chat.GetStreamingChatMessageContentsAsync(history);
    await foreach (var message in result)
    {
        response.Append(message.Content);
    }

    history.AddAssistantMessage(response.ToString());

    p.AddDescription(response.ToString());

    Console.WriteLine(response.ToString());
    Console.WriteLine();
}

return;

string GetQuestion(Product p) =>
    $"Get a unique sentence to describe (in language based in the culture '{culture}' and price based in the currency '{currency}') in no more 500 characters. Make sure that in the maximum of these exactly 500 characters the description is completed and finalize with a dot. " +
    $"The product: Category: {p.Type}, Type: {p.Type}, Color: {p.Color}, Model: {p.Model}, Price: {p.Price}.";

public record Product(string Name, string Type, string Model, Color Color, double Price)
{
    public string? Description { get; private set; }
    public void AddDescription(string description) => this.Description = description;
};
