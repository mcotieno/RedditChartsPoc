

using RedditCharts.Common.Providers;
using RedditChartsService.Services;

await Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHttpClient();
                    services.AddHostedService<RedditChartsWorkerService>();
                    services.AddSingleton<IRedditChartsHttpClient, RedditChartsHttpClient>();
                    services.AddSingleton<IRedditChartsPostProcessor, RedditChartsPostProcessor>();
                })
                .ConfigureLogging((hostContext, logging) =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                    logging.AddEventLog();
                })
                .UseWindowsService()
                .UseSystemd()
                .Build()
                .RunAsync();