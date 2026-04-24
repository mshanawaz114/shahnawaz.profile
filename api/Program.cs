using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ShahnawazProfile.Api.Services;

// Azure Functions isolated worker host. SWA's managed API requires Functions
// (not ASP.NET Core Web API), so the chat endpoint is exposed as an HTTP-triggered
// function in Functions/ChatFunction.cs and resolved by the SWA routing layer at /api/chat.
var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureAppConfiguration(config =>
    {
        // Load appsettings.json from the published output so Groq:Model / Temperature etc.
        // behave the same as in the Web API project we replaced.
        config.SetBasePath(AppContext.BaseDirectory)
              .AddJsonFile("appsettings.json", optional: true)
              .AddEnvironmentVariables();
    })
    .ConfigureServices((context, services) =>
    {
        // Typed HttpClient for Groq REST API. Same configuration as the prior Web API.
        services.AddHttpClient<IGroqChatService, GroqChatService>(client =>
        {
            client.BaseAddress = new Uri("https://api.groq.com/");
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        services.AddSingleton<IPromptBuilder, PromptBuilder>();
        services.AddSingleton<IContentProvider, ContentProvider>();
    })
    .Build();

host.Run();
