using Azure.Search.Documents;
using Microsoft.SemanticKernel;
using ChatBotApi;

var builder = WebApplication.CreateBuilder(args);

// Load configuration from appsettings.json
var configuration = builder.Configuration;

// Register SearchClient
builder.Services.AddSingleton<SearchClient>(sp =>
{
    var endpoint = configuration["CognitiveSearch:Endpoint"];
    var apiKey = configuration["CognitiveSearch:ApiKey"];
    var indexName = configuration["CognitiveSearch:IndexName"];

    return new SearchClient(new Uri(endpoint), indexName, new Azure.AzureKeyCredential(apiKey));
});

builder.Services.AddSingleton<SearchService>();

// Add Semantic Kernel with Azure OpenAI Chat Completion
builder.Services.AddSingleton<Kernel>(sp =>
{
    var apiKey = configuration["AzureOpenAI:ApiKey"];
    var endpoint = configuration["AzureOpenAI:Endpoint"];
    var deploymentName = configuration["AzureOpenAI:DeploymentName"];

    var kernel = Kernel
        .CreateBuilder()
        .AddAzureOpenAIChatCompletion(deploymentName, endpoint, apiKey)
        .Build();;

    return kernel;
});

// Register services
builder.Services.AddSingleton<SemanticKernelService>();

// Add controllers
builder.Services.AddControllers();

var app = builder.Build();

// Configure middleware
app.UseRouting();
app.UseAuthorization();
app.MapControllers();

app.Run();