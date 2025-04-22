//
// REF: https://github.com/microsoft/onnxruntime-genai/tree/main/examples/csharp/HelloPhi3V
//
// 
// huggingface - cli download microsoft/Phi-3-vision-128k-instruct-onnx-cpu --include cpu-int4-rtn-block-32-acc-level-4/* --local-dir .

using Microsoft.ML.OnnxRuntimeGenAI;

namespace Phi3Vision;

public static class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine(@"--------------------");
        Console.WriteLine(@"Hello, Phi-3-Vision!");
        Console.WriteLine(@"--------------------");

        Run(@"C:\phi-3\models\cpu-int4-rtn-block-32-acc-level-4");
    }

    private static void Run(string modelPath)
    {
        using var model = new Model(modelPath);
        using var processor = new MultiModalProcessor(model);
        using var tokenizerStream = processor.CreateStream();

        var fileImagePath = Path.Combine(Directory.GetCurrentDirectory(), "images",
            "PlayingWithAspire.png");
        if (!File.Exists(fileImagePath))
        {
            Console.WriteLine(@"Image not found.");
            return;
        }

        var images = Images.Load(fileImagePath);

        const string text = "What do you see in the picture?";
        var prompt = "<|user|>\n";
        if (images != null)
        {
            prompt += "<|image_1|>\n";
        }

        prompt += text + "<|end|>\n<|assistant|>\n";

        Console.WriteLine(@"Processing image and prompt...");
        var inputTensors = processor.ProcessImages(prompt, images);

        Console.WriteLine(@"Generating response...");
        using var generatorParams = new GeneratorParams(model);
        generatorParams.SetSearchOption("max_length", 3072);
        generatorParams.SetInputs(inputTensors);

        using var generator = new Generator(model, generatorParams);
        while (!generator.IsDone())
        {
            generator.ComputeLogits();
            generator.GenerateNextToken();
            Console.Write(tokenizerStream.Decode(generator.GetSequence(0)[^1]));
        }
    }
}
