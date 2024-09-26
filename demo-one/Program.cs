using System.Reflection;
using demo_one;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Microsoft.SemanticKernel.Connectors.OpenAI;

var builder = new ConfigurationBuilder()
    .AddUserSecrets(Assembly.GetExecutingAssembly(), true)
    .AddEnvironmentVariables()
    .AddCommandLine(args);

var configuration = builder.Build();

Console.WriteLine("Hello, Vitas!");


var kernelBuilder = Kernel.CreateBuilder();
kernelBuilder.Services.AddLogging(kb => kb.AddConsole().SetMinimumLevel(LogLevel.Trace));


var kernel = kernelBuilder
    .AddAzureOpenAIChatCompletion(
        "gpt-4",  
        configuration["AZURE_OPENAI_API_ENDPOINT"] ?? throw new InvalidOperationException("Please set your open ai endpoint in the user secrets"), 
        configuration["AZURE_OPENAI_API_KEY"] ?? throw new InvalidOperationException("Please set your open ai key in the user secrets"))
    .Build();

var chatService = kernel.GetRequiredService<IChatCompletionService>();
var chatHistory = new ChatHistory();

kernel.ImportPluginFromType<DemoGraphics>();

var executionSettings = new AzureOpenAIPromptExecutionSettings() 
{
    ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
};

var running = true;

        
while(running) 
{
    Console.Write("A: ");
    var input = Console.ReadLine();
    if (input == "exit") 
    {
        running = false;
        continue;
    }
    
    chatHistory.AddUserMessage(input!);
    var aiResponse = await chatService.GetChatMessageContentAsync(chatHistory, executionSettings, kernel);
    
    
    chatHistory.Add(aiResponse);
    
    Console.WriteLine($"A: {aiResponse}");
}
