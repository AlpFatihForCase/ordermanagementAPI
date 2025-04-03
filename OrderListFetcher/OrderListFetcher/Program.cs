using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OrderListFetcher; 
using OrderListFetcher.Settings; 
using OrderListFetcher.Services; 

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.Configure<TokenSettings>(hostContext.Configuration.GetSection("TokenSettings"));
        services.Configure<OrderListSettings>(hostContext.Configuration.GetSection("OrderListSettings"));
        services.Configure<WorkerSettings>(hostContext.Configuration.GetSection("WorkerSettings"));

        services.AddHttpClient(); 
        services.AddSingleton<TokenService>(); 
        services.AddSingleton<OrderFetchingService>(); 

        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();