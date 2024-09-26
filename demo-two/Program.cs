// See https://aka.ms/new-console-template for more information
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.Memory;
    using System.Linq;

#pragma warning disable SKEXP0010, SKEXP0001, SKEXP0050

var builder = new ConfigurationBuilder()
    .AddUserSecrets(Assembly.GetExecutingAssembly(), true)
    .AddEnvironmentVariables()
    .AddCommandLine(args);

IConfiguration configuration = builder.Build();


Console.WriteLine("Hello, Vitas!");

var kernelBuilder = Kernel.CreateBuilder();


var kernel = kernelBuilder.AddAzureOpenAITextEmbeddingGeneration(
    "text-embedding-ada-002",
    configuration["AZURE_OPENAI_API_ENDPOINT"] ??
    throw new InvalidOperationException("Please set your open ai endpoint in the user secrets"),
    configuration["AZURE_OPENAI_API_KEY"] ??
    throw new InvalidOperationException("Please set your open ai key in the user secrets")
).Build();

var embeddingsService = kernel.GetRequiredService<ITextEmbeddingGenerationService>();
var memory = new SemanticTextMemory(new VolatileMemoryStore(), embeddingsService);
const string memoryCollectionName = "vitas";
var id = 0;

Console.WriteLine("Welcome to the Vitas embedding demo!");

var running = true;

while (running)
{
    Console.WriteLine("Next command please");
    var command = Console.ReadLine();
    switch (command)
    {
        case "Exit":
            running = false;
            break;
        case "Embed":
            await Demo.RunEmbedding(memory, memoryCollectionName, $"embedding{id}");
            id++;
            break;
        case "Ask":
            await Demo.RunAsk(memory, memoryCollectionName);
            break;
    }
}

public static class Demo 
{
    public static async Task RunEmbedding(SemanticTextMemory memory, string collection, string embeddingId)
    {
        Console.WriteLine("Enter text to be embedded:");
        var text = Console.ReadLine()!;
        await memory.SaveInformationAsync(collection, id: embeddingId, text: text);
    }
    
    public static async Task RunAsk(SemanticTextMemory memory, string collection) 
    {
        Console.WriteLine("Enter the question you want to ask:");
        var text = Console.ReadLine()!;
        var responses = memory.SearchAsync(collection, text);
        await foreach (var response in responses)
        {
            Console.WriteLine("Q: " + text);
            Console.WriteLine("A: " + response?.Relevance + "\t" + response?.Metadata.Text);   
        }
    }
}