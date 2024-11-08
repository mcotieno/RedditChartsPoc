using RedditCharts.Common.Providers;

namespace RedditChartsService.Services
{
    /// <summary>
    /// Background service implementation that can be run as windows or linux daemon
    /// </summary>
    public class RedditChartsWorkerService : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly IRedditChartsHttpClient _redditChartsHttpClient;
        private readonly IRedditChartsPostProcessor _redditChartsPostProcessor;
        private readonly ILogger<RedditChartsWorkerService> _logger;

        public RedditChartsWorkerService(
            IConfiguration configuration,
            IRedditChartsHttpClient redditChartsHttpClient,
            IRedditChartsPostProcessor redditChartsPostProcessor,
            ILogger<RedditChartsWorkerService> logger)
        {
            _configuration = configuration;
            _redditChartsHttpClient = redditChartsHttpClient;
            _redditChartsPostProcessor = redditChartsPostProcessor;
            _logger = logger;
        } 

        /// <summary>
        /// Execute method of the service 
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Reddit Monitor Service starting...");

            var subreddits = _configuration.GetSection("Reddit:Subreddits").Get<List<string>>();

            if (subreddits == null || !subreddits.Any())
            {
                _logger.LogError("No subreddits configured. Please check appsettings.json");
                return;
            }

            _logger.LogInformation("Monitoring subreddits: {Subreddits}", string.Join(", ", subreddits));

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessSubredditsAsync(subreddits, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in monitoring loop");
                }

                await Task.Delay(TimeSpan.FromSeconds(_configuration.GetValue("Reddit:PollingIntervalSeconds", 30)), stoppingToken);
            }
        }

        /// <summary>
        /// Asynchronous processing of multiple subreddits
        /// </summary>
        /// <param name="subreddits"></param>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        private async Task ProcessSubredditsAsync(IEnumerable<string> subreddits, CancellationToken stoppingToken)
        {
           
            var tasks = subreddits.Select(async subreddit =>
            {
                var posts = await _redditChartsHttpClient.GetNewPostsAsync(subreddit, stoppingToken);

                await _redditChartsPostProcessor.ProcessPostAsync(posts, stoppingToken);
            });

            await Task.WhenAll(tasks);
        }
    }
}

